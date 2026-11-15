using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

internal interface ISouvenirClient : IDisposable
{
    DateTimeOffset ServerNow { get; }
    Task<SouvenirClient.Visit> StartVisit(string key, string performer, CancellationToken stop);
    Task ProgressVisit(SouvenirClient.Visit visit, int index, CancellationToken stop);
    Task Complete(SouvenirClient.Visit visit, CancellationToken stop);
    Task Reset(SouvenirClient.Visit visit, CancellationToken stop);
    Task<SouvenirClient.Claim> Assign(SouvenirClient.Visit visit, CancellationToken stop);
}

internal sealed class SouvenirClient : ISouvenirClient
{
    [Serializable] sealed class Kiosk { public string kioskId; }
    [Serializable] sealed class Challenge { public string challengeId, message; public long expiresAt; }
    [Serializable] sealed class Proof { public string challengeId, signature; }
    [Serializable] sealed class Session { public string session; public long expiresAt; }
    [Serializable] sealed class Start { public string performer; }
    [Serializable] sealed class Progress { public string eventId, milestone; public int sequence; }
    [Serializable] sealed class ErrorBody { public ErrorCode error; }
    [Serializable] sealed class ErrorCode { public string code; }
    [Serializable] internal sealed class Visit { public string visitId, visitToken; }
    [Serializable] internal sealed class Claim { public string assignmentId, claimUrl, issuedAt, expiresAt; }

    internal sealed class Failure : Exception
    {
        public readonly int Status;
        public readonly string Code;
        public Failure(int status, string code) { Status = status; Code = code; }
    }

    internal static readonly string[] Milestones = { "entered", "journey-started", "journey-finished", "closing-reached" };
    static readonly Regex Token = new(@"\A[A-Za-z0-9_-]{43}\z");
    WindowsSouvenirSigner signer;
    Session session;
    DateTimeOffset sessionRenewAt;
    bool disposed;
    TimeSpan serverTimeOffset;
    public DateTimeOffset ServerNow => DateTimeOffset.UtcNow + serverTimeOffset;

    public async Task<Visit> StartVisit(string key, string performer, CancellationToken stop)
    {
        var visit = Parse<Visit>(await Authorized("/api/visits", new Start { performer = performer }, null, key, stop));
        if (!Guid.TryParseExact(visit.visitId, "D", out _) || !Token.IsMatch(visit.visitToken ?? ""))
            throw new Failure(0, "invalid_response");
        return visit;
    }

    public async Task ProgressVisit(Visit visit, int index, CancellationToken stop) =>
        await Authorized(Path(visit, "progress"),
            new Progress { eventId = "living-glass-" + (index + 1), sequence = index + 1, milestone = Milestones[index] },
            visit, null, stop);
    public async Task Complete(Visit visit, CancellationToken stop) =>
        await Authorized(Path(visit, "complete"), null, visit, null, stop);
    public async Task Reset(Visit visit, CancellationToken stop) =>
        await Authorized(Path(visit, "reset"), null, visit, null, stop);

    public async Task<Claim> Assign(Visit visit, CancellationToken stop)
    {
        var claim = Parse<Claim>(await Authorized(Path(visit, "assignment"), null, visit, null, stop));
        if (!Guid.TryParseExact(claim.assignmentId, "D", out _) ||
            claim.claimUrl == null || !claim.claimUrl.StartsWith("/claim#", StringComparison.Ordinal) ||
            !Token.IsMatch(claim.claimUrl.Substring(7)) ||
            !DateTimeOffset.TryParse(claim.issuedAt, out var issued) || !DateTimeOffset.TryParse(claim.expiresAt, out var expiry) ||
            expiry <= issued || expiry - issued > TimeSpan.FromHours(1))
            throw new Failure(0, "invalid_response");
        return claim;
    }

    static string Path(Visit visit, string action) => "/api/visits/" + visit.visitId + "/" + action;

