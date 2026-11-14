using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

internal interface IReplyAudio : IDisposable
{
    bool IsPlaying { get; }
    bool TryPlay(float[] samples);
    void Stop();
}

internal sealed class UnityReplyAudio : IReplyAudio
{
    private readonly GameObject host;
    private GameObject playback;
    private AudioSource source;
    private AudioClip ownedClip;
    private double endDsp = -1, fadeStarted = -1;
    private const float SpeechVolume = .78f;
    private const double StopFadeSeconds = .035;

    internal UnityReplyAudio(GameObject host) { this.host = host; }
    public bool IsPlaying => source != null && source.clip != null
        && (source.isPlaying || AudioSettings.dspTime < endDsp);

    public bool TryPlay(float[] samples)
    {
        if (samples == null || samples.Length == 0) return false;
        Stop();
        try
        {
            if (!PrepareSource()) return false;
            ownedClip = AudioClip.Create("Living Glass reply", samples.Length, 1, ReplySpeechWave.Rate, false);
            if (ownedClip == null || !ownedClip.SetData(samples, 0)) { Stop(); return false; }
            return StartClip(ownedClip);
        }
        catch (Exception) { Stop(); return false; }
    }

    internal bool TryPlayGuide(AudioClip cached)
    {
        Stop();
        try { return cached != null && PrepareSource() && StartClip(cached); }
        catch (Exception) { Stop(); return false; }
    }

    private bool PrepareSource()
    {
        if (host == null || !host.activeInHierarchy) return false;
        if (source != null) return true;
        playback = new GameObject("Living Glass voice playback");
        playback.transform.SetParent(host.transform, false);
        source = playback.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0;
        source.loop = false;
        return true;
    }

    private bool StartClip(AudioClip clip)
    {
        if (clip.frequency <= 0 || clip.samples <= 0) { Stop(); return false; }
        source.volume = SpeechVolume;
        source.clip = clip;
        AudioSettings.GetDSPBufferSize(out int block, out int count);
        int outputRate = Math.Max(1, AudioSettings.outputSampleRate);
        double queue = (double)block * count / outputRate;
        double start = AudioSettings.dspTime + Math.Max(.06, queue);
        endDsp = CompletionTime(start, clip.samples, clip.frequency, queue);
        source.PlayScheduled(start);
        return true;
    }

    internal static double CompletionTime(double start, int frames, int rate, double queue) =>
        start + (double)frames / rate + Math.Max(queue, .025);

    internal static float CancellationGain(double elapsed) =>
        (1f + Mathf.Cos(Mathf.PI * Mathf.Clamp01((float)(elapsed / StopFadeSeconds)))) * .5f;

    internal void Fade()
    {
        if (source == null || !source.isPlaying) { Stop(); return; }
        fadeStarted = Time.unscaledTimeAsDouble;
    }

    internal void Tick()
    {
        if (fadeStarted >= 0 && source != null)
        {
            float gain = CancellationGain(Time.unscaledTimeAsDouble - fadeStarted);
            if (gain <= 0f) { Stop(); return; }
            source.volume = SpeechVolume * gain;
        }
        if (!IsPlaying && endDsp >= 0) Stop();
    }

    public void Stop()
    {
        if (source != null) { source.Stop(); source.clip = null; }
        if (ownedClip != null) UnityEngine.Object.Destroy(ownedClip);
        ownedClip = null;
        endDsp = fadeStarted = -1;
    }

    public void Dispose()
    {
        Stop();
        if (playback != null) UnityEngine.Object.Destroy(playback);
        playback = null;
        source = null;
    }
}

// One output owner for the bound conversation; the transcript owns its individual reply rows.
internal sealed class LivingGlassReplySpeech : IDisposable
{
    private sealed class Reply
    {
        public Button Button;
        public Label Status;
        public string Text;
    }

    private readonly VisualElement scope;
    private readonly IReplySpeech backend;
    private readonly IReplyAudio audio;
    private readonly Func<bool> current, audioAllowed;
    private readonly Action beforeReply;
    private readonly IVisualElementScheduledItem ticker;
    private readonly string locale;
    private readonly Color accent;
    private Reply active;
    private Task<ReplySpeechResult> generation;
    private CancellationTokenSource cancellation;
    private bool playing, disposed;

    internal LivingGlassReplySpeech(VisualElement scope, IReplySpeech backend, IReplyAudio audio,
        AppLanguage language, Color accent, Func<bool> current, Func<bool> audioAllowed, Action beforeReply = null)
    {
        this.scope = scope;
        this.backend = backend;
        this.audio = audio;
        this.current = current;
        this.audioAllowed = audioAllowed;
        this.beforeReply = beforeReply;
        this.accent = accent;
        locale = language == AppLanguage.Chinese ? "zh" : "en";
        ticker = scope.schedule.Execute(Tick).Every(40);
        ticker.Pause();
        scope.RegisterCallback<DetachFromPanelEvent>(OnDetach);
    }

