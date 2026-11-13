#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AwakeningCelebrations.Interaction;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>Read-only Living Glass configuration validation; no scene authoring or service startup.</summary>
public static class LivingGlassProjectValidation
{
    public const string ScenePath = "Assets/Scenes/AwakeningLivingGlass.unity";
    public const string AssetsPath = "Assets/LivingGlass/Assets/LivingGlassAssets.asset";
    public const string PanelPath = "Assets/LivingGlass/UI/LivingGlassPanel.asset";

    public static void ValidateConfiguration()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode required.");
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null) throw new InvalidOperationException("Primary Living Glass scene missing.");
        var entries = EditorBuildSettings.scenes;
        if (entries.Length !=1 || !entries[0].enabled || entries[0].path !=ScenePath) throw new InvalidOperationException("Living Glass must be the sole development entry.");
        foreach (Match match in Regex.Matches(File.ReadAllText(ScenePath), @"m_Script: \{fileID: \d+, guid: ([a-f0-9]{32})"))
            if (string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(match.Groups[1].Value))) throw new InvalidOperationException("Missing scene script " + match.Groups[1].Value);
        var assets = AssetDatabase.LoadAssetAtPath<LivingGlassAssets>(AssetsPath);
        if (assets == null || assets.portraits.Length !=6 || assets.activation.Length !=6 || assets.living.Length !=6
            || assets.portraits.Any(a=>a==null) || assets.activation.Any(a=>a==null) || assets.living.Any(a=>a==null)
            || assets.bodyEnglish ==null || assets.displayEnglish ==null || assets.traditionalChinese ==null || assets.styles ==null)
            throw new InvalidOperationException("Performer art/font/style configuration incomplete.");
        var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelPath);
        if (panel ==null || panel.textSettings ==null) throw new InvalidOperationException("Panel/text settings incomplete.");
        LivingGlassContractChecks.ValidateRouteContract();
    }

    [MenuItem("Awakening Celebrations/Living Glass/Validate primary scene (no runtime)")]
    public static void Validate()
    {
        ValidateConfiguration();
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path !=ScenePath) throw new InvalidOperationException("Open the primary scene for serialized object inspection.");
        var controllers = UnityEngine.Object.FindObjectsByType<AwakeningApplicationController>(FindObjectsInactive.Include);
        var bridges = UnityEngine.Object.FindObjectsByType<MediaPipeGestureBridge>(FindObjectsInactive.Include);
        var docs = UnityEngine.Object.FindObjectsByType<UIDocument>(FindObjectsInactive.Include);
        if (controllers.Length !=1 || bridges.Length !=1 || docs.Length !=1) throw new InvalidOperationException("Expected one flow/bridge/document owner.");
        var so = new SerializedObject(controllers[0]);
        foreach (string name in new[] { "livingGlass", "gestureBridge", "navigationDocument" })
            if (so.FindProperty(name).objectReferenceValue ==null) throw new InvalidOperationException("Missing " + name);
        var clips = so.FindProperty("performerPortraitVideos");
        if (clips.arraySize !=6) throw new InvalidOperationException("Expected six video slots.");
        var findings = new List<string>();
        for (int i=0;i<6;i++)
        {
            if (clips.GetArrayElementAtIndex(i).objectReferenceValue ==null) throw new InvalidOperationException("Missing video " + i);
            findings.Add("PASS: " + LivingGlassRoutes.Performers[i] + " clip=" + AssetDatabase.GetAssetPath(clips.GetArrayElementAtIndex(i).objectReferenceValue));
        }
        foreach (var go in scene.GetRootGameObjects()) foreach (var t in go.GetComponentsInChildren<Transform>(true))
            if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) !=0) throw new InvalidOperationException("Missing script on " + t.name);
        findings.Add("PASS: primary entry, one service/view rig, panel,52 bilingual routes and all-six media references. Runtime NOT RUN.");
        Directory.CreateDirectory("Documentation/Stage3LivingGlass");
        File.WriteAllLines("Documentation/Stage3LivingGlass/SerializedValidation.txt", findings);
        Debug.Log(string.Join("\n", findings));
    }
}

public sealed class LivingGlassBuildValidator : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildReport report)
    {
        try { LivingGlassProjectValidation.ValidateConfiguration(); }
        catch (Exception error) { throw new BuildFailedException("Living Glass configuration: " + error.Message); }
    }
}
#endif
