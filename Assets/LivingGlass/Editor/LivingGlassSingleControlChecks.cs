using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// Runs against the existing service-free preview; Bind/StartRecording are never called.
// Only state events, file-worker result objects, and UI Toolkit input events are injected.
public static class LivingGlassSingleControlChecks
{
    private const string Output = "Documentation/Stage4VoiceExperience/Stage4_2/SingleControlEditorChecks.txt";
    private const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly Type Capture = Type.GetType("LivingGlassSpeechCapture, Assembly-CSharp", true);
    private static readonly Type State = Capture.GetNestedType("State", BindingFlags.NonPublic);
    private static readonly Type Worker = Type.GetType("LocalSpeechWorker, Assembly-CSharp", true);
    private static readonly Type Result = Type.GetType("SpeechResult, Assembly-CSharp", true);
    private static readonly Type Outcome = Type.GetType("SpeechOutcome, Assembly-CSharp", true);
    private static readonly MethodInfo FromResult = typeof(Task).GetMethod("FromResult").MakeGenericMethod(Result);
    private static readonly string[] Specimens = { "Idle", "Starting", "Recording", "Processing", "Review", "Idle" };
    private static readonly List<string> Lines = new List<string>();
    private static LivingGlassPreviewWindow window;
    private static LivingGlassPresentation view;
    private static Binding binding;
    private static EditorApplication.CallbackFunction step;
    private static int index;
    private static Rect askBounds, micBounds, inputBounds;
    private static double due;

    [MenuItem("Awakening Celebrations/Living Glass/Run isolated single-control voice checks")]
    public static void Run()
    {
        Guard();
        if (window != null) throw new InvalidOperationException("Already running.");
        Lines.Clear();
        File.WriteAllText(Output, "RUNNING");
        try
        {
            window = LivingGlassPreviewWindow.Open();
            view = window.Presentation;
            Require(typeof(LivingGlassPresentation).GetField("flow", Hidden).GetValue(view) == null, "Preview must not have a flow/service owner");
            window.Focus();
            index = 0;
            StartCase();
            step = Tick;
            EditorApplication.update += step;
        }
        catch (Exception ex) { Finish(ex); }
    }

