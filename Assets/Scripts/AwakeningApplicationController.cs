using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[DefaultExecutionOrder(-100)]
public sealed class AwakeningApplicationController : MonoBehaviour
{
    private const string HomeScreen = "Home";
    private const string PrivacyScreen = "Privacy";
    private const string SettingsScreen = "SETTINGS---EXPERIENCE-CONTROLS";
    private const string PerformerSelectScreen = "PERFORMER-SELECT---FALLBACK-INPUT";
    private const string AttractScreen = "ATTRACT---MONOCHROME---FALLBACK-INPUT";
    private const string ColorReleaseScreen = "COLOR-RELEASE";
    private const string AssistantChooserScreen = "AssistantChooser";
    private static readonly string[] TutorialScreens =
    {
        "TUTORIAL---ENTRY---DIM",
        "TUTORIAL---INTRODUCTION",
        "TUTORIAL---LOOK---MOVE",
        "TUTORIAL---POINT",
        "TUTORIAL---HOLD---SELECT",
        "TUTORIAL---EXPERIENCE",
        "TUTORIAL---READY"
    };

    private static readonly string[] Performers =
    {
        "GAIA", "PANDEMIC", "RE-BIRTH", "WHITE-GUARDIAN", "CONSCIENCE", "SANCTUARY"
    };

    private static readonly string[] PerformerCodes =
    {
        "PER-001", "PER-002", "PER-003", "PER-004", "PER-005", "PER-006"
    };

    private static readonly string[] Stages =
    {
        "FOCUS-VIEW", "ACTIVATION", "LIVING-PORTRAIT", "RELEASE", "POST-VIDEO-AI"
    };

    [SerializeField] private UIDocument navigationDocument;
    [SerializeField] private RuntimeScreenCatalog screenCatalog;
    [SerializeField] private HomeLanguageController homeController;

    private readonly Stack<RouteState> backStack = new Stack<RouteState>();
    private readonly List<Action> screenUnbind = new List<Action>();
    private readonly Stack<OverlayState> overlays = new Stack<OverlayState>();
    private readonly Stack<List<Action>> overlayUnbind = new Stack<List<Action>>();
    private AppLanguage language = AppLanguage.English;
    private string currentScreen = HomeScreen;
    private string activeBindingScreen = HomeScreen;
    private int selectedPerformer = -1;
    private bool consentAccepted;
    private bool hasAwakenedThisSession;
    private ManifestSnapshot nodeSnapshot;
    private StyleSheet interactionStyles;
    private bool interactionStylesAdded;

    public string CurrentScreen { get { return currentScreen; } }
    public AppLanguage Language { get { return language; } }
    public int SelectedPerformer { get { return selectedPerformer; } }
    public bool HasAwakenedThisSession { get { return hasAwakenedThisSession; } }

    private sealed class RouteState
    {
        public string screen;
        public AppLanguage language;
    }

    private sealed class OverlayState
    {
        public VisualElement tree;
        public VisualElement baseRoot;
    }

    [Serializable]
    private sealed class ManifestSnapshot
    {
        public ManifestScreen[] screens;
    }

    [Serializable]
    private sealed class ManifestScreen
    {
        public string rootNodeId;
        public string assetName;
        public ManifestNode[] nodes;
    }

    [Serializable]
    private sealed class ManifestNode
    {
        public string figmaNodeId;
        public string path;
        public string tag;
        public string classes;
    }

    private void Awake()
    {
        if (screenCatalog == null)
            screenCatalog = Resources.Load<RuntimeScreenCatalog>("RuntimeScreenCatalog");
        TextAsset snapshot = Resources.Load<TextAsset>("RuntimeNodeBindings");
        if (snapshot != null)
            nodeSnapshot = JsonUtility.FromJson<ManifestSnapshot>(snapshot.text);
        interactionStyles = Resources.Load<StyleSheet>("HomeInteraction");
        if (homeController == null)
            homeController = FindObjectOfType<HomeLanguageController>();

        if (navigationDocument == null)
        {
            UIDocument[] documents = FindObjectsOfType<UIDocument>(true);
            for (int i = 0; i < documents.Length; i++)
            {
                if (documents[i].gameObject.name == "UIDocument")
                {
                    navigationDocument = documents[i];
                    break;
                }
            }
        }

        if (navigationDocument != null)
        {
            navigationDocument.sortingOrder = 10;
            navigationDocument.enabled = false;
        }
    }

    private void OnEnable()
    {
        HomeLanguageController.LanguageChanged += OnLanguageChanged;
        HomeLanguageController.PrivacyRequested += OnPrivacyRequested;
        HomeLanguageController.SettingsRequested += OnSettingsRequested;
        HomeLanguageController.ReviewConsentRequested += OnReviewConsentRequested;
        HomeLanguageController.ConsentChanged += OnConsentChanged;
    }

