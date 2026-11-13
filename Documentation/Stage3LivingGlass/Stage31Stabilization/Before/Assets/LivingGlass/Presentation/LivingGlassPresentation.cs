using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>Living Glass view composition. Business commands belong to the flow port.</summary>
[DisallowMultipleComponent]
public sealed class LivingGlassPresentation : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private LivingGlassAssets assets;
    private AwakeningApplicationController flow;
    private VisualElement root, content, header, dock, atmosphere, overlayPlane, panel, body, resume;
    private VisualElement opener;
    private VisualElement bridgeProxy, bridgeGallery;
    private readonly LivingGlassMotion motion = new LivingGlassMotion();
    private readonly object themeKey = new object();
    private readonly VisualElement[] artSurfaces = new VisualElement[6];
    private readonly Image[] artImages = new Image[6];
    private readonly float[] pose = new float[6];
    private readonly Vector2[] pointerPose = new Vector2[6];
    private Label identity, identityDetail, identityIndex, cameraStatus, portraitStatus, announcement;
    private Button confirm;
    private Image preview;
    private AppLanguage language;
    private string route, overlayKind;
    private int selected = -1, target = -1;
    private bool ready, consent, closing;
    private bool keyboardGalleryOwner;
    private int viewGeneration;
    private float panelProgress;
    private Vector2 panelOrigin;
    private Color accent = LivingGlassAssets.Neutral;
    private LivingGlassAtmosphere softLight;
    public UIDocument Document { get { return document; } }
    public VisualElement Content { get { return content; } }
    public VisualElement Root { get { return root; } }
    public VisualElement[] PerformerTargets { get; private set; }
    public bool OverlayVisible { get { return overlayPlane != null; } }
    public bool IsInitialized { get { return root != null; } }
    public bool ReducedMotion { get { return assets != null && assets.reducedMotion; } }
    private string T(string en, string zh) { return language == AppLanguage.Chinese ? zh : en; }

    public void Initialize(AwakeningApplicationController owner)
    {
        flow = owner;
        InitializeRoot(document.rootVisualElement);
    }
    // Editor-only static preview entry: no flow initialization, capture, video or HTTP calls.
    public void InitializePreview(VisualElement previewRoot, LivingGlassAssets data)
    {
        assets = data;
        InitializeRoot(previewRoot);
    }
    private void InitializeRoot(VisualElement host)
    {
        root = host;
        root.Clear();
        root.AddToClassList("lg-app");
        if (assets != null && assets.styles != null) root.styleSheets.Add(assets.styles);
        root.RegisterCallback<KeyDownEvent>(OnKey, TrickleDown.TrickleDown);
        root.RegisterCallback<GeometryChangedEvent>(evt => root.EnableInClassList("lg-compact", evt.newRect.width < 1800));
        motion.Reduced = ReducedMotion;
    }
    public void Shutdown()
    {
        motion.Clear();
        if (root != null) root.UnregisterCallback<KeyDownEvent>(OnKey, TrickleDown.TrickleDown);
        if (preview != null) preview.image = null;
    }
    private void Update() { motion.Tick(Time.unscaledDeltaTime); }
    public void PreviewTick(float delta) { motion.Tick(delta); }

    private static VisualElement Box(VisualElement parent, string name, string classes)
    {
        var box = new VisualElement { name = name };
        foreach (string c in classes.Split(' ')) if (c.Length > 0) box.AddToClassList(c);
        parent.Add(box); return box;
    }
    private Label Text(VisualElement parent, string value, string cls = "lg-body")
    {
        var label = new Label(value) { enableRichText = false, pickingMode = PickingMode.Ignore };
        if (cls == "lg-display" && language == AppLanguage.English && assets.displayEnglish != null)
            label.style.unityFontDefinition = FontDefinition.FromFont(assets.displayEnglish);
        label.AddToClassList(cls); parent.Add(label); return label;
    }
    private Button Action(VisualElement parent, string key, string text, System.Action command, string variant = "")
    {
        var b = new Button(() => command?.Invoke()) { name = key, text = text };
        b.AddToClassList("lg-action"); if (variant.Length > 0) b.AddToClassList(variant);
        parent.Add(b); return b;
    }
    private Image Art(VisualElement parent, int performer, string cls = "lg-art", Texture2D replacement = null)
    {
        var image = new Image { name = "art-" + performer, scaleMode = ScaleMode.StretchToFill, pickingMode = PickingMode.Ignore };
        if (assets != null && performer >= 0 && performer < assets.portraits.Length) image.image = replacement != null ? replacement : assets.portraits[performer];
        image.RegisterCallback<GeometryChangedEvent>(evt =>
        {
            Texture texture = image.image;
            if (texture == null || evt.newRect.width <= 0 || evt.newRect.height <= 0) return;
            float sourceAspect = (float)texture.width / texture.height;
            float apertureAspect = evt.newRect.width / evt.newRect.height;
            float u = Mathf.Min(1, apertureAspect/sourceAspect), v = Mathf.Min(1, sourceAspect/apertureAspect);
            image.uv = new Rect((1-u)*(performer == 0 ? 1 : .5f), (1-v)*.68f, u, v);
        });
        image.AddToClassList(cls); parent.Add(image); return image;
    }
    public void ClearRoute()
    {
        ClearBridge();
        motion.Clear();
        CloseOverlay(true);
        motion.Clear();
        if (preview != null) preview.image = null;
        root.Clear();
        content = null;
        PerformerTargets = null;
        Array.Clear(artSurfaces, 0, 6); Array.Clear(artImages, 0, 6); Array.Clear(pose, 0, 6); Array.Clear(pointerPose, 0, 6);
        identity = identityDetail = identityIndex = null; confirm = null; portraitStatus = null;
    }
    public void Render(string screen, AppLanguage locale, int performer, bool accepted, bool selectionReady)
    {
        if (root == null) return;
        motion.Reduced = ReducedMotion;
        int generation = ++viewGeneration;
        bool galleryBridge = route == "COLOR-RELEASE" && LivingGlassRoutes.TryChapter(screen, out _, out int nextStage) && nextStage == 0;
        VisualElement oldGallery = galleryBridge && !ReducedMotion ? content : null;
        Rect oldBounds = default;
        Rect oldUv = new Rect(0,0,1,1);
        Texture bridgeTexture = null;
        if (oldGallery != null && performer >= 0 && artSurfaces[performer] != null)
        {
            oldBounds = artSurfaces[performer].worldBound; bridgeTexture = artImages[performer].image;
            Rect imageBounds = artImages[performer].worldBound, uv = artImages[performer].uv;
            oldUv = LivingGlassMotion.VisibleCrop(uv, imageBounds, oldBounds);
            artSurfaces[performer].style.opacity = 0;
            oldGallery.RemoveFromHierarchy();
        }
        ClearRoute();
        route = screen; language = locale; selected = performer; ready = selectionReady; consent = accepted; target = -1;
        keyboardGalleryOwner = false;
        root.style.unityFontDefinition = FontDefinition.FromFont(language == AppLanguage.Chinese ? assets.traditionalChinese : assets.bodyEnglish);
        root.EnableInClassList("lg-zh", language == AppLanguage.Chinese);
        root.EnableInClassList("lg-reduced", ReducedMotion);
        root.EnableInClassList("lg-opaque", assets.reducedEffects);
        root.EnableInClassList("lg-protected", LivingGlassRoutes.Protected(route));
        softLight = new LivingGlassAtmosphere { name = "atmosphere" };
        softLight.AddToClassList("lg-atmosphere"); root.Add(softLight); atmosphere = softLight;
        content = Box(root, "content", "lg-content");
        BuildHeader();
        string family = LivingGlassRoutes.Family(screen);
        switch (family)
        {
            case "Home": BuildHome(); break;
            case "Privacy": BuildPrivacy(content, true); break;
            case "Tutorial": BuildTutorial(); break;
            case "Attract": BuildAttract(); break;
            case "Gallery": BuildGallery(); break;
            case "Settings": body = Reading(content, T("Experience controls", "體驗設定")); BuildSettings(body); break;
            case "AssistantChooser": body = Reading(content, T("Choose a conversation", "選擇對話主題")); BuildChooser(body); break;
            case "Assistant": body = Reading(content, T("Ask a presence", "與存在對話")); BuildChat(body, Mathf.Max(0, Array.IndexOf(LivingGlassRoutes.Performers, route.Substring(10)))); break;
            case "Closing": BuildClosing(); break;
            case "Souvenir": BuildSouvenir(); break;
            default:
                if (LivingGlassRoutes.TryChapter(screen, out int p, out int stage)) BuildChapter(p, stage);
                else Text(content, T("This experience is unavailable.", "此體驗目前無法使用。"));
                break;
        }
        BuildDock();
        BuildCamera();
        announcement = Text(root, "", "lg-announcement");
        if (performer >= 0) ApplyTheme(performer, true);
        if (oldGallery != null && bridgeTexture != null)
            BuildBridge(oldGallery, oldBounds, bridgeTexture, oldUv);
        else if (!LivingGlassRoutes.Protected(route))
        {
            motion.Run(content, family == "Gallery" ? .52f : .3f, t => { content.style.opacity = t; LivingGlassMotion.Pose(content, new Vector2(0, (1-t)*8), 1); });
        }
        root.schedule.Execute(() =>
        {
            if (generation != viewGeneration || content == null || content.panel == null || OverlayVisible) return;
            if (route == "COLOR-RELEASE" && performer >= 0 && PerformerTargets != null) PerformerTargets[performer].Focus();
            else FirstFocusable(content)?.Focus();
        });
    }
    private void BuildHeader()
    {
        header = Box(root, "header", "lg-header");
        var brand = Box(header, "brand", "lg-brand");
        Text(brand, "AWAKENING CELEBRATIONS", "lg-wordmark");
        Text(brand, T("A LIVING GALLERY", "感知流動的藝廊"), "lg-eyebrow");
        Button locale = Action(header, "language", language == AppLanguage.English ? "English / 繁體中文" : "繁體中文 / English",
            () => flow?.LivingLanguage(language == AppLanguage.English ? AppLanguage.Chinese : AppLanguage.English), "lg-quiet");
        locale.style.unityFontDefinition = FontDefinition.FromFont(assets.traditionalChinese);
        locale.SetEnabled(!LivingGlassRoutes.Protected(route));
    }
    private void BuildDock()
    {
        dock = Box(root, "shell", "lg-dock");
        bool cinematic = LivingGlassRoutes.Protected(route);
        if (cinematic)
        {
            Text(dock, T("Settings and AI after this moment", "此段落結束後可開啟設定與 AI"), "lg-small");
            Action(dock, "privacy-stop", T("Stop camera", "停止相機"), () => { flow?.LivingConsentChoice(false); AnnounceCameraOff(); }, "lg-quiet");
            Button exit = Action(dock, "cinematic-back", T("Back", "返回"), () => flow?.Back(), "lg-quiet");
            exit.SetEnabled(LivingGlassRoutes.TryChapter(route, out _, out int s) && s == 2);
            exit.tooltip = T("Back is available during Living Portrait. Activation completes automatically.", "動態肖像播放時可返回；啟動段落會自動完成。");
        }
        else
        {
            Action(dock, "shell-settings", T("Settings", "設定"), () => flow?.LivingGlassOpenOverlay("Settings"), "lg-dock-action");
            Action(dock, "shell-ai", T("Ask AI", "詢問 AI"), () => flow?.LivingGlassOpenOverlay("AssistantChooser"), "lg-dock-action");
            Action(dock, "shell-help", T("Help", "說明"), () => flow?.LivingGlassOpenOverlay("Help"), "lg-dock-action");
            Action(dock, "shell-privacy", T("Privacy", "隱私"), () => flow?.LivingGlassOpenOverlay("Privacy"), "lg-dock-action");
            Button back = Action(dock, "shell-back", T("Back", "返回"), () => flow?.Back(), "lg-dock-action");
            back.SetEnabled(flow != null ? flow.LivingCanBack : route != "Home" && route != "COLOR-RELEASE" && LivingGlassRoutes.Family(route) != "Tutorial" && route != "Privacy");
        }
    }
    private void BuildCamera()
    {
        var status = Box(root, "camera", "lg-camera");
        preview = new Image { name = "camera-preview", scaleMode = ScaleMode.ScaleToFit, pickingMode = PickingMode.Ignore };
        preview.AddToClassList("lg-preview"); preview.style.display = DisplayStyle.None; status.Add(preview);
        cameraStatus = Text(status, T("Camera off", "相機關閉"), "lg-camera-status");
    }
    public void UpdateCamera(string status, Texture texture)
    {
        if (cameraStatus == null) return;
        if (cameraStatus.text != status) cameraStatus.text = status;
        preview.image = texture;
        preview.style.display = texture != null ? DisplayStyle.Flex : DisplayStyle.None;
        if (texture != null) preview.MarkDirtyRepaint();
    }
    public void StyleCursor()
    {
        VisualElement cursor = root?.Q("ac-gesture-cursor");
        if (cursor == null) return;
        cursor.AddToClassList("lg-pointer");
        cursor.style.backgroundColor = LivingGlassAssets.Hex("#0C1117");
        Color edge = LivingGlassAssets.Hex("#F3F0E8");
        cursor.style.borderTopColor = cursor.style.borderBottomColor = cursor.style.borderLeftColor = cursor.style.borderRightColor = edge;
        if (cursor.childCount > 0) cursor[0].style.backgroundColor = edge;
    }
    private VisualElement Standard(string eyebrow, string title, string description, int art = 5)
    {
        var layout = Box(content, "standard", "lg-standard");
        var copy = Box(layout, "copy", "lg-copy");
        Text(copy, eyebrow, "lg-eyebrow"); Text(copy, title, "lg-display"); Text(copy, description);
        Art(layout, art, "lg-standard-art");
        return copy;
    }
    private VisualElement Reading(VisualElement parent, string title)
    {
        var surface = Box(parent, "reading", "lg-reading");
        Text(surface, title, "lg-panel-title");
        var scroll = new ScrollView(ScrollViewMode.Vertical); scroll.AddToClassList("lg-reading-body"); surface.Add(scroll); return scroll;
    }
    private void BuildHome()
    {
        var copy = Standard(T("AN INVITATION TO FEEL", "邀請您，一同感知"), T("A presence.\nAn awakening.", "與存在相遇。\n讓感知甦醒。"),
            T("Enter six artistic worlds. Let attention become an encounter.", "走進六種藝術世界，讓專注化為一場相遇。"));
        var consentToggle = new Toggle { name = "consent", text = T("I agree to session-only camera tracking", "我同意於本次體驗使用相機追蹤") };
        consentToggle.AddToClassList("lg-consent"); consentToggle.SetValueWithoutNotify(consent);
        consentToggle.RegisterValueChangedCallback(evt => flow?.LivingConsentChoice(evt.newValue)); copy.Add(consentToggle);
        Text(copy, T("Camera starts only in eligible experiences. Preview does not grant permission.", "相機僅於適用體驗中啟動，預覽不代表授權。"), "lg-small");
        var actions = Box(copy, "actions", "lg-actions");
        Button begin = Action(actions, "begin", T("Review privacy & begin", "閱讀隱私並開始"), () => flow?.LivingBegin(), "lg-primary"); begin.SetEnabled(consent);
        Action(actions, "privacy-details", T("Read privacy", "閱讀隱私"), () => flow?.LivingGlassOpenOverlay("Privacy"), "lg-quiet");
    }
    public void ConsentChanged(bool value)
    {
        consent = value;
        root?.Q<Toggle>("consent")?.SetValueWithoutNotify(value);
        root?.Q<Toggle>("privacy-consent")?.SetValueWithoutNotify(value);
        root?.Q<Button>("begin")?.SetEnabled(value);
        root?.Q<Button>("privacy-continue")?.SetEnabled(value);
    }
    private void BuildPrivacy(VisualElement parent, bool full)
    {
        var reading = full ? Reading(parent, T("Your presence. Your choice.", "您的參與，由您選擇。")) : parent;
        Text(reading, T("Camera and gesture tracking", "相機與手勢追蹤"), "lg-subtitle");
        Text(reading, T("This experience uses the existing on-device hand-tracking pipeline. Permission lasts for this session and is never remembered as camera consent. You can stop capture at any time.", "本體驗使用既有的裝置端手部追蹤流程。相機許可僅限本次體驗，不會保存為下次授權；您可隨時停止影像擷取。"));
        Text(reading, T("Live preview uses the same camera source. Hiding a preview does not stop capture. Settings and Privacy suspend capture; returning resumes only with your session permission and an eligible route.", "即時預覽使用同一相機來源。隱藏預覽並不會停止擷取。設定與隱私畫面會暫停擷取；返回時，僅在本次已授權且適用的畫面恢復。"));
        Text(reading, T("AI questions are sent to the existing assistant service. Do not include personal or sensitive information. Souvenir delivery and QR generation are unavailable.", "AI 問題會傳送至既有助理服務，請勿輸入個人或敏感資訊。紀念品傳送與 QR 碼產生功能目前無法使用。"));
        Text(reading, T("No claim is made here about unverified backend retention. Ask the exhibition team for the operational privacy notice.", "此處不對尚未核實的後端資料保留作出承諾；請向展覽團隊索取正式營運隱私說明。"), "lg-small");
        var permission = new Toggle { name = "privacy-consent", text = T("Allow camera tracking during this session", "允許於本次體驗使用相機追蹤") };
        permission.AddToClassList("lg-consent"); permission.SetValueWithoutNotify(consent);
        permission.RegisterValueChangedCallback(evt => flow?.LivingConsentChoice(evt.newValue)); reading.Add(permission);
        Text(reading, T("Capture remains suspended here. It resumes only after leaving Privacy for an eligible experience with permission enabled.", "此處維持暫停擷取。僅在離開隱私畫面、進入適用體驗，且已啟用許可時才恢復。"), "lg-small");
        var actions = Box(reading, "privacy-actions", "lg-actions");
        if (full)
        {
            Button next = Action(actions, "privacy-continue", T("Continue", "繼續"), () => flow?.LivingContinue(), "lg-primary"); next.SetEnabled(consent);
            Action(actions, "privacy-decline", T("Decline & return", "不同意並返回"), () => { flow?.LivingConsentChoice(false); flow?.ShowHome(false); }, "lg-quiet");
            Action(actions, "privacy-return", T("Return without starting", "暫不開始，返回"), () => flow?.ShowHome(false), "lg-quiet");
        }
        else Action(actions, "privacy-revoke", T("Stop camera now", "立即停止相機"), () => { flow?.LivingConsentChoice(false); AnnounceCameraOff(); }, "lg-primary");
    }
    private void BuildTutorial()
    {
        int step = Array.IndexOf(LivingGlassRoutes.Tutorials, route);
        if (step < 0) step = 1;
        string[] titlesEn = { "Make space for attention.", "Move naturally.", "Awaken the gallery.", "Point to explore.", "Make a fist to choose.", "Become part of the portrait.", "You are ready." };
        string[] titlesZh = { "為感知留一點空間。", "自然地移動。", "喚醒藝廊。", "指向，探索。", "握拳，確認選擇。", "成為肖像的一部分。", "準備好，與存在相遇。" };
        string[] detailsEn = { "Seven short steps introduce the experience. Mouse and keyboard alternatives remain available.", "Keep your hands visible. Hold one open palm for two seconds to go Back where the experience permits.", "Wave side to side to release colour. Clap to play the existing environmental sound.", "Point at one of six presences. Pointing identifies a target; it never confirms your choice.", "After targeting a presence, make a fist to confirm. You can also click a portrait or press 1–6.", "In Focus, show both open palms toward the camera to start. A Play video button provides the same action.", "The gallery awaits. Continue, then wave—or use the on-screen awakening action." };
        string[] detailsZh = { "透過七個簡短步驟認識體驗，也可使用滑鼠與鍵盤。", "讓雙手保持在鏡頭範圍內。在允許返回的階段，張開一隻手掌並保持兩秒即可返回。", "左右揮手釋放色彩；拍手播放既有環境音效。", "指向六種存在之一。指向僅辨識目標，不會確認選擇。", "鎖定目標後握拳確認。也可點擊肖像或按 1–6 選擇。", "在專注畫面中，將張開的雙掌朝向相機以開始；也可按「播放影片」。", "藝廊正等待著您。繼續後揮手，或使用畫面上的喚醒按鈕。" };
        var layout = Box(content, "tutorial", "lg-standard");
        var copy = Box(layout, "tutorial-copy", "lg-copy");
        Text(copy, T("INTERACTION / ", "互動教學／") + (step + 1) + " / 7", "lg-eyebrow");
        Text(copy, T(titlesEn[step], titlesZh[step]), "lg-display"); Text(copy, T(detailsEn[step], detailsZh[step]));
        var progress = Box(copy, "tutorial-progress", "lg-progress");
        for (int i = 0; i < 7; i++) { var dot = Box(progress, "step-" + i, "lg-step"); dot.EnableInClassList("lg-current", i <= step); }
        Action(copy, "tutorial-next", T(step == 6 ? "Enter the gallery" : "Continue", step == 6 ? "進入藝廊" : "繼續"), () => flow?.LivingContinue(), "lg-primary");
        Text(copy, T("Tracking begins in the experience after consent. Illustrations demonstrate poses, not recognition results.", "同意後，追蹤將於體驗中開始。圖示示範手勢，不表示已辨識成功。"), "lg-small");
        var gestures = Box(layout, "gesture-art", "lg-gesture-stage");
        string[] types = { "point", "palm", "wave", "point", "fist", "raised", "clap" };
        var symbol = new LivingGlassGestureIllustration(types[step]); symbol.AddToClassList("lg-gesture-symbol"); gestures.Add(symbol);
        Text(gestures, T(types[step].ToUpperInvariant(), new[] { "指向", "張開手掌", "揮手", "指向", "握拳", "雙掌朝向鏡頭", "拍手" }[step]), "lg-eyebrow");
        if (!ReducedMotion)
            motion.Run(symbol, .8f, t => { LivingGlassMotion.Pose(symbol, new Vector2((step == 2 ? Mathf.Sin(t * Mathf.PI * 2) * 16 : 0), (1-t)*6), 1); });
    }
    private void BuildAttract()
    {
        var copy = Standard(T("THE FIRST MOMENT", "初次相遇"), T("Let colour\nfind its way.", "讓色彩，\n流入此刻。"), T("Wave side to side to awaken the gallery. Clap for environmental sound.", "左右揮手喚醒藝廊，拍手播放環境音效。"), 0);
        Action(copy, "awaken", T("Awaken the gallery", "喚醒藝廊"), () => flow?.LivingContinue(), "lg-primary");
        Text(copy, T("Mouse / keyboard alternative", "滑鼠／鍵盤替代操作"), "lg-small");
    }
    private void BuildGallery()
    {
        var title = Box(content, "gallery-title", "lg-gallery-title");
        var titleCopy = Box(title, "gallery-title-copy", "lg-gallery-title-copy");
        Text(titleCopy, T("COLOR RELEASE / SIX PRESENCES", "色彩釋放／六種存在"), "lg-eyebrow");
        Text(titleCopy, T("Find a presence.\nBegin an awakening.", "與存在相遇。\n讓感知甦醒。"), "lg-display");
        Text(title, T("Point to explore. Make a fist to confirm.\nOr click a portrait. Keys 1–6 also choose.", "指向以探索，握拳以確認。\n也可點擊肖像，或按 1–6 選擇。"), "lg-gallery-guidance");
        var row = Box(content, "gallery-arc", "lg-gallery-arc");
        PerformerTargets = new VisualElement[6];
        string[] depths = { "lg-far", "lg-mid", "lg-near", "lg-near", "lg-mid", "lg-far" };
        for (int i = 0; i < 6; i++)
        {
            int p = i;
            var hit = new Button(() => flow?.LivingConfirm(p)) { name = "presence-" + p, tooltip = LivingGlassRoutes.Performers[p] };
            hit.AddToClassList("lg-presence"); hit.AddToClassList(depths[p]); row.Add(hit); PerformerTargets[p] = hit;
            hit.EnableInClassList("lg-first-presence", p == 0); hit.EnableInClassList("lg-last-presence", p == 5);
            var surface = Box(hit, "aperture-" + p, "lg-aperture"); surface.pickingMode = PickingMode.Ignore;
            artSurfaces[p] = surface; artImages[p] = Art(surface, p, "lg-portrait");
            artImages[p].style.scale = new Scale(new Vector3(1.035f,1.035f,1));
            var rim = Box(surface, "rim-" + p, "lg-rim"); rim.pickingMode = PickingMode.Ignore;
            Text(hit, LivingGlassRoutes.Performers[p], "lg-performer-name").name = "performer-name-" + p;
            hit.SetEnabled(ready);
            hit.RegisterCallback<PointerLeaveEvent>(_ => { pointerPose[p] = Vector2.zero; if (target == p && !keyboardGalleryOwner) flow?.LivingTarget(-1); });
            hit.RegisterCallback<FocusInEvent>(_ =>
            {
                pointerPose[p] = Vector2.zero;
                // Programmatic initial/return focus is not fresh input ownership and must not block a hand target.
                if (keyboardGalleryOwner) flow?.LivingTarget(p);
                else if (selected == p) TargetChanged(p);
            });
            hit.RegisterCallback<FocusOutEvent>(evt =>
            {
                var next = evt.relatedTarget as VisualElement;
                if (next != null && !next.name.StartsWith("presence-", StringComparison.Ordinal) && next.name != "confirm-presence") flow?.LivingReleaseTargetOwner();
            });
            hit.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (evt.deltaPosition.sqrMagnitude > .01f)
                {
                    keyboardGalleryOwner = false;
                    if (target != p) flow?.LivingTarget(p);
                }
                Rect b = hit.worldBound;
                pointerPose[p] = new Vector2(Mathf.Clamp((evt.position.x - b.center.x) / Mathf.Max(1, b.width*.5f), -1, 1), Mathf.Clamp((evt.position.y - b.center.y) / Mathf.Max(1, b.height*.5f), -1, 1));
                if (target == p && !OverlayVisible && !ReducedMotion) ApplyPose(p, pose[p]);
            });
            float delay = p * .02f;
            motion.Run(surface, .52f + delay, t =>
            {
                float reveal = Mathf.Clamp01((t * (.52f + delay) - delay) / .52f);
                surface.style.opacity = reveal;
                LivingGlassMotion.Pose(surface, new Vector2(0, (1-reveal)*8), 1);
            });
        }
        var plate = Box(content, "identification", "lg-identification");
        identityIndex = Text(plate, "—", "lg-identity-index");
        var copy = Box(plate, "identity-copy", "lg-identity-copy");
        identity = Text(copy, T("Six artistic presences", "六種藝術存在"), "lg-identity-title");
        identityDetail = Text(copy, ready ? T("Choose a presence to begin.", "選擇一種存在，開始體驗。") : T("Colour is awakening…", "色彩正甦醒……"), "lg-small");
        confirm = Action(plate, "confirm-presence", T("Enter Focus", "進入專注"), () => flow?.LivingConfirm(target), "lg-primary"); confirm.SetEnabled(false);
    }
    public void TargetChanged(int index)
    {
        if (root == null || route != "COLOR-RELEASE" || identity == null || index == target) return;
        target = index;
        identity.text = index < 0 ? T("Six artistic presences", "六種藝術存在") : LivingGlassRoutes.Performers[index];
        identityDetail.text = index < 0 ? T("Point, then make a fist. Or click a portrait.", "指向後握拳確認，或點擊肖像。") : T(LivingGlassAssets.ThemeEnglish[index] + " · Targeted, not yet chosen", LivingGlassAssets.ThemeChinese[index] + " · 已鎖定，尚未確認");
        identityIndex.text = index < 0 ? "—" : "0" + (index + 1);
        confirm.SetEnabled(index >= 0 && ready);
        for (int i = 0; i < 6; i++)
        {
            int p = i; float from = pose[p]; float to = p == index ? 1 : 0;
            PerformerTargets[p].EnableInClassList("lg-targeted", p == index);
            PerformerTargets[p].RemoveFromClassList("lg-acquiring");
            if (artSurfaces[p] != null)
                motion.Run(artSurfaces[p], p == index ? .24f : .34f, t => ApplyPose(p, Mathf.Lerp(from, to, t)));
        }
        ApplyTheme(index, false);
    }
    public void AcquisitionChanged(int candidate)
    {
        if (PerformerTargets == null || OverlayVisible) return;
        for (int i=0;i<6;i++) PerformerTargets[i].EnableInClassList("lg-acquiring", candidate == i && target != i);
        if (identityDetail != null && candidate >= 0 && candidate != target)
            identityDetail.text = T("Acquiring ", "正在辨識：") + LivingGlassRoutes.Performers[candidate];
    }
    private void ApplyPose(int p, float amount)
    {
        pose[p] = amount;
        if (artSurfaces[p] == null) return;
        artSurfaces[p].style.opacity = 1;
        Vector2 pointer = pointerPose[p];
        bool staticMode = ReducedMotion || OverlayVisible;
        Vector2 offset = staticMode ? Vector2.zero : new Vector2(pointer.x*4, -6 - pointer.y) * amount;
        LivingGlassMotion.Pose(artSurfaces[p], offset, staticMode ? 1 : 1 + amount*.02f);
        // UI Toolkit overlay is 2D. No invented perspective: separate artwork parallax and edge light supply the optical fallback.
        LivingGlassMotion.Pose(artImages[p], staticMode ? Vector2.zero : new Vector2(pointer.x*3, pointer.y*2)*amount, 1.035f);
    }
    private void ApplyTheme(int performer, bool instant)
    {
        Color from = accent, to = performer < 0 ? LivingGlassAssets.Neutral : LivingGlassAssets.Accents[performer];
        Color atmosphereFrom = softLight.Tint;
        Color atmosphereTo = performer < 0 ? new Color(0,0,0,0) : LivingGlassAssets.Atmospheres[performer];
        atmosphereTo.a = assets.reducedEffects ? 0 : OverlayVisible ? .025f : .1f;
        motion.Run(themeKey, instant ? 0 : .28f, t =>
        {
            accent = Color.Lerp(from, to, t);
            softLight.Tint = Color.Lerp(atmosphereFrom, atmosphereTo, t);
            if (identityIndex != null) identityIndex.style.color = accent;
            for (int p = 0; p < 6; p++)
            {
                VisualElement rim = artSurfaces[p]?.Q("rim-" + p);
                if (rim != null) { Color edge = accent; edge.a = target == p ? .55f : 0; rim.style.borderTopColor = edge; rim.style.borderBottomColor = edge; }
            }
        });
    }
    private void BuildBridge(VisualElement oldGallery, Rect fromWorld, Texture artwork, Rect fromUv)
    {
        bridgeGallery = oldGallery;
        oldGallery.pickingMode = PickingMode.Ignore;
        oldGallery.SetEnabled(false); root.Insert(1, oldGallery);
        var proxy = new Image { image = artwork, uv = fromUv, scaleMode = ScaleMode.StretchToFill, pickingMode = PickingMode.Ignore };
        proxy.AddToClassList("lg-bridge"); root.Add(proxy);
        bridgeProxy = proxy;
        Vector2 local = root.WorldToLocal(fromWorld.position);
        Rect from = new Rect(local, root.WorldToLocal(fromWorld.position+fromWorld.size)-local);
        Image destinationArt = content.Q<Image>("art-" + selected);
        // Fixed art window matches the chapter's 4% / 13% safe composition.
        float w = root.resolvedStyle.width, h = root.resolvedStyle.height;
        Rect to = new Rect(w*.45f, h*.14f, w*.48f, h*.67f);
        content.style.opacity = 0;
        motion.Run(proxy, .65f, t =>
        {
            if (destinationArt != null && destinationArt.worldBound.width > 0)
            {
                Rect bounds = destinationArt.worldBound;
                Vector2 origin = root.WorldToLocal(bounds.position);
                to = new Rect(origin, root.WorldToLocal(bounds.position+bounds.size)-origin);
            }
            Rect toUv = destinationArt != null ? destinationArt.uv : fromUv;
            proxy.uv = new Rect(Mathf.Lerp(fromUv.x,toUv.x,t), Mathf.Lerp(fromUv.y,toUv.y,t), Mathf.Lerp(fromUv.width,toUv.width,t), Mathf.Lerp(fromUv.height,toUv.height,t));
            proxy.style.left = Mathf.Lerp(from.x, to.x, t); proxy.style.top = Mathf.Lerp(from.y, to.y, t);
            proxy.style.width = Mathf.Lerp(from.width, to.width, t); proxy.style.height = Mathf.Lerp(from.height, to.height, t);
            oldGallery.style.opacity = 1-t; content.style.opacity = Mathf.Clamp01((t-.35f)/.65f);
            proxy.style.opacity = t < .7f ? 1 : 1-(t-.7f)/.3f;
        }, ClearBridge);
    }
    private void ClearBridge()
    {
        bridgeGallery?.RemoveFromHierarchy(); bridgeProxy?.RemoveFromHierarchy();
        bridgeGallery = bridgeProxy = null;
        if (content != null) content.style.opacity = 1;
    }
    public void SettleRoutePresentation()
    {
        motion.Clear(); ClearBridge();
        if (content != null) LivingGlassMotion.Pose(content, Vector2.zero, 1);
    }
    private void BuildChapter(int performer, int stage)
    {
        var chapter = Box(content, "chapter", "lg-chapter");
        Texture2D art = stage == 1 ? assets.activation[performer] : stage == 2 ? assets.living[performer] : assets.portraits[performer];
        if (stage == 2)
        {
            var surface = Box(chapter, "livingPortraitVideoSurface", "lg-video-surface");
            surface.style.backgroundImage = new StyleBackground(art);
            portraitStatus = Text(chapter, T("Preparing Living Portrait…", "正在準備動態肖像……"), "lg-portrait-status");
            return;
        }
        Image image = Art(chapter, performer, "lg-chapter-art", art);
        if (stage == 1) image.name = "activationArtwork";
        var copy = Box(chapter, "chapter-copy", "lg-chapter-copy lg-reading-tint");
        Text(copy, "0" + (performer+1) + " / " + T(new[] { "FOCUS", "ACTIVATION", "LIVING PORTRAIT", "RELEASE", "CONVERSATION" }[stage], new[] { "專注", "啟動", "動態肖像", "釋放", "對話" }[stage]), "lg-eyebrow");
        Text(copy, LivingGlassRoutes.Performers[performer], "lg-display");
        Text(copy, T(LivingGlassAssets.ThemeEnglish[performer], LivingGlassAssets.ThemeChinese[performer]), "lg-body");
        if (stage == 0)
        {
            Text(copy, T("Hold both open palms toward the camera. Or begin with the button below.", "將張開的雙掌朝向相機，或使用下方按鈕開始。"));
            Action(copy, "play-performer-video", T("Play video", "播放影片"), () => flow?.LivingContinue(), "lg-primary");
        }
        else if (stage == 1) Text(copy, T("A presence is awakening…", "存在，正在甦醒……"));
        else if (stage == 3) Action(copy, "release-continue", T("Reflect & converse", "沉澱並對話"), () => flow?.LivingContinue(), "lg-primary");
        else
        {
            copy.AddToClassList("lg-postvideo-copy");
            BuildChat(copy, performer);
            Action(copy, "finish-chapter", T("Continue to Closing", "繼續至結語"), () => flow?.LivingContinue(), "lg-primary");
        }
    }
    public void UpdatePortraitStatus(bool fallback, bool firstFrame, bool autoAdvance, float seconds)
    {
        if (portraitStatus == null) return;
        portraitStatus.style.display = firstFrame && !fallback ? DisplayStyle.None : DisplayStyle.Flex;
        portraitStatus.text = fallback ? T("Video unavailable · artwork remains. " + (autoAdvance ? "The configured fallback will continue automatically." : "Use Back to return."), "影片無法使用，保留作品。" + (autoAdvance ? "依既有替代流程自動繼續。" : "請使用返回。")) : T("Preparing Living Portrait…", "正在準備動態肖像……");
    }
    private void BuildClosing()
    {
        var copy = Standard(T("UNTIL WE MEET AGAIN", "期待再次相遇"), T("Carry the moment\nwith you.", "讓此刻，\n留在心中。"), T("Return to the gallery to meet another presence.", "回到藝廊，與另一種存在相遇。"), Mathf.Max(0, selected));
        Action(copy, "closing-gallery", T("Explore another presence", "探索另一種存在"), () => flow?.LivingReturnGallery(), "lg-primary");
        Action(copy, "closing-takeaway", T("About the takeaway", "關於紀念品"), () => flow?.LivingSouvenir(), "lg-quiet");
    }
    private void BuildSouvenir()
    {
        var copy = Standard(T("TAKEAWAY / NOT AVAILABLE", "紀念品／尚未提供"), T("A memory,\nwithout a code.", "留下一份回憶。"), T("Souvenir delivery is deliberately deferred. No QR code, download or personal-data collection is active.", "紀念品傳送功能刻意延後，目前沒有 QR 碼、下載或個人資料蒐集服務。"), Mathf.Max(0, selected));
        Action(copy, "souvenir-done", T("Done", "完成"), () => flow?.LivingDone(), "lg-primary");
    }
    private void BuildSettings(VisualElement parent)
    {
        Text(parent, T("Available controls and future capabilities are clearly separated.", "可用操作與未來功能皆明確標示。"), "lg-small");
        Setting(parent, T("Language", "語言"), T("Preserves your current route and session.", "保留目前畫面與本次體驗狀態。"), language == AppLanguage.English ? "繁體中文" : "English", () => flow?.LivingLanguage(language == AppLanguage.English ? AppLanguage.Chinese : AppLanguage.English));
        Setting(parent, T("Privacy & camera", "隱私與相機"), T("Review permission or stop capture immediately.", "檢視許可，或立即停止影像擷取。"), T("Review", "檢視"), () => flow?.LivingGlassOpenOverlay("Privacy"));
        Setting(parent, T("AI assistant", "AI 助理"), T("Choose one of six conversation themes.", "選擇六種對話主題之一。"), T("Choose", "選擇"), () => flow?.LivingGlassOpenOverlay("AssistantChooser"));
        Deferred(parent, T("Sound preferences", "聲音偏好"), T("Clap audio remains available in supported experiences. Volume and mute controls are deferred.", "適用體驗中仍可拍手播放音效；音量與靜音設定延後提供。"));
        Deferred(parent, T("Accessibility preferences", "無障礙偏好"), T("Keyboard access and readable surfaces are included. Saved preferences are deferred; reduced modes are exhibition configuration.", "提供鍵盤操作與易讀介面；偏好儲存延後提供，減少動態模式由展覽設定。"));
        Deferred(parent, T("Calibration", "校正"), T("Conditional feature; no verified calibration workflow exists.", "條件式功能，尚無已核實的校正流程。"));
        Deferred(parent, T("Restart / End Experience", "重新開始／結束體驗"), T("A full session reset is deferred. Existing Back and Done paths remain available.", "完整重設流程延後提供，既有返回與完成流程仍可使用。"));
    }
    private void Setting(VisualElement parent, string title, string detail, string action, System.Action command)
    {
        var row = Box(parent, "", "lg-setting-row"); var copy = Box(row, "", "lg-setting-copy");
        Text(copy, title, "lg-subtitle"); Text(copy, detail, "lg-small"); Action(row, "setting-" + parent.childCount, action, command, "lg-quiet");
    }
    private void Deferred(VisualElement parent, string title, string detail)
    {
        var row = Box(parent, "", "lg-setting-row"); var copy = Box(row, "", "lg-setting-copy");
        Text(copy, title, "lg-subtitle"); Text(copy, detail, "lg-small");
        Button unavailable = Action(row, "deferred-" + parent.childCount, T("Not available", "尚未提供"), null, "lg-quiet"); unavailable.SetEnabled(false);
    }
    private void BuildChooser(VisualElement parent)
    {
        Text(parent, T("Choose a presence to begin a conversation. The assistant uses the existing service and does not claim a verified knowledge archive.", "選擇一種存在展開對話。助理使用既有服務，不宣稱擁有已核實的知識庫。"), "lg-small");
        var choices = Box(parent, "assistant-choices", "lg-chooser");
        for (int i=0; i<6; i++)
        {
            int p = i;
            var choice = new Button(() => flow?.LivingGlassOpenOverlay("Assistant-" + LivingGlassRoutes.Performers[p]));
            choice.AddToClassList("lg-assistant-choice"); choice.name = "assistant-" + p; choices.Add(choice);
            Art(choice, p, "lg-assistant-thumb"); Text(choice, LivingGlassRoutes.Performers[p], "lg-choice-name");
        }
    }
    private void BuildChat(VisualElement parent, int performer)
    {
        Text(parent, LivingGlassRoutes.Performers[performer] + T(" / AI conversation", "／AI 對話"), "lg-subtitle");
        Text(parent, T("Ask about your experience. Verified performer context is limited; avoid sharing personal information.", "可詢問體驗感受。已核實的表演者背景有限，請勿分享個人資訊。"), "lg-small");
        var suggestions = Box(parent, "suggestions", "lg-suggestions");
        foreach (string s in new[] { T("What can I notice?", "我可以留意什麼？"), T("Help me reflect.", "引導我回顧感受。"), T("What is known about this presence?", "關於這種存在，有哪些已知資訊？") })
            Action(suggestions, "suggestion-" + suggestions.childCount, s, null, "lg-suggestion");
        var conversation = new ScrollView(ScrollViewMode.Vertical) { name = "assistantConversation" }; conversation.AddToClassList("lg-conversation"); parent.Add(conversation);
        conversation.focusable = true; conversation.tabIndex = 0;
        Text(conversation, T("Your conversation begins here.", "從這裡開始對話。"), "lg-empty").name = "assistant-empty";
        var status = new Label { name = "assistantChatStatus", enableRichText = false }; status.AddToClassList("lg-chat-status"); conversation.Add(status);
        var input = new TextField(T("Your question", "您的問題")) { name = "assistantChatInput", maxLength = 3600, multiline = true };
        input.AddToClassList("lg-chat-input"); parent.Add(input);
        var controls = Box(parent, "chat-controls", "lg-actions");
        Action(controls, "assistantChatSend", T("Ask", "詢問"), null, "lg-primary");
        Action(controls, "assistantCancel", T("Cancel request", "取消請求"), null, "lg-quiet").SetEnabled(false);
    }
    public VisualElement OpenOverlay(string kind, AppLanguage locale, int performer)
    {
        bool replacing = overlayPlane != null;
        language = locale;
        if (replacing && closing && overlayKind == kind)
        {
            resume?.RemoveFromHierarchy(); resume = null;
            closing = false;
            float from = panelProgress;
            motion.Run(overlayPlane, .36f * (1-from), t => SetPanelProgress(Mathf.Lerp(from, 1, t)));
            panel.Q<Button>("window-close")?.Focus();
            return panel;
        }
        motion.Clear();
        ClearBridge();
        content.style.opacity = 1;
        LivingGlassMotion.Pose(content, Vector2.zero, 1);
        if (!replacing) opener = root.focusController?.focusedElement as VisualElement;
        if (overlayPlane != null) { motion.Cancel(overlayPlane); overlayPlane.RemoveFromHierarchy(); }
        overlayKind = kind; closing = false;
        resume?.RemoveFromHierarchy(); resume = null;
        content.SetEnabled(false); dock.SetEnabled(false); header.SetEnabled(false);
        for (int i = 0; i < 6; i++) if (artSurfaces[i] != null) { motion.Cancel(artSurfaces[i]); ApplyPose(i, 0); }
        ApplyTheme(target >= 0 ? target : selected, true);
        overlayPlane = Box(root, "overlay-plane", "lg-overlay-plane");
        overlayPlane.focusable = true; overlayPlane.tabIndex = -1;
        panel = Box(overlayPlane, "glass-window", "lg-window");
        panel.focusable = true; panel.tabIndex = -1;
        var bar = Box(panel, "window-titlebar", "lg-window-titlebar");
        string title = kind == "Settings" ? T("Experience controls", "體驗設定") : kind == "Privacy" ? T("Privacy & camera", "隱私與相機") : kind == "Help" ? T("Here to guide you", "操作說明") : T("A conversation", "一場對話");
        Text(bar, title, "lg-panel-title");
        if (kind == "Settings" || kind.StartsWith("Assistant-", StringComparison.Ordinal))
            Action(bar, "window-minimize", T("Minimize", "縮小"), () => flow?.LivingMinimize(), "lg-window-tool");
        var close = Action(bar, "window-close", T("Close", "關閉"), () => flow?.LivingGlassCloseOverlay(), "lg-window-tool");
        var scroll = new ScrollView(ScrollViewMode.Vertical); scroll.AddToClassList("lg-window-body"); panel.Add(scroll); body = scroll;
        if (kind == "Settings") BuildSettings(body);
        else if (kind == "Privacy") BuildPrivacy(body, false);
        else if (kind == "Help") BuildHelp(body);
        else if (kind.StartsWith("Assistant-", StringComparison.Ordinal)) BuildChat(body, performer);
        else BuildChooser(body);
        Vector2 origin = opener != null ? root.WorldToLocal(opener.worldBound.center) : new Vector2(root.resolvedStyle.width*.5f, root.resolvedStyle.height*.94f);
        panelOrigin = new Vector2(Mathf.Clamp(origin.x - root.resolvedStyle.width*.76f, -520, 120), Mathf.Clamp(origin.y - root.resolvedStyle.height*.5f, -200, 400));
        panelProgress = 0;
        motion.Run(overlayPlane, .36f, t => SetPanelProgress(t));
        close.Focus();
        return panel;
    }
    private void BuildHelp(VisualElement parent)
    {
        Text(parent, T("Your next action", "下一步操作"), "lg-subtitle");
        string family = LivingGlassRoutes.Family(route);
        Text(parent, family == "Gallery" ? T("Point to identify a presence, then make a fist to confirm. Click or press 1–6 to choose directly.", "指向以鎖定存在，再握拳確認。亦可點擊，或按 1–6 直接選擇。") : family == "FOCUS-VIEW" ? T("Show both open palms toward the camera, or choose Play video.", "將張開的雙掌朝向相機，或選擇播放影片。") : T("Use the visible action to continue. Tab moves focus; Enter or Space activates a focused button. Escape dismisses a window first.", "使用畫面上的按鈕繼續。Tab 移動焦點，Enter 或空白鍵啟動焦點按鈕；Escape 優先關閉視窗。"));
        foreach (string type in new[] { "point", "fist", "palm", "wave", "raised", "clap" })
        {
            var row = Box(parent, "help-" + type, "lg-help-row");
            var symbol = new LivingGlassGestureIllustration(type); symbol.AddToClassList("lg-help-symbol"); row.Add(symbol);
            string[] en = { "Point · target a performer", "Fist · confirm a targeted performer", "Open palm held · Back where allowed", "Wave · initial awakening", "Both open palms · start from Focus", "Clap · environmental sound" };
            string[] zh = { "指向 · 鎖定表演者", "握拳 · 確認已鎖定的表演者", "保持張開手掌 · 允許時返回", "揮手 · 初次喚醒", "雙掌朝向鏡頭 · 從專注開始", "拍手 · 環境音效" };
            int i = Array.IndexOf(new[] { "point", "fist", "palm", "wave", "raised", "clap" }, type); Text(row, T(en[i], zh[i]), "lg-small");
        }
        Deferred(parent, T("Calibration & extended guidance", "校正與延伸導覽"), T("Conditional future capabilities; no calibration result is simulated.", "條件式未來功能，不模擬校正結果。"));
    }
    private void SetPanelProgress(float value)
    {
        panelProgress = value;
        if (panel == null) return;
        panel.style.opacity = value;
        LivingGlassMotion.Pose(panel, panelOrigin * (1-value), .94f + value*.06f);
    }
    public void CloseOverlay(bool immediate)
    {
        if (overlayPlane == null) return;
        VisualElement plane = overlayPlane;
        float start = panelProgress;
        closing = true;
        // Close works during opening; cancel samples from the current pose.
        motion.Run(plane, immediate ? 0 : .22f, t => SetPanelProgress(start * (1-t)), () =>
        {
            plane.RemoveFromHierarchy();
            if (overlayPlane != plane) return;
            overlayPlane = panel = null; overlayKind = null; closing = false;
            body = null;
            content?.SetEnabled(true); dock?.SetEnabled(true); header?.SetEnabled(true);
            if (opener != null && opener.panel != null && opener.enabledInHierarchy) opener.Focus();
            else if (resume != null) resume.Focus();
            else FirstFocusable(dock)?.Focus();
            opener = null;
            if (atmosphere != null) ApplyTheme(target >= 0 ? target : selected, false);
        });
    }
    public void ShowResume(string kind)
    {
        resume?.RemoveFromHierarchy();
        resume = Action(root, "resume-window", T("Resume ", "繼續開啟") + (kind == "Settings" ? T("Settings", "設定") : T("AI · new conversation", "AI · 新對話")), () => flow?.LivingGlassOpenOverlay(kind), "lg-resume");
    }
    public bool IsAssistantActive(VisualElement chatRoot)
    {
        if (chatRoot == null || chatRoot.panel == null || closing) return false;
        string family = LivingGlassRoutes.Family(route);
        return OverlayVisible ? panel == chatRoot && overlayKind != null && overlayKind.StartsWith("Assistant-", StringComparison.Ordinal) : content == chatRoot && (family == "POST-VIDEO-AI" || family == "Assistant");
    }
    public void AnnounceProtected() { if (announcement != null) announcement.text = T("Available after this cinematic moment. Open explicitly when ready.", "此沉浸段落結束後可使用，屆時請自行開啟。 "); }
    public void AnnounceCameraOff() { if (announcement != null) announcement.text = T("Camera stopped. Your experience remains on this screen.", "相機已停止，體驗維持在目前畫面。"); }
    private static VisualElement FirstFocusable(VisualElement parent)
    {
        if (parent == null) return null;
        if (parent.focusable && parent.enabledInHierarchy && parent.resolvedStyle.display != DisplayStyle.None) return parent;
        foreach (VisualElement child in parent.Children()) { var found = FirstFocusable(child); if (found != null) return found; }
        return null;
    }
    private void OnKey(KeyDownEvent evt)
    {
        var keyTarget = evt.target as VisualElement;
        bool editing = evt.target is TextField || keyTarget?.GetFirstAncestorOfType<TextField>() != null || !string.IsNullOrEmpty(Input.compositionString);
        if (evt.keyCode == KeyCode.Tab)
        {
            keyboardGalleryOwner = true;
            MoveKeyboardFocus(OverlayVisible ? panel : root, evt.shiftKey);
            evt.StopImmediatePropagation();
            root.focusController.IgnoreEvent(evt);
            return;
        }
        if (!editing && (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Space))
        {
            Button button = keyTarget as Button ?? keyTarget?.GetFirstAncestorOfType<Button>();
            if (button != null && button.enabledInHierarchy)
            {
                using (var submit = NavigationSubmitEvent.GetPooled()) button.SendEvent(submit);
                evt.StopImmediatePropagation();
                root.focusController.IgnoreEvent(evt);
                return;
            }
        }
        if (OverlayVisible)
        {
            if (evt.keyCode == KeyCode.Escape) { flow?.LivingGlassCloseOverlay(); evt.StopImmediatePropagation(); return; }
            return;
        }
        // Editors keep IME, Enter, Backspace and navigation keys. No universal gesture click.
        if (editing) return;
        if (evt.keyCode == KeyCode.Tab || evt.keyCode == KeyCode.LeftArrow || evt.keyCode == KeyCode.RightArrow || evt.keyCode == KeyCode.UpArrow || evt.keyCode == KeyCode.DownArrow) keyboardGalleryOwner = true;
        if (evt.keyCode == KeyCode.Escape) { flow?.Back(); evt.StopPropagation(); return; }
        if (route == "COLOR-RELEASE" && ready && evt.keyCode >= KeyCode.Alpha1 && evt.keyCode <= KeyCode.Alpha6)
        { flow?.LivingConfirm((int)evt.keyCode - (int)KeyCode.Alpha1); evt.StopImmediatePropagation(); }
    }
    private void MoveKeyboardFocus(VisualElement scope, bool reverse)
    {
        var candidates = scope.Query<VisualElement>().ToList().FindAll(v => (v is Button || v is TextField || v is Toggle || v is ScrollView)
            && v.focusable && v.tabIndex >= 0 && v.enabledInHierarchy && v.resolvedStyle.display != DisplayStyle.None && v.worldBound.height > 0);
        if (candidates.Count == 0) return;
        VisualElement focused = root.focusController.focusedElement as VisualElement;
        int index = -1;
        while (focused != null && index < 0) { index = candidates.IndexOf(focused); focused = focused.parent; }
        int next = index < 0 ? (reverse ? candidates.Count-1 : 0) : (index+(reverse?-1:1)+candidates.Count)%candidates.Count;
        candidates[next].Focus();
    }
}
