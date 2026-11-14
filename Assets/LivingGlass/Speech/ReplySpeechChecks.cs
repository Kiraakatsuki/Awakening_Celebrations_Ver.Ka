#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// Runs only when explicitly invoked in idle Edit Mode. The UI fixtures are
// service-free and never instantiate an AudioSource, camera, or microphone.
internal static class ReplySpeechChecks
{
    private const string Evidence = "Documentation/Stage4VoiceExperience/Stage4_3/";
    private static readonly List<string> Results = new List<string>();
    private const string Fixture = "Every experience leaves something with us.";
    private static EditorWindow window;
    private static LivingGlassPresentation view;
    private static LivingGlassReplySpeech output;
    private static int index;
    private static Rect ask;
    private static double due;
    private static bool prepared;

    [MenuItem("Awakening Celebrations/Living Glass/Check local reply audio in Edit Mode")]
    internal static async void RunNative()
    {
        Guard();
        string path = Evidence + "Stage4_3B_WorkerChecks.json";
        var results = new Dictionary<string, object>();
        File.WriteAllText(path, "RUNNING");
        try
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var worker = LocalQwenReplySpeech.Discover(Application.dataPath);
            Require(worker != null && worker.IsAvailable, "operator config and installed model");
            Require((await new LocalQwenReplySpeech(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())).GenerateAsync(Fixture, "en", default)).Error
                == ReplySpeechError.Unavailable, "missing installation fallback");
            Require(LocalVoiceInference.Slot.Wait(0), "inference slot must be available before the fixture");
            try
            {
                Require((await worker.GenerateAsync(Fixture, "en", default)).Error == ReplySpeechError.Busy,
                    "Qwen does not queue behind active inference");
            }
            finally { LocalVoiceInference.Slot.Release(); }
            results["MissingInstallAndBusy"] = "PASS";
            using (var cancellation = new CancellationTokenSource(100))
            {
                var result = await worker.GenerateAsync(Fixture, "en", cancellation.Token);
                Require(result.Error == ReplySpeechError.Cancelled, "in-flight cancellation");
                results["CancellationIncludingTeardownMs"] = watch.ElapsedMilliseconds;
            }
            foreach (var sample in new[] { (text: Fixture, locale: "en"), (text: "別著急，慢慢探索。", locale: "zh") })
            {
                Guard();
                watch.Restart();
                var result = await worker.GenerateAsync(sample.text, sample.locale, default);
                Require(result.Error == ReplySpeechError.None && result.Samples.Length > 1000
                    && result.Samples.Any(a => Math.Abs(a) > .005f), sample.locale + " ICL PCM: " + result.Error);
                results[sample.locale] = new { result = "PASS", elapsedMs = watch.ElapsedMilliseconds,
                    seconds = (double)result.Samples.Length / ReplySpeechWave.Rate };
            }
            Guard();
            results["Outcome"] = "PASS";
        }
        catch (Exception error) { results["Outcome"] = "FAIL: " + error; }
        File.WriteAllText(path, JsonConvert.SerializeObject(results, Formatting.Indented));
    }

    [MenuItem("Awakening Celebrations/Living Glass/Check reply audio UI in Edit Mode")]
    internal static void RunUI()
    {
        Guard();
        if (window != null) throw new InvalidOperationException("Fixture already active.");
        Results.Clear();
        File.WriteAllText(Evidence + "Stage4_3B_EditorChecks.txt", "RUNNING");
        try
        {
            TestProtocolData();
            var type = Type.GetType("LivingGlassPreviewWindow, Assembly-CSharp-Editor", true);
            window = (EditorWindow)type.GetMethod("Open").Invoke(null, null);
            const System.Reflection.BindingFlags hidden = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            view = (LivingGlassPresentation)type.GetProperty("Presentation", hidden).GetValue(window);
            Require(typeof(LivingGlassPresentation).GetField("flow", hidden).GetValue(view) == null, "service-free preview");
            window.Focus();
            index = 0;
            StartLayout();
            EditorApplication.update += LayoutTick;
        }
        catch (Exception error) { Finish(error); }
    }

    private static void TestProtocolData()
    {
        var text = string.Join(" ", Enumerable.Repeat("Take a moment. 你可以與 GAIA 展開對話。", 20));
        var parts = ReplySpeechText.Split(text);
        Require(parts.Count > 1 && parts.All(s => s.Length <= 145) &&
            string.Join("", parts).Replace(" ", "") == text.Replace(" ", ""), "chunking loses text");
        Require(ReplySpeechText.Language("你好，GAIA", "en") == "mixed" &&
            ReplySpeechText.Language("English answer", "zh") == "en" &&
            ReplySpeechText.Language("繁體中文", "en") == "zh", "language routing");
        Require(ReplySpeechText.Split(new string('a', 2401)) == null, "maximum response size");
        byte[] raw;
        using (var buffer = new MemoryStream())
        {
            using (var writer = new BinaryWriter(buffer, System.Text.Encoding.ASCII, true))
            {
                writer.Write(0x46464952); writer.Write(40); writer.Write(0x45564157);
                writer.Write(0x20746d66); writer.Write(16); writer.Write(0x00010001);
                writer.Write(24000); writer.Write(48000); writer.Write(0x00100002);
                writer.Write(0x61746164); writer.Write(4); writer.Write(short.MinValue); writer.Write((short)8192);
            }
            raw = buffer.ToArray();
        }
        var decoded = ReplySpeechWave.Decode(Convert.ToBase64String(raw));
        Require(decoded.Length == 2 && decoded[0] == -1 && decoded[1] == .25f, "PCM decode");
        raw[24] = 0;
        Require(ReplySpeechWave.Decode(Convert.ToBase64String(raw)) == null &&
                ReplySpeechWave.Decode("not base64") == null, "bad WAV rejection");
        Results.Add("PASS: bounded multilingual chunking, locale fallback, and strict PCM/WAV parsing.");
    }

    private static void StartLayout()
    {
        int p = index % 6;
        var locale = (AppLanguage)((index / 6) % 2);
        int surface = (index / 12) % 3;
        bool compact = index >= 36;
        view.Root.style.width = compact ? 1280 : 1920;
        view.Root.style.height = compact ? 720 : 1080;
        Render(p,locale,surface);
        prepared = false;
        due = EditorApplication.timeSinceStartup + .1;
    }

    private static void Render(int performer, AppLanguage locale, int surface)
    {
        string assistant = "Assistant-" + LivingGlassRoutes.Performers[performer];
        view.Render(surface == 0 ? LivingGlassRoutes.Chapter(performer,4) : surface == 1 ? assistant : "COLOR-RELEASE",
            locale, performer, false, true);
        if (surface == 2) view.OpenOverlay(assistant,locale,performer);
        view.PreviewTick(1);
    }

    private static void LayoutTick()
    {
        if (EditorApplication.timeSinceStartup < due) return;
        try
        {
            Guard();
            var transcript = view.Root.Q<ScrollView>("assistantConversation");
            var button = view.Root.Q<Button>("assistantChatSend");
            var scale = view.Root.worldBound.width / view.Root.resolvedStyle.width;
            Require(scale > 0 && !float.IsInfinity(scale), "viewport not laid out");
            int p = index % 6;
            var locale = (AppLanguage)((index / 6) % 2);
            if (!prepared)
            {
                ask = button.worldBound;
                var backend = new PendingReply();
                var scope = (index / 12) % 3 == 2 ? view.Root.Q("glass-window") : view.Content;
                output = new LivingGlassReplySpeech(scope, backend, new FixtureAudio(),
                    locale, LivingGlassAssets.Accents[p], () => true, () => true);
                output.Add(transcript, Fixture);
                Require(backend.Calls == 0, "receiving text starts no synthesis");
                prepared = true;
                due = EditorApplication.timeSinceStartup + .08;
                return;
            }
            var row = transcript.Q("reply-speech-controls");
            var listen = row.Q<Button>("reply-speech-action");
            Require(listen.text == (locale == AppLanguage.Chinese ? "朗讀" : "Listen") && listen.focusable, "localized keyboard target");
            Require(listen.worldBound.width / scale >= 109 && listen.worldBound.height / scale >= 45, "reply target too small");
            Require(Vector2.Distance(ask.position, button.worldBound.position) < .75f &&
                Vector2.Distance(ask.size, button.worldBound.size) < .75f, "Ask moved");
            output.Dispose();
            output = null;
            if (++index < 72) StartLayout();
            else
            {
                Results.Add("PASS: 72 actual UI Toolkit layouts, all six themes, EN/ZH, three chat surfaces, 1920×1080 and 1280×720; no autoplay and Ask stays anchored.");
                TestLifecycle();
                Finish(null);
            }
        }
        catch (Exception error) { Finish(error); }
    }

    private static void TestLifecycle()
    {
        Render(0, AppLanguage.English, 0);
        var transcript = view.Root.Q<ScrollView>("assistantConversation");
        var reply = new PendingReply();
        var audio = new FixtureAudio();
        bool current = true, allowed = true;
        using var panel = new LivingGlassReplySpeech(view.Content, reply, audio, AppLanguage.English,
            LivingGlassAssets.Accents[0], () => current, () => allowed);
        var action = panel.Add(transcript, Fixture).Q<Button>();
        void Click()
        {
            action.Focus();
            using var e = KeyDownEvent.GetPooled(new Event { type = EventType.KeyDown, keyCode = KeyCode.Return });
            e.target = action;
            action.SendEvent(e);
            using var up = KeyUpEvent.GetPooled(new Event { type = EventType.KeyUp, keyCode = KeyCode.Return });
            up.target = action;
            action.SendEvent(up);
        }
        allowed = false;
        Click();
        Require(reply.Calls == 0, "audio/capture priority");
        allowed = true;
        Click();
        Require(reply.Calls == 1 && action.text == "Stop", "keyboard native submit starts preparation");
        var pending = reply.Promise;
        Click();
        Require(reply.Token.IsCancellationRequested && action.text == "Listen", "Stop during preparation");
        pending.SetResult(new ReplySpeechResult(ReplySpeechError.None, new[] { .1f }));
        panel.Tick();
        Require(audio.Starts == 0, "cancelled result played");
        Click();
        reply.Promise.SetResult(new ReplySpeechResult(ReplySpeechError.Timeout));
        panel.Tick();
        Require(action.text == "Listen" && action.parent.Q<Label>().text == "Audio timed out", "localized timeout status");
        Click();
        reply.Promise.SetResult(new ReplySpeechResult(ReplySpeechError.None, new[] { .1f }));
        panel.Tick();
        Require(audio.Starts == 1 && audio.IsPlaying, "ready clip not played");
        Click();
        Require(!audio.IsPlaying && action.text == "Listen", "Stop during playback");
        Click();
        reply.Promise.SetResult(new ReplySpeechResult(ReplySpeechError.None, new[] { .1f }));
        panel.Tick();
        audio.Stop();
        panel.Tick();
        Require(action.text == "Listen", "completion does not restore idle");
        Click();
        current = false;
        pending = reply.Promise;
        pending.SetResult(new ReplySpeechResult(ReplySpeechError.None, new[] { .1f }));
        panel.Tick();
        Require(audio.Starts == 2 && action.text == "Listen", "stale context played");
        current = true;
        Click();
        pending = reply.Promise;
        Render(5, AppLanguage.Chinese, 2);
        Require(reply.Token.IsCancellationRequested && audio.Disposed, "performer/locale/navigation detach");
        pending.SetResult(new ReplySpeechResult(ReplySpeechError.None, new[] { .1f }));
        panel.Tick();
        Require(audio.Starts == 2, "detached result played");
        Results.Add("PASS: UI Toolkit submit, preparation/Stop, cancellation, stale result, completion, failure, detach during context change, disposal, and exhibition audio/capture priority using fake audio.");
    }

    private static void Guard()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            throw new InvalidOperationException("Idle Edit Mode required.");
    }

    private static void Finish(Exception error)
    {
        EditorApplication.update -= LayoutTick;
        output?.Dispose();
        output = null;
        if (window != null) window.Close();
        window = null;
        view = null;
        if (error != null) Results.Add("FAIL at specimen " + index + ": " + error);
        File.WriteAllLines(Evidence + "Stage4_3B_EditorChecks.txt", Results);
    }

    private static void Require(bool value, string label)
    {
        if (!value) throw new InvalidOperationException(label);
    }

    private sealed class PendingReply : IReplySpeech
    {
        public bool IsAvailable => true;
        public int Calls;
        public CancellationToken Token;
        public TaskCompletionSource<ReplySpeechResult> Promise;
        public Task<ReplySpeechResult> GenerateAsync(string text, string locale, CancellationToken cancellation)
        {
            Calls++; Token = cancellation; Promise = new TaskCompletionSource<ReplySpeechResult>();
            return Promise.Task;
        }
    }

    private sealed class FixtureAudio : IReplyAudio
    {
        public int Starts;
        public bool Disposed;
        public bool IsPlaying { get; private set; }
        public bool TryPlay(float[] samples) { Starts++; IsPlaying = true; return true; }
        public void Stop() { IsPlaying = false; }
        public void Dispose() { Disposed = true; Stop(); }
    }
}
#endif