    private void OnDisable()
    {
        HomeLanguageController.LanguageChanged -= OnLanguageChanged;
        HomeLanguageController.PrivacyRequested -= OnPrivacyRequested;
        HomeLanguageController.SettingsRequested -= OnSettingsRequested;
        HomeLanguageController.ReviewConsentRequested -= OnReviewConsentRequested;
        HomeLanguageController.ConsentChanged -= OnConsentChanged;
        ClearBindings(screenUnbind);
        while (overlays.Count > 0)
            CloseTopOverlay();
    }

    private IEnumerator Start()
    {
        yield return null;
        if (screenCatalog == null)
            Debug.LogError("AwakeningApplicationController: Resources/RuntimeScreenCatalog.asset is missing.", this);
        if (navigationDocument == null)
            Debug.LogError("AwakeningApplicationController: Assign the navigation UIDocument in the scene.", this);
        if (homeController == null)
            Debug.LogError("AwakeningApplicationController: HomeLanguageController was not found.", this);

        if (homeController != null)
            homeController.SetHomeVisible(true);
    }

    private void OnLanguageChanged(AppLanguage selectedLanguage)
    {
        if (language == selectedLanguage)
            return;
        language = selectedLanguage;

        if (overlays.Count > 0)
        {
            while (overlays.Count > 0)
                CloseTopOverlay();
        }

        if (currentScreen != HomeScreen)
            Navigate(currentScreen, language, false, true);
    }

    private void OnConsentChanged(bool accepted)
    {
        consentAccepted = accepted;
    }

    private void OnPrivacyRequested()
    {
        Navigate(PrivacyScreen, language, true);
    }

    private void OnSettingsRequested()
    {
        Navigate(SettingsScreen, language, true);
    }

    private void OnReviewConsentRequested()
    {
        Navigate(PrivacyScreen, language, true);
    }

    public bool Navigate(string screenId, AppLanguage targetLanguage, bool recordHistory, bool preserveHistory = false)
    {
        VisualTreeAsset asset;
        if (screenCatalog == null || !screenCatalog.TryGet(screenId, targetLanguage, out asset))
        {
            Debug.LogError("AwakeningApplicationController: No " + targetLanguage + " asset is registered for logical screen '" + screenId + "'.", this);
            return false;
        }
        if (navigationDocument == null)
            return false;

        while (overlays.Count > 0)
            CloseTopOverlay();
        if (recordHistory && currentScreen != HomeScreen)
            backStack.Push(new RouteState { screen = currentScreen, language = language });
        else if (!recordHistory && !preserveHistory)
            backStack.Clear();

        ClearBindings(screenUnbind);
        navigationDocument.enabled = false;
        navigationDocument.visualTreeAsset = asset;
        navigationDocument.enabled = true;
        ApplyInteractionStyles();
        currentScreen = screenId;
        activeBindingScreen = screenId;
        language = targetLanguage;
        if (homeController != null)
            homeController.SetHomeVisible(false);
        BindCurrentScreen();
        return true;
    }

    public void ShowHome(bool recordHistory)
    {
        if (recordHistory && currentScreen != HomeScreen)
            backStack.Push(new RouteState { screen = currentScreen, language = language });
        else if (!recordHistory)
            backStack.Clear();
        ClearBindings(screenUnbind);
        while (overlays.Count > 0)
            CloseTopOverlay();
        if (navigationDocument != null)
            navigationDocument.enabled = false;
        currentScreen = HomeScreen;
        activeBindingScreen = HomeScreen;
        if (homeController != null)
            homeController.SetHomeVisible(true);
    }

    public void Back()
    {
        if (overlays.Count > 0)
        {
            CloseTopOverlay();
            return;
        }
        if (backStack.Count == 0)
        {
            ShowHome(false);
            return;
        }
        RouteState previous = backStack.Pop();
        if (previous.screen == HomeScreen)
            ShowHome(false);
        else
            Navigate(previous.screen, previous.language, false, true);
    }