    private static void Guard()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            throw new InvalidOperationException("Idle Edit Mode required.");
    }

    private static void StartCase()
    {
        int stage = index % Specimens.Length;
        int p = (index / 6) % 6;
        var locale = (AppLanguage)((index / 36) % 2);
        bool zh = locale == AppLanguage.Chinese;
        int surface = (index / 72) % 3;
        bool compact = index >= 216;
        if (stage == 0)
        {
            binding?.Dispose();
            view.Root.style.width = compact ? 1280 : 1920;
            view.Root.style.height = compact ? 720 : 1080;
            // Compact includes opaque and reduced-motion fallback.
            window.PreviewAssets.reducedMotion = compact;
            window.PreviewAssets.reducedEffects = compact;
            binding = Open(p, locale, surface);
        }
        var labels = zh
            ? new[] { "語音輸入", "啟動中……", "聆聽中……", "轉錄中……", "取代草稿？", "檢查麥克風權限" }
            : new[] { "Dictate", "Starting…", "Listening…", "Transcribing…", "Replace draft?", "Check mic access" };
        binding.Show(Specimens[stage], labels[stage]);
        view.PreviewTick(1);
        due = EditorApplication.timeSinceStartup + .085;
    }

    private static Binding Open(int performer, AppLanguage locale, int surface)
    {
        string assistant = "Assistant-" + LivingGlassRoutes.Performers[performer];
        view.Render(surface == 0 ? LivingGlassRoutes.Chapter(performer, 4) : surface == 1 ? assistant : "COLOR-RELEASE",
            locale, performer, false, true);
        var scope = surface == 2 ? view.OpenOverlay(assistant, locale, performer) : view.Content;
        view.PreviewTick(1);
        return new Binding(scope, performer, locale, typeof(LivingGlassPresentation).GetProperty("Motion", Hidden).GetValue(view));
    }

    private static void Tick()
    {
        if (EditorApplication.timeSinceStartup < due) return;
        try
        {
            Guard();
            CheckLayout();
            if (++index < 432) { StartCase(); return; }
            Lines.Add("PASS: 432 actual UI Toolkit layouts (6 themes × 2 locales × 3 surfaces × 2 viewport/fallback profiles × 6 voice specimens).");
            TestTransitions();
            Finish(null);
        }
        catch (Exception ex) { Finish(ex); }
    }

    private static void CheckLayout()
    {
        int stage = index % Specimens.Length;
        var mic = binding.Mic;
        var review = binding.Rail.Q("voice-draft-confirmation");
        var scale = view.Root.worldBound.width / view.Root.resolvedStyle.width;
        Require(scale > 0 && !float.IsInfinity(scale), "panel size");
        if (stage == 0)
        {
            askBounds = binding.Ask.worldBound;
            micBounds = mic.worldBound;
            inputBounds = binding.Input.worldBound;
        }
        Require(Same(askBounds, binding.Ask.worldBound) && Same(inputBounds, binding.Input.worldBound), "Ask/editor shifted");
        Require(binding.Rail.worldBound.xMax <= binding.Input.worldBound.xMin + .75f, "voice overlaps editor");
        Require(binding.Rail.Q("voice-primary") == null && binding.Rail.Q("voice-cancel") == null, "obsolete submenu still present");
        Require(binding.Rail.Query<Button>().ToList().Count == 3, "unexpected voice actions");
        Require(binding.Status.contentRect.height >= 40 && binding.Status.contentRect.width > 0, "status slot collapsed");
        Vector2 measured = binding.Status.MeasureTextSize(binding.Status.text, binding.Status.contentRect.width, VisualElement.MeasureMode.Exactly,
            0, VisualElement.MeasureMode.Undefined);
        Require(measured.y <= binding.Status.contentRect.height + 1,
            "localized status clipped: " + binding.Status.text + " (" + measured + " / " + binding.Status.contentRect + ")");
        if (stage == 4)
        {
            Require(mic.resolvedStyle.display == DisplayStyle.None && review.resolvedStyle.display == DisplayStyle.Flex, "draft confirmation visibility");
            foreach (var button in review.Query<Button>().ToList())
                Require(binding.Rail.worldBound.Contains(button.worldBound.center) && button.worldBound.height / scale >= 43, "draft choice target");
            return;
        }
        Require(review.resolvedStyle.display == DisplayStyle.None && mic.resolvedStyle.display == DisplayStyle.Flex, "one visible voice button");
        Require(Same(micBounds, mic.worldBound) && mic.worldBound.width / scale >= 51 && mic.worldBound.height / scale >= 51, "microphone moved or shrank");
        Require(mic.focusable, "microphone is not keyboard focusable");
        Require(binding.Glyph(stage == 2 ? "stop" : stage == 1 || stage == 3 ? "busy" : "mic") > .99f, "wrong glyph");
        Require(binding.Status.worldBound.yMax <= binding.Rail.worldBound.yMax + .75f, "caption outside rail");
    }

    private static void TestTransitions()
    {
        binding.Dispose();
        binding = Open(0, AppLanguage.English, 0);
        var current = binding;
        int starts = 0, stops = 0, sent = 0;
        current.Ask.clicked += () => sent++;
        Action start = () => { starts++; current.Show("Starting", "Starting…"); };
        Action stop = () => { stops++; current.Invoke("ShowTranscribing"); };
        current.Activate(0, start, stop);
        current.Activate(.01f, start, stop);
        current.Invoke("ObserveCaptureProgress", 0, .05f);
        Require(starts == 1 && current.Current == "Starting", "repeated activation or zero-position false Listening");
        current.Invoke("ObserveCaptureProgress", 64, .06f);
        current.Activate(.09f, start, stop);
        Require(stops == 0 && current.Status.text == "Listening…", "readiness or rapid-stop guard");
        current.Activate(.4f, start, stop);
        current.Activate(.41f, start, stop);
        Require(stops == 1 && current.Current == "Processing" && current.Status.text == "Transcribing…", "stop must transition once and suppress restart");
        current.Complete("Ready", "How does the light change?");
        Require(current.Current == "Idle" && current.Input.value == "How does the light change?" && sent == 0, "editable transcript/Ask separation");
        Lines.Add("PASS: Idle → Starting → Listening only after capture progress → Transcribing → editable draft, with repeat gating and no Ask.");

        current.Input.value = "A manually typed draft";
        current.Show("Starting", "Starting…");
        current.Invoke("FailCaptureStart");
        Require(current.Current == "Idle" && current.Status.text == "Check mic access" && current.Input.value == "A manually typed draft", "permission failure");
        current.Complete("Error", null);
        Require(current.Current == "Idle" && current.Status.text == "Voice error" && current.Input.value == "A manually typed draft", "worker failure");
        current.Activate(1, start, stop);
        Require(starts == 2 && current.Current == "Starting", "recovery");
        Lines.Add("PASS: capture permission/start failure and worker error restore an available control, preserve the draft and allow retry.");

        current.Complete("Ready", "Suggested replacement");
        Require(current.Current == "Review" && current.Input.value == "A manually typed draft", "draft silently overwritten");
        current.Invoke("KeepDraft");
        Require(current.Input.value == "A manually typed draft" && current.Current == "Idle", "Keep failed");
        current.Complete("Ready", "Reviewed replacement");
        current.Invoke("UseTranscript");
        Require(current.Input.value == "Reviewed replacement" && current.Current == "Idle" && sent == 0, "Replace failed or sent automatically");
        var confirm = Capture.GetMethod("NeedsDraftConfirmation", BindingFlags.Static | BindingFlags.NonPublic);
        Require((bool)confirm.Invoke(null, new object[] { "", "注" }), "IME guard not applied");
        Lines.Add("PASS: Keep and Replace, no autosubmit, IME policy.");

        current.Show("Recording", "Listening…");
        current.Invoke("CancelForContextChange");
        Require(current.Current == "Idle", "context transition did not cancel recording");
        current.Show("Recording", "Listening…");
        view.Render("Home", AppLanguage.English, -1, false, true);
        Require(current.Disposed, "route render did not dispose recording binding");

        binding = Open(0, AppLanguage.English, 2);
        using (var cancellation = new CancellationTokenSource())
        {
            object late = binding.Pending(cancellation);
            var previous = binding;
            view.CloseOverlay(false);
            view.PreviewTick(1);
            Require(previous.Disposed && cancellation.IsCancellationRequested && previous.Request == null, "assistant close did not cancel processing");
            binding = Open(5, AppLanguage.Chinese, 2);
            late.GetType().GetMethod("SetResult").Invoke(late, new[] { MakeResult("Ready", "late result") });
            Require(binding.Input.value == "" && binding.Status.text == "語音輸入", "late result or old locale leaked");
        }
        Lines.Add("PASS: recording cancellation on context change and route detach; AI close cancels pending processing; late result cannot reach another performer/locale.");

        var keyButton = (Button)Activator.CreateInstance(Type.GetType("LivingGlassVoiceButton, Assembly-CSharp", true), new object[] { (Action)(() => starts++) });
        binding.Rail.Add(keyButton);
        keyButton.Focus();
        Require(keyButton.focusController.focusedElement == keyButton, "button focus");
        int before = starts;
        Key(keyButton, KeyCode.Return, true);
        Key(keyButton, KeyCode.Return, true);
        Require(starts == before + 1, "Enter auto-repeat reactivated button");
        Key(keyButton, KeyCode.Return, false);
        Key(keyButton, KeyCode.Space, true);
        Key(keyButton, KeyCode.Space, false);
        Require(starts == before + 2 && keyButton.focusController.focusedElement == keyButton, "Space or focus retention");
        keyButton.RemoveFromHierarchy();
        Lines.Add("PASS: focused keyboard activation, Enter auto-repeat prevention, and Space via actual UI Toolkit events.");
    }

    private static void Key(Button target, KeyCode key, bool down)
    {
        var native = new Event { type = down ? EventType.KeyDown : EventType.KeyUp, keyCode = key };
        if (down)
        {
            using (var evt = KeyDownEvent.GetPooled(native)) { evt.target = target; target.SendEvent(evt); }
        }
        else
        {
            using (var evt = KeyUpEvent.GetPooled(native)) { evt.target = target; target.SendEvent(evt); }
        }
    }

    private static object MakeResult(string outcome, string text) => Activator.CreateInstance(Result, Enum.Parse(Outcome, outcome), text);
    private static bool Same(Rect a, Rect b) => Vector2.Distance(a.position, b.position) < .75f && Vector2.Distance(a.size, b.size) < .75f;
    private static void Require(bool value, string reason)
    {
        if (!value) throw new InvalidOperationException(reason);
    }

    private static void Finish(Exception ex)
    {
        if (step != null) EditorApplication.update -= step;
        step = null;
        binding?.Dispose();
        binding = null;
        if (window != null) window.Close();
        window = null;
        view = null;
        if (ex != null) Lines.Add("FAIL case " + index + ": " + (ex.InnerException ?? ex));
        Lines.Add("NOT RUN: real microphone, OS permission/denial, native IME, Play Mode, live assistant, Player build.");
        File.WriteAllLines(Output, Lines);
    }

    private sealed class Binding : IDisposable
    {
        private readonly IDisposable capture;
        public readonly VisualElement Rail;
        public readonly TextField Input;
        public readonly Button Ask;
        public Button Mic => Rail.Q<Button>("voice-microphone");
        public Label Status => Rail.Q<Label>("voice-status");
        public bool Disposed => (bool)Field("disposed").GetValue(capture);
        public string Current => Field("state").GetValue(capture).ToString();
        public object Request => Field("request").GetValue(capture);
        public Binding(VisualElement scope, int performer, AppLanguage locale, object motion)
        {
            Rail = scope.Q("voice-affordance");
            Input = scope.Q<TextField>("assistantChatInput");
            Ask = scope.Q<Button>("assistantChatSend");
            var worker = Activator.CreateInstance(Worker, Path.GetFullPath("VoiceLocal"));
            capture = (IDisposable)Activator.CreateInstance(Capture, Hidden, null,
                new object[] { scope, worker, Input, Ask, performer, locale, (Func<bool>)(() => true), (Func<bool>)(() => false), motion }, null);
            Pause();
        }
        private static FieldInfo Field(string name) => Capture.GetField(name, Hidden);
        public void Invoke(string name, params object[] args) => Capture.GetMethod(name, Hidden | BindingFlags.Public).Invoke(capture, args);
        public void Activate(float now, Action onStart, Action onStop) => Invoke("HandleActivation", now, onStart, onStop);
        public void Show(string state, string status) { Invoke("Show", Enum.Parse(State, state), status); Pause(); }
        public float Glyph(string glyph) => Mic.Q("voice-" + glyph + "-glyph").style.opacity.value;
        public void Complete(string outcome, string text)
        {
            Field("requestCancellation").SetValue(capture, new CancellationTokenSource());
            Field("request").SetValue(capture, FromResult.Invoke(null, new[] { MakeResult(outcome, text) }));
            Invoke("Receive");
            Pause();
        }
        public object Pending(CancellationTokenSource cancellation)
        {
            var source = Activator.CreateInstance(typeof(TaskCompletionSource<>).MakeGenericType(Result));
            Field("requestCancellation").SetValue(capture, cancellation);
            Field("request").SetValue(capture, source.GetType().GetProperty("Task").GetValue(source));
            Show("Processing", "Transcribing…");
            return source;
        }
        private void Pause() => ((IVisualElementScheduledItem)Field("ticker").GetValue(capture)).Pause();
        public void Dispose() => capture.Dispose();
    }
}
