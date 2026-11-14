using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

internal enum SpeechOutcome { Ready, NoSpeech, Unavailable, Error, Timeout, Cancelled }

internal readonly struct SpeechResult
{
    public readonly SpeechOutcome Outcome;
    public readonly string Text;

    public SpeechResult(SpeechOutcome outcome, string text = null)
    {
        Outcome = outcome;
        Text = text;
    }
}

internal sealed class LocalSpeechWorker
{
    private const string Model = "ggml-large-v3-turbo-q5_0.bin";
    private const string Vad = "ggml-silero-v6.2.0.bin";
    internal const int MaximumSeconds = 20;
    private static readonly SemaphoreSlim WorkerSlot = new SemaphoreSlim(1);
    private readonly string executable, model, vad, directory;
    private readonly TimeSpan requestLimit;

    public LocalSpeechWorker(string root) : this(root, TimeSpan.FromSeconds(12)) { }

    public LocalSpeechWorker(string root, TimeSpan requestLimit)
    {
        directory = Path.Combine(root, "bin");
        executable = Path.Combine(directory, "whisper-cli.exe");
        model = Path.Combine(root, "models", Model);
        vad = Path.Combine(root, "models", Vad);
        ScriptDirectory = Path.Combine(root, "script");
        MicrophoneSelectionPath = Path.Combine(root, "microphone.txt");
        this.requestLimit = requestLimit;
    }

    public string ScriptDirectory { get; }
    public string MicrophoneSelectionPath { get; }
    public bool IsInstalled => File.Exists(executable) && File.Exists(model) && File.Exists(vad)
        && File.Exists(Path.Combine(directory, "ggml-cuda.dll"))
        && File.Exists(Path.Combine(directory, "cublas64_11.dll"))
        && File.Exists(Path.Combine(directory, "cublasLt64_11.dll"))
        && File.Exists(Path.Combine(ScriptDirectory, "STCharacters.txt"))
        && File.Exists(Path.Combine(ScriptDirectory, "STPhrases.txt"));

    public Task<SpeechResult> Transcribe(float[] samples, int channels, int rate, bool chinese,
        CancellationToken cancellation)
    {
        return Task.Run(async () =>
        {
            try
            {
                await WorkerSlot.WaitAsync(cancellation).ConfigureAwait(false);
                try
                {
                    byte[] wav = EncodeWave(samples, channels, rate, cancellation);
                    try { return Run(wav, chinese, cancellation); }
                    finally { Array.Clear(wav, 0, wav.Length); }
                }
                finally { WorkerSlot.Release(); }
            }
            catch (OperationCanceledException) { return new SpeechResult(SpeechOutcome.Cancelled); }
            catch (Exception) { return new SpeechResult(SpeechOutcome.Error); }
            finally { Array.Clear(samples, 0, samples.Length); }
        });
    }

    internal static byte[] EncodeWave(float[] samples, int channels, int rate, CancellationToken cancellation)
    {
        if (channels < 1 || channels > 16 || rate < 8000 || rate > 192000
            || samples.Length % channels != 0 || samples.Length > MaximumSeconds * rate * channels)
            throw new ArgumentException("Invalid speech input format");

        const int targetRate = 16000;
        int frames = samples.Length / channels;
        int count = (int)((long)frames * targetRate / rate);
        using (var output = new MemoryStream(44 + count * 2))
        using (var writer = new BinaryWriter(output, Encoding.ASCII))
        {
            writer.Write(0x46464952); // RIFF
            writer.Write(36 + count * 2);
            writer.Write(0x45564157); // WAVE
            writer.Write(0x20746D66); // fmt
            writer.Write(16);
            writer.Write((ushort)1);
            writer.Write((ushort)1);
            writer.Write(targetRate);
            writer.Write(targetRate * 2);
            writer.Write((ushort)2);
            writer.Write((ushort)16);
            writer.Write(0x61746164); // data
            writer.Write(count * 2);
            double ratio = (double)rate / targetRate;
            for (int i = 0; i < count; i++)
            {
                if ((i & 4095) == 0) cancellation.ThrowIfCancellationRequested();
                double start = i * ratio;
                double end = Math.Min(frames, (i + 1) * ratio);
                int first = (int)start;
                double value = 0;
                if (ratio < 1)
                {
                    float a = MonoFrame(samples, channels, first);
                    float b = MonoFrame(samples, channels, Math.Min(frames - 1, first + 1));
                    value = a + (b - a) * (start - first);
                }
                else
                {
                    for (int frame = first; frame < end; frame++)
                        value += MonoFrame(samples, channels, frame)
                            * (Math.Min(end, frame + 1) - Math.Max(start, frame));
                    value /= end - start;
                }
                writer.Write((short)Math.Round(Math.Max(-1, Math.Min(1, value)) * 32767));
            }
            writer.Flush();
            var buffer = output.ToArray();
            Array.Clear(output.GetBuffer(), 0, (int)output.Length);
            return buffer;
        }
    }