    private void BindCurrentScreen()
    {
        VisualElement root = GetUxmlRoot(navigationDocument.rootVisualElement);
        if (root == null)
            return;

        activeBindingScreen = currentScreen;
        root.AddToClassList("ac-screen-runtime");
        root.AddToClassList("ac-interactive");
        EventCallback<FocusInEvent> rootFocusIn = evt => root.AddToClassList("ac-keyboard-focus");
        EventCallback<FocusOutEvent> rootFocusOut = evt => root.RemoveFromClassList("ac-keyboard-focus");
        root.RegisterCallback(rootFocusIn);
        root.RegisterCallback(rootFocusOut);
        screenUnbind.Add(() => root.UnregisterCallback(rootFocusIn));
        screenUnbind.Add(() => root.UnregisterCallback(rootFocusOut));
        root.focusable = false;
        root.tabIndex = -1;
        RegisterKeyHandling(root, screenUnbind);
        if (currentScreen == PrivacyScreen)
        {
            BindPath(root, "1/26", "VisualElement", language == AppLanguage.English ? "51:65" : "53:63", () => ContinueFromPrivacy(), screenUnbind);
            BindPath(root, "1/27", "Label", language == AppLanguage.English ? "51:66" : "53:64", () => ContinueFromPrivacy(), screenUnbind, false);
            BindPath(root, "1/28", "VisualElement", language == AppLanguage.English ? "51:67" : "53:65", () =>
            {
                if (homeController != null)
                    homeController.SetConsentFromApplication(false);
                else
                    consentAccepted = false;
                ShowHome(false);
            }, screenUnbind);
            BindPath(root, "1/29", "Label", language == AppLanguage.English ? "51:68" : "53:66", () =>
            {
                if (homeController != null)
                    homeController.SetConsentFromApplication(false);
                else
                    consentAccepted = false;
                ShowHome(false);
            }, screenUnbind, false);
            BindPath(root, "1/30", "VisualElement", language == AppLanguage.English ? "51:69" : "53:67", () => ShowHome(false), screenUnbind);
            BindPath(root, "1/31", "Label", language == AppLanguage.English ? "51:70" : "53:68", () => ShowHome(false), screenUnbind, false);
            FocusFirstFocusable(root);
            return;
        }

        int tutorialIndex = ArrayIndexOf(TutorialScreens, currentScreen);
        if (tutorialIndex >= 0)
        {
            if (tutorialIndex == TutorialScreens.Length - 1)
            {
                BindPath(root, "1/29", "VisualElement", language == AppLanguage.English ? "24:602" : "105:700", () => Navigate(AttractScreen, language, true), screenUnbind);
                BindPath(root, "1/29/1", "Label", language == AppLanguage.English ? "24:603" : "105:701", () => Navigate(AttractScreen, language, true), screenUnbind, false);
                BindPath(root, "1/30", "Label", language == AppLanguage.English ? "24:604" : "105:702", () => Navigate(AttractScreen, language, true), screenUnbind);
                RegisterKeyOnRoot(root, () => Navigate(AttractScreen, language, true), screenUnbind);
            }
            else
            {
                if (tutorialIndex == 0)
                {
                    BindPath(root, "1/29/5", "VisualElement", language == AppLanguage.English ? "24:315" : "105:341", () => Navigate(TutorialScreens[1], language, true), screenUnbind);
                    BindPath(root, "1/29/5/1", "Label", language == AppLanguage.English ? "24:316" : "105:342", () => Navigate(TutorialScreens[1], language, true), screenUnbind, false);
                }
                else
                {
                    string[] englishGuideIds = { "24:377", "24:425", "24:475", "24:521", "24:567" };
                    string[] chineseGuideIds = { "105:403", "105:481", "105:542", "105:597", "105:653" };
                    string[] guidePaths = { "1/37", "1/41", "1/43", "1/39", "1/39" };
                    int guideIndex = tutorialIndex - 1;
                    BindPath(root, guidePaths[guideIndex], "Label", language == AppLanguage.English ? englishGuideIds[guideIndex] : chineseGuideIds[guideIndex], () => Navigate(TutorialScreens[tutorialIndex + 1], language, true), screenUnbind);
                }
                RegisterKeyOnRoot(root, () => Navigate(TutorialScreens[tutorialIndex + 1], language, true), screenUnbind);
            }
            FocusFirstFocusable(root);
            return;
        }

        if (currentScreen == "TUTORIAL---RETURN---DISMISS")
        {
            BindPath(root, "1/28/3", "Label", language == AppLanguage.English ? "24:644" : "105:742", () => Navigate("TUTORIAL---INTRODUCTION", language, false), screenUnbind);
            RegisterKeyOnRoot(root, () => Navigate("TUTORIAL---INTRODUCTION", language, false), screenUnbind);
            FocusFirstFocusable(root);
            return;
        }

        if (currentScreen == AttractScreen)
        {
            Action advance = () => Navigate(hasAwakenedThisSession ? PerformerSelectScreen : ColorReleaseScreen, language, true);
            RegisterClick(root, advance, screenUnbind);
            RegisterKeyOnRoot(root, advance, screenUnbind);
            FocusFirstFocusable(root);
            return;
        }
        if (currentScreen == ColorReleaseScreen)
        {
            Action completeAwakening = () =>
            {
                hasAwakenedThisSession = true;
                Navigate(PerformerSelectScreen, language, true);
            };
            RegisterClick(root, completeAwakening, screenUnbind);
            RegisterKeyOnRoot(root, completeAwakening, screenUnbind);
            FocusFirstFocusable(root);
            return;
        }

        if (currentScreen == PerformerSelectScreen || currentScreen == AssistantChooserScreen)
        {
            BindPerformerChoices(root, currentScreen == AssistantChooserScreen, screenUnbind);
            if (currentScreen == AssistantChooserScreen)
            {
                BindPath(root, "1/23", "Label", language == AppLanguage.English ? "124:5184" : "124:5212", () => Navigate(SettingsScreen, language, false), screenUnbind);
            }
            FocusFirstFocusable(root);
            return;
        }

        if (currentScreen == SettingsScreen)
        {
            int localeNodeOffset = language == AppLanguage.English ? 0 : 54;
            Action switchLanguage = () =>
            {
                if (homeController != null)
                    homeController.SetLanguageFromApplication(language == AppLanguage.English ? AppLanguage.Chinese : AppLanguage.English);
            };
            BindPath(root, "1/7/2", "VisualElement", "124:" + (5227 + localeNodeOffset), switchLanguage, screenUnbind);
            BindPath(root, "1/7/4", "Label", "124:" + (5229 + localeNodeOffset), switchLanguage, screenUnbind, false);
            BindPath(root, "1/7/6", "VisualElement", "124:" + (5231 + localeNodeOffset), switchLanguage, screenUnbind, false);
            BindPath(root, "1/7/7", "Label", "124:" + (5232 + localeNodeOffset), switchLanguage, screenUnbind, false);

            // These imported controls have no verified effect or service in the
            // current prototype/project, so expose them as unavailable, not active.
            DisableSettingsControls(root, localeNodeOffset);

            string privacyId = "124:" + (5260 + localeNodeOffset);
            string privacySurfaceId = "124:" + (5257 + localeNodeOffset);
            string assistantId = "124:" + (5266 + localeNodeOffset);
            string assistantSurfaceId = "124:" + (5265 + localeNodeOffset);
            string backId = language == AppLanguage.English ? "124:5269" : "124:5323";
            BindPath(root, "1/7/32", "VisualElement", privacySurfaceId, () => Navigate(PrivacyScreen, language, true), screenUnbind);
            BindPath(root, "1/7/35", "Label", privacyId, () => Navigate(PrivacyScreen, language, true), screenUnbind, false);
            BindPath(root, "1/8/4", "VisualElement", assistantSurfaceId, () => Navigate(AssistantChooserScreen, language, true), screenUnbind);
            BindPath(root, "1/8/5", "Label", assistantId, () => Navigate(AssistantChooserScreen, language, true), screenUnbind, false);
            BindPath(root, "1/9", "Label", backId, () => Navigate(PerformerSelectScreen, language, false), screenUnbind);
            FocusFirstFocusable(root);
            return;
        }

        if (IsPerformerStage(currentScreen, out int performer, out int stage))
        {
            selectedPerformer = performer;
            string actionPath;
            int nodeOffset;
            switch (stage)
            {
                case 0: actionPath = "1/14"; nodeOffset = 14; break; // Gesture cue
                case 1: actionPath = "1/14"; nodeOffset = 14; break; // Gesture cue
                case 2: actionPath = "1/15"; nodeOffset = 15; break; // Return cue
                case 3: actionPath = "1/11"; nodeOffset = 11; break; // Interaction cue
                default: actionPath = "1/6/11"; nodeOffset = 17; break; // Continue
            }
            RuntimeScreenCatalog.Entry stageEntry;
            string nodeId = null;
            if (screenCatalog != null && screenCatalog.TryGetEntry(currentScreen, language, out stageEntry))
            {
                string[] rootParts = stageEntry.figmaRootNodeId.Split(':');
                int rootIndex;
                if (rootParts.Length == 2 && int.TryParse(rootParts[1], out rootIndex))
                    nodeId = rootParts[0] + ":" + (rootIndex + nodeOffset);
            }
            if (nodeId == null)
            {
                Debug.LogError("Could not derive the verified stage action node for " + currentScreen + ".", this);
                return;
            }
            Action advanceStage = () => AdvancePerformerStage(performer, stage);
            BindPath(root, actionPath, "Label", nodeId, advanceStage, screenUnbind);
            RegisterKeyOnRoot(root, advanceStage, screenUnbind);
            FocusFirstFocusable(root);
            return;
        }

        if (currentScreen == "CLOSING---CELEBRATION")
        {
            BindPath(root, "1/7/3", "VisualElement", language == AppLanguage.English ? "124:5337" : "124:5351", () => Navigate("SOUVENIR---QR-DELIVERY", language, true), screenUnbind);
            BindPath(root, "1/7/4", "Label", language == AppLanguage.English ? "124:5338" : "124:5352", () => Navigate("SOUVENIR---QR-DELIVERY", language, true), screenUnbind, false);
            BindPath(root, "1/7/5", "VisualElement", language == AppLanguage.English ? "124:5339" : "124:5353", () => Navigate(PerformerSelectScreen, language, false), screenUnbind);
            BindPath(root, "1/7/6", "Label", language == AppLanguage.English ? "124:5340" : "124:5354", () => Navigate(PerformerSelectScreen, language, false), screenUnbind, false);
            FocusFirstFocusable(root);
            return;
        }

        if (currentScreen == "SOUVENIR---QR-DELIVERY")
        {
            BindPath(root, "1/7/7", "VisualElement", language == AppLanguage.English ? "124:5431" : "124:5510", () => Navigate(PerformerSelectScreen, language, false), screenUnbind);
            BindPath(root, "1/7/8", "Label", language == AppLanguage.English ? "124:5432" : "124:5511", () => Navigate(PerformerSelectScreen, language, false), screenUnbind, false);
        }
        FocusFirstFocusable(root);
    }

