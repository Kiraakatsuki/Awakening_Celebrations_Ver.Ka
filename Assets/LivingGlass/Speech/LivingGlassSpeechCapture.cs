using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

internal sealed class LivingGlassSpeechCapture : IDisposable
{
    private enum State { Idle, Starting, Recording, Processing, Review }
    private const float StopGuardSeconds = .25f;
    private static LivingGlassSpeechCapture microphoneOwner;
    private readonly LocalSpeechWorker worker;
    private readonly VisualElement rail, marker, controls;
    private readonly TextField input;
    private readonly Button ask, replace, keep;
    private readonly LivingGlassVoiceButton mic;
    private readonly LivingGlassMotion motion;
    private readonly VisualElement microphoneGlyph, stopGlyph, busyGlyph;
    private readonly Label status;
    private readonly Func<bool> isCurrent;
    private readonly Func<bool> exhibitionAudioActive;
    private readonly bool chinese;
    private readonly Color accent;
    private readonly IVisualElementScheduledItem ticker;
    private State state;
    private AudioClip clip;
    private string device;
    private CancellationTokenSource requestCancellation;
    private Task<SpeechResult> request;
    private float started, advanced, recordingSince;
    private float stopMix, busyMix, edgeMix;
    private int position;
    private string transcript;
    private bool disposed;