    private static float MonoFrame(float[] data, int channels, int index)
    {
        float sum = 0;
        for (int channel = 0; channel < channels; channel++) sum += data[index * channels + channel];
        float average = sum / channels;
        return float.IsNaN(average) || float.IsInfinity(average) ? 0 : average;
    }

    private SpeechResult Run(byte[] wav, bool chinese, CancellationToken cancellation)
    {
        if (!IsInstalled) return new SpeechResult(SpeechOutcome.Unavailable);
        if (wav.Length < 8044) return new SpeechResult(SpeechOutcome.NoSpeech);
        // This Win64 CLI routes -of - through CON. Use a private result file
        // while keeping all PCM in the anonymous input pipe.
        string scratchRoot = Path.Combine(Path.GetTempPath(), "AwakeningVoice");
        Directory.CreateDirectory(scratchRoot);
        if ((File.GetAttributes(scratchRoot) & FileAttributes.ReparsePoint) != 0)
            return new SpeechResult(SpeechOutcome.Error);
        RemoveAbandonedResults(scratchRoot);
        string scratch = Path.Combine(scratchRoot, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(scratch);
        try
        {
            using (File.Open(Path.Combine(scratch, "active.lock"), FileMode.CreateNew, FileAccess.Write, FileShare.None))
                return RunProcess(wav, chinese, cancellation, scratch);
        }
        finally { RemoveResultDirectory(scratch); }
    }

    private sealed class Output
    {
        public Segment[] transcription;
    }

    private sealed class Segment
    {
        public string text;
    }

    private SpeechResult RunProcess(byte[] wav, bool chinese, CancellationToken cancellation, string scratch)
    {
        string prefix = Path.Combine(scratch, "result");
        var start = new ProcessStartInfo
        {
            FileName = executable, WorkingDirectory = directory, UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8,
            Arguments = "-m \"" + model + "\" -vm \"" + vad + "\" -f - -l " + (chinese ? "zh" : "en") +
                " -t 4 -bs 1 -bo 1 -mc 0 -nf -sns -nt -fa --vad -vt 0.5 -vspd 250 -oj -of \"" + prefix + "\""
        };
        using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellation))
        using (var job = new KillOnCloseJob())
        using (var process = new Process { StartInfo = start })
        {
            timeout.CancelAfter(requestLimit);
            timeout.Token.ThrowIfCancellationRequested();
            if (!process.Start()) return new SpeechResult(SpeechOutcome.Error);
            try
            {
                job.Assign(process);
                using (timeout.Token.Register(job.Dispose))
                {
                    var output = ReadBounded(process.StandardOutput, 8192);
                    var diagnostics = ReadBounded(process.StandardError, 65536);
                    try
                    {
                        process.StandardInput.BaseStream.Write(wav, 0, wav.Length);
                        process.StandardInput.Close();
                        process.WaitForExit();
                    }
                    catch (IOException) when (timeout.IsCancellationRequested) { }
                    if (cancellation.IsCancellationRequested) return new SpeechResult(SpeechOutcome.Cancelled);
                    if (timeout.IsCancellationRequested) return new SpeechResult(SpeechOutcome.Timeout);
                    output.GetAwaiter().GetResult();
                    string status = diagnostics.GetAwaiter().GetResult();
                    if (process.ExitCode != 0 || status == null ||
                        !status.Contains("found GPU device 0: CUDA0"))
                        return new SpeechResult(SpeechOutcome.Error);
                    var file = new FileInfo(prefix + ".json");
                    if (!file.Exists || file.Length > 65536) return new SpeechResult(SpeechOutcome.Error);
                    var decoded = JsonConvert.DeserializeObject<Output>(File.ReadAllText(file.FullName, Encoding.UTF8));
                    if (decoded?.transcription == null) return new SpeechResult(SpeechOutcome.Error);
                    string text = string.Concat(Array.ConvertAll(decoded.transcription, segment => segment.text)).Trim();
                    if (text.Length == 0) return new SpeechResult(SpeechOutcome.NoSpeech);
                    if (chinese) text = TraditionalSpeechText.Convert(text, ScriptDirectory);
                    return text.Length <= 3600 ? new SpeechResult(SpeechOutcome.Ready, text)
                        : new SpeechResult(SpeechOutcome.Error);
                }
            }
            finally
            {
                job.Dispose();
                if (!process.HasExited) process.Kill();
                process.WaitForExit(2000);
            }
        }
    }

    private static async Task<string> ReadBounded(StreamReader reader, int maximum)
    {
        var output = new StringBuilder();
        var buffer = new char[1024];
        bool overflow = false;
        int count;
        while ((count = await reader.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
        {
            if (output.Length + count > maximum) overflow = true;
            if (!overflow) output.Append(buffer, 0, count);
        }
        return overflow ? null : output.ToString();
    }

    private static void RemoveAbandonedResults(string root)
    {
        if (!Directory.Exists(root) || (File.GetAttributes(root) & FileAttributes.ReparsePoint) != 0) return;
        foreach (string scratch in Directory.EnumerateDirectories(root))
        {
            if (!Guid.TryParseExact(Path.GetFileName(scratch), "N", out _)) continue;
            try
            {
                if ((File.GetAttributes(scratch) & FileAttributes.ReparsePoint) != 0) continue;
                // Leave a new directory alone while another instance may be acquiring its lock.
                if (Directory.GetCreationTimeUtc(scratch) > DateTime.UtcNow.AddMinutes(-1)) continue;
                string lease = Path.Combine(scratch, "active.lock");
                if (!File.Exists(lease) || (File.GetAttributes(lease) & FileAttributes.ReparsePoint) != 0) continue;
                using (File.Open(lease, FileMode.Open, FileAccess.Read, FileShare.None)) { }
                RemoveResultDirectory(scratch);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }

    private static void RemoveResultDirectory(string directory)
    {
        if ((File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0) return;
        File.Delete(Path.Combine(directory, "result.json"));
        File.Delete(Path.Combine(directory, "active.lock"));
        Directory.Delete(directory);
    }
}

// A Windows job owns the worker even if the Unity process exits unexpectedly.
internal sealed class KillOnCloseJob : IDisposable
{
    private IntPtr handle;

    public KillOnCloseJob()
    {
        if (IntPtr.Size != 8) throw new PlatformNotSupportedException("Local speech requires Windows x64");
        handle = CreateJobObject(IntPtr.Zero, null);
        if (handle == IntPtr.Zero) throw new InvalidOperationException("Cannot create voice worker job");
        // JOBOBJECT_EXTENDED_LIMIT_INFORMATION on Win64. LimitFlags is at offset 16.
        var limits = new byte[144];
        limits[17] = 0x20; // JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE (0x2000)
        if (SetInformationJobObject(handle, 9, limits, limits.Length)) return;
        Dispose();
        throw new InvalidOperationException("Cannot configure voice worker job");
    }

    public void Assign(Process process)
    {
        if (!AssignProcessToJobObject(handle, process.Handle))
            throw new InvalidOperationException("Cannot assign voice worker to job");
    }

    public void Dispose()
    {
        IntPtr job = Interlocked.Exchange(ref handle, IntPtr.Zero);
        if (job != IntPtr.Zero) CloseHandle(job);
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateJobObject(IntPtr attributes, string name);
    [DllImport("kernel32.dll")]
    private static extern bool SetInformationJobObject(IntPtr job, int kind, byte[] information, int length);
    [DllImport("kernel32.dll")]
    private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);
    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr handle);
}