    private void ContinueFromPrivacy()
    {
        if (!consentAccepted)
        {
            Debug.LogWarning("Consent is required before continuing. Return Home and select the consent checkbox first.", this);
            return;
        }
        Navigate(TutorialScreens[0], language, true);
    }

    private void BindPerformerChoices(VisualElement root, bool assistantChoice, List<Action> cleanup)
    {
        for (int i = 0; i < Performers.Length; i++)
        {
            int performerIndex = i;
            string surfacePath = "1/" + (5 + (i * 3));
            string labelPath = "1/" + (6 + (i * 3));
            int surfaceNode = (assistantChoice
                ? (language == AppLanguage.English ? 5166 : 5194)
                : (language == AppLanguage.English ? 3872 : 3900)) + (i * 3);
            int labelNode = surfaceNode + 1;
            BindPath(root, surfacePath, "VisualElement", "124:" + surfaceNode, () => SelectPerformer(performerIndex, assistantChoice), cleanup);
            BindPath(root, labelPath, "Label", "124:" + labelNode, () => SelectPerformer(performerIndex, assistantChoice), cleanup, false);
            BindPath(root, "1/" + (7 + (i * 3)), "Label", "124:" + (labelNode + 1), () => SelectPerformer(performerIndex, assistantChoice), cleanup, false);
        }
    }

