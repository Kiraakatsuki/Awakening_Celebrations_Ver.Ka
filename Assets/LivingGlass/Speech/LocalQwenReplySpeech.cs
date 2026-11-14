using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

internal sealed class LocalQwenReplySpeech : IReplySpeech
{
    internal const string VoiceId = "awakening-guide";
    private const string RuntimeId = "b91bca43f9adc5df839161ce4c88b0f6743b27ff";
    private const int StartupMilliseconds = 20000, PartMilliseconds = 35000, TotalMilliseconds = 90000;
    private readonly string root, executable, worker;
    private readonly bool installed;

    public static LocalQwenReplySpeech Discover(string appDataPath)
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        if (IntPtr.Size != 8) return null;
        // Operator opt-in: a single relative location beside Unity's data directory.
        var basePath = Path.GetDirectoryName(appDataPath);
        string configuration = Path.Combine(basePath, "QwenLocal", "config.json");
        try
        {
            var info = new FileInfo(configuration);
            if (!info.Exists || info.Length > 2048) return null;
            var settings = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(configuration));
            if (settings == null || !settings.enabled || settings.voiceId != VoiceId || settings.conditioning != "icl") return null;
            string relative = settings.runtimeDirectory;
            if (string.IsNullOrWhiteSpace(relative) || Path.IsPathRooted(relative) || relative.IndexOf(':') >= 0) return null;
            string boundary = Path.GetFullPath(Path.GetDirectoryName(configuration)) + Path.DirectorySeparatorChar;
            string directory = Path.GetFullPath(Path.Combine(boundary, relative));
            if (!directory.StartsWith(boundary, StringComparison.OrdinalIgnoreCase)) return null;
            return new LocalQwenReplySpeech(directory);
        }
        catch (Exception) { return null; }
#else
        return null;
