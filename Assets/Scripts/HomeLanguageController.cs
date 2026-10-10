using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Interaction prototype for the two Figma-generated Home UXML screens.
/// The Figma IDs and paths below were verified against the current connector
/// manifest. Paths are snapshot mappings, so every binding is validated before
/// any callback is registered.
/// </summary>
public sealed class HomeLanguageController : MonoBehaviour
{
    public static event Action<AppLanguage> LanguageChanged;
    public static event Action PrivacyRequested;
    public static event Action SettingsRequested;
    public static event Action ReviewConsentRequested;
    public static event Action<bool> ConsentChanged;
    private const string EnglishAssetName = "HOME---ENGLISH---SANCTUARY";
    private const string ChineseAssetName = "HOME--------SANCTUARY";
    private const int ExpectedChildCount = 34;

    [Header("Home UI Variants")]
    [SerializeField] private GameObject homeUIEnglish;
    [SerializeField] private GameObject homeUIChinese;

    private UIDocument englishDocument;
    private UIDocument chineseDocument;
    private VisualElement englishRoot;
    private VisualElement chineseRoot;
    private InteractionGroup[] interactionGroups;
    private VisualElement englishSelectorSurface;
    private VisualElement chineseSelectorSurface;
    private VisualElement chineseScreenEnglishSelectorSurface;
    private VisualElement chineseScreenChineseSelectorSurface;
    private VisualElement englishCheckbox;
    private VisualElement chineseCheckbox;
    private Label englishCheckmark;
    private Label chineseCheckmark;
    private bool consentAccepted;
    private Language currentLanguage = Language.None;
    private bool initialized;

    private enum Language
    {
        None,
        English,
        Chinese
    }

    private enum ExpectedElementType
    {
        VisualElement,
        Label
    }

    private sealed class NodeMapping
    {
        public readonly string Role;
        public readonly string FigmaNodeId;
        public readonly string Path;
        public readonly ExpectedElementType ElementType;

        public NodeMapping(string role, string figmaNodeId, string path, ExpectedElementType elementType)
        {
            Role = role;
            FigmaNodeId = figmaNodeId;
            Path = path;
            ElementType = elementType;
        }
    }

    private sealed class ScreenMapping
    {
        public readonly string RootNodeId;
        public readonly string AssetName;
        public readonly NodeMapping[] Nodes;

        public ScreenMapping(string rootNodeId, string assetName, NodeMapping[] nodes)
        {
            RootNodeId = rootNodeId;
            AssetName = assetName;
            Nodes = nodes;
        }
    }

    private sealed class ElementEvents
    {
        private readonly VisualElement element;
        private readonly EventCallback<PointerEnterEvent> pointerEnter;
        private readonly EventCallback<PointerLeaveEvent> pointerLeave;
        private readonly EventCallback<PointerDownEvent> pointerDown;
        private readonly EventCallback<PointerUpEvent> pointerUp;
        private readonly EventCallback<PointerCancelEvent> pointerCancel;
        private readonly EventCallback<ClickEvent> click;
        private readonly EventCallback<KeyDownEvent> keyDown;
        private readonly EventCallback<FocusInEvent> focusIn;
        private readonly EventCallback<FocusOutEvent> focusOut;

        public ElementEvents(VisualElement target, InteractionGroup owner)
        {
            element = target;
            pointerEnter = evt => owner.PointerEntered(element, evt);
            pointerLeave = evt => owner.PointerLeft(element, evt);
            pointerDown = evt => owner.PointerPressed(evt);
            pointerUp = evt => owner.PointerReleased(evt);
            pointerCancel = evt => owner.PointerCancelled(evt);
            click = evt => owner.Clicked(evt);
            keyDown = evt => owner.KeyboardActivated(evt);
            focusIn = evt => element.AddToClassList("ac-keyboard-focus");
            focusOut = evt => element.RemoveFromClassList("ac-keyboard-focus");

            element.focusable = true;
            element.tabIndex = 0;
            element.RegisterCallback(pointerEnter);
            element.RegisterCallback(pointerLeave);
            element.RegisterCallback(pointerDown);
            element.RegisterCallback(pointerUp);
            element.RegisterCallback(pointerCancel);
            element.RegisterCallback(click);
            element.RegisterCallback(keyDown);
            element.RegisterCallback(focusIn);
            element.RegisterCallback(focusOut);
        }

