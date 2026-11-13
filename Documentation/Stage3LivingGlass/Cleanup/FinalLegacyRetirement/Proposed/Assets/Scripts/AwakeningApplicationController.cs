using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using AwakeningCelebrations.Interaction;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using UnityEngine.Video;

[DefaultExecutionOrder(-100)]
public sealed partial class AwakeningApplicationController : MonoBehaviour
{
    private const string HomeScreen = "Home";
    private const string PrivacyScreen = "Privacy";
    private const string SettingsScreen = "SETTINGS---EXPERIENCE-CONTROLS";
    private const string AttractScreen = "ATTRACT---MONOCHROME---FALLBACK-INPUT";
    private const string ColorReleaseScreen = "COLOR-RELEASE";
    private const string AssistantChooserScreen = "AssistantChooser";
    private static readonly string[] TutorialScreens = LivingGlassRoutes.Tutorials;

    private static readonly string[] Performers = LivingGlassRoutes.Performers;


    private static readonly string[] Stages = LivingGlassRoutes.Stages;

    [SerializeField] private UIDocument navigationDocument;



    [SerializeField, Min(0f), Tooltip("Presentation interval before gallery selection becomes ready; preserves the existing session timing.")]
    private float colorReleasePresentationSeconds = 2f;
    [SerializeField, Min(0.1f), Tooltip("Total time spent on Activation, including its fade in and fade out.")]
    private float activationSequenceDurationSeconds = 3f;
    [SerializeField, Min(0.05f), Tooltip("Fade duration used for Focus, Activation, Living Portrait and Release transitions.")]
    private float performerTransitionFadeSeconds = 0.65f;
    [SerializeField, Tooltip("Temporary fallback only. When disabled, an unassigned Living Portrait remains on its placeholder until another application-controlled action.")]
    private bool automaticallyAdvancePlaceholderPortrait = true;
    [SerializeField, Min(0f), Tooltip("Preview time on the existing placeholder before automatically opening Release, used only when that performer's video slot is empty.")]
    private float placeholderPortraitPreviewSeconds = 10f;
    [SerializeField, Tooltip("Assign one VideoClip per performer in the existing performer order: Gaia, Pandemic, Re-Birth, White Guardian, Conscience, Sanctuary.")]
    private VideoClip[] performerPortraitVideos = new VideoClip[6];
    [SerializeField, Tooltip("Optional VideoPlayer. If empty, the controller creates and reuses one at runtime.")]
    private VideoPlayer livingPortraitVideoPlayer;
    [SerializeField] private AudioClip environmentalMusicClip;
    [SerializeField] private AudioSource environmentalMusicSource;
    [SerializeField, Min(0.1f), Tooltip("Higher values make the gesture cursor catch up faster to the tracked fingertip.")]
    private float gestureCursorSmoothingSpeed = 16f;
    [SerializeField, Min(0f)] private float gestureTargetStabilitySeconds = 0.12f;
    [SerializeField, Min(0f)] private float gestureTargetReleaseSeconds = 0.75f;
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



    private AppLanguage language = AppLanguage.English;
    private string currentScreen = HomeScreen;

    private int selectedPerformer = -1;
    private bool consentAccepted;
    private bool hasAwakenedThisSession;
    private bool awakeningSequenceCompletedThisSession;
    private bool waveCompleted;
    private bool colorReleaseSelectionReady;
    private AudioClip fallbackClapSound;
    [SerializeField] private MediaPipeGestureBridge gestureBridge;
    private bool gestureBridgeSubscribed;
    private bool warnedMissingGestureBridge;
    private int targetedPerformer = -1;
    private int gestureTarget = -1;
    private int? gestureTargetCandidate;
    private float gestureTargetCandidateSince;
    private float gestureTargetLostSince = -1f;
    private bool gesturePointing;
    private VisualElement[] performerTargets;

    private Coroutine colorReleaseRoutine;
    private Coroutine performerStageRoutine;
    private Coroutine placeholderPortraitRoutine;
    private Coroutine livingPortraitTransitionRoutine;
    private bool transitionInProgress;
    private bool performerSequenceActive;
    private bool performerFlowTransitionLock;
    private bool livingPortraitCompletionRequested;
    private bool livingPortraitVisible;
    private bool livingPortraitPlayerEventsRegistered;
    private bool livingPortraitFirstFrameReceived;
    private bool livingPortraitVideoPrepared;
    private bool livingPortraitPlaybackStarted;
    private bool placeholderFallbackRequested;
    private int livingPortraitPerformer = -1;
    private VisualElement livingPortraitSurface;
    private Image livingPortraitVideoImage;
    private StyleBackground livingPortraitPlaceholderBackground;
    private StyleEnum<ScaleMode> livingPortraitPlaceholderScaleMode;
    private StyleEnum<Overflow> livingPortraitSurfaceOverflow;
    private RenderTexture livingPortraitRenderTexture;






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

        public VisualElement root;
        public TextField input;

        public ScrollView conversation;
        public Label status;
        public Button send;
        public Button cancel;
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