    async Task<string> Authorized(string path, object data, Visit visit, string key, CancellationToken stop)
    {
        for (int auth = 0; ; auth++)
        {
            await EnsureSession(stop);
            try { return await Send(path, data, session.session, visit?.visitToken, key, true, stop); }
            catch (Failure problem) when (problem.Status == 401 && problem.Code == "kiosk_auth_required" && auth == 0)
            {
                session = null;
            }
        }
    }

    async Task EnsureSession(CancellationToken stop)
    {
        if (session != null && DateTimeOffset.UtcNow < sessionRenewAt) return;
        signer ??= new WindowsSouvenirSigner();
        for (int attempt = 0; ; attempt++)
        {
            var challenge = Parse<Challenge>(await Send("/api/kiosks/challenge", new Kiosk { kioskId = signer.KioskId },
                null, null, null, false, stop));
            long remaining = challenge.expiresAt - ServerNow.ToUnixTimeSeconds();
            if (remaining < 1 || remaining > 65) throw new Failure(0, "invalid_response");
            string signature = await Task.Run(() => signer.Sign(challenge.challengeId, challenge.expiresAt, challenge.message), stop);
            try
            {
                var result = Parse<Session>(await Send("/api/kiosks/session",
                    new Proof { challengeId = challenge.challengeId, signature = signature }, null, null, null, false, stop));
                long duration = result.expiresAt - ServerNow.ToUnixTimeSeconds();
                if (!Token.IsMatch(result.session ?? "") || duration < 1 || duration > 305)
                    throw new Failure(0, "invalid_response");
                session = result;
                sessionRenewAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(0, duration - 12));
                return;
            }
            catch (Failure problem) when (attempt == 0 && (problem.Status == 0 || problem.Status >= 500))
            {
                // A lost response may have consumed the challenge. Start again with a fresh one.
                await Task.Delay(500, stop);
            }
        }
    }

    async Task<string> Send(string path, object data, string bearer, string visitToken, string key, bool repeat, CancellationToken stop)
    {
        for (int attempt = 0; ; attempt++)
        {
            try
            {
                if (disposed) throw new Failure(0, "transport");
                stop.ThrowIfCancellationRequested();
                using var request = new UnityWebRequest(WindowsSouvenirSigner.Origin + path, "POST")
                {
                    uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(data == null ? "{}" : JsonUtility.ToJson(data))),
                    downloadHandler = new DownloadHandlerBuffer(),
                    timeout = 8,
                    redirectLimit = 0,
                };
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("User-Agent", "Awakening-LivingGlass-Souvenir/1.0");
                if (bearer != null) request.SetRequestHeader("Authorization", "Bearer " + bearer);
                if (visitToken != null) request.SetRequestHeader("X-Visit-Token", visitToken);
                if (key != null) request.SetRequestHeader("Idempotency-Key", key);
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    if (stop.IsCancellationRequested || request.downloadedBytes > 16384)
                    {
                        request.Abort();
                        stop.ThrowIfCancellationRequested();
                        throw new Failure(0, "invalid_response");
                    }
                    await Task.Yield();
                }
                if (DateTimeOffset.TryParse(request.GetResponseHeader("Date"), out var date))
                    serverTimeOffset = date - DateTimeOffset.UtcNow;
                if (request.downloadedBytes > 16384) throw new Failure(0, "invalid_response");
                string body = request.downloadHandler.text;
                if (request.result == UnityWebRequest.Result.Success) return body;
                string code = null;
                try { code = JsonUtility.FromJson<ErrorBody>(body)?.error?.code; }
                catch (ArgumentException) { }
                throw new Failure(Math.Max(0, (int)request.responseCode), code);
            }
            catch (Failure problem) when (repeat && attempt < 1 && (problem.Status == 0 || problem.Status >= 500) &&
                problem.Code != "issuance_paused")
            {
            }
            await Task.Delay(500 * (1 << attempt), stop);
        }
    }

    static T Parse<T>(string text) where T : class =>
        JsonUtility.FromJson<T>(text) ?? throw new Failure(0, "invalid_response");

    public void Dispose() { disposed = true; session = null; }
}