        public void Dispose()
        {
            element.UnregisterCallback(pointerEnter);
            element.UnregisterCallback(pointerLeave);
            element.UnregisterCallback(pointerDown);
            element.UnregisterCallback(pointerUp);
            element.UnregisterCallback(pointerCancel);
            element.UnregisterCallback(click);
            element.UnregisterCallback(keyDown);
            element.UnregisterCallback(focusIn);
            element.UnregisterCallback(focusOut);
        }
    }

    private sealed class InteractionGroup
    {
        private readonly List<VisualElement> elements;
        private readonly HashSet<VisualElement> hovered = new HashSet<VisualElement>();
        private readonly List<ElementEvents> registrations = new List<ElementEvents>();
        private readonly Action onClick;
        private bool pressed;

        public InteractionGroup(IEnumerable<VisualElement> targets, Action activation)
        {
            elements = targets.Where(target => target != null).Distinct().ToList();
            onClick = activation;

            foreach (VisualElement element in elements)
            {
                element.AddToClassList("ac-interactive");
                registrations.Add(new ElementEvents(element, this));
            }
            if (elements.Count > 0)
            {
                elements[0].focusable = true;
                elements[0].tabIndex = 0;
                for (int i = 1; i < elements.Count; i++)
                {
                    elements[i].focusable = false;
                    elements[i].tabIndex = -1;
                }
            }
        }

        public void PointerEntered(VisualElement element, PointerEnterEvent evt)
        {
            hovered.Add(element);
            UpdateHover();
            if (evt.pressedButtons != 0)
                SetPressed(true);
        }

        public void PointerLeft(VisualElement element, PointerLeaveEvent evt)
        {
            hovered.Remove(element);
            UpdateHover();
            SetPressed(false);
        }

        public void PointerPressed(PointerDownEvent evt)
        {
            if (evt.button == 0)
                SetPressed(true);
        }

        public void PointerReleased(PointerUpEvent evt)
        {
            if (evt.button == 0 || evt.pressedButtons == 0)
                SetPressed(false);
        }

        public void PointerCancelled(PointerCancelEvent evt)
        {
            SetPressed(false);
        }

        public void Clicked(ClickEvent evt)
        {
            SetPressed(false);
            evt.StopPropagation();
            if (onClick != null)
                onClick();
        }

        public void KeyboardActivated(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.Return && evt.keyCode != KeyCode.KeypadEnter && evt.keyCode != KeyCode.Space)
                return;
            evt.StopPropagation();
            if (onClick != null)
                onClick();
        }

        public void ResetTransientState()
        {
            hovered.Clear();
            pressed = false;
            UpdateHover();
            UpdatePressed();
            foreach (VisualElement element in elements)
                element.RemoveFromClassList("ac-keyboard-focus");
        }