    private void Awake()
    {
        gestureCursor = new GestureCursorController(gestureCursorSmoothingSpeed);
        GestureCameraConsentGate.SetConsent(false);
        if (livingGlass == null || livingGlass.Document == null)
        {
            Debug.LogError("Living Glass presentation and UIDocument must be assigned.", this);
            enabled = false;
            return;
        }
        navigationDocument = livingGlass.Document;
    }

    private void OnValidate()
    {
        if (performerPortraitVideos == null || performerPortraitVideos.Length != Performers.Length)
            Array.Resize(ref performerPortraitVideos, Performers.Length);
    }

    private void OnEnable()
    {
        TryConnectGestureBridge();
    }

    private void OnDisable()
    {
        if (livingGlass != null)
        {
            ClearBindings(livingChatUnbind);
            CancelAssistantRequest();
            livingGlass.Shutdown();
        }
        GestureCameraConsentGate.SetConsent(false);
        StopEnvironmentalMusic();
        StopPerformerExperienceSequence();
        if (gestureBridge != null)
        {
            gestureBridge.GestureDetected -= OnGestureDetected;
            gestureBridge.PointingChanged -= OnPointingChanged;
            gestureBridge.TrackingChanged -= OnTrackingChanged;
            gestureBridgeSubscribed = false;
        }
        if (colorReleaseRoutine != null)
        {
            StopCoroutine(colorReleaseRoutine);
            colorReleaseRoutine = null;
        }
        transitionInProgress = false;
        performerFlowTransitionLock = false;
    }

    private IEnumerator Start()
    {
        yield return null;
        livingGlass.Initialize(this);
        language = PlayerPrefs.GetString("AwakeningCelebrations.Language", "en") == "zh" ? AppLanguage.Chinese : AppLanguage.English;
        LivingGlassRender();
    }

    private void Update()
    {
        LivingGlassUpdateStatus();
        gestureCursor?.Tick(Time.unscaledDeltaTime);
        if (gesturePointing)
            UpdatePointedPerformer();
        if (!gesturePointing && gestureTargetLostSince >= 0f &&
            Time.unscaledTime - gestureTargetLostSince >= gestureTargetReleaseSeconds)
        {
            ClearGestureTarget();
        }
    }



    private void TryConnectGestureBridge()
    {
        if (gestureBridge == null || gestureBridgeSubscribed)
            return;

        gestureBridge.GestureDetected += OnGestureDetected;
        gestureBridge.PointingChanged += OnPointingChanged;
        gestureBridge.TrackingChanged += OnTrackingChanged;
        gestureBridgeSubscribed = true;
        Debug.Log("AwakeningApplicationController connected to its serialized gesture rig.", this);
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
                // Pointing is a continuous target operation. It never confirms or
                // navigates; the cursor hit test updates targetedPerformer below.
                break;
            case SemanticGesture.Fist:
                if (!transitionInProgress && currentScreen == ColorReleaseScreen && colorReleaseSelectionReady &&
                    gestureTarget >= 0 &&
                    (gesturePointing || (gestureTargetLostSince >= 0f &&
                        Time.unscaledTime - gestureTargetLostSince < gestureTargetReleaseSeconds)))
                {
                    SetTargetedPerformer(gestureTarget);
                    HandleInteractionCommand(InteractionCommand.ConfirmPerformer);
                }
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
        if (livingGlass != null && livingGlass.OverlayVisible)
            return;
        if (livingGlass != null && livingPointerOwnsTarget)
            return;
        gestureCursor?.SetPointing(isPointing, normalizedPosition);
        if (!isPointing)
        {
            if (gesturePointing)
                gestureTargetLostSince = Time.unscaledTime;
            gesturePointing = false;
            gestureTargetCandidate = null;
            return;
        }

        gesturePointing = true;
        gestureTargetLostSince = -1f;
    }

    private void UpdatePointedPerformer()
    {
        if (livingGlass != null && livingGlass.OverlayVisible)
            return;
        if (currentScreen != ColorReleaseScreen || !colorReleaseSelectionReady ||
            performerTargets == null)
            return;

        if (gestureCursor == null || !gestureCursor.TryPickAtCurrentPosition(out VisualElement picked))
        {
            UpdateGestureTarget(-1);
            return;
        }

        while (picked != null)
        {
            for (int i = 0; i < performerTargets.Length; i++)
            {
                if (performerTargets[i] != null && picked == performerTargets[i])
                {
                    UpdateGestureTarget(i);
                    return;
                }
            }
            picked = picked.parent;
        }
        UpdateGestureTarget(-1);
    }

    private void UpdateGestureTarget(int candidate)
    {
        if (candidate == gestureTarget)
        {
            gestureTargetCandidate = null;
            return;
        }

        if (gestureTargetCandidate != candidate)
        {
            gestureTargetCandidate = candidate;
            gestureTargetCandidateSince = Time.unscaledTime;
            if (livingGlass != null) livingGlass.AcquisitionChanged(candidate);
        }

        if (Time.unscaledTime - gestureTargetCandidateSince >= gestureTargetStabilitySeconds)
        {
            ClearGestureTarget();
            gestureTarget = candidate;
            if (candidate >= 0)
                SetTargetedPerformer(candidate);
        }
    }

