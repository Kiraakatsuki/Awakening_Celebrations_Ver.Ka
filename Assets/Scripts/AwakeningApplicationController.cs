using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using AwakeningCelebrations.Interaction;
using UnityEngine;
using UnityEngine.Networking;
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
    [SerializeField, Min(0f), Tooltip("The imported Color Release screen has no completion event or duration; this configurable presentation interval is a provisional default.")]
    private float colorReleasePresentationSeconds = 2f;
    [SerializeField] private AudioClip environmentalMusicClip;
    [SerializeField] private AudioSource environmentalMusicSource;
    [SerializeField, Min(0.1f), Tooltip("Higher values make the gesture cursor catch up faster to the tracked fingertip.")]
    private float gestureCursorSmoothingSpeed = 16f;
    [SerializeField, Tooltip("The requirements identify a prepared environmental rhythm layer but do not specify repeat/stop behavior. Choose the desired behavior when assigning the approved clip.")]
    private EnvironmentalMusicMode environmentalMusicMode = EnvironmentalMusicMode.LoopUntilStopped;
    [SerializeField, Tooltip("Public HTTPS base URL for the Render assistant service. Keep API credentials on the backend.")]
    private string assistantBackendBaseUrl = "https://awakening-celebrations-ver-ka.onrender.com";

    private enum EnvironmentalMusicMode
    {
        PlayOnce,
        LoopUntilStopped,
        Toggle
    }

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
    private bool awakeningSequenceCompletedThisSession;
    private bool waveCompleted;
    private bool clapCompleted;
    private bool warnedMissingEnvironmentalMusic;
    private MediaPipeGestureBridge gestureBridge;
    private bool gestureBridgeSubscribed;
    private float nextGestureBridgeSearchTime;
    private bool warnedMissingGestureBridge;
    private int targetedPerformer = -1;
    private VisualElement[] performerTargets;
    private Coroutine colorReleaseRoutine;
    private bool transitionInProgress;
    private ManifestSnapshot nodeSnapshot;
    private StyleSheet interactionStyles;
    private bool interactionStylesAdded;
    private GestureCursorController gestureCursor;
    private AssistantSession assistantSession;
    private Coroutine assistantRequestRoutine;
    private UnityWebRequest assistantWebRequest;
    private int assistantSessionSequence;

    private sealed class AssistantSession
    {
        public int id;
        public int performerIndex;
        public string performerId;
        public string assistantScreenId;
        public VisualElement root;
        public TextField input;
        public Label placeholder;
        public ScrollView conversation;
        public Label status;
        public Button send;
        public string failedMessage;
        public bool requestInProgress;
    }

    [Serializable]
    private sealed class AssistantChatRequest
    {
        public string message;
    }

    [Serializable]
    private sealed class AssistantChatResponse
    {
        public string reply;
        public string error;
    }

    private enum InteractionCommand
    {
        WaveCompleted,
        ClapCompleted,
        ColorAwakeningCompleted,
        ConfirmPerformer,
        ActivateLivingPortrait,
        AdvanceStage,
        BackRequested
    }

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
        gestureCursor = new GestureCursorController(gestureCursorSmoothingSpeed);
        GestureCameraConsentGate.SetConsent(false);
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
        TryConnectGestureBridge();
    }

    private void OnDisable()
    {
        GestureCameraConsentGate.SetConsent(false);
        StopEnvironmentalMusic();
        HomeLanguageController.LanguageChanged -= OnLanguageChanged;
        HomeLanguageController.PrivacyRequested -= OnPrivacyRequested;
        HomeLanguageController.SettingsRequested -= OnSettingsRequested;
        HomeLanguageController.ReviewConsentRequested -= OnReviewConsentRequested;
        HomeLanguageController.ConsentChanged -= OnConsentChanged;
        if (gestureBridge != null)
        {
            gestureBridge.GestureDetected -= OnGestureDetected;
            gestureBridge.PointingChanged -= OnPointingChanged;
            gestureBridge.TrackingChanged -= OnTrackingChanged;
            gestureBridgeSubscribed = false;
        }
        ClearBindings(screenUnbind);
        if (colorReleaseRoutine != null)
        {
            StopCoroutine(colorReleaseRoutine);
            colorReleaseRoutine = null;
        }
        transitionInProgress = false;
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

        if (gestureBridge == null && !warnedMissingGestureBridge)
        {
            warnedMissingGestureBridge = true;
            Debug.LogWarning("No MediaPipeGestureBridge is loaded. Gesture input will connect automatically when the existing Hand Landmarker sample scene is loaded.", this);
        }
    }

    private void Update()
    {
        gestureCursor?.Tick(Time.unscaledDeltaTime);
        if (gestureBridge == null && Time.unscaledTime >= nextGestureBridgeSearchTime)
        {
            nextGestureBridgeSearchTime = Time.unscaledTime + 1f;
            TryConnectGestureBridge();
        }
    }

    private void TryConnectGestureBridge()
    {
        MediaPipeGestureBridge availableBridge = FindObjectOfType<MediaPipeGestureBridge>(true);
        if (availableBridge == null)
            return;

        if (gestureBridge != availableBridge)
        {
            if (gestureBridge != null && gestureBridgeSubscribed)
            {
                gestureBridge.GestureDetected -= OnGestureDetected;
                gestureBridge.PointingChanged -= OnPointingChanged;
                gestureBridge.TrackingChanged -= OnTrackingChanged;
            }
            gestureBridgeSubscribed = false;
            gestureBridge = availableBridge;
        }
        if (!gestureBridgeSubscribed)
        {
            gestureBridge.GestureDetected += OnGestureDetected;
            gestureBridge.PointingChanged += OnPointingChanged;
            gestureBridge.TrackingChanged += OnTrackingChanged;
            gestureBridgeSubscribed = true;
            Debug.Log("AwakeningApplicationController connected to MediaPipe gesture events.", this);
        }
    }

    private void OnGestureDetected(GestureRecognitionEvent gesture)
    {
        switch (gesture.Gesture)
        {
            case SemanticGesture.Wave:
                HandleInteractionCommand(InteractionCommand.WaveCompleted);
                break;
            case SemanticGesture.Clap:
                HandleInteractionCommand(InteractionCommand.ClapCompleted);
                break;
            case SemanticGesture.Point:
                break;
            case SemanticGesture.Fist:
                if (!transitionInProgress)
                    gestureCursor?.ClickAtCurrentPosition();
                break;
            case SemanticGesture.BothHandsRaised:
                HandleInteractionCommand(InteractionCommand.ActivateLivingPortrait);
                break;
            case SemanticGesture.OpenPalmHeld:
                HandleInteractionCommand(InteractionCommand.BackRequested);
                break;
        }
    }

    private void OnPointingChanged(bool isPointing, Vector2 normalizedPosition)
    {
        gestureCursor?.SetPointing(isPointing, normalizedPosition);
    }

    private void OnTrackingChanged(bool isTracking)
    {
        if (!isTracking)
            gestureCursor?.ClearPosition();
    }

    private void HandleInteractionCommand(InteractionCommand command)
    {
        if (transitionInProgress)
            return;

        if (command == InteractionCommand.BackRequested)
        {
            if (currentScreen != HomeScreen)
            {
                Debug.Log("Back requested from " + currentScreen + ".", this);
                Back();
            }
            return;
        }

        switch (command)
        {
            case InteractionCommand.WaveCompleted:
                if (currentScreen != AttractScreen)
                    return;
                if (hasAwakenedThisSession)
                {
                    Navigate(PerformerSelectScreen, language, true);
                    return;
                }
                if (!consentAccepted || waveCompleted || !Navigate(ColorReleaseScreen, language, true))
                    return;
                waveCompleted = true;
                hasAwakenedThisSession = true;
                Debug.Log("Wave completed the initial color awakening.", this);
                break;

            case InteractionCommand.ClapCompleted:
                if (!hasAwakenedThisSession || currentScreen == HomeScreen || currentScreen == PrivacyScreen || ArrayIndexOf(TutorialScreens, currentScreen) >= 0)
                    return;
                PlayEnvironmentalMusic();
                if (currentScreen == ColorReleaseScreen && !clapCompleted)
                {
                    clapCompleted = true;
                    colorReleaseRoutine = StartCoroutine(CompleteColorReleaseAfterPresentation());
                }
                break;

            case InteractionCommand.ColorAwakeningCompleted:
                if (currentScreen != ColorReleaseScreen || !hasAwakenedThisSession || !clapCompleted)
                    return;
                colorReleaseRoutine = null;
                if (Navigate(PerformerSelectScreen, language, true))
                    awakeningSequenceCompletedThisSession = true;
                break;

            case InteractionCommand.ConfirmPerformer:
                if (currentScreen == PerformerSelectScreen && targetedPerformer >= 0)
                {
                    Debug.Log("Performer confirmed: " + Performers[targetedPerformer] + ".", this);
                    SelectPerformer(targetedPerformer, false);
                }
                break;

            case InteractionCommand.ActivateLivingPortrait:
                if (IsPerformerStage(currentScreen, out int activationPerformer, out int activationStage) && activationStage <= 1)
                {
                    Debug.Log("Both hands raised for " + Performers[activationPerformer] + " on " + Stages[activationStage] + ".", this);
                    AdvancePerformerStage(activationPerformer, activationStage);
                }
                break;

            case InteractionCommand.AdvanceStage:
                if (currentScreen == AttractScreen)
                {
                    HandleInteractionCommand(InteractionCommand.WaveCompleted);
                }
                else if (currentScreen == ColorReleaseScreen)
                {
                    HandleInteractionCommand(InteractionCommand.ColorAwakeningCompleted);
                }
                else if (IsPerformerStage(currentScreen, out int performer, out int stage))
                {
                    if (stage <= 1)
                        HandleInteractionCommand(InteractionCommand.ActivateLivingPortrait);
                    else
                        AdvancePerformerStage(performer, stage);
                }
                break;
        }
    }

    private IEnumerator CompleteColorReleaseAfterPresentation()
    {
        yield return new WaitForSecondsRealtime(colorReleasePresentationSeconds);
        if (currentScreen == ColorReleaseScreen && hasAwakenedThisSession && clapCompleted)
            HandleInteractionCommand(InteractionCommand.ColorAwakeningCompleted);
    }

    private void SetTargetedPerformer(int index)
    {
        if (index < -1 || index >= Performers.Length || targetedPerformer == index)
            return;
        if (performerTargets != null && targetedPerformer >= 0 && targetedPerformer < performerTargets.Length && performerTargets[targetedPerformer] != null)
            performerTargets[targetedPerformer].RemoveFromClassList("ac-performer-targeted");
        targetedPerformer = index;
        if (index >= 0 && performerTargets != null && index < performerTargets.Length && performerTargets[index] != null)
            performerTargets[index].AddToClassList("ac-performer-targeted");
        if (index >= 0)
            Debug.Log("Performer targeted: " + Performers[index] + ".", this);
    }

    private void ClearTargetedPerformer()
    {
        if (performerTargets != null && targetedPerformer >= 0 && targetedPerformer < performerTargets.Length && performerTargets[targetedPerformer] != null)
            performerTargets[targetedPerformer].RemoveFromClassList("ac-performer-targeted");
        targetedPerformer = -1;
        performerTargets = null;
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
        if (!accepted)
            GestureCameraConsentGate.SetConsent(false);
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
        if (transitionInProgress)
            return false;
        if (hasAwakenedThisSession && (screenId == HomeScreen || screenId == AttractScreen || screenId == ColorReleaseScreen || ArrayIndexOf(TutorialScreens, screenId) >= 0))
        {
            screenId = PerformerSelectScreen;
            recordHistory = false;
            preserveHistory = true;
        }
        if (screenId == AttractScreen && !consentAccepted)
        {
            GestureCameraConsentGate.SetConsent(false);
            Debug.LogWarning("The Attract experience cannot start until consent is accepted.", this);
            return false;
        }
        VisualTreeAsset asset;
        if (screenCatalog == null || !screenCatalog.TryGet(screenId, targetLanguage, out asset))
        {
            Debug.LogError("AwakeningApplicationController: No " + targetLanguage + " asset is registered for logical screen '" + screenId + "'.", this);
            return false;
        }
        if (navigationDocument == null)
            return false;

        transitionInProgress = true;

        if (currentScreen == ColorReleaseScreen && screenId != ColorReleaseScreen && colorReleaseRoutine != null)
        {
            StopCoroutine(colorReleaseRoutine);
            colorReleaseRoutine = null;
        }
        ClearTargetedPerformer();
        while (overlays.Count > 0)
            CloseTopOverlay();
        if (assistantSession != null || assistantWebRequest != null || assistantRequestRoutine != null)
            CancelAssistantRequest();
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
        bool cameraAllowed = consentAccepted && (screenId == AttractScreen || screenId == ColorReleaseScreen || screenId == PerformerSelectScreen || IsPerformerStage(screenId, out _, out _) || (hasAwakenedThisSession && (screenId == "CLOSING---CELEBRATION" || screenId == "SOUVENIR---QR-DELIVERY")));
        GestureCameraConsentGate.SetConsent(cameraAllowed);
        // MediaPipe capture supports gesture input; none of the current imported
        // application screens calls for the sample's full-screen camera canvas.
        GestureCameraConsentGate.SetPreviewVisible(false);
        if (!cameraAllowed)
            gestureCursor?.ClearPosition();
        if (homeController != null)
            homeController.SetHomeVisible(false);
        BindCurrentScreen();
        StartCoroutine(ReleaseTransitionLockNextFrame());
        return true;
    }

    private IEnumerator ReleaseTransitionLockNextFrame()
    {
        yield return null;
        transitionInProgress = false;
    }

    public void ShowHome(bool recordHistory)
    {
        if (hasAwakenedThisSession)
        {
            if (currentScreen != PerformerSelectScreen)
                Navigate(PerformerSelectScreen, language, false);
            return;
        }
        if (transitionInProgress)
            return;
        transitionInProgress = true;
        if (colorReleaseRoutine != null)
        {
            StopCoroutine(colorReleaseRoutine);
            colorReleaseRoutine = null;
        }
        ClearTargetedPerformer();
        GestureCameraConsentGate.SetConsent(false);
        StopEnvironmentalMusic();
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
        StartCoroutine(ReleaseTransitionLockNextFrame());
    }

    private void ReturnHomeFromDone()
    {
        if (transitionInProgress)
            return;

        transitionInProgress = true;
        if (colorReleaseRoutine != null)
        {
            StopCoroutine(colorReleaseRoutine);
            colorReleaseRoutine = null;
        }
        ClearTargetedPerformer();
        backStack.Clear();
        GestureCameraConsentGate.SetConsent(false);
        gestureCursor?.ClearPosition();
        StopEnvironmentalMusic();
        ClearBindings(screenUnbind);
        while (overlays.Count > 0)
            CloseTopOverlay();
        if (navigationDocument != null)
            navigationDocument.enabled = false;
        currentScreen = HomeScreen;
        activeBindingScreen = HomeScreen;
        if (homeController != null)
            homeController.SetHomeVisible(true);
        StartCoroutine(ReleaseTransitionLockNextFrame());
    }

    public void Back()
    {
        if (!awakeningSequenceCompletedThisSession)
            return;
        if (overlays.Count > 0)
        {
            CloseTopOverlay();
            return;
        }
        if (hasAwakenedThisSession)
        {
            if (currentScreen == ColorReleaseScreen && !clapCompleted)
                return;
            if (currentScreen == PerformerSelectScreen)
                return;

            while (backStack.Count > 0)
            {
                RouteState candidate = backStack.Pop();
                if (!IsAllowedPostAwakeningBackTarget(candidate.screen))
                    continue;

                Navigate(candidate.screen, candidate.language, false, true);
                return;
            }

            Navigate(PerformerSelectScreen, language, false);
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
        gestureCursor?.Attach(root);
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
            Action advance = () => HandleInteractionCommand(InteractionCommand.AdvanceStage);
            RegisterClick(root, advance, screenUnbind);
            RegisterKeyOnRoot(root, advance, screenUnbind);
            FocusFirstFocusable(root);
            return;
        }
        if (currentScreen == ColorReleaseScreen)
        {
            Action clap = () => HandleInteractionCommand(InteractionCommand.ClapCompleted);
            RegisterClick(root, clap, screenUnbind);
            RegisterKeyOnRoot(root, clap, screenUnbind);
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
            BindPath(root, "1/9", "Label", backId, () => HandleInteractionCommand(InteractionCommand.BackRequested), screenUnbind);
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
            int rootIndex = -1;
            if (screenCatalog != null && screenCatalog.TryGetEntry(currentScreen, language, out stageEntry))
            {
                string[] rootParts = stageEntry.figmaRootNodeId.Split(':');
                if (rootParts.Length == 2 && int.TryParse(rootParts[1], out rootIndex))
                    nodeId = rootParts[0] + ":" + (rootIndex + nodeOffset);
            }
            if (nodeId == null)
            {
                Debug.LogError("Could not derive the verified stage action node for " + currentScreen + ".", this);
                return;
            }
            Action advanceStage = () => HandleInteractionCommand(InteractionCommand.AdvanceStage);
            BindPath(root, actionPath, "Label", nodeId, advanceStage, screenUnbind);
            if (stage == Stages.Length - 1)
                ConfigurePerformerAgentPage(root, performer, rootIndex, screenUnbind);
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
            BindPath(root, "1/7/7", "VisualElement", language == AppLanguage.English ? "124:5431" : "124:5510", ReturnHomeFromDone, screenUnbind);
            BindPath(root, "1/7/8", "Label", language == AppLanguage.English ? "124:5432" : "124:5511", ReturnHomeFromDone, screenUnbind, false);
        }
        FocusFirstFocusable(root);
    }

    private void ContinueFromPrivacy()
    {
        if (hasAwakenedThisSession)
        {
            Navigate(PerformerSelectScreen, language, false);
            return;
        }
        if (!consentAccepted)
        {
            Debug.LogWarning("Consent is required before continuing. Return Home and select the consent checkbox first.", this);
            return;
        }
        Navigate(TutorialScreens[0], language, true);
    }

    private bool IsAllowedPostAwakeningBackTarget(string screenId)
    {
        return screenId == PerformerSelectScreen || screenId == SettingsScreen || screenId == AssistantChooserScreen ||
            screenId == "CLOSING---CELEBRATION" || screenId == "SOUVENIR---QR-DELIVERY" ||
            IsPerformerStage(screenId, out _, out _);
    }

    private void PlayEnvironmentalMusic()
    {
        if (environmentalMusicClip == null)
        {
            if (!warnedMissingEnvironmentalMusic)
            {
                warnedMissingEnvironmentalMusic = true;
                Debug.LogWarning("Clap was accepted, but no approved environmental music clip is assigned.", this);
            }
            return;
        }

        if (environmentalMusicSource == null)
            environmentalMusicSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        environmentalMusicSource.playOnAwake = false;
        environmentalMusicSource.clip = environmentalMusicClip;
        if (environmentalMusicMode == EnvironmentalMusicMode.Toggle && environmentalMusicSource.isPlaying)
        {
            environmentalMusicSource.Stop();
            return;
        }
        if (environmentalMusicMode == EnvironmentalMusicMode.LoopUntilStopped)
            environmentalMusicSource.loop = true;
        else
            environmentalMusicSource.loop = false;

        if (environmentalMusicSource.isPlaying)
            return;
        if (environmentalMusicMode == EnvironmentalMusicMode.PlayOnce)
            environmentalMusicSource.PlayOneShot(environmentalMusicClip);
        else
            environmentalMusicSource.Play();
    }

    private void StopEnvironmentalMusic()
    {
        if (environmentalMusicSource != null && environmentalMusicSource.isPlaying)
            environmentalMusicSource.Stop();
    }

    private void BindPerformerChoices(VisualElement root, bool assistantChoice, List<Action> cleanup)
    {
        if (!assistantChoice)
            performerTargets = new VisualElement[Performers.Length];

        for (int i = 0; i < Performers.Length; i++)
        {
            int performerIndex = i;
            string surfacePath = "1/" + (5 + (i * 3));
            string labelPath = "1/" + (6 + (i * 3));
            string cuePath = "1/" + (7 + (i * 3));
            int surfaceNode = (assistantChoice
                ? (language == AppLanguage.English ? 5166 : 5194)
                : (language == AppLanguage.English ? 3872 : 3900)) + (i * 3);
            int labelNode = surfaceNode + 1;
            Action activate = () =>
            {
                if (assistantChoice)
                {
                    SelectPerformer(performerIndex, true);
                    return;
                }
                SetTargetedPerformer(performerIndex);
                HandleInteractionCommand(InteractionCommand.ConfirmPerformer);
            };
            BindPath(root, surfacePath, "VisualElement", "124:" + surfaceNode, activate, cleanup);
            BindPath(root, labelPath, "Label", "124:" + labelNode, activate, cleanup, false);
            BindPath(root, cuePath, "Label", "124:" + (labelNode + 1), activate, cleanup, false);

            if (!assistantChoice)
            {
                VisualElement surface;
                if (IsVerifiedPerformerNode(surfacePath, "VisualElement", "124:" + surfaceNode) &&
                    TryResolvePath(root, surfacePath, out surface))
                {
                    performerTargets[performerIndex] = surface;
                    RegisterTargetHover(surface, performerIndex, cleanup);
                }
                VisualElement label;
                if (IsVerifiedPerformerNode(labelPath, "Label", "124:" + labelNode) &&
                    TryResolvePath(root, labelPath, out label))
                    RegisterTargetHover(label, performerIndex, cleanup);
                VisualElement cue;
                if (IsVerifiedPerformerNode(cuePath, "Label", "124:" + (labelNode + 1)) &&
                    TryResolvePath(root, cuePath, out cue))
                    RegisterTargetHover(cue, performerIndex, cleanup);
            }
        }
    }

    private bool IsVerifiedPerformerNode(string path, string expectedTag, string figmaNodeId)
    {
        string ownerRootNodeId;
        ManifestNode mapping = FindManifestNode(figmaNodeId, out ownerRootNodeId);
        RuntimeScreenCatalog.Entry ownerEntry;
        return mapping != null && mapping.path == path && mapping.tag == expectedTag &&
               screenCatalog != null && screenCatalog.TryGetEntry(PerformerSelectScreen, language, out ownerEntry) &&
               ownerEntry.figmaRootNodeId == ownerRootNodeId;
    }

    private void RegisterTargetHover(VisualElement element, int index, List<Action> cleanup)
    {
        EventCallback<PointerEnterEvent> pointerEnter = evt => SetTargetedPerformer(index);
        element.RegisterCallback(pointerEnter);
        cleanup.Add(() => element.UnregisterCallback(pointerEnter));
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
        if (performer < 0 || performer >= Performers.Length)
        {
            Debug.LogError("Cannot open an assistant for invalid performer index " + performer + ".", this);
            return;
        }

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
        tree.AddToClassList("ac-assistant-overlay-container");
        panelRoot.Add(tree);
        overlays.Push(new OverlayState { tree = tree, baseRoot = baseRoot });
        selectedPerformer = performer;
        GestureCameraConsentGate.SetPreviewVisible(false);
        List<Action> cleanup = new List<Action>();
        overlayUnbind.Push(cleanup);
        VisualElement importedRoot = GetUxmlRoot(tree);
        int baseNode = language == AppLanguage.English ? 4970 + performer * 32 : 4986 + performer * 32;
        activeBindingScreen = overlayId;
        importedRoot.AddToClassList("ac-screen-runtime");
        importedRoot.focusable = false;
        importedRoot.tabIndex = -1;
        ConfigureAssistantChat(importedRoot, performer, cleanup);
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
        Debug.Log("Opened assistant overlay for " + Performers[performer] + ".", this);
    }

    private void ConfigureAssistantChat(VisualElement root, int performer, List<Action> cleanup)
    {
        assistantSession = new AssistantSession
        {
            id = ++assistantSessionSequence,
            performerIndex = performer,
            performerId = Performers[performer],
            assistantScreenId = "Assistant-" + Performers[performer],
            root = root
        };

        // Assistant replies need room for paragraphs, lists, and code output.
        // Keep the existing horizontal layout while extending only this overlay.
        root.style.height = 640;

        string[] suggestionPaths = { "1/4", "1/6", "1/8" };
        string[] suggestionLabelPaths = { "1/5", "1/7", "1/9" };
        for (int i = 0; i < suggestionPaths.Length; i++)
        {
            VisualElement surface;
            VisualElement labelElement;
            if (TryResolvePath(root, suggestionPaths[i], out surface) &&
                TryResolvePath(root, suggestionLabelPaths[i], out labelElement) && labelElement is Label suggestionLabel)
            {
                AssistantSession suggestionSession = assistantSession;
                string suggestionPrompt = suggestionLabel.text;
                Action askSuggestion = () => SubmitAssistantMessage(suggestionSession, suggestionPrompt);
                RegisterClick(surface, askSuggestion, cleanup);
                RegisterClick(suggestionLabel, askSuggestion, cleanup);
            }
            else
            {
                Debug.LogWarning("Assistant suggestion controls could not be resolved at " + suggestionPaths[i] + ".", this);
            }
        }

        StyleSheet chatStyles = Resources.Load<StyleSheet>("AssistantChatRuntime");
        if (chatStyles != null)
        {
            root.styleSheets.Add(chatStyles);
            root.AddToClassList("ac-assistant-accent-" + Performers[performer].ToLowerInvariant().Replace("-", ""));
        }
        else
        {
            Debug.LogError("Resources/AssistantChatRuntime.uss is missing.", this);
        }

        TextField input = new TextField { name = "assistantChatInput", maxLength = 3600 };
        input.AddToClassList("ac-assistant-chat-input");
        root.Add(input);
        assistantSession.input = input;

        Label placeholder = new Label(language == AppLanguage.English ? "Ask a question…" : "輸入問題…");
        placeholder.AddToClassList("ac-assistant-chat-placeholder");
        placeholder.pickingMode = PickingMode.Ignore;
        root.Add(placeholder);
        assistantSession.placeholder = placeholder;

        Button send = new Button { name = "assistantChatSend" };
        send.text = language == AppLanguage.English ? "Ask" : "詢問";
        send.AddToClassList("ac-assistant-chat-send");
        root.Add(send);
        assistantSession.send = send;

        ScrollView conversation = new ScrollView(ScrollViewMode.Vertical) { name = "assistantConversation" };
        conversation.AddToClassList("ac-assistant-conversation");
        root.Add(conversation);
        assistantSession.conversation = conversation;

        Label status = new Label();
        status.AddToClassList("ac-assistant-chat-status");
        status.AddToClassList("ac-assistant-chat-hidden");
        conversation.Add(status);
        assistantSession.status = status;

        AssistantSession session = assistantSession;
        Action sendAction = () => SubmitAssistantMessage(session, session.input.value);
        send.clicked += sendAction;
        cleanup.Add(() => send.clicked -= sendAction);

        EventCallback<KeyDownEvent> submitOnEnter = evt =>
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                evt.StopPropagation();
                SubmitAssistantMessage(session, session.input.value);
            }
        };
        input.RegisterCallback(submitOnEnter);
        cleanup.Add(() => input.UnregisterCallback(submitOnEnter));

        EventCallback<ChangeEvent<string>> updatePlaceholder = evt =>
        {
            if (string.IsNullOrWhiteSpace(evt.newValue))
                placeholder.RemoveFromClassList("ac-assistant-chat-placeholder-hidden");
            else
                placeholder.AddToClassList("ac-assistant-chat-placeholder-hidden");
        };
        input.RegisterCallback(updatePlaceholder);
        cleanup.Add(() => input.UnregisterCallback(updatePlaceholder));

        EventCallback<FocusInEvent> hidePlaceholder = _ => placeholder.AddToClassList("ac-assistant-chat-placeholder-hidden");
        EventCallback<FocusOutEvent> showPlaceholder = _ =>
        {
            if (string.IsNullOrWhiteSpace(input.value))
                placeholder.RemoveFromClassList("ac-assistant-chat-placeholder-hidden");
        };
        input.RegisterCallback(hidePlaceholder);
        input.RegisterCallback(showPlaceholder);
        cleanup.Add(() => input.UnregisterCallback(hidePlaceholder));
        cleanup.Add(() => input.UnregisterCallback(showPlaceholder));

        assistantSession = session;
        if (!TryBuildAssistantChatUrl(out _))
            SetAssistantStatus(session, language == AppLanguage.English ? "Assistant service URL is not configured." : "尚未設定助理服務網址。", false);
    }

    private void ConfigurePerformerAgentPage(VisualElement root, int performer, int figmaRootIndex, List<Action> cleanup)
    {
        string screenId = GetPerformerScreen(performer, Stages.Length - 1);
        assistantSession = new AssistantSession
        {
            id = ++assistantSessionSequence,
            performerIndex = performer,
            performerId = Performers[performer],
            assistantScreenId = screenId,
            root = root
        };

        StyleSheet chatStyles = Resources.Load<StyleSheet>("AssistantChatRuntime");
        if (chatStyles != null)
        {
            root.styleSheets.Add(chatStyles);
            root.AddToClassList("ac-assistant-accent-" + Performers[performer].ToLowerInvariant().Replace("-", ""));
        }
        else
        {
            Debug.LogError("Resources/AssistantChatRuntime.uss is missing.", this);
        }

        ScrollView conversation = new ScrollView(ScrollViewMode.Vertical) { name = "performerAgentConversation" };
        conversation.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
        conversation.verticalScrollerVisibility = ScrollerVisibility.Auto;
        conversation.AddToClassList("ac-performer-agent-conversation");
        root.Add(conversation);

        Label status = new Label(language == AppLanguage.English
            ? "Choose a question below to start a conversation."
            : "請選擇下方問題開始對話。");
        status.AddToClassList("ac-assistant-chat-status");
        conversation.Add(status);

        assistantSession.conversation = conversation;
        assistantSession.status = status;
        AssistantSession session = assistantSession;

        int[] promptSurfaceOffsets = { 10, 12, 14 };
        for (int i = 0; i < promptSurfaceOffsets.Length; i++)
        {
            string promptSurfacePath = "1/6/" + (4 + i * 2);
            string promptLabelPath = "1/6/" + (5 + i * 2);
            VisualElement surface;
            VisualElement labelElement;
            if (TryResolvePath(root, promptSurfacePath, out surface) &&
                TryResolvePath(root, promptLabelPath, out labelElement) && labelElement is Label promptLabel)
            {
                int promptOffset = promptSurfaceOffsets[i];
                string promptSurfaceId = "124:" + (figmaRootIndex + promptOffset);
                Action askPrompt = () => SubmitAssistantMessage(session, promptLabel.text);
                BindPath(root, promptSurfacePath, "VisualElement", promptSurfaceId, askPrompt, cleanup);
            }
            else
            {
                Debug.LogWarning("Performer agent prompt controls could not be resolved for " + screenId + " at " + promptSurfacePath + ".", this);
            }
        }

        if (!TryBuildAssistantChatUrl(out _))
            SetAssistantStatus(session, language == AppLanguage.English ? "Assistant service URL is not configured." : "尚未設定助理服務網址。", false);
    }

    private bool TryBuildAssistantChatUrl(out string chatUrl)
    {
        chatUrl = null;
        Uri baseUri;
        string configuredUrl = assistantBackendBaseUrl == null ? string.Empty : assistantBackendBaseUrl.Trim().TrimEnd('/');
        if (!Uri.TryCreate(configuredUrl, UriKind.Absolute, out baseUri) ||
            baseUri.Scheme != Uri.UriSchemeHttps || baseUri.AbsolutePath != "/" ||
            !string.IsNullOrEmpty(baseUri.Query) || !string.IsNullOrEmpty(baseUri.Fragment))
            return false;

        chatUrl = configuredUrl + "/chat";
        return true;
    }

    private void SubmitAssistantMessage(AssistantSession session, string rawMessage)
    {
        if (session == null || session != assistantSession || session.id != assistantSessionSequence ||
            session.performerIndex < 0 || session.performerIndex >= Performers.Length ||
            session.performerId != Performers[session.performerIndex] ||
            !IsAssistantSessionViewActive(session) ||
            selectedPerformer != session.performerIndex ||
            session.root == null || session.root.panel == null || session.requestInProgress)
            return;

        string message = rawMessage == null ? string.Empty : rawMessage.Trim();
        if (message.Length == 0)
        {
            SetAssistantStatus(session, language == AppLanguage.English ? "Enter a question before sending." : "請先輸入問題再送出。", false);
            return;
        }
        if (message.Length > 3600)
        {
            SetAssistantStatus(session, language == AppLanguage.English ? "Your question is too long. Please shorten it and retry." : "問題內容過長，請縮短後重試。", false);
            return;
        }
        if (!TryBuildAssistantChatUrl(out string chatUrl))
        {
            SetAssistantStatus(session, language == AppLanguage.English ? "Assistant service URL is not configured." : "尚未設定助理服務網址。", false);
            return;
        }

        bool retryingFailedMessage = session.failedMessage == message;
        if (!retryingFailedMessage)
        {
            Label userMessage = new Label(message) { enableRichText = false };
            userMessage.AddToClassList("ac-assistant-chat-message");
            userMessage.AddToClassList("ac-assistant-chat-user-message");
            session.conversation.Add(userMessage);
        }

        session.requestInProgress = true;
        if (session.send != null)
            session.send.SetEnabled(false);
        SetAssistantStatus(session, language == AppLanguage.English ? "Thinking…" : "思考中…", true);
        string scopedMessage = BuildPerformerScopedMessage(session, message);
        assistantRequestRoutine = StartCoroutine(SendAssistantRequest(session, scopedMessage, chatUrl, message));
    }

    private bool IsAssistantSessionViewActive(AssistantSession session)
    {
        if (session == null)
            return false;
        if (session.assistantScreenId == activeBindingScreen)
            return true;

        return session.assistantScreenId == "Assistant-" + session.performerId && overlays.Count > 0 &&
               GetUxmlRoot(overlays.Peek().tree) == session.root;
    }

    private string BuildPerformerScopedMessage(AssistantSession session, string visitorMessage)
    {
        string languageInstruction = language == AppLanguage.English
            ? "Reply in the same language as the visitor's question."
            : "請使用訪客問題所使用的語言回答。";
        string evidenceInstruction = language == AppLanguage.English
            ? "Use only facts supplied in this request. Do not invent performer-specific facts; say when verified context is insufficient."
            : "僅使用此請求提供的事實。不要捏造表演者相關資訊；若缺乏已核實的背景資料，請明確說明。";

        return "Selected performer assistant: " + session.performerId + ". " + languageInstruction + " " + evidenceInstruction +
               "\nVisitor question:\n" + visitorMessage;
    }

    private IEnumerator SendAssistantRequest(AssistantSession session, string message, string chatUrl, string visitorMessage)
    {
        AssistantChatRequest payload = new AssistantChatRequest { message = message };
        byte[] body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload));
        using (UnityWebRequest request = new UnityWebRequest(chatUrl, UnityWebRequest.kHttpVerbPOST))
        {
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = 45;
            request.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
            assistantWebRequest = request;
            yield return request.SendWebRequest();

            assistantWebRequest = null;
            assistantRequestRoutine = null;
            if (session != assistantSession || session.id != assistantSessionSequence || session.root == null || session.root.panel == null)
                yield break;

            session.requestInProgress = false;
            if (session.send != null)
                session.send.SetEnabled(true);
            if (request.result != UnityWebRequest.Result.Success)
            {
                string retryHint = session.send == null
                    ? (language == AppLanguage.English ? "Select the same question again to retry." : "再次選擇相同問題即可重試。")
                    : (language == AppLanguage.English ? "Tap Retry to try again." : "請點選「重試」再試一次。");
                string error = request.result == UnityWebRequest.Result.ConnectionError
                    ? (language == AppLanguage.English ? "Network error. Check your connection. " + retryHint : "網路連線失敗，請確認連線。" + retryHint)
                    : (language == AppLanguage.English ? "Assistant server error (HTTP " + request.responseCode + "). " + retryHint : "助理伺服器錯誤（HTTP " + request.responseCode + "）。" + retryHint);
                Debug.LogWarning("Assistant request failed with HTTP status " + request.responseCode + " (" + request.result + ").", this);
                FailAssistantMessage(session, visitorMessage, error);
                yield break;
            }

            AssistantChatResponse result = null;
            try
            {
                result = JsonUtility.FromJson<AssistantChatResponse>(request.downloadHandler.text);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Assistant response JSON could not be parsed (" + exception.GetType().Name + ").", this);
            }

            if (result == null || string.IsNullOrWhiteSpace(result.reply))
            {
                FailAssistantMessage(session, visitorMessage, language == AppLanguage.English
                    ? (session.send == null ? "The assistant returned an unexpected response. Select the same question again to retry." : "The assistant returned an unexpected response. Tap Retry to try again.")
                    : (session.send == null ? "助理回覆格式無法辨識，請再次選擇相同問題重試。" : "助理回覆格式無法辨識，請點選「重試」。"));
                yield break;
            }

            Label reply = AppendAssistantReply(session, result.reply);
            session.failedMessage = null;
            if (session.input != null && session.input.value.Trim() == visitorMessage)
                session.input.value = string.Empty;
            if (session.send != null)
                session.send.text = language == AppLanguage.English ? "Ask" : "詢問";
            SetAssistantStatus(session, string.Empty, false);
            session.conversation.schedule.Execute(() =>
            {
                if (session == assistantSession && session.root != null && session.root.panel != null)
                    session.conversation.ScrollTo(reply);
            }).ExecuteLater(1);
        }
    }

    private Label AppendAssistantReply(AssistantSession session, string replyText)
    {
        string[] lines = replyText.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        bool inCodeBlock = false;
        Label lastContent = null;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            string trimmed = line.Trim();
            if (trimmed.StartsWith("```", StringComparison.Ordinal))
            {
                inCodeBlock = !inCodeBlock;
                continue;
            }

            if (line.Length == 0 && !inCodeBlock)
            {
                VisualElement paragraphGap = new VisualElement();
                paragraphGap.AddToClassList("ac-assistant-chat-paragraph-gap");
                session.conversation.Add(paragraphGap);
                continue;
            }

            string displayLine = line;
            string lineClass = inCodeBlock ? "ac-assistant-chat-code-line" : "ac-assistant-chat-paragraph";
            if (!inCodeBlock)
            {
                int headingMarkers = 0;
                while (headingMarkers < line.Length && headingMarkers < 6 && line[headingMarkers] == '#')
                    headingMarkers++;
                if (headingMarkers > 0 && headingMarkers < line.Length && line[headingMarkers] == ' ')
                {
                    displayLine = line.Substring(headingMarkers + 1);
                    lineClass = "ac-assistant-chat-heading";
                }
                else if (trimmed.StartsWith("- ", StringComparison.Ordinal) ||
                         trimmed.StartsWith("* ", StringComparison.Ordinal) ||
                         trimmed.StartsWith("+ ", StringComparison.Ordinal))
                {
                    displayLine = "• " + trimmed.Substring(2);
                    lineClass = "ac-assistant-chat-list-item";
                }
                else if (TryFormatNumberedListLine(trimmed, out displayLine))
                {
                    lineClass = "ac-assistant-chat-list-item";
                }
                else if (trimmed.StartsWith("> ", StringComparison.Ordinal))
                {
                    displayLine = "│ " + trimmed.Substring(2);
                    lineClass = "ac-assistant-chat-quote";
                }
            }

            Label lineLabel = new Label(displayLine) { enableRichText = false };
            lineLabel.AddToClassList("ac-assistant-chat-message");
            lineLabel.AddToClassList("ac-assistant-chat-reply-message");
            lineLabel.AddToClassList(lineClass);
            session.conversation.Add(lineLabel);
            lastContent = lineLabel;
        }

        if (lastContent == null)
        {
            lastContent = new Label(replyText) { enableRichText = false };
            lastContent.AddToClassList("ac-assistant-chat-message");
            lastContent.AddToClassList("ac-assistant-chat-reply-message");
            session.conversation.Add(lastContent);
        }

        return lastContent;
    }

    private static bool TryFormatNumberedListLine(string line, out string displayLine)
    {
        displayLine = line;
        int markerEnd = 0;
        while (markerEnd < line.Length && char.IsDigit(line[markerEnd]))
            markerEnd++;
        if (markerEnd == 0 || markerEnd + 1 >= line.Length ||
            (line[markerEnd] != '.' && line[markerEnd] != ')') || line[markerEnd + 1] != ' ')
            return false;

        displayLine = line.Substring(0, markerEnd + 1) + " " + line.Substring(markerEnd + 2);
        return true;
    }

    private void FailAssistantMessage(AssistantSession session, string message, string status)
    {
        session.failedMessage = message;
        if (session.send != null)
            session.send.text = language == AppLanguage.English ? "Retry" : "重試";
        SetAssistantStatus(session, status, false);
    }

    private static void SetAssistantStatus(AssistantSession session, string message, bool isLoading)
    {
        if (session == null || session.status == null)
            return;
        session.status.text = message;
        if (string.IsNullOrEmpty(message))
            session.status.AddToClassList("ac-assistant-chat-hidden");
        else
            session.status.RemoveFromClassList("ac-assistant-chat-hidden");

        if (isLoading)
            session.status.AddToClassList("ac-assistant-chat-loading");
        else
            session.status.RemoveFromClassList("ac-assistant-chat-loading");

        if (!string.IsNullOrEmpty(message) && session.conversation != null)
        {
            session.status.RemoveFromHierarchy();
            session.conversation.Add(session.status);
            session.conversation.ScrollTo(session.status);
        }
    }

    private void CancelAssistantRequest()
    {
        assistantSessionSequence++;
        if (assistantWebRequest != null)
        {
            assistantWebRequest.Abort();
            assistantWebRequest.Dispose();
            assistantWebRequest = null;
        }
        if (assistantRequestRoutine != null)
        {
            StopCoroutine(assistantRequestRoutine);
            assistantRequestRoutine = null;
        }
        assistantSession = null;
    }

    private void CloseTopOverlay()
    {
        if (overlays.Count == 0)
            return;
        CancelAssistantRequest();
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
        GestureCameraConsentGate.SetPreviewVisible(false);
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
                HandleInteractionCommand(InteractionCommand.BackRequested);
            }
            else if (currentScreen == PerformerSelectScreen && (int)evt.keyCode >= (int)KeyCode.Alpha1 && (int)evt.keyCode <= (int)KeyCode.Alpha6)
            {
                SetTargetedPerformer((int)evt.keyCode - (int)KeyCode.Alpha1);
                HandleInteractionCommand(InteractionCommand.ConfirmPerformer);
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
