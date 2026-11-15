using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using ZXing;
using ZXing.QrCode;

internal sealed class LivingGlassSouvenir : IDisposable
{
    internal enum Display { Idle, Loading, Ready, Paused, Unavailable, Denied, Conflict, Throttled, Expired }

    [Serializable] sealed class Journal
    {
        public string localKey, performer, pendingKey, pendingPerformer;
        public bool remoteAttempted;
    }

    readonly CancellationTokenSource lifetime = new();
    readonly string journalPath;
    readonly Func<ISouvenirClient> createClient;
    Journal journal;
    ISouvenirClient client;
    Texture2D qr;
    DateTimeOffset expiry;
    bool busy;
    bool disposed;

    internal Display Status { get; private set; }
    internal bool Busy => busy;
    internal bool Eligible => !string.IsNullOrEmpty(journal?.performer);
    internal Texture2D Qr { get { RefreshExpiry(); return qr; } }
    internal int SecondsLeft => (int)Math.Max(0, Math.Ceiling((expiry - (client?.ServerNow ?? DateTimeOffset.UtcNow)).TotalSeconds));

    public LivingGlassSouvenir() : this(() => new SouvenirClient(),
        Path.Combine(WindowsSouvenirSigner.ProfileDirectory, "visitor-journal.json")) { }

    internal LivingGlassSouvenir(Func<ISouvenirClient> createClient, string journalPath)
    {
        this.createClient = createClient;
        this.journalPath = journalPath;
        try
        {
            journal = File.Exists(journalPath) ? JsonUtility.FromJson<Journal>(File.ReadAllText(journalPath)) : new Journal();
            if (journal == null || !ValidId(journal.localKey) || !ValidId(journal.pendingKey) ||
                !ValidPerformer(journal.performer) || !ValidPerformer(journal.pendingPerformer) ||
                (!string.IsNullOrEmpty(journal.pendingKey) && string.IsNullOrEmpty(journal.pendingPerformer)) ||
                (journal.remoteAttempted && (string.IsNullOrEmpty(journal.localKey) || string.IsNullOrEmpty(journal.performer))))
                throw new InvalidDataException();
        }
        catch
        {
            journal = null;
            Status = Display.Unavailable;
        }
    }

    static bool ValidId(string id) => string.IsNullOrEmpty(id) || Guid.TryParseExact(id, "N", out _);
    static bool ValidPerformer(string name) => string.IsNullOrEmpty(name) || Array.IndexOf(LivingGlassRoutes.Performers, name) >= 0;

    internal void Begin()
    {
        if (journal == null || !string.IsNullOrEmpty(journal.localKey)) return;
        journal.localKey = Guid.NewGuid().ToString("N");
        Save();
    }

    internal void Qualified(string performer)
    {
        if (journal == null || string.IsNullOrEmpty(journal.localKey) || Eligible) return;
        journal.performer = performer;
        if (Save()) Request();
    }

    internal async void Request()
    {
        RefreshExpiry();
        if (busy || !Eligible || Status == Display.Ready || Status == Display.Expired) return;
        busy = true;
        Status = Display.Loading;
        try
        {
            var stop = lifetime.Token;
            client ??= createClient();
            await FlushReset(stop);
            if (!journal.remoteAttempted)
            {
                journal.remoteAttempted = true;
                if (!Save()) return;
            }
            var visit = await client.StartVisit(journal.localKey, journal.performer, stop);
            for (int i = 0; i < SouvenirClient.Milestones.Length; i++)
                await client.ProgressVisit(visit, i, stop);
            await client.Complete(visit, stop);
            var claim = await client.Assign(visit, stop);
            stop.ThrowIfCancellationRequested();
            expiry = DateTimeOffset.Parse(claim.expiresAt, System.Globalization.CultureInfo.InvariantCulture);
            if (SecondsLeft == 0) { Status = Display.Expired; return; }
            qr = BuildQr(WindowsSouvenirSigner.Origin + claim.claimUrl);
            Status = Display.Ready;
        }
        catch (SouvenirClient.Failure error) when (!disposed)
        {
#if UNITY_EDITOR
            Debug.LogWarning("Souvenir request status: " + error.Status);
#endif
            Status = error.Code == "issuance_paused" ? Display.Paused : error.Status switch
            {
                401 or 403 => Display.Denied, 409 => Display.Conflict, 410 => Display.Expired,
                429 => Display.Throttled, _ => Display.Unavailable
            };
        }
        catch (Exception error)
        {
            if (!disposed)
            {
#if UNITY_EDITOR
                Debug.LogWarning("Souvenir client failure: " + error.GetType().Name + " / " + error.HResult +
                    " / " + error.TargetSite?.DeclaringType?.Name + "." + error.TargetSite?.Name);
#endif
                Status = Display.Unavailable;
            }
        }
        finally { busy = false; }
    }

    // Clear the visitor UI even offline. The journal blocks opening any replacement
    // server visit until reset of the previous server visit has been acknowledged.
    internal async void NewVisitor()
    {
        if (busy) return;
        ClearQr();
        Status = Display.Idle;
        if (journal == null) { Status = Display.Unavailable; return; }
        if (journal.remoteAttempted)
        {
            journal.pendingKey = journal.localKey;
            journal.pendingPerformer = journal.performer;
        }
        journal.localKey = journal.performer = null;
        journal.remoteAttempted = false;
        if (!Save() || string.IsNullOrEmpty(journal.pendingKey)) return;
        busy = true;
        try
        {
            client ??= createClient();
            await FlushReset(lifetime.Token);
        }
        catch (Exception) { if (!disposed) Status = Display.Unavailable; }
        finally { busy = false; }
    }

    async Task FlushReset(CancellationToken stop)
    {
        if (string.IsNullOrEmpty(journal.pendingKey)) return;
        var visit = await client.StartVisit(journal.pendingKey, journal.pendingPerformer, stop);
        await client.Reset(visit, stop);
        stop.ThrowIfCancellationRequested();
        journal.pendingKey = journal.pendingPerformer = null;
        if (!Save()) throw new IOException();
    }

    bool Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(journalPath));
            string temporary = journalPath + ".tmp";
            File.WriteAllText(temporary, JsonUtility.ToJson(journal));
            if (File.Exists(journalPath)) File.Replace(temporary, journalPath, null);
            else File.Move(temporary, journalPath);
            return true;
        }
        catch
        {
            journal = null;
            Status = Display.Unavailable;
            return false;
        }
    }

    internal void RefreshExpiry()
    {
        if (Status != Display.Ready || SecondsLeft > 0) return;
        ClearQr();
        Status = Display.Expired;
    }

    void ClearQr()
    {
        if (qr != null) UnityEngine.Object.Destroy(qr);
        qr = null;
        expiry = default;
    }

    static Texture2D BuildQr(string value)
    {
        var grid = new QRCodeWriter().encode(value, BarcodeFormat.QR_CODE, 0, 0);
        int size = grid.Width;
        var image = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave, name = "Digital souvenir"
        };
        var colors = new Color32[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                colors[(size - y - 1) * size + x] = grid[x, y] ? new Color32(0, 0, 0, 255) : new Color32(255, 255, 255, 255);
        image.SetPixels32(colors);
        image.Apply(false, true);
        return image;
    }

    public void Dispose()
    {
        disposed = true;
        lifetime.Cancel();
        client?.Dispose();
        ClearQr();
        lifetime.Dispose();
    }
}