    private void SelectPerformer(int index, bool openAssistant)
    {
        selectedPerformer = index;
        if (openAssistant)
        {
            OpenAssistantOverlay(index);
            return;
        }
        Navigate(GetPerformerScreen(index, 0), language, true);
    }

    private void AdvancePerformerStage(int performer, int stage)
    {
        if (stage < Stages.Length - 1)
            Navigate(GetPerformerScreen(performer, stage + 1), language, true);
        else
            Navigate("CLOSING---CELEBRATION", language, true);
    }

    private string GetPerformerScreen(int performer, int stage)
    {
        return Performers[performer] + "---" + Stages[stage] + "---" + PerformerCodes[performer];
    }

    private bool IsPerformerStage(string screen, out int performer, out int stage)
    {
        for (int p = 0; p < Performers.Length; p++)
        {
            for (int s = 0; s < Stages.Length; s++)
            {
                if (screen == GetPerformerScreen(p, s))
                {
                    performer = p;
                    stage = s;
                    return true;
                }
            }
        }
        performer = -1;
        stage = -1;
        return false;
    }

    private void OpenAssistantOverlay(int performer)
    {
        string overlayId = "Assistant-" + Performers[performer];
        VisualTreeAsset asset;
        if (screenCatalog == null || !screenCatalog.TryGet(overlayId, language, out asset))
        {
            Debug.LogError("No assistant overlay is registered for " + Performers[performer] + " / " + language + ".", this);
            return;
        }

        VisualElement tree = asset.Instantiate();
        VisualElement panelRoot = navigationDocument.rootVisualElement;
        VisualElement baseRoot = GetUxmlRoot(panelRoot);
        if (baseRoot != null)
            baseRoot.SetEnabled(false);
        panelRoot.Add(tree);
        overlays.Push(new OverlayState { tree = tree, baseRoot = baseRoot });
        List<Action> cleanup = new List<Action>();
        overlayUnbind.Push(cleanup);
        VisualElement importedRoot = GetUxmlRoot(tree);
        int baseNode = language == AppLanguage.English ? 4970 + performer * 32 : 4986 + performer * 32;
        activeBindingScreen = overlayId;
        importedRoot.AddToClassList("ac-screen-runtime");
        importedRoot.focusable = false;
        importedRoot.tabIndex = -1;
        DisableAssistantControls(importedRoot, baseNode);
        BindPath(importedRoot, "1/14", "VisualElement", "124:" + (baseNode + 13), CloseTopOverlay, cleanup);
        BindPath(importedRoot, "1/15", "Label", "124:" + (baseNode + 14), CloseTopOverlay, cleanup, false);
        EventCallback<KeyDownEvent> overlayKey = evt =>
        {
            if (evt.keyCode == KeyCode.Escape)
            {
                evt.StopPropagation();
                CloseTopOverlay();
            }
        };
        importedRoot.RegisterCallback(overlayKey);
        cleanup.Add(() => importedRoot.UnregisterCallback(overlayKey));
        for (int i = 0; i < importedRoot.childCount; i++)
        {
            VisualElement child = importedRoot[i];
            if (child.focusable)
            {
                child.Focus();
                break;
            }
        }
        activeBindingScreen = currentScreen;
        Debug.Log("Opened assistant overlay for " + Performers[performer] + ". Assistant responses require an unconfigured service.", this);
    }

    private void CloseTopOverlay()
    {
        if (overlays.Count == 0)
            return;
        OverlayState state = overlays.Pop();
        if (overlayUnbind.Count > 0)
            ClearBindings(overlayUnbind.Pop());
        if (state.tree != null)
            state.tree.RemoveFromHierarchy();
        if (state.baseRoot != null)
        {
            state.baseRoot.SetEnabled(true);
            FocusFirstFocusable(state.baseRoot);
        }
        activeBindingScreen = currentScreen;
    }

