#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>Static Editor-only view inspection. No application, recognizer, player or transport is initialized.</summary>
public sealed partial class LivingGlassPreviewWindow : EditorWindow
{
    private LivingGlassPresentation view;
    private GameObject owner;
    private VisualElement canvas;
    private LivingGlassAssets previewData;
    // Typed access for isolated Editor checks; neither property initializes services.
    internal LivingGlassPresentation Presentation { get { return view; } }
    internal LivingGlassAssets PreviewAssets { get { return previewData; } }
    private double lastTick, due;
    private Queue<Shot> queue;
    private Shot current;
    private readonly List<string> geometry = new List<string>();
    private readonly List<string> geometryChecks = new List<string>();
    private Rect[] neutralHitBounds;
    private sealed class Shot
    {
        public string name, route, overlay;
        public AppLanguage language;
        public int performer = -1, target = -1;
        public bool reduced, opaque;
    }
    [MenuItem("Awakening Celebrations/Living Glass/Static visual review")]
    public static LivingGlassPreviewWindow Open()
    {
        var window = CreateInstance<LivingGlassPreviewWindow>();
        window.titleContent = new GUIContent("Living Glass · STATIC EDITOR PREVIEW");
        window.position = new Rect(0,0,1920,1080);
        window.ShowPopup();
        window.Setup();
        window.ShowShot(new Shot { name = "Gallery-en", route = "COLOR-RELEASE", language = AppLanguage.English });
        return window;
    }
    private void Setup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Preview requires Edit Mode.");
        owner = new GameObject("LivingGlassStaticPreview") { hideFlags = HideFlags.HideAndDontSave };
        view = owner.AddComponent<LivingGlassPresentation>();
        var data = AssetDatabase.LoadAssetAtPath<LivingGlassAssets>(LivingGlassProjectValidation.AssetsPath);
        previewData = Instantiate(data); previewData.hideFlags = HideFlags.HideAndDontSave;
        canvas = new VisualElement { name = "design-viewport" };
        canvas.style.position = Position.Absolute;
        canvas.style.width = 1920; canvas.style.height = 1080;
        canvas.style.transformOrigin = new TransformOrigin(0,0,0);
        rootVisualElement.Add(canvas);
        rootVisualElement.style.backgroundColor = LivingGlassAssets.Hex("#080B10");
        rootVisualElement.RegisterCallback<GeometryChangedEvent>(_ => FitCanvas());
        view.InitializePreview(canvas, previewData);
        FitCanvas();
        lastTick = EditorApplication.timeSinceStartup;
        EditorApplication.update += Tick;
    }
    private void ShowShot(Shot shot)
    {
        current = shot;
        previewData.reducedMotion = shot.reduced; previewData.reducedEffects = shot.opaque;
        view.Render(shot.route, shot.language, shot.performer, false, true);
        if (shot.target >= 0) view.TargetChanged(shot.target);
        if (shot.overlay != null) view.OpenOverlay(shot.overlay, shot.language, Math.Max(0, shot.performer));
        due = EditorApplication.timeSinceStartup + 1.1;
        Repaint();
    }
    private void FitCanvas()
    {
        float width = rootVisualElement.resolvedStyle.width, height = rootVisualElement.resolvedStyle.height;
        float reserved = voiceReviewToolbar == null ? 0 : 64;
        float scale = Mathf.Max(0, Mathf.Min(width/1920, (height-reserved)/1080));
        canvas.style.scale = new Scale(new Vector3(scale,scale,1));
        canvas.style.left = (width-1920*scale)*.5f;
        canvas.style.top = reserved + (height-reserved-1080*scale)*.5f;
    }
    public static string CapturePackage()
    {
        var window = Open(); window.queue = new Queue<Shot>();
        string[] routes = { "Home", "Privacy", "TUTORIAL---POINT", "ATTRACT---MONOCHROME---FALLBACK-INPUT", "COLOR-RELEASE", "CLOSING---CELEBRATION", "SOUVENIR---QR-DELIVERY" };
        foreach (AppLanguage locale in new[] { AppLanguage.English, AppLanguage.Chinese })
        {
            string suffix = locale == AppLanguage.English ? "en" : "zh";
            foreach (string route in routes) window.queue.Enqueue(new Shot { name = LivingGlassRoutes.Family(route) + "-" + suffix, route = route, language = locale });
            foreach (string overlay in new[] { "Settings", "AssistantChooser", "Assistant-GAIA", "Help", "Privacy" })
                window.queue.Enqueue(new Shot { name = "Window-" + overlay + "-" + suffix, route = "COLOR-RELEASE", overlay = overlay, language = locale, performer = 0, target = 0 });
            for (int s=0;s<5;s++) window.queue.Enqueue(new Shot { name = "GAIA-" + LivingGlassRoutes.Stages[s] + "-" + suffix, route = LivingGlassRoutes.Chapter(0,s), performer = 0, language = locale });
        }
        for (int p=0;p<6;p++) window.queue.Enqueue(new Shot { name = "Theme-" + LivingGlassRoutes.Performers[p], route = "COLOR-RELEASE", target = p, language = AppLanguage.English });
        foreach (AppLanguage locale in new[] { AppLanguage.English, AppLanguage.Chinese })
            window.queue.Enqueue(new Shot { name = "Reduced-Gallery-" + (locale == AppLanguage.English ? "en" : "zh"), route = "COLOR-RELEASE", target = 0, language = locale, reduced = true, opaque = true });
        window.ShowShot(window.queue.Dequeue());
        return "Queued 42 static Editor preview captures. No runtime services are initialized.";
    }
    private void Tick()
    {
        if (view == null) return;
        double now = EditorApplication.timeSinceStartup;
        view.PreviewTick((float)Math.Min(.05, now-lastTick)); lastTick = now;
        if (queue == null || now < due) return;
        Capture();
        if (queue.Count > 0) ShowShot(queue.Dequeue());
        else
        {
            File.WriteAllLines("Documentation/Stage3LivingGlass/EditorPreviewGeometry.csv", geometry);
            File.WriteAllLines("Documentation/Stage3LivingGlass/EditorGeometryChecks.txt", geometryChecks);
            queue = null;
            Debug.Log("Living Glass static Editor preview captures finished. These do not verify runtime behavior.");
            Close();
        }
    }
    private void Capture()
    {
        string folder = "Documentation/Stage3LivingGlass/EditorPreviews/Final";
        Directory.CreateDirectory(folder);
        int width = Mathf.RoundToInt(position.width), height = Mathf.RoundToInt(position.height);
        Color[] pixels = UnityEditorInternal.InternalEditorUtility.ReadScreenPixel(position.position, width, height);
        var texture = new Texture2D(width,height,TextureFormat.RGB24,false);
        texture.SetPixels(pixels); texture.Apply();
        File.WriteAllBytes(folder + "/" + current.name + ".png", texture.EncodeToPNG());
        DestroyImmediate(texture);
        if (geometry.Count == 0) geometry.Add("preview,element,x,y,width,height,enabled");
        foreach (string name in new[] { "content", "gallery-arc", "identification", "shell", "glass-window", "camera" })
        {
            VisualElement element = rootVisualElement.Q(name);
            if (element != null) { Rect b = element.worldBound; geometry.Add(current.name+","+name+","+b.x+","+b.y+","+b.width+","+b.height+","+element.enabledInHierarchy); }
        }
        if (current.route == "COLOR-RELEASE" && current.overlay == null)
        {
            var bounds = new Rect[6]; bool valid = true, stable = true;
            for (int i=0;i<6;i++)
            {
                bounds[i] = rootVisualElement.Q("presence-" + i).worldBound;
                Rect name = rootVisualElement.Q("performer-name-" + i).worldBound;
                valid &= name.yMin >= bounds[i].yMin-.5f && name.yMax <= bounds[i].yMax+.5f;
                if (i>0) valid &= bounds[i-1].xMax <= bounds[i].xMin;
                if (neutralHitBounds != null) stable &= Vector2.Distance(bounds[i].position, neutralHitBounds[i].position) < .01f && Vector2.Distance(bounds[i].size, neutralHitBounds[i].size) < .01f;
            }
            if (current.name == "Gallery-en") neutralHitBounds = bounds;
            geometryChecks.Add((valid && stable ? "PASS" : "FAIL") + ": " + current.name + " / six distinct stable wrappers; names fit vertical bounds. Static Editor layout only.");
        }
    }
    private void OnDisable()
    {
        AssemblyReloadEvents.beforeAssemblyReload -= Close;
        EditorApplication.update -= Tick;
        view?.Shutdown();
        if (owner != null) DestroyImmediate(owner);
        if (previewData != null) DestroyImmediate(previewData);
    }
}
#endif