    private void ClearGestureTarget()
    {
        if (targetedPerformer == gestureTarget)
            SetTargetedPerformer(-1);
        gestureTarget = -1;
        gestureTargetCandidate = null;
        gestureTargetLostSince = -1f;
    }



    private void OnTrackingChanged(bool isTracking)
    {
        if (!isTracking)
        {
            gestureCursor?.ClearPosition();
            gesturePointing = false;
            ClearGestureTarget();
        }
    }

    private void HandleInteractionCommand(InteractionCommand command)
    {
        if (livingGlass != null && livingGlass.OverlayVisible)
        {
            if (command == InteractionCommand.BackRequested) LivingGlassCloseOverlay();
            return;
        }
        if (command == InteractionCommand.BackRequested)
        {
            if (currentScreen != HomeScreen)
            {
                Debug.Log("Back requested from " + currentScreen + ".", this);
                Back();
            }
            return;
        }

        if (transitionInProgress)
            return;

        switch (command)
        {
            case InteractionCommand.WaveCompleted:
                if (currentScreen != AttractScreen)
                    return;
                if (hasAwakenedThisSession)
                {
                    Navigate(ColorReleaseScreen, language, true);
                    return;
                }
                if (!consentAccepted || waveCompleted || !Navigate(ColorReleaseScreen, language, true))
                    return;
                waveCompleted = true;
                hasAwakenedThisSession = true;
                colorReleaseRoutine = StartCoroutine(CompleteColorReleaseAfterPresentation());
                Debug.Log("Wave completed the initial color awakening.", this);
                break;

            case InteractionCommand.ClapCompleted:
                if (!GestureCameraConsentGate.ConsentGranted)
                    return;
                PlayEnvironmentalMusic();
                break;

            case InteractionCommand.ColorAwakeningCompleted:
                if (currentScreen != ColorReleaseScreen || !hasAwakenedThisSession || colorReleaseSelectionReady)
                    return;
                colorReleaseRoutine = null;
                awakeningSequenceCompletedThisSession = true;
                colorReleaseSelectionReady = true;
                LivingGlassRender();
                break;

            case InteractionCommand.ConfirmPerformer:
                if (currentScreen == ColorReleaseScreen && colorReleaseSelectionReady && targetedPerformer >= 0)
                {
                    Debug.Log("Performer confirmed: " + Performers[targetedPerformer] + ".", this);
                    SelectPerformer(targetedPerformer);
                }
                break;

            case InteractionCommand.ActivateLivingPortrait:
                if (!transitionInProgress && !performerSequenceActive &&
                    IsPerformerStage(currentScreen, out int activationPerformer, out int activationStage) && activationStage == 0)
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
                    if (stage == 0)
                        HandleInteractionCommand(InteractionCommand.ActivateLivingPortrait);
                    else if (stage == 3 || stage == Stages.Length - 1)
                        AdvancePerformerStage(performer, stage);
                }
                break;
        }
    }

    private IEnumerator CompleteColorReleaseAfterPresentation()
    {
        yield return new WaitForSecondsRealtime(colorReleasePresentationSeconds);
        if (currentScreen == ColorReleaseScreen && hasAwakenedThisSession)
            HandleInteractionCommand(InteractionCommand.ColorAwakeningCompleted);
    }

    private void SetTargetedPerformer(int index)
    {
        if (index < -1 || index >= Performers.Length || targetedPerformer == index)
            return;
        if (performerTargets != null && targetedPerformer >= 0 && targetedPerformer < performerTargets.Length && performerTargets[targetedPerformer] != null)
            performerTargets[targetedPerformer].RemoveFromClassList("ac-performer-targeted");
        targetedPerformer = index;
        if (livingGlass != null)
            livingGlass.TargetChanged(index);
        if (index >= 0 && performerTargets != null && index < performerTargets.Length && performerTargets[index] != null)
            performerTargets[index].AddToClassList("ac-performer-targeted");
        if (index >= 0)
            Debug.Log("Performer targeted: " + Performers[index] + ".", this);
    }

    private void ClearTargetedPerformer()
    {
        livingPointerOwnsTarget = false;
        if (livingGlass != null) livingGlass.TargetChanged(-1);
        gestureCursor?.ClearPosition();
        if (performerTargets != null && targetedPerformer >= 0 && targetedPerformer < performerTargets.Length && performerTargets[targetedPerformer] != null)
            performerTargets[targetedPerformer].RemoveFromClassList("ac-performer-targeted");
        targetedPerformer = -1;
        performerTargets = null;
        gestureTarget = -1;
        gestureTargetCandidate = null;
        gestureTargetLostSince = -1f;
        gesturePointing = false;
    }


    private void UpdateNavigationHistoryLanguage(AppLanguage selectedLanguage)
    {
        if (backStack.Count == 0)
            return;

        RouteState[] routes = backStack.ToArray();
        backStack.Clear();
        for (int i = routes.Length - 1; i >= 0; i--)
        {
            routes[i].language = selectedLanguage;
            backStack.Push(routes[i]);
        }
    }





    public bool Navigate(string screenId, AppLanguage targetLanguage, bool recordHistory, bool preserveHistory = false)
    {
        if (transitionInProgress)
            return false;
        if (performerSequenceActive &&
            IsPerformerStage(currentScreen, out int activePerformer, out int activeStage) &&
            (activeStage == 1 || activeStage == 2) &&
            screenId != GetPerformerScreen(activePerformer, activeStage + 1))
            return false;
        bool leavingLivingPortrait = (currentScreen != screenId || targetLanguage != language) &&
            IsPerformerStage(currentScreen, out _, out int currentStage) && currentStage == 2;
        if (hasAwakenedThisSession && (screenId == HomeScreen || screenId == AttractScreen || screenId == ColorReleaseScreen || Array.IndexOf(TutorialScreens, screenId) >= 0))
        {
            screenId = ColorReleaseScreen;
            recordHistory = false;
            preserveHistory = true;
        }
        if (screenId == AttractScreen && !consentAccepted)
        {
            GestureCameraConsentGate.SetConsent(false);
            Debug.LogWarning("The Attract experience cannot start until consent is accepted.", this);
            return false;
        }
        if (!LivingGlassRoutes.Supports(screenId))
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
        if (assistantSession != null || assistantWebRequest != null || assistantRequestRoutine != null)
            CancelAssistantRequest();
        if (recordHistory && currentScreen != HomeScreen)
            backStack.Push(new RouteState { screen = currentScreen, language = language });
        else if (!recordHistory && !preserveHistory)
            backStack.Clear();

        if (leavingLivingPortrait)
        {
            StopLivingPortraitVideoPlayer(false, true);
            livingPortraitSurface = null;
            livingPortraitPerformer = -1;
            livingPortraitVisible = false;
            livingPortraitCompletionRequested = false;
            placeholderFallbackRequested = false;
        }
        if (navigationDocument == null)
        {
            transitionInProgress = false;
            return false;
        }
        currentScreen = screenId;
        language = targetLanguage;
        bool cameraAllowed = consentAccepted && (screenId == AttractScreen || screenId == ColorReleaseScreen || IsPerformerStage(screenId, out _, out _) || (hasAwakenedThisSession && (screenId == "CLOSING---CELEBRATION" || screenId == "SOUVENIR---QR-DELIVERY")));
        GestureCameraConsentGate.SetConsent(cameraAllowed);
        // MediaPipe capture supports gesture input; none of the current imported
        // application screens calls for the sample's full-screen camera canvas.
        GestureCameraConsentGate.SetPreviewVisible(false);
        if (!cameraAllowed)
            gestureCursor?.ClearPosition();
        LivingGlassRender();
        LivingGlassUpdateStatus();
        StartCoroutine(ReleaseTransitionLockNextFrame());
        return true;
    }

    private IEnumerator ReleaseTransitionLockNextFrame()
    {
        yield return null;
        if (!performerFlowTransitionLock)
            transitionInProgress = false;
    }

    public void ShowHome(bool recordHistory)
    {
        LivingGlassHome(false);
    }



    public void Back()
    {
        if (livingGlass != null && livingGlass.OverlayVisible)
        {
            LivingGlassCloseOverlay();
            return;
        }
        bool leavingVideo = IsPerformerStage(currentScreen, out _, out int stage) && stage == 2;
        if (!leavingVideo && (transitionInProgress || performerSequenceActive || performerFlowTransitionLock))
            return;

        if (!awakeningSequenceCompletedThisSession && currentScreen != SettingsScreen && currentScreen != AttractScreen)
            return;
        if (hasAwakenedThisSession)
        {
            if (currentScreen == ColorReleaseScreen)
                return;

            if (leavingVideo)
            {
                StopPerformerExperienceSequence();
                transitionInProgress = false;
            }

            while (backStack.Count > 0)
            {
                RouteState candidate = backStack.Pop();
                if (!IsAllowedPostAwakeningBackTarget(candidate.screen))
                    continue;

                if (Navigate(candidate.screen, candidate.language, false, true) && leavingVideo)
                    Debug.Log("Living Portrait stopped. Returned to " + currentScreen + ".", this);
                return;
            }

            Navigate(ColorReleaseScreen, language, false);
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


    private void ContinueFromPrivacy()
    {
        if (hasAwakenedThisSession)
        {
            Navigate(ColorReleaseScreen, language, false);
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
        return screenId == ColorReleaseScreen || screenId == SettingsScreen || screenId == AssistantChooserScreen ||
            screenId == "CLOSING---CELEBRATION" || screenId == "SOUVENIR---QR-DELIVERY" ||
            (IsPerformerStage(screenId, out _, out int stage) && stage != 1 && stage != 2);
    }

    private void PlayEnvironmentalMusic()
    {
        if (environmentalMusicSource == null)
        {
            environmentalMusicSource = GetComponent<AudioSource>();
            if (environmentalMusicSource == null)
                environmentalMusicSource = gameObject.AddComponent<AudioSource>();
        }

        environmentalMusicSource.playOnAwake = false;
        if (environmentalMusicClip == null)
        {
            if (fallbackClapSound == null)
                fallbackClapSound = CreateClapChime();
            environmentalMusicSource.PlayOneShot(fallbackClapSound);
            Debug.Log("Clap chime started.", this);
            return;
        }

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

    private static AudioClip CreateClapChime()
    {
        const int sampleRate = 24000;
        var samples = new float[sampleRate];
        for (int i = 0; i < samples.Length; i++)
        {
            float time = (float)i / sampleRate;
            float envelope = Mathf.Min(time * 100f, 1f) * Mathf.Exp(-5f * time) * (1f - time);
            samples[i] = envelope * (0.18f * Mathf.Sin(2f * Mathf.PI * 523.25f * time) +
                0.09f * Mathf.Sin(2f * Mathf.PI * 783.99f * time));
        }

        AudioClip clip = AudioClip.Create("Awakening chime", samples.Length, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void OnDestroy()
    {
        if (fallbackClapSound != null)
            Destroy(fallbackClapSound);
    }

    private void StopEnvironmentalMusic()
    {
        if (environmentalMusicSource != null && environmentalMusicSource.isPlaying)
            environmentalMusicSource.Stop();
    }







    private void SelectPerformer(int index)
    {
        selectedPerformer = index;
        Navigate(GetPerformerScreen(index, 0), language, true);
    }

    private void AdvancePerformerStage(int performer, int stage)
    {
        if (performer < 0 || performer >= Performers.Length || transitionInProgress)
            return;

        selectedPerformer = performer;
        if (stage == 0)
        {
            if (performerSequenceActive)
                return;
            if (livingGlass != null) livingGlass.SettleRoutePresentation();
            performerSequenceActive = true;
            performerFlowTransitionLock = true;
            transitionInProgress = true;
            performerStageRoutine = StartCoroutine(FadeFocusIntoActivation(performer));
            return;
        }

        // Activation and Living Portrait are automatic states. Their input is
        // deliberately ignored so a gesture or click cannot skip either state.
        if (stage == 1 || stage == 2)
            return;

        if (stage == Stages.Length - 1)
            Navigate("CLOSING---CELEBRATION", language, true);
        else if (stage == 3)
            Navigate(GetPerformerScreen(performer, stage + 1), language, true);
    }

    private IEnumerator FadeFocusIntoActivation(int performer)
    {
        string focusScreen = GetPerformerScreen(performer, 0);
        VisualElement focusRoot = LivingContent;
        yield return FadeVisualElement(focusRoot, 1f, 0f, performerTransitionFadeSeconds);
        if (currentScreen != focusScreen || selectedPerformer != performer)
        {
            performerStageRoutine = null;
            performerSequenceActive = false;
            performerFlowTransitionLock = false;
            transitionInProgress = false;
            yield break;
        }

        performerStageRoutine = null;
        NavigateWithinPerformerFlow(GetPerformerScreen(performer, 1));
    }

    private IEnumerator PlayActivationSequence(VisualElement root, int performer)
    {
        string activationScreen = GetPerformerScreen(performer, 1);
        VisualElement artwork = root.Q<VisualElement>("activationArtwork");
        if (root != null)
            root.style.opacity = 0f;
        if (artwork != null)
            artwork.style.opacity = 0.68f;

        float fade = Mathf.Max(0.05f, performerTransitionFadeSeconds);
        float duration = Mathf.Max(activationSequenceDurationSeconds, fade * 2f);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (currentScreen != activationScreen || selectedPerformer != performer)
            {
                performerStageRoutine = null;
                yield break;
            }

            float alpha = elapsed < fade
                ? Mathf.Clamp01(elapsed / fade)
                : elapsed > duration - fade
                    ? Mathf.Clamp01((duration - elapsed) / fade)
                    : 1f;
            float awakening = Mathf.SmoothStep(0.68f, 1f, Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration - fade)));
            if (root != null)
                root.style.opacity = alpha;
            if (artwork != null)
                artwork.style.opacity = awakening;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (root != null)
            root.style.opacity = 0f;
        performerStageRoutine = null;
        NavigateWithinPerformerFlow(GetPerformerScreen(performer, 2));
    }

    private IEnumerator FadeInLivingPortrait(VisualElement root, int performer)
    {
        yield return FadeVisualElement(root, 0f, 1f, performerTransitionFadeSeconds);
        if (!IsCurrentPerformerStage(performer, 2))
        {
            performerStageRoutine = null;
            yield break;
        }

        livingPortraitVisible = true;
        performerStageRoutine = null;
        performerFlowTransitionLock = false;
        transitionInProgress = false;
        TryStartLivingPortraitPlayback();
        if (placeholderFallbackRequested)
            StartPlaceholderPortraitTimer(performer);
        if (livingPortraitCompletionRequested)
            StartLivingPortraitExitTransition(performer);
    }

    private IEnumerator FadeInRelease(VisualElement root)
    {
        yield return FadeVisualElement(root, 0f, 1f, performerTransitionFadeSeconds);
        performerStageRoutine = null;
        performerFlowTransitionLock = false;
        transitionInProgress = false;
    }

    private IEnumerator FadeVisualElement(VisualElement element, float from, float to, float duration)
    {
        if (element == null)
            yield break;
        float safeDuration = livingGlass != null && livingGlass.ReducedMotion ? 0.001f : Mathf.Max(0.05f, duration);
        float elapsed = 0f;
        while (elapsed < safeDuration)
        {
            element.style.opacity = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / safeDuration));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        element.style.opacity = to;
    }

    private bool NavigateWithinPerformerFlow(string destination)
    {
        transitionInProgress = false;
        performerFlowTransitionLock = true;
        if (Navigate(destination, language, true))
            return true;

        performerFlowTransitionLock = false;
        performerSequenceActive = false;
        transitionInProgress = false;
        return false;
    }

    private bool IsCurrentPerformerStage(int performer, int stage)
    {
        return selectedPerformer == performer && currentScreen == GetPerformerScreen(performer, stage);
    }

    private void StartLivingPortrait(VisualElement root, int performer)
    {
        StopLivingPortraitVideoPlayer(true, true);
        livingPortraitPerformer = performer;
        livingPortraitCompletionRequested = false;
        livingPortraitVisible = false;
        livingPortraitFirstFrameReceived = false;
        livingPortraitVideoPrepared = false;
        livingPortraitPlaybackStarted = false;
        placeholderFallbackRequested = false;
        livingPortraitSurface = root.Q<VisualElement>("livingPortraitVideoSurface");
        if (livingPortraitSurface == null)
        {
            Debug.LogError("Living Portrait is missing its named video surface; the existing artwork remains visible.", this);
            placeholderFallbackRequested = true;
        }
        else
        {
            livingPortraitPlaceholderBackground = livingPortraitSurface.style.backgroundImage;
            livingPortraitPlaceholderScaleMode = livingPortraitSurface.style.unityBackgroundScaleMode;
            livingPortraitSurfaceOverflow = livingPortraitSurface.style.overflow;
        }

        VideoClip clip = performerPortraitVideos != null && performer < performerPortraitVideos.Length
            ? performerPortraitVideos[performer]
            : null;
        if (clip == null)
        {
            Debug.LogWarning("No Living Portrait video is assigned for " + Performers[performer] + "; keeping the placeholder fallback.", this);
            placeholderFallbackRequested = true;
        }
        else if (livingPortraitSurface != null)
            PrepareLivingPortraitVideo(clip);

        if (root != null)
            root.style.opacity = 0f;
        performerStageRoutine = StartCoroutine(FadeInLivingPortrait(root, performer));
    }

    private void PrepareLivingPortraitVideo(VideoClip clip)
    {
        if (livingPortraitVideoPlayer == null)
        {
            livingPortraitVideoPlayer = GetComponent<VideoPlayer>();
            if (livingPortraitVideoPlayer == null)
            {
                livingPortraitVideoPlayer = gameObject.AddComponent<VideoPlayer>();
            }
        }

        int width = clip.width >= 64 && clip.width <= 4096 ? (int)clip.width : 1920;
        int height = clip.height >= 64 && clip.height <= 4096 ? (int)clip.height : 1080;
        livingPortraitRenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
        {
            name = "Living Portrait Video - " + Performers[livingPortraitPerformer]
        };
        livingPortraitRenderTexture.Create();

        livingPortraitVideoImage = new Image
        {
            name = "livingPortraitVideoImage",
            scaleMode = ScaleMode.ScaleToFit,
            pickingMode = PickingMode.Ignore
        };
        livingPortraitVideoImage.style.position = Position.Absolute;
        livingPortraitVideoImage.style.left = 0f;
        livingPortraitVideoImage.style.top = 0f;
        livingPortraitVideoImage.style.right = 0f;
        livingPortraitVideoImage.style.bottom = 0f;
        livingPortraitVideoImage.style.display = DisplayStyle.None;
        livingPortraitSurface.style.overflow = Overflow.Hidden;
        livingPortraitSurface.Add(livingPortraitVideoImage);

        livingPortraitVideoPlayer.enabled = true;
        livingPortraitVideoPlayer.Stop();
        livingPortraitVideoPlayer.playOnAwake = false;
        livingPortraitVideoPlayer.isLooping = false;
        livingPortraitVideoPlayer.waitForFirstFrame = true;
        livingPortraitVideoPlayer.skipOnDrop = true;
        livingPortraitVideoPlayer.sendFrameReadyEvents = true;
        livingPortraitVideoPlayer.source = VideoSource.VideoClip;
        livingPortraitVideoPlayer.clip = clip;
        livingPortraitVideoPlayer.renderMode = VideoRenderMode.RenderTexture;
        livingPortraitVideoPlayer.targetTexture = livingPortraitRenderTexture;
        livingPortraitVideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        livingPortraitVideoPlayer.prepareCompleted += OnLivingPortraitVideoPrepared;
        livingPortraitVideoPlayer.frameReady += OnLivingPortraitVideoFrameReady;
        livingPortraitVideoPlayer.loopPointReached += OnLivingPortraitVideoCompleted;
        livingPortraitVideoPlayer.errorReceived += OnLivingPortraitVideoError;
        livingPortraitPlayerEventsRegistered = true;
        livingPortraitVideoPlayer.Prepare();
    }

    private void OnLivingPortraitVideoPrepared(VideoPlayer source)
    {
        if (source != livingPortraitVideoPlayer || livingPortraitSurface == null ||
            !IsCurrentPerformerStage(livingPortraitPerformer, 2) || livingPortraitRenderTexture == null)
            return;

        if (!livingPortraitRenderTexture.IsCreated())
        {
            OnLivingPortraitVideoError(source, "The Living Portrait RenderTexture is not created.");
            return;
        }

        livingPortraitVideoImage.image = livingPortraitRenderTexture;
        livingPortraitVideoPrepared = true;
        TryStartLivingPortraitPlayback();
    }

    private void TryStartLivingPortraitPlayback()
    {
        if (!livingPortraitVisible || !livingPortraitVideoPrepared ||
            livingPortraitPlaybackStarted || livingPortraitVideoPlayer == null ||
            !IsCurrentPerformerStage(livingPortraitPerformer, 2))
            return;

        livingPortraitPlaybackStarted = true;
        livingPortraitVideoPlayer.Play();
        Debug.Log("Living Portrait playback started for " + Performers[livingPortraitPerformer] +
            " at frame " + livingPortraitVideoPlayer.frame + " (" + livingPortraitVideoPlayer.width + "x" +
            livingPortraitVideoPlayer.height + ", " + livingPortraitVideoPlayer.frameRate.ToString("0.###") + " fps).", this);
    }

    private void OnLivingPortraitVideoFrameReady(VideoPlayer source, long frameIndex)
    {
        if (source != livingPortraitVideoPlayer ||
            !IsCurrentPerformerStage(livingPortraitPerformer, 2) || livingPortraitRenderTexture == null ||
            livingPortraitVideoImage == null)
            return;

        if (!livingPortraitFirstFrameReceived)
        {
            livingPortraitFirstFrameReceived = true;
            livingPortraitVideoImage.style.display = DisplayStyle.Flex;
            if (livingPortraitSurface != null)
                livingPortraitSurface.style.backgroundImage = new StyleBackground();
            Debug.Log("Living Portrait first frame rendered for " + Performers[livingPortraitPerformer] +
                " (frame " + frameIndex + ", RenderTexture " + livingPortraitRenderTexture.width + "x" +
                livingPortraitRenderTexture.height + ").", this);
        }

        // The RenderTexture is updated by VideoPlayer outside the UI Toolkit
        // style system. Invalidate this Image on each decoded frame so cached
        // UI content samples the latest texture instead of retaining frame 0.
        livingPortraitVideoImage.MarkDirtyRepaint();
    }

    private void OnLivingPortraitVideoCompleted(VideoPlayer source)
    {
        if (source == livingPortraitVideoPlayer && IsCurrentPerformerStage(livingPortraitPerformer, 2))
            RequestLivingPortraitCompletion(livingPortraitPerformer);
    }

    private void OnLivingPortraitVideoError(VideoPlayer source, string message)
    {
        if (source != livingPortraitVideoPlayer || !IsCurrentPerformerStage(livingPortraitPerformer, 2))
            return;

        int performer = livingPortraitPerformer;
        Debug.LogError("Living Portrait video failed for " + Performers[performer] +
            (livingPortraitFirstFrameReceived ? " after rendering its first frame" : " before rendering a frame") +
            "; restoring the existing placeholder fallback. " + message, this);
        StopLivingPortraitVideoPlayer(true, true);
        placeholderFallbackRequested = true;
        if (livingPortraitVisible)
            StartPlaceholderPortraitTimer(performer);
    }

    private void StartPlaceholderPortraitTimer(int performer)
    {
        if (!automaticallyAdvancePlaceholderPortrait || placeholderPortraitPreviewSeconds <= 0f ||
            placeholderPortraitRoutine != null || !IsCurrentPerformerStage(performer, 2))
            return;
        placeholderPortraitRoutine = StartCoroutine(WaitForPlaceholderPortrait(performer));
    }

    private IEnumerator WaitForPlaceholderPortrait(int performer)
    {
        yield return new WaitForSecondsRealtime(placeholderPortraitPreviewSeconds);
        placeholderPortraitRoutine = null;
        if (IsCurrentPerformerStage(performer, 2))
            RequestLivingPortraitCompletion(performer);
    }

    private void RequestLivingPortraitCompletion(int performer)
    {
        if (livingPortraitCompletionRequested || !IsCurrentPerformerStage(performer, 2))
            return;

        livingPortraitCompletionRequested = true;
        if (placeholderPortraitRoutine != null)
        {
            StopCoroutine(placeholderPortraitRoutine);
            placeholderPortraitRoutine = null;
        }
        StopLivingPortraitVideoPlayer(false, false);
        if (livingPortraitVisible)
            StartLivingPortraitExitTransition(performer);
    }

    private void StartLivingPortraitExitTransition(int performer)
    {
        if (livingPortraitTransitionRoutine == null && IsCurrentPerformerStage(performer, 2))
            livingPortraitTransitionRoutine = StartCoroutine(LeaveLivingPortrait(performer));
    }

    private IEnumerator LeaveLivingPortrait(int performer)
    {
        transitionInProgress = true;
        performerFlowTransitionLock = true;
        VisualElement root = LivingContent;
        yield return FadeVisualElement(root, 1f, 0f, performerTransitionFadeSeconds);
        if (!IsCurrentPerformerStage(performer, 2))
        {
            livingPortraitTransitionRoutine = null;
            yield break;
        }

        livingPortraitTransitionRoutine = null;
        NavigateWithinPerformerFlow(GetPerformerScreen(performer, 3));
    }

    private void StopLivingPortraitVideoPlayer(bool restorePlaceholder, bool releaseTexture)
    {
        if (livingPortraitPlayerEventsRegistered && livingPortraitVideoPlayer != null)
        {
            livingPortraitVideoPlayer.prepareCompleted -= OnLivingPortraitVideoPrepared;
            livingPortraitVideoPlayer.frameReady -= OnLivingPortraitVideoFrameReady;
            livingPortraitVideoPlayer.loopPointReached -= OnLivingPortraitVideoCompleted;
            livingPortraitVideoPlayer.errorReceived -= OnLivingPortraitVideoError;
            livingPortraitPlayerEventsRegistered = false;
        }

        if (livingPortraitVideoPlayer != null)
        {
            livingPortraitVideoPlayer.Stop();
            livingPortraitVideoPlayer.targetTexture = null;
            livingPortraitVideoPlayer.clip = null;
        }

        if (restorePlaceholder && livingPortraitSurface != null)
        {
            livingPortraitSurface.style.backgroundImage = livingPortraitPlaceholderBackground;
            livingPortraitSurface.style.unityBackgroundScaleMode = livingPortraitPlaceholderScaleMode;
            livingPortraitSurface.style.overflow = livingPortraitSurfaceOverflow;
        }
        if ((restorePlaceholder || releaseTexture) && livingPortraitVideoImage != null)
        {
            livingPortraitVideoImage.RemoveFromHierarchy();
            livingPortraitVideoImage = null;
        }
        if (releaseTexture)
            ReleaseLivingPortraitRenderTexture();
        if (restorePlaceholder || releaseTexture)
        {
            livingPortraitSurface = null;
            livingPortraitPerformer = -1;
        }
    }

    private void ReleaseLivingPortraitRenderTexture()
    {
        if (livingPortraitRenderTexture == null)
            return;
        livingPortraitRenderTexture.Release();
        if (Application.isPlaying)
            Destroy(livingPortraitRenderTexture);
        else
            DestroyImmediate(livingPortraitRenderTexture);
        livingPortraitRenderTexture = null;
    }

    private void StopPerformerExperienceSequence()
    {
        if (performerStageRoutine != null)
        {
            StopCoroutine(performerStageRoutine);
            performerStageRoutine = null;
        }
        if (placeholderPortraitRoutine != null)
        {
            StopCoroutine(placeholderPortraitRoutine);
            placeholderPortraitRoutine = null;
        }
        if (livingPortraitTransitionRoutine != null)
        {
            StopCoroutine(livingPortraitTransitionRoutine);
            livingPortraitTransitionRoutine = null;
        }
        StopLivingPortraitVideoPlayer(true, true);
        performerSequenceActive = false;
        performerFlowTransitionLock = false;
        livingPortraitVisible = false;
        livingPortraitCompletionRequested = false;
        placeholderFallbackRequested = false;
    }

    private string GetPerformerScreen(int performer, int stage)
    {
        return LivingGlassRoutes.Chapter(performer, stage);
    }

    private bool IsPerformerStage(string screen, out int performer, out int stage)
    {
        return LivingGlassRoutes.TryChapter(screen, out performer, out stage);
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
        if (livingGlass != null) session.root.Q("assistant-empty")?.AddToClassList("lg-hidden");
        if (session.send != null)
            session.send.SetEnabled(false);
        if (session.cancel != null)
            session.cancel.SetEnabled(true);
        SetAssistantStatus(session, language == AppLanguage.English ? "Thinking…" : "思考中…", true);
        string scopedMessage = BuildPerformerScopedMessage(session, message);
        assistantRequestRoutine = StartCoroutine(SendAssistantRequest(session, scopedMessage, chatUrl, message));
    }

    private bool IsAssistantSessionViewActive(AssistantSession session)
    {
        return session != null && livingGlass != null && livingGlass.IsAssistantActive(session.root)
            && selectedPerformer == session.performerIndex;
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
            if (session.cancel != null)
                session.cancel.SetEnabled(false);
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

















    private static void ClearBindings(List<Action> cleanup)
    {
        for (int i = cleanup.Count - 1; i >= 0; i--)
            cleanup[i]();
        cleanup.Clear();
    }
}
