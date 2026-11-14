#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

internal static class VoiceExperienceChecks
{
    private const string Output = "Documentation/Stage4VoiceExperience/Stage4_3/Stage4_3C_EditorChecks.txt";
    private static readonly List<string> rows = new List<string>();
    private static LivingGlassPresentation view;
    private static EditorWindow window;
    private static LivingGlassAssets assets;
    private static int index, stage;
    private static Rect beginBounds;
    private static double due;

    [MenuItem("Awakening Celebrations/Living Glass/Check voice preferences in Edit Mode")]
    internal static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            throw new InvalidOperationException("Edit Mode only.");
        if (window != null) return;
        rows.Clear();
        File.WriteAllText(Output, "RUNNING");
        try
        {
            Tests();
            Type type = Type.GetType("LivingGlassPreviewWindow, Assembly-CSharp-Editor", true);
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            window = (EditorWindow)type.GetMethod("Open").Invoke(null, null);
            view = (LivingGlassPresentation)type.GetProperty("Presentation", flags).GetValue(window);
            assets = (LivingGlassAssets)type.GetProperty("PreviewAssets", flags).GetValue(window);
            Require(typeof(LivingGlassPresentation).GetField("flow", flags).GetValue(view) == null, "preview flow");
            index = 0;
            Show();
            EditorApplication.update += Tick;
        }
        catch (Exception ex) { End(ex); }
    }

    private static void Tests()
    {
        var fixture = new float[24000];
        fixture[23999] = .4f;
        var tail = ReplySpeechWave.FinishPhrase(fixture, true);
        Require(tail.Length == 29952 && tail[23999] == .4f, "final sample preservation");
        Require(tail[24000] > .39f && tail[24191] == 0f && tail[29951] == 0f, "declick and trailing silence");
        Require(!ReplySpeechWave.ReachedTokenLimit(449 * 1920) && ReplySpeechWave.ReachedTokenLimit(450 * 1920), "clipped generation threshold");
        Require(Math.Abs(UnityReplyAudio.CompletionTime(10, 24000, 24000, .08) - 11.08) < .0001, "duration/DSP drain");
        Require(UnityReplyAudio.CancellationGain(0) == 1f &&
                Math.Abs(UnityReplyAudio.CancellationGain(.0175) - .5f) < .01f &&
                UnityReplyAudio.CancellationGain(.035) == 0f, "cancellation ramp");
        rows.Add("PASS: known-duration PCM calculation, last sample retained, short de-click bridge and silence, intentional Stop gain, and codec-cap detection.");
        foreach (string route in LivingGlassRoutes.AllRoutes)
            if (LivingGlassRoutes.Protected(route)) Require(GuideCues.For(route) == null, "protected narration");
        for (int performer = 0; performer < 6; performer++)
            Require(GuideCues.For(LivingGlassRoutes.Chapter(performer,4)) == "reflection", "performer cue parity");
        string[] tutorialCues = { "overview", "tutorial-move", "tutorial-colour", "gestures",
            "tutorial-confirm", "tutorial-portrait", "tutorial-ready" };
        Require(LivingGlassRoutes.Tutorials.Length == tutorialCues.Length, "all numbered steps covered");
        for (int step = 0; step < tutorialCues.Length; step++)
            Require(GuideCues.For(LivingGlassRoutes.Tutorials[step]) == tutorialCues[step], "tutorial cue order");
        Require(GuideCues.For("TUTORIAL---RETURN---DISMISS") == tutorialCues[1], "return route shares step 2 cue");
        var visit = new GuideVisit();
        var authority = new GuideLoadAuthority();
        Require(!visit.TryConsume("welcome", AppLanguage.English), "new visitor Off");
        visit.Set(true);
        var index = new ApprovedGuideCatalog { approved = false, revision = "draft", clips = new[]
        {
            new GuideClipEntry { id = "welcome", en = "LivingGlassGuide/Approved/welcome-en", zh = "LivingGlassGuide/Approved/welcome-zh" }
        }};
        Require(index.PathFor("welcome", AppLanguage.English) == null, "draft narration exposed");
        index.approved = true;
        var en = index.PathFor("welcome", AppLanguage.English);
        var zh = index.PathFor("welcome", AppLanguage.Chinese);
        Require(en.EndsWith("-en") && zh.EndsWith("-zh"), "locale selection");
        Require(!GuidePlaybackPolicy.CanLoad(null, false, false, visit, "welcome", AppLanguage.English)
                 && !GuidePlaybackPolicy.CanLoad(en, true, false, visit, "welcome", AppLanguage.English)
                 && !GuidePlaybackPolicy.CanLoad(en, false, true, visit, "welcome", AppLanguage.English), "missing/busy/modal gates");
        Require(GuidePlaybackPolicy.CanLoad(en, false, false, visit, "welcome", AppLanguage.English), "explicit opt-in");
        uint old = authority.Begin();
        authority.Invalidate();
        Require(!authority.Accepts(old), "navigation invalidation");
        Require(!GuidePlaybackPolicy.CanLoad(en,false,false,visit,"welcome",AppLanguage.English), "replay after rerender");
        Require(GuidePlaybackPolicy.CanLoad(zh,false,false,visit,"welcome",AppLanguage.Chinese), "language preference");
        uint pending = authority.Begin();
        visit.Set(false);
        authority.Invalidate();
        Require(!authority.Accepts(pending) && !GuidePlaybackPolicy.CanLoad(zh,false,false,visit,"welcome",AppLanguage.Chinese), "disable invalidation");
        visit.Reset();
        Require(!visit.Enabled && !visit.TryConsume("welcome",AppLanguage.English), "reset preference");
        visit.Set(true);
        Require(visit.TryConsume("welcome",AppLanguage.English), "fresh visitor");
        var release = Resources.Load<TextAsset>("LivingGlassGuide/ApprovedCatalog");
        Require(release != null, "approved guide catalog missing");
        var approved = JsonUtility.FromJson<ApprovedGuideCatalog>(release.text);
        Require(approved?.approved == true && approved.revision != "stage43c-draft-1"
            && approved.clips != null, "only an explicit release catalog may be installed");
        var ids = new HashSet<string>();
        foreach (var item in approved.clips)
        {
            Require(item != null && ids.Add(item.id), "duplicate or invalid approved cue");
            foreach (AppLanguage language in new[] { AppLanguage.English, AppLanguage.Chinese })
            {
                var path = approved.PathFor(item.id, language);
                var clip = path == null ? null : Resources.Load<AudioClip>(path);
                Require(clip != null && clip.channels == 1 && clip.frequency == 24000 && clip.samples > 0,
                    "missing or invalid approved clip: " + item.id + " / " + language);
            }
        }
        var expectedIds = new HashSet<string>(tutorialCues)
            { "welcome", "privacy", "awakening", "discovery", "reflection", "closing" };
        Require(ids.SetEquals(expectedIds), "approved cue coverage");
        rows.Add("PASS: seven distinct tutorial cue mappings, bilingual imported clips, and the step 2 return alias.");
        rows.Add("PASS: bilingual cue mapping, protected state exclusion, missing/preview asset gating, replay suppression, locale, rapid context invalidation, disable and visit reset policies.");
    }

    private static void Show()
    {
        bool compact = (index & 2) != 0;
        assets.reducedMotion = (index & 4) != 0;
        assets.reducedEffects = assets.reducedMotion;
        view.Root.style.width = compact ? 1280 : 1920;
        view.Root.style.height = compact ? 720 : 1080;
        view.Render("Home", (AppLanguage)(index & 1), -1, true, false);
        view.PreviewTick(1);
        stage = 0;
        due = EditorApplication.timeSinceStartup + .15;
    }

    private static void Tick()
    {
        if (EditorApplication.timeSinceStartup < due) return;
        try
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode only.");
            var scope = stage == 2 ? view.Root.Q("glass-window") : view.Content;
            var action = scope.Q<Button>("voice-preference");
            float scale = view.Root.worldBound.width / view.Root.resolvedStyle.width;
            Require(action != null && action.focusable && action.worldBound.height / scale >= 45, "sized focus target");
            if (stage == 0)
            {
                var begin = scope.Q<Button>("begin");
                beginBounds = begin.worldBound;
                Require(action.worldBound.yMax <= beginBounds.yMin + 1, "Home preference overlaps Begin");
                Require(!beginBounds.Overlaps(view.Root.Q("shell").worldBound),
                    "Begin overlaps dock: " + beginBounds + " / " + view.Root.Q("shell").worldBound);
                Require(action.text == ((index & 1) == 0 ? "Off" : "關閉"), "initial language / off state");
                view.VoicePreferenceChanged(true);
            }
            if (stage == 1)
            {
                Require(action.text == ((index & 1) == 0 ? "On" : "開啟"), "enabled copy");
                Require(Vector2.Distance(beginBounds.position, scope.Q<Button>("begin").worldBound.position) < .75f, "preference moved Begin");
                view.OpenOverlay("Settings", (AppLanguage)(index & 1), 0);
                view.PreviewTick(1);
            }
            if (stage++ < 2)
            {
                due = EditorApplication.timeSinceStartup + .12;
                return;
            }
            if (++index < 8) { Show(); return; }
            rows.Add("PASS: Home and Settings, both locales, 1920×1080 / 1280×720 and reduced motion/opaque; accessible target, localized states and stable Begin.");
            End(null);
        }
        catch (Exception ex) { End(ex); }
    }

    private static void End(Exception error)
    {
        EditorApplication.update -= Tick;
        if (window != null) window.Close();
        window = null;
        view = null;
        assets = null;
        if (error != null) rows.Add("FAIL specimen " + index + "/" + stage + ": " + error);
        File.WriteAllLines(Output,rows);
    }
    private static void Require(bool condition,string name)
    {
        if (!condition) throw new InvalidOperationException(name);
    }
}
#endif