    private static VisualElement GetUxmlRoot(VisualElement documentRoot)
    {
        if (documentRoot == null)
            return null;
        if (documentRoot.childCount == 1)
            return documentRoot[0];
        return documentRoot;
    }

    private static bool FocusFirstFocusable(VisualElement root)
    {
        if (root == null)
            return false;
        if (root.focusable)
        {
            root.Focus();
            return true;
        }
        for (int i = 0; i < root.childCount; i++)
            if (FocusFirstFocusable(root[i]))
                return true;
        return false;
    }

    private void BindPath(VisualElement root, string path, string expectedTag, string figmaNodeId, Action action, List<Action> cleanup, bool keyboardFocusable = true)
    {
        string ownerRootNodeId;
        ManifestNode mapping = FindManifestNode(figmaNodeId, out ownerRootNodeId);
        if (mapping == null || mapping.path != path || mapping.tag != expectedTag)
        {
            Debug.LogError("Figma snapshot validation failed for node " + figmaNodeId + ": expected " + path + " / " + expectedTag + ". Revalidate the connector import before binding.", this);
            return;
        }
        RuntimeScreenCatalog.Entry ownerEntry;
        if (screenCatalog == null || !screenCatalog.TryGetEntry(activeBindingScreen, language, out ownerEntry) || ownerEntry.figmaRootNodeId != ownerRootNodeId)
        {
            Debug.LogError("Figma binding rejected: node " + figmaNodeId + " does not belong to the registered screen '" + activeBindingScreen + "'.", this);
            return;
        }

        VisualElement element;
        if (!TryResolvePath(root, path, out element))
        {
            Debug.LogError("Figma binding failed: node " + figmaNodeId + " at " + activeBindingScreen + "/" + path + " is not present.", this);
            return;
        }
        string actualTag = element is Label ? "Label" : element.GetType() == typeof(VisualElement) ? "VisualElement" : element.GetType().Name;
        if (actualTag != expectedTag)
        {
            Debug.LogError("Figma binding rejected: node " + figmaNodeId + " at " + path + " is " + actualTag + ", expected " + expectedTag + ".", this);
            return;
        }

        element.AddToClassList("ac-interactive");
        EventCallback<FocusInEvent> focusIn = evt => element.AddToClassList("ac-keyboard-focus");
        EventCallback<FocusOutEvent> focusOut = evt => element.RemoveFromClassList("ac-keyboard-focus");
        element.RegisterCallback(focusIn);
        element.RegisterCallback(focusOut);
        cleanup.Add(() => element.UnregisterCallback(focusIn));
        cleanup.Add(() => element.UnregisterCallback(focusOut));
        element.focusable = keyboardFocusable;
        element.tabIndex = keyboardFocusable ? 0 : -1;
        EventCallback<PointerEnterEvent> pointerEnter = evt =>
        {
            element.AddToClassList("ac-hover");
            if (evt.pressedButtons != 0)
                element.AddToClassList("ac-pressed");
        };
        EventCallback<PointerLeaveEvent> pointerLeave = evt =>
        {
            element.RemoveFromClassList("ac-hover");
            element.RemoveFromClassList("ac-pressed");
        };
        EventCallback<PointerDownEvent> pointerDown = evt =>
        {
            if (evt.button == 0)
                element.AddToClassList("ac-pressed");
        };
        EventCallback<PointerUpEvent> pointerUp = evt =>
        {
            if (evt.button == 0 || evt.pressedButtons == 0)
                element.RemoveFromClassList("ac-pressed");
        };
        EventCallback<PointerCancelEvent> pointerCancel = evt => element.RemoveFromClassList("ac-pressed");
        element.RegisterCallback(pointerEnter);
        element.RegisterCallback(pointerLeave);
        element.RegisterCallback(pointerDown);
        element.RegisterCallback(pointerUp);
        element.RegisterCallback(pointerCancel);
        cleanup.Add(() => element.UnregisterCallback(pointerEnter));
        cleanup.Add(() => element.UnregisterCallback(pointerLeave));
        cleanup.Add(() => element.UnregisterCallback(pointerDown));
        cleanup.Add(() => element.UnregisterCallback(pointerUp));
        cleanup.Add(() => element.UnregisterCallback(pointerCancel));
        EventCallback<ClickEvent> callback = evt =>
        {
            evt.StopPropagation();
            action();
        };
        element.RegisterCallback(callback);
        cleanup.Add(() => element.UnregisterCallback(callback));
        EventCallback<KeyDownEvent> keyCallback = evt =>
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Space)
            {
                evt.StopPropagation();
                action();
            }
        };
        element.RegisterCallback(keyCallback);
        cleanup.Add(() => element.UnregisterCallback(keyCallback));
    }

    private ManifestNode FindManifestNode(string figmaNodeId, out string ownerRootNodeId)
    {
        ownerRootNodeId = null;
        if (nodeSnapshot == null || nodeSnapshot.screens == null)
        {
            Debug.LogError("RuntimeNodeBindings.json could not be loaded; no imported control will be bound.", this);
            return null;
        }
        for (int i = 0; i < nodeSnapshot.screens.Length; i++)
        {
            ManifestNode[] nodes = nodeSnapshot.screens[i].nodes;
            if (nodes == null)
                continue;
            for (int j = 0; j < nodes.Length; j++)
                if (nodes[j].figmaNodeId == figmaNodeId)
                {
                    ownerRootNodeId = nodeSnapshot.screens[i].rootNodeId;
                    return nodes[j];
                }
        }
        return null;
    }

    private void ApplyInteractionStyles()
    {
        if (interactionStylesAdded || interactionStyles == null || navigationDocument == null || navigationDocument.rootVisualElement == null)
            return;
        navigationDocument.rootVisualElement.styleSheets.Add(interactionStyles);
        interactionStylesAdded = true;
    }

    private void RegisterClick(VisualElement element, Action action, List<Action> cleanup)
    {
        element.AddToClassList("ac-touch-surface");
        element.focusable = true;
        element.tabIndex = 0;
        EventCallback<PointerEnterEvent> pointerEnter = evt =>
        {
            element.AddToClassList("ac-hover");
            if (evt.pressedButtons != 0)
                element.AddToClassList("ac-pressed");
        };
        EventCallback<PointerLeaveEvent> pointerLeave = evt =>
        {
            element.RemoveFromClassList("ac-hover");
            element.RemoveFromClassList("ac-pressed");
        };
        EventCallback<PointerDownEvent> pointerDown = evt =>
        {
            if (evt.button == 0)
                element.AddToClassList("ac-pressed");
        };
        EventCallback<PointerUpEvent> pointerUp = evt =>
        {
            if (evt.button == 0 || evt.pressedButtons == 0)
                element.RemoveFromClassList("ac-pressed");
        };
        EventCallback<PointerCancelEvent> pointerCancel = evt => element.RemoveFromClassList("ac-pressed");
        element.RegisterCallback(pointerEnter);
        element.RegisterCallback(pointerLeave);
        element.RegisterCallback(pointerDown);
        element.RegisterCallback(pointerUp);
        element.RegisterCallback(pointerCancel);
        cleanup.Add(() => element.UnregisterCallback(pointerEnter));
        cleanup.Add(() => element.UnregisterCallback(pointerLeave));
        cleanup.Add(() => element.UnregisterCallback(pointerDown));
        cleanup.Add(() => element.UnregisterCallback(pointerUp));
        cleanup.Add(() => element.UnregisterCallback(pointerCancel));

        EventCallback<ClickEvent> callback = evt =>
        {
            evt.StopPropagation();
            element.RemoveFromClassList("ac-pressed");
            action();
        };
        element.RegisterCallback(callback);
        cleanup.Add(() => element.UnregisterCallback(callback));
    }

    private void DisableSettingsControls(VisualElement root, int localeNodeOffset)
    {
        string unavailable = language == AppLanguage.English ? "Unavailable" : "目前無法使用";
        string guideUnavailable = language == AppLanguage.English ? "GUIDE UNAVAILABLE" : "指南目前無法使用";
        string[][] controls =
        {
            // Sound: row surface, title, value, supporting copy, action surface, action label.
            new[] { "1/7/8", "VisualElement", "5233", null },
            new[] { "1/7/9", "Label", "5234", null },
            new[] { "1/7/10", "Label", "5235", unavailable },
            new[] { "1/7/11", "Label", "5236", null },
            new[] { "1/7/12", "VisualElement", "5237", null },
            new[] { "1/7/13", "Label", "5238", unavailable },
            // Accessibility.
            new[] { "1/7/14", "VisualElement", "5239", null },
            new[] { "1/7/15", "Label", "5240", null },
            new[] { "1/7/16", "Label", "5241", unavailable },
            new[] { "1/7/17", "Label", "5242", null },
            new[] { "1/7/18", "VisualElement", "5243", null },
            new[] { "1/7/19", "Label", "5244", unavailable },
            // Tracking and recalibration.
            new[] { "1/7/20", "VisualElement", "5245", null },
            new[] { "1/7/21", "Label", "5246", null },
            new[] { "1/7/22", "Label", "5247", unavailable },
            new[] { "1/7/23", "Label", "5248", null },
            new[] { "1/7/24", "VisualElement", "5249", null },
            new[] { "1/7/25", "Label", "5250", unavailable },
            // Restart and its unavailable action.
            new[] { "1/7/26", "VisualElement", "5251", null },
            new[] { "1/7/28", "Label", "5253", null },
            new[] { "1/7/29", "Label", "5254", null },
            new[] { "1/7/30", "VisualElement", "5255", null },
            new[] { "1/7/31", "Label", "5256", unavailable },
            // Imported guide affordance has no verified destination/action.
            new[] { "1/10", "VisualElement", "5270", null },
            new[] { "1/10/1", "Label", "5271", guideUnavailable }
        };

        for (int i = 0; i < controls.Length; i++)
        {
            string[] control = controls[i];
            MarkUnavailable(root, control[0], control[1], "124:" + (int.Parse(control[2]) + localeNodeOffset), control[3]);
        }
    }

    private void DisableAssistantControls(VisualElement root, int baseNode)
    {
        string unavailable = language == AppLanguage.English ? "Unavailable" : "目前無法使用";
        string suggestionUnavailable = language == AppLanguage.English ? "Assistant unavailable" : "助理服務目前無法使用";
        string[][] controls =
        {
            new[] { "1/4", "VisualElement", "3", null },
            new[] { "1/5", "Label", "4", suggestionUnavailable },
            new[] { "1/6", "VisualElement", "5", null },
            new[] { "1/7", "Label", "6", suggestionUnavailable },
            new[] { "1/8", "VisualElement", "7", null },
            new[] { "1/9", "Label", "8", suggestionUnavailable },
            new[] { "1/10", "VisualElement", "9", null },
            new[] { "1/11", "Label", "10", suggestionUnavailable },
            new[] { "1/12", "VisualElement", "11", null },
            new[] { "1/13", "Label", "12", unavailable }
        };
        for (int i = 0; i < controls.Length; i++)
        {
            string[] control = controls[i];
            MarkUnavailable(root, control[0], control[1], "124:" + (baseNode + int.Parse(control[2])), control[3]);
        }
    }

    private void MarkUnavailable(VisualElement root, string path, string expectedTag, string figmaNodeId, string replacementText)
    {
        string ownerRootNodeId;
        ManifestNode mapping = FindManifestNode(figmaNodeId, out ownerRootNodeId);
        RuntimeScreenCatalog.Entry ownerEntry;
        if (mapping == null || mapping.path != path || mapping.tag != expectedTag ||
            screenCatalog == null || !screenCatalog.TryGetEntry(activeBindingScreen, language, out ownerEntry) ||
            ownerEntry.figmaRootNodeId != ownerRootNodeId)
        {
            Debug.LogError("Figma unavailable-state mapping validation failed for node " + figmaNodeId + " at " + path + ".", this);
            return;
        }

        VisualElement element;
        if (!TryResolvePath(root, path, out element))
        {
            Debug.LogError("Could not resolve unavailable control node " + figmaNodeId + " at " + path + ".", this);
            return;
        }
        string actualTag = element is Label ? "Label" : element.GetType() == typeof(VisualElement) ? "VisualElement" : element.GetType().Name;
        if (actualTag != expectedTag)
        {
            Debug.LogError("Unavailable control node " + figmaNodeId + " has runtime type " + actualTag + ", expected " + expectedTag + ".", this);
            return;
        }

        element.AddToClassList("ac-control-unavailable");
        element.SetEnabled(false);
        element.focusable = false;
        element.tabIndex = -1;
        element.pickingMode = PickingMode.Ignore;
        if (replacementText != null && element is Label label)
            label.text = replacementText;
    }

    private void RegisterKeyOnRoot(VisualElement root, Action action, List<Action> cleanup)
    {
        EventCallback<KeyDownEvent> callback = evt =>
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Space)
            {
                evt.StopPropagation();
                action();
            }
        };
        root.RegisterCallback(callback);
        cleanup.Add(() => root.UnregisterCallback(callback));
    }

    private void RegisterKeyHandling(VisualElement root, List<Action> cleanup)
    {
        EventCallback<KeyDownEvent> callback = evt =>
        {
            if (evt.keyCode == KeyCode.Escape || (evt.keyCode == KeyCode.Backspace && !(evt.target is TextField)))
            {
                evt.StopPropagation();
                Back();
            }
            else if (currentScreen == PerformerSelectScreen && (int)evt.keyCode >= (int)KeyCode.Alpha1 && (int)evt.keyCode <= (int)KeyCode.Alpha6)
            {
                SelectPerformer((int)evt.keyCode - (int)KeyCode.Alpha1, false);
            }
            else if (currentScreen == AssistantChooserScreen && (int)evt.keyCode >= (int)KeyCode.Alpha1 && (int)evt.keyCode <= (int)KeyCode.Alpha6)
            {
                SelectPerformer((int)evt.keyCode - (int)KeyCode.Alpha1, true);
            }
        };
        root.RegisterCallback(callback);
        cleanup.Add(() => root.UnregisterCallback(callback));
    }

    private static bool TryResolvePath(VisualElement root, string path, out VisualElement element)
    {
        element = null;
        if (root == null)
            return false;
        string[] parts = path.Split('/');
        if (parts.Length < 1 || parts[0] != "1")
            return false;
        VisualElement current = root;
        for (int i = 1; i < parts.Length; i++)
        {
            int index;
            if (!int.TryParse(parts[i], out index) || index < 1 || index > current.childCount)
                return false;
            current = current[index - 1];
        }
        element = current;
        return true;
    }

    private static int ArrayIndexOf(string[] values, string value)
    {
        for (int i = 0; i < values.Length; i++)
            if (values[i] == value)
                return i;
        return -1;
    }

    private static void ClearBindings(List<Action> cleanup)
    {
        for (int i = cleanup.Count - 1; i >= 0; i--)
            cleanup[i]();
        cleanup.Clear();
    }
}