    public static LivingGlassSpeechCapture Bind(VisualElement root, TextField input, Button ask, int performer,
        AppLanguage language, Func<bool> isCurrent, Func<bool> exhibitionAudioActive, LivingGlassMotion motion)
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        if (!Application.isPlaying || IntPtr.Size != 8) return null;
        string baseDirectory = Path.GetDirectoryName(Application.dataPath);
        var worker = new LocalSpeechWorker(Path.Combine(baseDirectory, "VoiceLocal"));
        if (!worker.IsInstalled) return null;
        return new LivingGlassSpeechCapture(root, worker, input, ask, performer, language, isCurrent, exhibitionAudioActive, motion);
#else
        return null;
#endif
    }

    private LivingGlassSpeechCapture(VisualElement root, LocalSpeechWorker worker, TextField input, Button ask,
        int performer, AppLanguage language, Func<bool> isCurrent, Func<bool> exhibitionAudioActive, LivingGlassMotion motion)
    {
        this.worker = worker;
        this.input = input;
        this.ask = ask;
        this.isCurrent = isCurrent;
        this.exhibitionAudioActive = exhibitionAudioActive;
        this.motion = motion;
        chinese = language == AppLanguage.Chinese;
        accent = LivingGlassAssets.Accents[performer];
        rail = root.Q("voice-affordance");
        rail.AddToClassList("lg-voice-bound");
        status = rail.Q<Label>("voice-status");
        marker = rail.Q("microphone-unavailable");
        if (marker == null) throw new InvalidOperationException("Voice composer marker is missing");
        mic = new LivingGlassVoiceButton(Activate) { name = "voice-microphone" };
        mic.AddToClassList("lg-microphone");
        mic.AddToClassList("lg-action");
        var icons = new VisualElement { pickingMode = PickingMode.Ignore };
        icons.AddToClassList("lg-voice-icons");
        microphoneGlyph = new LivingGlassMicrophoneIcon { name = "voice-mic-glyph" };
        stopGlyph = new LivingGlassVoiceSymbol(false) { name = "voice-stop-glyph" };
        busyGlyph = new LivingGlassVoiceSymbol(true) { name = "voice-busy-glyph" };
        foreach (var icon in new[] { microphoneGlyph, stopGlyph, busyGlyph })
        {
            icon.AddToClassList("lg-voice-glyph");
            icons.Add(icon);
        }
        mic.Add(icons);
        int index = rail.IndexOf(marker);
        marker.RemoveFromHierarchy();
        rail.Insert(index, mic);
        controls = new VisualElement { name = "voice-draft-confirmation" };
        controls.AddToClassList("lg-voice-draft-confirmation");
        replace = new Button(UseTranscript) { name = "voice-replace-draft", text = T("Replace", "取代") };
        keep = new Button(KeepDraft) { name = "voice-keep-draft", text = T("Keep", "保留") };
        foreach (Button button in new[] { replace, keep })
        {
            button.AddToClassList("lg-action");
            button.AddToClassList("lg-voice-action");
            controls.Add(button);
        }
        rail.Insert(index + 1, controls);
        rail.RegisterCallback<DetachFromPanelEvent>(OnDetached);
        Application.focusChanged += OnFocusChanged;
        ticker = rail.schedule.Execute(Tick).Every(40);
        Show(State.Idle, T("Dictate", "語音輸入"));
    }

    private string T(string en, string zh) => chinese ? zh : en;
    private bool CanUse => !disposed && (!Application.isPlaying || Application.isFocused)
        && rail.enabledInHierarchy && isCurrent();

    private void Activate()
    {
        HandleActivation(Time.realtimeSinceStartup, StartRecording, StopRecording);
    }

    private void HandleActivation(float now, Action startCapture, Action stopCapture)
    {
        if (!CanUse || !ask.enabledInHierarchy) return;
        if (state == State.Idle) startCapture();
        else if (state == State.Recording && now - recordingSince >= StopGuardSeconds) stopCapture();
    }

    private void StartRecording()
    {
        if (!Application.isPlaying || microphoneOwner != null) return;
        if (exhibitionAudioActive())
        {
            Finish(T("Audio playing", "音效播放中"));
            return;
        }
        if (!worker.IsInstalled)
        {
            Finish(T("Voice unavailable", "語音尚未提供"));
            return;
        }
        try
        {
            string[] devices = Microphone.devices;
            if (devices.Length == 0)
            {
                Finish(T("No mic found", "找不到麥克風"));
                return;
            }
            var selection = new FileInfo(worker.MicrophoneSelectionPath);
            if (selection.Exists)
            {
                if (selection.Length > 1024)
                {
                    Finish(T("Check mic setup", "檢查麥克風設定"));
                    return;
                }
                device = File.ReadAllText(selection.FullName).Trim();
                if (Array.IndexOf(devices, device) < 0)
                {
                    device = null;
                    Finish(T("No mic found", "找不到麥克風"));
                    return;
                }
            }
            else device = devices[0];
            microphoneOwner = this;
            // A non-looping buffer with a guard second preserves the final position.
            clip = Microphone.Start(device, false, LocalSpeechWorker.MaximumSeconds + 1, 48000);
            started = advanced = Time.realtimeSinceStartup;
            position = 0;
            if (clip == null)
            {
                FailCaptureStart();
                return;
            }
            Show(State.Starting, T("Starting…", "啟動中……"));
        }
        catch (Exception) { FailCaptureStart(); }
    }

    private void FailCaptureStart() => Abort(T("Check mic access", "檢查麥克風權限"));

    private void Tick()
    {
        if (disposed || state == State.Idle) return;
        if (!CanUse || !ask.enabledInHierarchy)
        {
            Abort(T("Cancelled", "已取消"));
            return;
        }
        if (state == State.Processing)
        {
            if (request.IsCompleted) Receive();
            return;
        }
        if (state != State.Starting && state != State.Recording) return;
        if (exhibitionAudioActive())
        {
            Abort(T("Audio playing", "音效播放中"));
            return;
        }
        try
        {
            float now = Time.realtimeSinceStartup;
            int current = Microphone.GetPosition(device);
            ObserveCaptureProgress(current, now);
            if (state == State.Starting && now - started > 2)
                FailCaptureStart();
            else if (state == State.Recording)
            {
                if (!Microphone.IsRecording(device) || now - advanced > 1)
                    Abort(T("Mic interrupted", "麥克風已中斷"));
                else if (position >= clip.frequency * LocalSpeechWorker.MaximumSeconds)
                    StopRecording();
            }
        }
        catch (Exception) { Abort(T("Mic interrupted", "麥克風已中斷")); }
    }

    private void ObserveCaptureProgress(int current, float now)
    {
        if (current <= position) return;
        position = current;
        advanced = now;
        if (state != State.Starting) return;
        recordingSince = now;
        Show(State.Recording, T("Listening…", "聆聽中……"));
    }

    private void StopRecording()
    {
        float[] audio = null;
        try
        {
            int rate = clip.frequency, channels = clip.channels;
            int frames = Mathf.Clamp(Microphone.GetPosition(device), 0,
                Mathf.Min(clip.samples, rate * LocalSpeechWorker.MaximumSeconds));
            StopDevice();
            if (frames < rate / 4)
            {
                Finish(T("No speech detected", "未偵測到語音"));
                return;
            }
            audio = new float[frames * channels];
            if (!clip.GetData(audio, 0)) throw new InvalidOperationException("Audio unavailable");
            requestCancellation = new CancellationTokenSource();
            request = worker.Transcribe(audio, channels, rate, chinese, requestCancellation.Token);
            audio = null; // Ownership passed to the worker.
            ShowTranscribing();
        }
        catch (Exception) { Abort(T("Mic interrupted", "麥克風已中斷")); }
        finally
        {
            if (audio != null) Array.Clear(audio, 0, audio.Length);
            ReleaseClip();
        }
    }

    private void ShowTranscribing() => Show(State.Processing, T("Transcribing…", "轉錄中……"));

    private void Receive()
    {
        SpeechResult result = request.GetAwaiter().GetResult();
        request = null;
        requestCancellation.Dispose();
        requestCancellation = null;
        if (result.Outcome == SpeechOutcome.Ready)
        {
            transcript = result.Text;
            if (!NeedsDraftConfirmation(input.value, Input.compositionString))
                UseTranscript();
            else
            {
                Show(State.Review, T("Replace draft?", "取代草稿？"));
                replace.Focus();
            }
            return;
        }
        switch (result.Outcome)
        {
            case SpeechOutcome.NoSpeech: Finish(T("No speech detected", "未偵測到語音")); break;
            case SpeechOutcome.Unavailable: Finish(T("Voice unavailable", "語音尚未提供")); break;
            case SpeechOutcome.Timeout: Finish(T("Voice timed out", "辨識逾時")); break;
            case SpeechOutcome.Cancelled: Finish(T("Cancelled", "已取消")); break;
            default: Finish(T("Voice error", "語音辨識失敗")); break;
        }
    }

    private void UseTranscript()
    {
        if (!CanUse || string.IsNullOrEmpty(transcript) || !string.IsNullOrEmpty(Input.compositionString)) return;
        input.value = transcript;
        Finish(T("Review draft", "請確認草稿"));
        input.Focus();
    }

    internal static bool NeedsDraftConfirmation(string draft, string ime) =>
        !string.IsNullOrEmpty(draft) || !string.IsNullOrEmpty(ime);

    private void KeepDraft()
    {
        Finish(T("Draft kept", "已保留草稿"));
        input.Focus();
    }

    private void Show(State next, string message)
    {
        state = next;
        if (next == State.Idle) ticker.Pause();
        else ticker.Resume();
        status.text = message;
        bool review = next == State.Review;
        bool recording = next == State.Recording;
        bool busy = next == State.Starting || next == State.Processing;
        mic.style.display = review ? DisplayStyle.None : DisplayStyle.Flex;
        controls.style.display = review ? DisplayStyle.Flex : DisplayStyle.None;
        mic.tooltip = recording ? T("Stop recording", "停止錄音")
            : busy ? message : T("Record and transcribe on this computer", "在此電腦上錄音及轉錄");
        float fromStop = stopMix, fromBusy = busyMix, fromEdge = edgeMix;
        motion.Run(mic, .16f, progress =>
        {
            if (disposed) return;
            stopMix = Mathf.Lerp(fromStop, recording ? 1 : 0, progress);
            busyMix = Mathf.Lerp(fromBusy, busy ? 1 : 0, progress);
            edgeMix = Mathf.Lerp(fromEdge, recording ? 1 : 0, progress);
            microphoneGlyph.style.opacity = 1 - Mathf.Clamp01(stopMix + busyMix);
            stopGlyph.style.opacity = stopMix;
            busyGlyph.style.opacity = busyMix;
            mic.style.borderTopColor = Color.Lerp(LivingGlassAssets.Hex("#526475"), accent, .55f + .45f * edgeMix);
        });
    }

    private void StopDevice()
    {
        if (microphoneOwner != this) return;
        try { Microphone.End(device); }
        finally { microphoneOwner = null; device = null; }
    }

    private void ReleaseClip()
    {
        if (clip != null) UnityEngine.Object.Destroy(clip);
        clip = null;
    }

    private void Abort(string message)
    {
        try { StopDevice(); }
        catch (Exception) { }
        finally
        {
            ReleaseClip();
            var cancellation = requestCancellation;
            var pending = request;
            requestCancellation = null;
            request = null;
            if (cancellation != null)
            {
                cancellation.Cancel();
                if (pending == null) cancellation.Dispose();
                else _ = pending.ContinueWith(_ => cancellation.Dispose(), TaskScheduler.Default);
            }
            Finish(message);
        }
    }

    private void Finish(string message)
    {
        transcript = null;
        if (!disposed) Show(State.Idle, message);
    }

    private void OnFocusChanged(bool focused)
    {
        if (!focused && state != State.Idle) Abort(T("Cancelled", "已取消"));
    }

    private void OnDetached(DetachFromPanelEvent evt) => Dispose(false);

    public void CancelForContextChange()
    {
        if (!disposed && state != State.Idle) Abort(T("Cancelled", "已取消"));
    }

    public void Dispose() => Dispose(true);

    private void Dispose(bool restoreMarker)
    {
        if (disposed) return;
        disposed = true;
        ticker.Pause();
        Application.focusChanged -= OnFocusChanged;
        rail.UnregisterCallback<DetachFromPanelEvent>(OnDetached);
        // A panel can detach from inside a shared motion completion. Let its
        // guarded sample expire rather than mutating the active track list there.
        if (restoreMarker) motion.Cancel(mic);
        mic.clicked -= Activate;
        replace.clicked -= UseTranscript;
        keep.clicked -= KeepDraft;
        Abort("");
        if (!restoreMarker) return;
        mic.RemoveFromHierarchy();
        controls.RemoveFromHierarchy();
        rail.RemoveFromClassList("lg-voice-bound");
        rail.Insert(0, marker);
        status.text = T("Voice unavailable", "語音尚未提供");
    }
}

// Each physical key press can activate the control once, even when the icon
// changes while Enter or Space is still held down.
internal sealed class LivingGlassVoiceButton : Button
{
    private KeyCode heldKey;

    public LivingGlassVoiceButton(Action activate) : base(activate)
    {
        RegisterCallback<KeyDownEvent>(evt =>
        {
            if (!IsActivationKey(evt.keyCode)) return;
            evt.StopImmediatePropagation();
            focusController?.IgnoreEvent(evt);
            if (heldKey != KeyCode.None) return;
            heldKey = evt.keyCode;
            if (enabledInHierarchy) activate();
        }, TrickleDown.TrickleDown);
        RegisterCallback<KeyUpEvent>(evt =>
        {
            if (!IsActivationKey(evt.keyCode)) return;
            if (heldKey == evt.keyCode) heldKey = KeyCode.None;
            evt.StopImmediatePropagation();
            focusController?.IgnoreEvent(evt);
        }, TrickleDown.TrickleDown);
        RegisterCallback<BlurEvent>(_ => heldKey = KeyCode.None);
    }

    private static bool IsActivationKey(KeyCode key) =>
        key == KeyCode.Return || key == KeyCode.KeypadEnter || key == KeyCode.Space;
}
