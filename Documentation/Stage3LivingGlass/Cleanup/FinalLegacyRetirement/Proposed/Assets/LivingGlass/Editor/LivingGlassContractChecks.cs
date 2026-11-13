#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Inspected pure checks: no scene initialization, capture, network, video, Play Mode or test-runner side effects.</summary>
public static class LivingGlassContractChecks
{
    [MenuItem("Awakening Celebrations/Living Glass/Run pure contract checks")]
    public static string Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Checks require Edit Mode.");
        var results = new List<string>();
        var locales = ReadValidatedRoutes();
        results.Add("PASS: independent104 locale records /52 Living Glass routes match the preserved route contract.");
        for (int p=0;p<6;p++) for (int s=0;s<5;s++)
        {
            string route = LivingGlassRoutes.Chapter(p,s);
            Check(locales.ContainsKey(route), "chapter present in authoritative catalog " + route);
            Check(LivingGlassRoutes.TryChapter(route, out int rp, out int rs) && rp == p && rs == s, "chapter roundtrip");
            Check(LivingGlassRoutes.Protected(route) == (s == 1 || s == 2), "protected chapter classification");
        }
        Check(!LivingGlassRoutes.Supports("GAIA---INVALID---PER-001"), "unknown routes rejected");
        Check(!LivingGlassRoutes.Supports("Assistant-NOT-A-PERFORMER"), "unknown assistant rejected");
        results.Add("PASS: all30 performer chapters map exactly; only Activation/Portrait are protected families; invalid routes rejected.");
        var motion = new LivingGlassMotion();
        var key = new object(); float position = 0; int oldCompleted = 0, newCompleted = 0;
        motion.Run(key, 1, t => position = t, () => oldCompleted++);
        motion.Tick(.3f); float interrupted = position;
        motion.Run(key, .3f, t => position = Mathf.Lerp(interrupted, 0, t), () => newCompleted++);
        Check(Mathf.Approximately(position, interrupted), "retarget starts at current pose");
        motion.Tick(.3f);
        Check(position == 0 && oldCompleted == 0 && newCompleted == 1 && motion.ActiveCount == 0, "interruption completes only replacement");
        results.Add("PASS: interrupting a motion preserves its current pose and suppresses stale completion.");
        motion.Run(key, 1, t => position = t, () => oldCompleted++);
        motion.Clear(); motion.Tick(2);
        Check(oldCompleted == 0 && motion.ActiveCount == 0, "cancelled work cannot complete");
        motion.Reduced = true; motion.Run(key, 1, t => position = t, () => newCompleted++);
        Check(position == 1 && newCompleted == 2 && motion.ActiveCount == 0, "reduced mode immediate state");
        results.Add("PASS: clearing cancels visual completions; reduced motion applies the same final pose immediately.");
        Rect uv = new Rect(0,0,1,1);
        Rect crop = LivingGlassMotion.VisibleCrop(uv, new Rect(10,10,200,200), new Rect(60,60,100,100));
        Check(crop == new Rect(.25f,.25f,.5f,.5f), "visible crop preserves artwork aperture");
        Rect scaled = LivingGlassMotion.VisibleCrop(uv, new Rect(30,40,400,400), new Rect(130,140,200,200));
        Check(scaled == crop, "common viewport scale/offset preserves crop");
        Check(LivingGlassMotion.VisibleCrop(uv, Rect.zero, Rect.zero) == uv, "unresolved geometry is finite");
        results.Add("PASS: artwork proxy crop preserves aperture content, is invariant to shared scale/offset and handles unresolved geometry.");
        string report = string.Join("\n", results) + "\nRuntime, hardware, media playback and service calls NOT RUN.";
        Directory.CreateDirectory("Documentation/Stage3LivingGlass");
        File.WriteAllText("Documentation/Stage3LivingGlass/PureContractChecks.txt", report);
        Debug.Log(report); return report;
    }
    [Serializable] private sealed class RouteFixture { public RoutePair[] entries; }
    [Serializable] private sealed class RoutePair { public string route; public string locale; }
    public static void ValidateRouteContract() { ReadValidatedRoutes(); }
    private static Dictionary<string, HashSet<string>> ReadValidatedRoutes()
    {
        var asset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/LivingGlass/Editor/RouteContract.json");
        Check(asset != null, "route contract fixture exists");
        var fixture = JsonUtility.FromJson<RouteFixture>(asset.text);
        Check(fixture != null && fixture.entries != null && fixture.entries.Length ==104, "104 preserved locale records");
        var locales = new Dictionary<string, HashSet<string>>();
        foreach (var entry in fixture.entries)
        {
            Check(LivingGlassRoutes.Supports(entry.route), "supported preserved route " + entry.route);
            if (!locales.ContainsKey(entry.route)) locales.Add(entry.route, new HashSet<string>());
            Check(locales[entry.route].Add(entry.locale), "unique locale pair " + entry.route);
        }
        Check(locales.Count ==52 && new HashSet<string>(LivingGlassRoutes.AllRoutes).SetEquals(locales.Keys), "complete independent route set");
        foreach (var pair in locales) Check(pair.Value.SetEquals(new[] { "en", "zh" }), "locale parity " + pair.Key);
        return locales;
    }
    private static void Check(bool condition, string contract)
    { if (!condition) throw new InvalidOperationException("FAIL: " + contract); }
}
#endif