#endif
    }

    private sealed class Configuration
    {
        public bool enabled = false;
        public string voiceId = null, conditioning = null, runtimeDirectory = null;
    }

    internal LocalQwenReplySpeech(string root)
    {
        this.root = root;
        executable = Path.Combine(root, "Runtime", "PortablePython", "python.exe");
        worker = Path.Combine(root, "Scripts", "worker.py");
        installed = File.Exists(executable) && File.Exists(worker) && File.Exists(At("Scripts/engine.py"))
            && File.Exists(At("Models/qwen-talker-0.6b-base-Q8_0.gguf")) && File.Exists(At("Models/qwen-tokenizer-12hz-Q8_0.gguf"))
            && File.Exists(At("Conditioning/awakening-guide.spk")) && File.Exists(At("Conditioning/awakening-guide.rvq"))
            && File.Exists(At("Reference/transcript.en.txt"))
            && File.Exists(At("Runtime/Cuda/cudart64_12.dll")) && File.Exists(At("Runtime/Cuda/cublas64_12.dll"))
            && File.Exists(At("Runtime/Cuda/cublasLt64_12.dll"));
    }

    public bool IsAvailable => installed;

    public Task<ReplySpeechResult> GenerateAsync(string text, string locale, CancellationToken cancellation) =>
        Task.Run(async () =>
        {
            if (!IsAvailable) return new ReplySpeechResult(ReplySpeechError.Unavailable);
            var segments = ReplySpeechText.Split(text);
            if (segments == null) return new ReplySpeechResult(ReplySpeechError.TooLong);
            if (cancellation.IsCancellationRequested) return new ReplySpeechResult(ReplySpeechError.Cancelled);
            if (!LocalVoiceInference.Slot.Wait(0)) return new ReplySpeechResult(ReplySpeechError.Busy);
            try { return await Run(segments, locale, cancellation).ConfigureAwait(false); }
            finally { LocalVoiceInference.Slot.Release(); }
        });

    private string At(string relative) => Path.Combine(root, relative);

    private bool VoiceMatches() =>
        HashMatches("Conditioning/awakening-guide.spk", "474cddf90736262b3027e1eda846cfe93157c4cd8270ecac7b76def6e0e9a61d") &&
        HashMatches("Conditioning/awakening-guide.rvq", "614afb3b0418055642cf104c6269f241536a9e896911750077a87c1b12b56494") &&
        HashMatches("Reference/transcript.en.txt", "93b55ecbf646ea0e8a7bb69cc5c41e82710a0e3f72ea34ae5066f00b55c5a01c");

    private bool HashMatches(string path, string expected)
    {
        using var file = File.OpenRead(At(path));
        if (file.Length > 16384) return false;
        using var sha = SHA256.Create();
        return string.Equals(BitConverter.ToString(sha.ComputeHash(file)).Replace("-", ""), expected, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<ReplySpeechResult> Run(List<string> segments, string locale, CancellationToken cancellation)
    {
        bool ready = false;
        try
        {
            cancellation.ThrowIfCancellationRequested();
            if (!VoiceMatches()) return new ReplySpeechResult(ReplySpeechError.Unavailable);
            using var job = new KillOnCloseJob();
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executable, WorkingDirectory = root, Arguments = "-B -u Scripts/worker.py",
                    UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true,
                    RedirectStandardOutput = true, RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8
                }
            };
            if (!process.Start()) return new ReplySpeechResult(ReplySpeechError.Startup);
            try
            {
                try { job.Assign(process); }
                catch { if (!process.HasExited) process.Kill(); throw; }
                process.ErrorDataReceived += (_, __) => { }; // Drain; never store visitor request data.
                process.BeginErrorReadLine();
                var reader = new PipeReader(process.StandardOutput);
                var clock = Stopwatch.StartNew();
                Response hello = await Read(reader, StartupMilliseconds, cancellation).ConfigureAwait(false);
                if (hello.Event != "ready" || hello.VoiceId != VoiceId)
                    return new ReplySpeechResult(Classify(hello.Code, ReplySpeechError.Startup));
                await Send(process, "{\"op\":\"health\",\"requestId\":\"health\"}", 2000, cancellation).ConfigureAwait(false);
                Response health = await Read(reader, 2000, cancellation).ConfigureAwait(false);
                if (health.RequestId != "health" || health.Status != "ok" || !health.Ready || health.Busy ||
                    health.VoiceId != VoiceId || health.Runtime != RuntimeId)
                    return new ReplySpeechResult(ReplySpeechError.Startup);
                ready = true;
                var parts = new List<float[]>();
                int frames = 0;
                var queue = new LinkedList<string>(segments);
                while (queue.Count > 0)
                {
                    string segment = queue.First.Value;
                    queue.RemoveFirst();
                    int remaining = (int)(TotalMilliseconds - clock.ElapsedMilliseconds);
                    if (remaining < 100) return new ReplySpeechResult(ReplySpeechError.Timeout);
                    int deadline = Math.Min(remaining, PartMilliseconds);
                    string id = Guid.NewGuid().ToString("N");
                    string request = JsonConvert.SerializeObject(new
                    {
                        op = "synthesize", requestId = id, voiceId = VoiceId, conditioning = "icl",
                        language = ReplySpeechText.Language(segment, locale), text = segment, timeoutMs = deadline
                    }, new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii });
                    await Send(process, request, 2000, cancellation).ConfigureAwait(false);
                    Response response = await Read(reader, deadline + 1000, cancellation).ConfigureAwait(false);
                    if (response.RequestId != id) return new ReplySpeechResult(ReplySpeechError.Worker);
                    if (response.Status != "ok") return new ReplySpeechResult(Classify(response.Code, ReplySpeechError.Worker));
                    var pcm = response.Mime == "audio/wav" ? ReplySpeechWave.Decode(response.AudioBase64) : null;
                    if (pcm == null) return new ReplySpeechResult(ReplySpeechError.InvalidAudio);
                    if (ReplySpeechWave.ReachedTokenLimit(pcm.Length))
                    {
                        // Retry an incomplete phrase in shorter pieces without
                        // altering the displayed reply or appending clipped audio.
                        if (segment.Length < 32) return new ReplySpeechResult(ReplySpeechError.Incomplete);
                        var shorter = ReplySpeechText.Split(segment, segment.Length / 2);
                        if (shorter == null || shorter.Count < 2) return new ReplySpeechResult(ReplySpeechError.Incomplete);
                        for (int i = shorter.Count - 1; i >= 0; i--) queue.AddFirst(shorter[i]);
                        continue;
                    }
                    pcm = ReplySpeechWave.FinishPhrase(pcm, queue.Count == 0);
                    if (frames + pcm.Length > ReplySpeechWave.MaximumFrames)
                        return new ReplySpeechResult(ReplySpeechError.TooLong);
                    frames += pcm.Length;
                    parts.Add(pcm);
                }
                cancellation.ThrowIfCancellationRequested();
                var samples = new float[frames];
                int offset = 0;
                foreach (float[] part in parts)
                {
                    Array.Copy(part, 0, samples, offset, part.Length);
                    offset += part.Length;
                }
                return new ReplySpeechResult(ReplySpeechError.None, samples);
            }
            finally
            {
                // Shutdown also cancels the worker's active native request. Job close
                // is mandatory on a hung pipe, startup failure, crash or slow exit.
                try
                {
                    if (!process.HasExited)
                    {
                        await Send(process, "{\"op\":\"shutdown\",\"requestId\":\"shutdown\"}", 300, CancellationToken.None).ConfigureAwait(false);
                        if (!process.WaitForExit(600)) job.Dispose();
                    }
                }
                catch (Exception) { job.Dispose(); }
                if (!process.HasExited)
                {
                    job.Dispose();
                    process.WaitForExit(1500);
                }
            }
        }
        catch (OperationCanceledException) { return new ReplySpeechResult(ReplySpeechError.Cancelled); }
        catch (TimeoutException) { return new ReplySpeechResult(ReplySpeechError.Timeout); }
        catch (Exception) { return new ReplySpeechResult(ready ? ReplySpeechError.Worker : ReplySpeechError.Startup); }
    }

    private static ReplySpeechError Classify(string code, ReplySpeechError fallback) => code switch
    {
        "missing_assets" => ReplySpeechError.Unavailable, "busy" => ReplySpeechError.Busy,
        "timeout" => ReplySpeechError.Timeout, "cancelled" => ReplySpeechError.Cancelled,
        "startup_failure" => ReplySpeechError.Startup, "no_audio" => ReplySpeechError.InvalidAudio,
        _ => fallback
    };

    private static async Task Send(Process process, string message, int timeout, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        byte[] bytes = Encoding.ASCII.GetBytes(message + "\n");
        await Await(Write(process.StandardInput.BaseStream, bytes), timeout, cancellation).ConfigureAwait(false);
    }

    private static async Task<bool> Write(Stream stream, byte[] bytes)
    {
        await stream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
        await stream.FlushAsync().ConfigureAwait(false);
        return true;
    }

    private static async Task<Response> Read(PipeReader reader, int timeout, CancellationToken cancellation)
    {
        string message = await Await(reader.Line(), timeout, cancellation).ConfigureAwait(false);
        return JsonConvert.DeserializeObject<Response>(message) ?? throw new InvalidDataException();
    }

    private static async Task<T> Await<T>(Task<T> pending, int milliseconds, CancellationToken cancellation)
    {
        using var stop = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        Task cutoff = Task.Delay(milliseconds, stop.Token);
        if (await Task.WhenAny(pending, cutoff).ConfigureAwait(false) == pending)
        {
            stop.Cancel();
            cancellation.ThrowIfCancellationRequested();
            return await pending.ConfigureAwait(false);
        }
        _ = pending.ContinueWith(t => { _ = t.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
        cancellation.ThrowIfCancellationRequested();
        throw new TimeoutException();
    }

    private sealed class Response
    {
        public string Event = null, RequestId = null, Status = null, VoiceId = null, Code = null, Runtime = null, Mime = null, AudioBase64 = null;
        public bool Ready = false, Busy = false;
    }

    private sealed class PipeReader
    {
        private readonly StreamReader stream;
        private readonly char[] buffer = new char[4096];
        private int position, count;
        public PipeReader(StreamReader stream) { this.stream = stream; }
        public async Task<string> Line()
        {
            var result = new StringBuilder();
            while (true)
            {
                if (position == count)
                {
                    count = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                    if (count == 0) throw new EndOfStreamException();
                    position = 0;
                }
                int start = position;
                while (position < count && buffer[position] != '\n') position++;
                result.Append(buffer, start, position - start);
                if (result.Length > ReplySpeechWave.MaximumLine) throw new InvalidDataException();
                if (position < count) { position++; return result.ToString(); }
            }
        }
    }
}