        public void SetUnavailable(bool unavailable)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                VisualElement element = elements[i];
                element.EnableInClassList("ac-unavailable", unavailable);
                if (unavailable)
                {
                    element.RemoveFromClassList("ac-hover");
                    element.RemoveFromClassList("ac-pressed");
                    element.RemoveFromClassList("ac-keyboard-focus");
                }
                element.SetEnabled(!unavailable);
                element.focusable = !unavailable && i == 0;
                element.tabIndex = element.focusable ? 0 : -1;
            }
        }

        public void AddStateClass(string className, bool enabled)
        {
            foreach (VisualElement element in elements)
                element.EnableInClassList(className, enabled);
        }

        public void Dispose()
        {
            foreach (ElementEvents registration in registrations)
                registration.Dispose();

            registrations.Clear();
            ResetTransientState();
        }

        private void UpdateHover()
        {
            bool isHovered = hovered.Count > 0;
            foreach (VisualElement element in elements)
                element.EnableInClassList("ac-hover", isHovered);
        }

        private void SetPressed(bool value)
        {
            pressed = value;
            UpdatePressed();
        }

        private void UpdatePressed()
        {
            foreach (VisualElement element in elements)
                element.EnableInClassList("ac-pressed", pressed);
        }
    }

    private static readonly Color SelectedLanguageBackground = new Color(51f / 255f, 37f / 255f, 17f / 255f, 0.96f);
    private static readonly Color UnselectedLanguageBackground = new Color(18f / 255f, 17f / 255f, 13f / 255f, 0.88f);
    private static readonly Color SelectedLanguageBorder = new Color(243f / 255f, 185f / 255f, 95f / 255f, 0.55f);
    private static readonly Color UnselectedLanguageBorder = new Color(89f / 255f, 79f / 255f, 59f / 255f, 0.45f);
    private static readonly Color CheckedColor = new Color(243f / 255f, 185f / 255f, 95f / 255f, 1f);
    private static readonly Color UncheckedColor = new Color(20f / 255f, 18f / 255f, 13f / 255f, 1f);
    private static readonly Color CheckboxBorder = new Color(153f / 255f, 122f / 255f, 69f / 255f, 0.65f);

    private static readonly ScreenMapping EnglishMapping = new ScreenMapping(
        "51:4",
        EnglishAssetName,
        new[]
        {
            new NodeMapping("LanguageEnglishSurface", "51:16", "1/12", ExpectedElementType.VisualElement),
            new NodeMapping("LanguageEnglish", "51:17", "1/13", ExpectedElementType.Label),
            new NodeMapping("LanguageChineseSurface", "51:18", "1/14", ExpectedElementType.VisualElement),
            new NodeMapping("LanguageChinese", "51:19", "1/15", ExpectedElementType.Label),
            new NodeMapping("ConsentStatus", "51:25", "1/21", ExpectedElementType.VisualElement),
            new NodeMapping("ConsentCheckbox", "51:26", "1/22", ExpectedElementType.VisualElement),
            new NodeMapping("ConsentCheckboxLabel", "51:27", "1/23", ExpectedElementType.Label),
            new NodeMapping("ConsentCheckboxAlternateLabel", "51:28", "1/24", ExpectedElementType.Label),
            new NodeMapping("PrivacyDetails", "51:29", "1/25", ExpectedElementType.VisualElement),
            new NodeMapping("PrivacyDetailsLabel", "51:30", "1/26", ExpectedElementType.Label),
            new NodeMapping("Settings", "51:31", "1/27", ExpectedElementType.VisualElement),
            new NodeMapping("SettingsLabel", "51:32", "1/28", ExpectedElementType.Label),
            new NodeMapping("ReviewConsent", "51:33", "1/29", ExpectedElementType.VisualElement),
            new NodeMapping("ReviewConsentLabel", "51:34", "1/30", ExpectedElementType.Label),
            new NodeMapping("ConsentRequirement", "51:35", "1/31", ExpectedElementType.Label),
            new NodeMapping("SettingsAccess", "51:36", "1/32", ExpectedElementType.Label),
            new NodeMapping("Footer", "51:37", "1/33", ExpectedElementType.Label)
        });

    private static readonly ScreenMapping ChineseMapping = new ScreenMapping(
        "53:2",
        ChineseAssetName,
        new[]
        {
            new NodeMapping("LanguageEnglishSurface", "53:14", "1/12", ExpectedElementType.VisualElement),
            new NodeMapping("LanguageEnglish", "53:15", "1/13", ExpectedElementType.Label),
            new NodeMapping("LanguageChineseSurface", "53:16", "1/14", ExpectedElementType.VisualElement),
            new NodeMapping("LanguageChinese", "53:17", "1/15", ExpectedElementType.Label),
            new NodeMapping("ConsentStatus", "53:23", "1/21", ExpectedElementType.VisualElement),
            new NodeMapping("ConsentCheckbox", "53:24", "1/22", ExpectedElementType.VisualElement),
            new NodeMapping("ConsentCheckboxLabel", "53:25", "1/23", ExpectedElementType.Label),
            new NodeMapping("ConsentCheckboxAlternateLabel", "53:26", "1/24", ExpectedElementType.Label),
            new NodeMapping("PrivacyDetails", "53:27", "1/25", ExpectedElementType.VisualElement),
            new NodeMapping("PrivacyDetailsLabel", "53:28", "1/26", ExpectedElementType.Label),
            new NodeMapping("Settings", "53:29", "1/27", ExpectedElementType.VisualElement),
            new NodeMapping("SettingsLabel", "53:30", "1/28", ExpectedElementType.Label),
            new NodeMapping("ReviewConsent", "53:31", "1/29", ExpectedElementType.VisualElement),
            new NodeMapping("ReviewConsentLabel", "53:32", "1/30", ExpectedElementType.Label),
            new NodeMapping("ConsentRequirement", "53:33", "1/31", ExpectedElementType.Label),
            new NodeMapping("SettingsAccess", "53:34", "1/32", ExpectedElementType.Label),
            new NodeMapping("Footer", "53:35", "1/33", ExpectedElementType.Label)
        });

    private void Start()
    {
        // These documents are intentionally kept alive while other screens are shown so
        // their existing Home bindings and session consent state survive navigation.
        if (homeUIEnglish != null && !homeUIEnglish.activeSelf)
            homeUIEnglish.SetActive(true);
        if (homeUIChinese != null && !homeUIChinese.activeSelf)
            homeUIChinese.SetActive(true);

        if (!TryInitialize())
            FailSafe();
    }

    private void OnDestroy()
    {
        if (interactionGroups == null)
            return;

        foreach (InteractionGroup group in interactionGroups)
            group.Dispose();
    }

    private bool TryInitialize()
    {
        if (homeUIEnglish == null || homeUIChinese == null)
        {
            Debug.LogError("HomeLanguageController: Assign both Home UIDocument GameObjects.", this);
            return false;
        }

        englishDocument = homeUIEnglish.GetComponent<UIDocument>();
        chineseDocument = homeUIChinese.GetComponent<UIDocument>();
        if (englishDocument == null || chineseDocument == null)
        {
            Debug.LogError("HomeLanguageController: Both assigned GameObjects must contain a UIDocument.", this);
            return false;
        }

        if (!englishDocument.enabled || !chineseDocument.enabled ||
            !homeUIEnglish.activeInHierarchy || !homeUIChinese.activeInHierarchy)
        {
            Debug.LogError("HomeLanguageController: Both UIDocuments must be enabled and active in the scene.", this);
            return false;
        }

        englishRoot = englishDocument.rootVisualElement;
        chineseRoot = chineseDocument.rootVisualElement;
        englishRoot.AddToClassList("ac-screen-home");
        chineseRoot.AddToClassList("ac-screen-home");
        englishRoot.focusable = false;
        englishRoot.tabIndex = -1;
        chineseRoot.focusable = false;
        chineseRoot.tabIndex = -1;
        Dictionary<string, VisualElement> englishElements;
        Dictionary<string, VisualElement> chineseElements;
        if (!ValidateAndResolve(englishDocument, englishRoot, EnglishMapping, out englishElements) ||
            !ValidateAndResolve(chineseDocument, chineseRoot, ChineseMapping, out chineseElements))
            return false;

        StyleSheet feedbackSheet = Resources.Load<StyleSheet>("HomeInteraction");
        if (feedbackSheet == null)
        {
            Debug.LogError("HomeLanguageController: Could not load Resources/HomeInteraction.uss.", this);
            return false;
        }

        englishRoot.styleSheets.Add(feedbackSheet);
        chineseRoot.styleSheets.Add(feedbackSheet);

        englishSelectorSurface = englishElements["LanguageEnglishSurface"];
        chineseSelectorSurface = englishElements["LanguageChineseSurface"];
        chineseScreenEnglishSelectorSurface = chineseElements["LanguageEnglishSurface"];
        chineseScreenChineseSelectorSurface = chineseElements["LanguageChineseSurface"];
        englishCheckbox = englishElements["ConsentCheckbox"];
        chineseCheckbox = chineseElements["ConsentCheckbox"];
        englishCheckmark = AddCheckmark(englishCheckbox);
        chineseCheckmark = AddCheckmark(chineseCheckbox);

        VisualElement englishLanguageLabel = englishElements["LanguageEnglish"];
        VisualElement chineseLanguageLabel = englishElements["LanguageChinese"];
        VisualElement englishLanguageLabelChineseScreen = chineseElements["LanguageEnglish"];
        VisualElement chineseLanguageLabelChineseScreen = chineseElements["LanguageChinese"];

        InteractionGroup englishLanguage = new InteractionGroup(
            new[] { englishSelectorSurface, englishLanguageLabel },
            () => SelectLanguage(Language.English));
        InteractionGroup chineseLanguage = new InteractionGroup(
            new[] { chineseSelectorSurface, chineseLanguageLabel },
            () => SelectLanguage(Language.Chinese));
        InteractionGroup englishLanguageChineseScreen = new InteractionGroup(
            new[] { chineseScreenEnglishSelectorSurface, englishLanguageLabelChineseScreen },
            () => SelectLanguage(Language.English));
        InteractionGroup chineseLanguageChineseScreen = new InteractionGroup(
            new[] { chineseScreenChineseSelectorSurface, chineseLanguageLabelChineseScreen },
            () => SelectLanguage(Language.Chinese));

        InteractionGroup englishConsent = new InteractionGroup(
            new[] { englishCheckbox, englishElements["ConsentCheckboxLabel"], englishElements["ConsentCheckboxAlternateLabel"] },
            ToggleConsent);
        InteractionGroup chineseConsent = new InteractionGroup(
            new[] { chineseCheckbox, chineseElements["ConsentCheckboxLabel"], chineseElements["ConsentCheckboxAlternateLabel"] },
            ToggleConsent);

        InteractionGroup englishPrivacy = CreateActionGroup(englishElements, "PrivacyDetails", "PrivacyDetailsLabel", () => Raise(PrivacyRequested));
        InteractionGroup chinesePrivacy = CreateActionGroup(chineseElements, "PrivacyDetails", "PrivacyDetailsLabel", () => Raise(PrivacyRequested));
        InteractionGroup englishSettings = CreateActionGroup(englishElements, "Settings", "SettingsLabel", () => Raise(SettingsRequested));
        InteractionGroup chineseSettings = CreateActionGroup(chineseElements, "Settings", "SettingsLabel", () => Raise(SettingsRequested));
        InteractionGroup englishReview = CreateActionGroup(englishElements, "ReviewConsent", "ReviewConsentLabel", ReviewConsent);
        InteractionGroup chineseReview = CreateActionGroup(chineseElements, "ReviewConsent", "ReviewConsentLabel", ReviewConsent);
        englishReviewElements = englishReview;
        chineseReviewElements = chineseReview;
        InteractionGroup englishSettingsAccess = CreateActionGroup(englishElements, "SettingsAccess", null, () => Raise(SettingsRequested));
        InteractionGroup chineseSettingsAccess = CreateActionGroup(chineseElements, "SettingsAccess", null, () => Raise(SettingsRequested));

        interactionGroups = new[]
        {
            englishLanguage, chineseLanguage, englishLanguageChineseScreen, chineseLanguageChineseScreen,
            englishConsent, chineseConsent,
            englishPrivacy, chinesePrivacy, englishSettings, chineseSettings,
            englishReview, chineseReview, englishSettingsAccess, chineseSettingsAccess
        };

        englishReview.SetUnavailable(true);
        chineseReview.SetUnavailable(true);
        initialized = true;
        ApplyConsentState();
        ApplyLanguage(Language.English);
        return true;
    }

    private InteractionGroup CreateActionGroup(
        Dictionary<string, VisualElement> elements,
        string surfaceRole,
        string labelRole,
        Action activation)
    {
        List<VisualElement> targets = new List<VisualElement>();
        if (surfaceRole != null)
            targets.Add(elements[surfaceRole]);
        if (labelRole != null)
            targets.Add(elements[labelRole]);

        return new InteractionGroup(targets, activation);
    }

    private bool ValidateAndResolve(
        UIDocument document,
        VisualElement root,
        ScreenMapping mapping,
        out Dictionary<string, VisualElement> resolved)
    {
        resolved = new Dictionary<string, VisualElement>();
        if (root == null || document.visualTreeAsset == null || document.visualTreeAsset.name != mapping.AssetName)
        {
            Debug.LogError(
                "HomeLanguageController: UIDocument " + document.name + " must reference " + mapping.AssetName +
                " (Figma root " + mapping.RootNodeId + ").",
                this);
            return false;
        }

        VisualElement uxmlRoot = root;
        if (root.childCount == 1)
        {
            VisualElement possibleUxmlRoot = root.Children().First();
            if (possibleUxmlRoot.childCount == ExpectedChildCount)
                uxmlRoot = possibleUxmlRoot;
        }

        if (uxmlRoot.childCount != ExpectedChildCount)
        {
            Debug.LogError(
                "HomeLanguageController: " + mapping.AssetName + " could not be resolved as a UXML root with " +
                ExpectedChildCount + " children (UIDocument root has " + root.childCount +"). Revalidate its connector manifest before binding.",
                this);
            return false;
        }

        HashSet<string> seenPaths = new HashSet<string>();
        HashSet<string> seenIds = new HashSet<string>();
        foreach (NodeMapping node in mapping.Nodes)
        {
            if (!seenPaths.Add(node.Path) || !seenIds.Add(node.FigmaNodeId))
            {
                Debug.LogError("HomeLanguageController: Duplicate mapping path or Figma ID for role " + node.Role + ".", this);
                return false;
            }

            VisualElement element;
            if (!TryResolveManifestPath(uxmlRoot, node.Path, out element))
            {
                Debug.LogError(
                    "HomeLanguageController: Could not resolve " + mapping.AssetName + " Figma node " +
                    node.FigmaNodeId + " at manifest path " + node.Path + " (" + node.Role + ").",
                    this);
                return false;
            }

            bool correctType = node.ElementType == ExpectedElementType.Label
                ? element is Label
                : element.GetType() == typeof(VisualElement);
            if (!correctType)
            {
                Debug.LogError(
                    "HomeLanguageController: Figma node " + node.FigmaNodeId + " at " + node.Path +
                    " has runtime type " + element.GetType().Name + "; expected " + node.ElementType + ".",
                    this);
                return false;
            }

            if (resolved.ContainsKey(node.Role))
            {
                Debug.LogError("HomeLanguageController: Role " + node.Role + " is mapped more than once.", this);
                return false;
            }
            resolved.Add(node.Role, element);
        }

        return true;
    }

    private static bool TryResolveManifestPath(VisualElement root, string path, out VisualElement element)
    {
        element = null;
        string[] segments = path.Split('/');
        if (segments.Length != 2 || segments[0] != "1")
            return false;

        int oneBasedChildIndex;
        if (!int.TryParse(segments[1], out oneBasedChildIndex) || oneBasedChildIndex < 1 || oneBasedChildIndex > root.childCount)
            return false;

        element = root.Children().ElementAt(oneBasedChildIndex - 1);
        return element != null;
    }

    private static Label AddCheckmark(VisualElement checkbox)
    {
        Label mark = new Label("✓");
        mark.name = "ac-runtime-checkmark";
        mark.pickingMode = PickingMode.Ignore;
        mark.style.position = Position.Absolute;
        mark.style.left = 0;
        mark.style.right = 0;
        mark.style.top = 0;
        mark.style.bottom = 0;
        mark.style.unityTextAlign = TextAnchor.MiddleCenter;
        mark.style.color = new Color(20f / 255f, 18f / 255f, 13f / 255f, 1f);
        mark.style.fontSize = 19;
        mark.style.display = DisplayStyle.None;
        checkbox.Add(mark);
        return mark;
    }

    private void SelectLanguage(Language language)
    {
        if (!initialized || language == currentLanguage)
            return;

        ApplyLanguage(language);
    }

    private void ApplyLanguage(Language language)
    {
        currentLanguage = language;
        bool showEnglish = language == Language.English;

        ResetTransientStates();
        englishRoot.SetEnabled(showEnglish);
        englishRoot.style.display = showEnglish ? DisplayStyle.Flex : DisplayStyle.None;
        chineseRoot.SetEnabled(!showEnglish);
        chineseRoot.style.display = showEnglish ? DisplayStyle.None : DisplayStyle.Flex;

        ApplyLanguageStyle(englishSelectorSurface, showEnglish);
        ApplyLanguageStyle(chineseSelectorSurface, !showEnglish);
        ApplyLanguageStyle(chineseScreenEnglishSelectorSurface, showEnglish);
        ApplyLanguageStyle(chineseScreenChineseSelectorSurface, !showEnglish);
        (showEnglish ? englishSelectorSurface : chineseScreenChineseSelectorSurface).Focus();

        Debug.Log("Home language changed to " + (showEnglish ? "English" : "Chinese") + ".", this);
        LanguageChanged?.Invoke(showEnglish ? AppLanguage.English : AppLanguage.Chinese);
    }

    private static void ApplyLanguageStyle(VisualElement surface, bool selected)
    {
        surface.EnableInClassList("ac-selected", selected);
        surface.style.backgroundColor = selected ? SelectedLanguageBackground : UnselectedLanguageBackground;
        Color border = selected ? SelectedLanguageBorder : UnselectedLanguageBorder;
        surface.style.borderTopColor = border;
        surface.style.borderRightColor = border;
        surface.style.borderBottomColor = border;
        surface.style.borderLeftColor = border;
    }

    private void ToggleConsent()
    {
        consentAccepted = !consentAccepted;
        ApplyConsentState();
        ConsentChanged?.Invoke(consentAccepted);
        Debug.Log("Gesture-tracking consent " + (consentAccepted ? "accepted" : "cleared") + " for this session.", this);
    }

    private void ApplyConsentState()
    {
        ApplyCheckbox(englishCheckbox, englishCheckmark, consentAccepted);
        ApplyCheckbox(chineseCheckbox, chineseCheckmark, consentAccepted);

        if (interactionGroups == null)
            return;

        if (englishReviewElements != null)
            englishReviewElements.SetUnavailable(!consentAccepted);
        if (chineseReviewElements != null)
            chineseReviewElements.SetUnavailable(!consentAccepted);
    }

    private InteractionGroup englishReviewElements;
    private InteractionGroup chineseReviewElements;

    private static void ApplyCheckbox(VisualElement checkbox, Label checkmark, bool checkedState)
    {
        if (checkbox == null || checkmark == null)
            return;

        checkbox.EnableInClassList("ac-checked", checkedState);
        checkbox.style.backgroundColor = checkedState ? CheckedColor : UncheckedColor;
        Color border = checkedState ? CheckedColor : CheckboxBorder;
        checkbox.style.borderTopColor = border;
        checkbox.style.borderRightColor = border;
        checkbox.style.borderBottomColor = border;
        checkbox.style.borderLeftColor = border;
        checkmark.style.display = checkedState ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void ReviewConsent()
    {
        if (!consentAccepted)
        {
            Debug.Log("Review Consent & Start is unavailable until gesture-tracking consent is accepted.", this);
            return;
        }

        Raise(ReviewConsentRequested);
    }

    public void SetHomeVisible(bool visible)
    {
        if (!visible)
            ResetTransientStates();

        if (englishRoot != null)
        {
            englishRoot.SetEnabled(visible && currentLanguage == Language.English);
            englishRoot.style.display = visible && currentLanguage == Language.English ? DisplayStyle.Flex : DisplayStyle.None;
        }
        if (chineseRoot != null)
        {
            chineseRoot.SetEnabled(visible && currentLanguage == Language.Chinese);
            chineseRoot.style.display = visible && currentLanguage == Language.Chinese ? DisplayStyle.Flex : DisplayStyle.None;
        }
        if (visible)
        {
            VisualElement focusTarget = currentLanguage == Language.English ? englishSelectorSurface : chineseScreenChineseSelectorSurface;
            if (focusTarget != null)
            {
                focusTarget.Focus();
                focusTarget.AddToClassList("ac-keyboard-focus");
            }
        }
    }

    public void SetLanguageFromApplication(AppLanguage language)
    {
        Language target = language == AppLanguage.English ? Language.English : Language.Chinese;
        if (currentLanguage != target)
            ApplyLanguage(target);
    }

    public void SetConsentFromApplication(bool accepted)
    {
        if (consentAccepted == accepted)
            return;
        consentAccepted = accepted;
        ApplyConsentState();
        ConsentChanged?.Invoke(consentAccepted);
    }

    private static void Raise(Action callback)
    {
        callback?.Invoke();
    }

    private void ResetTransientStates()
    {
        if (interactionGroups == null)
            return;

        foreach (InteractionGroup group in interactionGroups)
            group.ResetTransientState();
    }

    private void FailSafe()
    {
        if (englishRoot != null)
        {
            englishRoot.SetEnabled(false);
            englishRoot.style.display = DisplayStyle.None;
        }
        if (chineseRoot != null)
        {
            chineseRoot.SetEnabled(false);
            chineseRoot.style.display = DisplayStyle.None;
        }
    }
}