    internal VisualElement Add(ScrollView transcript, string text)
    {
        if (disposed || !backend.IsAvailable || string.IsNullOrWhiteSpace(text)) return null;
        var row = new VisualElement { name = "reply-speech-controls" };
        row.AddToClassList("lg-reply-speech");
        var reply = new Reply { Text = text };
        // Use the same repeat-safe keyboard activation as the composer microphone.
        reply.Button = new LivingGlassVoiceButton(() => Activate(reply)) { name = "reply-speech-action" };
        reply.Button.AddToClassList("lg-action");
        reply.Button.AddToClassList("lg-reply-speech-action");
        reply.Status = new Label { name = "reply-speech-status", pickingMode = PickingMode.Ignore, enableRichText = false };
        reply.Status.AddToClassList("lg-reply-speech-status");
        row.Add(reply.Button);
        row.Add(reply.Status);
        Paint(reply, false, "");
        transcript.Add(row);
        return row;
    }

    private void Activate(Reply reply)
    {
        if (disposed || !current()) return;
        if (active == reply) { Stop(fade: true); return; }
        if (!audioAllowed()) { Paint(reply, false, T("Audio is busy", "音訊使用中")); return; }
        Stop();
        beforeReply?.Invoke();
        active = reply;
        Paint(reply, true, T("Preparing audio…", "正在準備音訊…"));
        cancellation = new CancellationTokenSource();
        try { generation = backend.GenerateAsync(reply.Text, locale, cancellation.Token); }
        catch (Exception) { Stop(T("Audio unavailable", "暫時無法播放音訊")); return; }
        ticker.Resume();
    }

    internal void Tick()
    {
        if (active == null) return;
        if (!current() || !audioAllowed()) { Stop(); return; }
        if (playing)
        {
            if (!audio.IsPlaying) Stop();
            return;
        }
        if (generation == null || !generation.IsCompleted) return;

        var task = generation;
        generation = null;
        cancellation.Dispose();
        cancellation = null;
        ReplySpeechResult result;
        try { result = task.GetAwaiter().GetResult(); }
        catch (Exception) { result = new ReplySpeechResult(ReplySpeechError.Worker); }
        if (result.Error == ReplySpeechError.None && result.Samples != null && audio.TryPlay(result.Samples))
        {
            playing = true;
            Paint(active, true, T("Playing", "播放中"));
        }
        else Stop(result.Error switch
        {
            ReplySpeechError.Timeout => T("Audio timed out", "音訊準備逾時"),
            ReplySpeechError.Busy => T("Audio is busy", "音訊使用中"),
            ReplySpeechError.TooLong => T("Reply too long for audio", "回覆過長，無法朗讀"),
            ReplySpeechError.Incomplete => T("Audio was incomplete", "語音內容不完整"),
            ReplySpeechError.Cancelled => "",
            _ => T("Audio unavailable", "暫時無法播放音訊")
        });
    }

    internal void Stop(string message = "", bool fade = false)
    {
        var previous = active;
        active = null;
        ticker.Pause();
        if (fade) LivingGlassVoiceCoordinator.FadeReply(audio);
        else audio.Stop();
        playing = false;
        var pending = generation;
        var token = cancellation;
        generation = null;
        cancellation = null;
        if (token != null)
        {
            token.Cancel();
            if (pending == null) token.Dispose();
            else _ = pending.ContinueWith(t => { _ = t.Exception; token.Dispose(); }, TaskScheduler.Default);
        }
        if (previous != null) Paint(previous, false, message);
    }

    private string T(string en, string zh) => locale == "zh" ? zh : en;

    private void Paint(Reply reply, bool busy, string status)
    {
        reply.Button.text = busy ? T("Stop", "停止") : T("Listen", "朗讀");
        reply.Button.tooltip = busy ? T("Stop reading this reply aloud", "停止朗讀這則回覆") : T("Read this reply aloud on this computer", "在此電腦上朗讀這則回覆");
        reply.Button.style.borderTopColor = busy ? new StyleColor(accent) : new StyleColor(StyleKeyword.Null);
        reply.Status.text = status;
    }

    private void OnDetach(DetachFromPanelEvent evt) => Dispose();

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        Stop();
        scope.UnregisterCallback<DetachFromPanelEvent>(OnDetach);
        audio.Dispose();
    }
}
