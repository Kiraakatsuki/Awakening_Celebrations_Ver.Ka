#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AwakeningCelebrations.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

/// <summary>One-time, guarded parallel scene authoring. Never overwrites SampleScene or build settings.</summary>
public static class LivingGlassSceneBuilder
{
    public const string ScenePath = "Assets/Scenes/AwakeningLivingGlass.unity";
    public const string AssetsPath = "Assets/LivingGlass/Assets/LivingGlassAssets.asset";
    public const string PanelPath = "Assets/LivingGlass/UI/LivingGlassPanel.asset";

    [MenuItem("Awakening Celebrations/Living Glass/Create parallel scene")]
    public static void Create()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Scene authoring requires Edit Mode.");
        if (SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Save your current scene before authoring. No user scene is saved automatically.");
        if (File.Exists(ScenePath)) throw new InvalidOperationException("Parallel scene already exists. Refusing to overwrite it.");
        LivingGlassAssets data = CreateAssets();
        PanelSettings panel = CreatePanel(data);
        AssetDatabase.SaveAssets();
        if (!AssetDatabase.CopyAsset("Assets/Scenes/SampleScene.unity", ScenePath)) throw new IOException("Could not create the parallel scene copy.");
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        // Scene replacement can unload newly authored assets; reload the persisted references.
        data = AssetDatabase.LoadAssetAtPath<LivingGlassAssets>(AssetsPath);
        panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelPath);
        var controllers = UnityEngine.Object.FindObjectsByType<AwakeningApplicationController>(FindObjectsInactive.Include);
        if (controllers.Length != 1) throw new InvalidOperationException("Expected exactly one source flow authority.");
        AwakeningApplicationController controller = controllers[0];
        // Only new-scene objects are removed. Original assets and original scene stay intact.
        foreach (UIDocument oldDocument in UnityEngine.Object.FindObjectsByType<UIDocument>(FindObjectsInactive.Include))
            UnityEngine.Object.DestroyImmediate(oldDocument.gameObject);
        HomeLanguageController legacyHome = controller.GetComponent<HomeLanguageController>();
        if (legacyHome != null) UnityEngine.Object.DestroyImmediate(legacyHome);
        ScreenDocumentManager legacyDocuments = controller.GetComponent<ScreenDocumentManager>();
        if (legacyDocuments != null) UnityEngine.Object.DestroyImmediate(legacyDocuments);
        controller.gameObject.name = "LivingGlassApplication";
        var documentObject = new GameObject("LivingGlassUI");
        UIDocument document = documentObject.AddComponent<UIDocument>();
        document.panelSettings = panel; document.sortingOrder = 10;
        LivingGlassPresentation presentation = controller.gameObject.AddComponent<LivingGlassPresentation>();
        var view = new SerializedObject(presentation);
        view.FindProperty("document").objectReferenceValue = document;
        view.FindProperty("assets").objectReferenceValue = data;
        view.ApplyModifiedPropertiesWithoutUndo();
        var state = new SerializedObject(controller);
        state.FindProperty("livingGlass").objectReferenceValue = presentation;
        state.FindProperty("navigationDocument").objectReferenceValue = document;
        state.FindProperty("screenDocumentManager").objectReferenceValue = null;
        state.FindProperty("homeController").objectReferenceValue = null;
        state.FindProperty("screenCatalog").objectReferenceValue = null;
        state.ApplyModifiedPropertiesWithoutUndo();
        var camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
        if (camera != null) { camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = LivingGlassAssets.Hex("#0C1117"); }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Living Glass parallel scene created; original scene and build entry preserved. Runtime validation is pending.");
    }

    private static LivingGlassAssets CreateAssets()
    {
        LivingGlassAssets data = AssetDatabase.LoadAssetAtPath<LivingGlassAssets>(AssetsPath);
        if (data != null) return data;
        data = ScriptableObject.CreateInstance<LivingGlassAssets>();
        string[] folders = { "PER_GAIA", "PER_PANDEMIC", "PER_RE-BIRTH", "PER_WHITE", "PER_CONSCIENCE", "PER_SANCTUARY" };
        for (int i=0;i<6;i++)
        {
            string folder = "Assets/Art/Figma/" + folders[i] + "/";
            data.portraits[i] = AssetDatabase.LoadAssetAtPath<Texture2D>(folder + "Approved Master Image.png");
            data.activation[i] = AssetDatabase.LoadAssetAtPath<Texture2D>(folder + "Master Image — Activation.png");
            data.living[i] = AssetDatabase.LoadAssetAtPath<Texture2D>(folder + "Living Master Image.png");
            if (data.portraits[i] == null || data.activation[i] == null || data.living[i] == null) throw new InvalidOperationException("Missing source art: " + folders[i]);
        }
        data.bodyEnglish = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/SourceSans3-Regular.ttf");
        data.displayEnglish = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/SourceSerif4-Regular.ttf");
        data.traditionalChinese = AssetDatabase.LoadAssetAtPath<Font>("Assets/LivingGlass/Assets/Fonts/NotoSansCJKtc-Regular.otf");
        data.styles = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/LivingGlass/UI/Styles/LivingGlass.uss");
        if (data.traditionalChinese == null || data.styles == null) throw new InvalidOperationException("Import fonts and styles before scene authoring.");
        AssetDatabase.CreateAsset(data, AssetsPath);
        return data;
    }

    private static PanelSettings CreatePanel(LivingGlassAssets data)
    {
        PanelSettings panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelPath);
        if (panel != null) return panel;
        panel = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<PanelSettings>("Assets/UI Toolkit/PanelSettings.asset"));
        panel.name = "LivingGlassPanel";
        panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        panel.referenceResolution = new Vector2Int(1920,1080);
        panel.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
        panel.match = .5f;
        var text = ScriptableObject.CreateInstance<PanelTextSettings>();
        text.name = "LivingGlassTextSettings";
        var fallback = FontAsset.CreateFontAsset(data.traditionalChinese);
        fallback.name = "LivingGlassTC Dynamic";
        var english = FontAsset.CreateFontAsset(data.bodyEnglish);
        english.name = "LivingGlassEnglish Dynamic";
        text.defaultFontAsset = english;
        text.fallbackFontAssets = new List<FontAsset> { fallback };
        panel.textSettings = text;
        AssetDatabase.CreateAsset(panel, PanelPath);
        AssetDatabase.AddObjectToAsset(text, panel);
        PersistFont(english, panel); PersistFont(fallback, panel);
        EditorUtility.SetDirty(panel);
        return panel;
    }
    private static void PersistFont(FontAsset font, UnityEngine.Object owner)
    {
        AssetDatabase.AddObjectToAsset(font, owner);
        if (font.material != null) AssetDatabase.AddObjectToAsset(font.material, owner);
        foreach (Texture2D texture in font.atlasTextures) if (texture != null) AssetDatabase.AddObjectToAsset(texture, owner);
    }

    [MenuItem("Awakening Celebrations/Living Glass/Validate serialized scene (no runtime)")]
    public static void Validate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Static validation requires Edit Mode.");
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath) throw new InvalidOperationException("Open the parallel scene before validation.");
        var findings = new List<string>();
        var controllers = UnityEngine.Object.FindObjectsByType<AwakeningApplicationController>(FindObjectsInactive.Include);
        var bridges = UnityEngine.Object.FindObjectsByType<MediaPipeGestureBridge>(FindObjectsInactive.Include);
        var docs = UnityEngine.Object.FindObjectsByType<UIDocument>(FindObjectsInactive.Include);
        if (controllers.Length != 1 || bridges.Length != 1 || docs.Length != 1) findings.Add("FAIL: duplicate/missing flow, bridge or UI owner.");
        if (controllers.Length == 1)
        {
            var so = new SerializedObject(controllers[0]);
            foreach (string key in new[] { "livingGlass", "gestureBridge", "navigationDocument" }) if (so.FindProperty(key).objectReferenceValue == null) findings.Add("FAIL: missing " + key);
            var clips = so.FindProperty("performerPortraitVideos");
            if (clips.arraySize != 6) findings.Add("FAIL: expected six media slots.");
            for (int i=0;i<clips.arraySize;i++) findings.Add("INFO: " + LivingGlassRoutes.Performers[i] + " clip=" + AssetDatabase.GetAssetPath(clips.GetArrayElementAtIndex(i).objectReferenceValue));
        }
        var catalog = Resources.Load<RuntimeScreenCatalog>("RuntimeScreenCatalog");
        int records = 0;
        foreach (var entry in catalog.Entries)
        {
            records++;
            if (!LivingGlassRoutes.Supports(entry.screenId)) findings.Add("FAIL: unsupported " + entry.screenId);
        }
        findings.Add("INFO: catalog records=" + records);
        foreach (GameObject go in scene.GetRootGameObjects())
            foreach (Transform child in go.GetComponentsInChildren<Transform>(true))
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) != 0) findings.Add("FAIL: missing script on " + child.name);
        if (docs.Length == 1 && (docs[0].panelSettings == null || docs[0].panelSettings.textSettings == null)) findings.Add("FAIL: missing panel/font configuration.");
        string report = string.Join("\n", findings);
        Directory.CreateDirectory("Documentation/Stage3LivingGlass");
        File.WriteAllText("Documentation/Stage3LivingGlass/SerializedValidation.txt", report);
        if (findings.Any(f => f.StartsWith("FAIL", StringComparison.Ordinal))) throw new InvalidOperationException(report);
        Debug.Log("PASS: static scene ownership, catalog coverage and required serialized references. Runtime NOT RUN.\n" + report);
    }
}
#endif
