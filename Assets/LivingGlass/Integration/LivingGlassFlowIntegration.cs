using System;
using System.Collections.Generic;
using AwakeningCelebrations.Interaction;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Living Glass command port into the single flow/service authority. No new camera,
/// decoder, recognizer or HTTP client. Imported path bindings are never used by this port.
/// </summary>
public sealed partial class AwakeningApplicationController
{
    [SerializeField] private LivingGlassPresentation livingGlass;
    private string livingOverlay;
    private int livingOverlayPerformer = -1;
    private int livingReturnPerformer = -1;
    private string livingParentOverlay;
    private bool livingPointerOwnsTarget;
    private readonly List<Action> livingChatUnbind = new List<Action>();
    private LivingGlassSpeechCapture livingSpeech;
    private VisualElement LivingContent { get { return livingGlass != null ? livingGlass.Content : null; } }
    public bool LivingConsent { get { return consentAccepted; } }
    public bool LivingReady { get { return colorReleaseSelectionReady; } }
    public bool LivingProtected { get { return LivingGlassRoutes.Protected(currentScreen) || performerFlowTransitionLock; } }
    public bool LivingCanBack { get { return (IsPerformerStage(currentScreen, out _, out int s) && s == 2) ||
        (!transitionInProgress && !LivingProtected && (awakeningSequenceCompletedThisSession ? currentScreen != HomeScreen && currentScreen != ColorReleaseScreen : currentScreen == SettingsScreen || currentScreen == AttractScreen)); } }

    private void LivingGlassRender()
    {
        if (livingGlass == null) return;
        ClearBindings(livingChatUnbind);
        if (LivingGlassRoutes.TryChapter(currentScreen, out int chapterPerformer, out _)) selectedPerformer = chapterPerformer;
        if (currentScreen.StartsWith("Assistant-", StringComparison.Ordinal)) selectedPerformer = Array.IndexOf(Performers, currentScreen.Substring(10));
        livingGlass.Render(currentScreen, language, selectedPerformer, consentAccepted, colorReleaseSelectionReady);
        VisualElement root = livingGlass.Content;
        gestureCursor?.Attach(livingGlass.Root);
        livingGlass.StyleCursor();
        performerTargets = livingGlass.PerformerTargets;
        if (currentScreen.StartsWith("Assistant-", StringComparison.Ordinal) && selectedPerformer >= 0)
        {
            LivingGlassBindChat(root, selectedPerformer);
            return;
        }
        if (!IsPerformerStage(currentScreen, out int performer, out int stage)) return;
        selectedPerformer = performer;
        if (stage == 0) gestureBridge?.ResetVideoGesture();
        else if (stage == 1)
        {
            performerSequenceActive = true;
            performerFlowTransitionLock = true;
            performerStageRoutine = StartCoroutine(PlayActivationSequence(root, performer));
        }
        else if (stage == 2)
        {
            performerSequenceActive = true;
            performerFlowTransitionLock = true;
            StartLivingPortrait(root, performer);
        }
        else if (stage == 3)
        {
            performerSequenceActive = false;
            performerFlowTransitionLock = true;
            performerStageRoutine = StartCoroutine(FadeInRelease(root));
        }
        else LivingGlassBindChat(root, performer);
    }

    public void LivingLanguage(AppLanguage value)
    {
        if (livingGlass == null || LivingProtected || language == value) return;
        language = value;
        PlayerPrefs.SetString("AwakeningCelebrations.Language", value == AppLanguage.English ? "en" : "zh");
        PlayerPrefs.Save();
        UpdateNavigationHistoryLanguage(value);
        string retainedOverlay = livingOverlay;
        int retainedPerformer = livingOverlayPerformer;
        LivingGlassCloseOverlay(true);
        CancelAssistantRequest();
        ClearTargetedPerformer();
        LivingGlassRender();
        if (!string.IsNullOrEmpty(retainedOverlay))
        {
            livingOverlayPerformer = retainedPerformer;
            LivingGlassOpenOverlay(retainedOverlay);
        }
    }

    public void LivingConsentChoice(bool accepted)
    {
        consentAccepted = accepted;
        if (!accepted)
        {
            GestureCameraConsentGate.SetConsent(false);
            gestureCursor?.ClearPosition();
            gesturePointing = false;
            ClearGestureTarget();
        }
        // Acceptance alone does not start capture: route authority applies eligibility.
        livingGlass?.ConsentChanged(accepted);
    }

    public void LivingBegin()
    {
        if (LivingProtected || livingGlass.OverlayVisible || transitionInProgress) return;
        Navigate(PrivacyScreen, language, true);
    }
    public void LivingContinue()
    {
        if (LivingProtected || livingGlass.OverlayVisible || transitionInProgress) return;
        if (currentScreen == PrivacyScreen) { ContinueFromPrivacy(); return; }
        int tutorial = Array.IndexOf(TutorialScreens, currentScreen);
        if (tutorial >= 0) { Navigate(tutorial + 1 < TutorialScreens.Length ? TutorialScreens[tutorial + 1] : AttractScreen, language, true); return; }
        if (currentScreen == "TUTORIAL---RETURN---DISMISS") { Navigate(TutorialScreens[1], language, false); return; }
        HandleInteractionCommand(InteractionCommand.AdvanceStage);
    }
    public void LivingTarget(int index)
    {
        if (currentScreen != ColorReleaseScreen || !colorReleaseSelectionReady || livingGlass.OverlayVisible) return;
        // Explicit pointer/keyboard ownership supersedes a stale hand target, without retuning recognition.
        gesturePointing = false;
        livingPointerOwnsTarget = index >= 0;
        gestureCursor?.ClearPosition();
        ClearGestureTarget();
        SetTargetedPerformer(index);
    }
    public void LivingConfirm(int index)
    {
        if (currentScreen != ColorReleaseScreen || !colorReleaseSelectionReady || livingGlass.OverlayVisible || transitionInProgress) return;
        LivingTarget(index);
        HandleInteractionCommand(InteractionCommand.ConfirmPerformer);
    }
    public void LivingReleaseTargetOwner()
    {
        livingPointerOwnsTarget = false;
        gesturePointing = false;
        ClearGestureTarget();
    }
    public void LivingReturnGallery()
    {
        if (LivingProtected || livingGlass.OverlayVisible) return;
        Navigate(ColorReleaseScreen, language, false);
    }
    public void LivingDone() { if (!LivingProtected && !livingGlass.OverlayVisible) LivingGlassHome(true); }
    public void LivingSouvenir() { if (!LivingProtected && !livingGlass.OverlayVisible) Navigate("SOUVENIR---QR-DELIVERY", language, true); }

    private void LivingGlassHome(bool fromDone)
    {
        if (transitionInProgress || LivingProtected) return;
        if (!fromDone && hasAwakenedThisSession) { Navigate(ColorReleaseScreen, language, false); return; }
        LivingGlassCloseOverlay(true);
        CancelAssistantRequest();
        StopPerformerExperienceSequence();
        if (colorReleaseRoutine != null) { StopCoroutine(colorReleaseRoutine); colorReleaseRoutine = null; }
        ClearTargetedPerformer();
        backStack.Clear();
        GestureCameraConsentGate.SetConsent(false);
        StopEnvironmentalMusic();
        currentScreen = HomeScreen;
        LivingGlassRender();
    }

    public void LivingGlassOpenOverlay(string kind)
    {
        if (livingGlass == null) return;
        if (LivingProtected && kind != "Privacy") { livingGlass.AnnounceProtected(); return; }
        if (kind == "Privacy" && LivingProtected)
        {
            // Immediate stop is independent of protected navigation. No cinematic modal.
            LivingConsentChoice(false);
            livingGlass.AnnounceCameraOff();
            return;
        }
        if (livingGlass.OverlayVisible && livingOverlay == kind) { LivingGlassCloseOverlay(); return; }
        livingParentOverlay = livingOverlay == "Settings" && kind == "Privacy" ? "Settings" : null;
        if (!livingGlass.OverlayVisible) livingReturnPerformer = selectedPerformer;
        livingSpeech?.CancelForContextChange();
        CancelAssistantRequest();
        livingOverlay = kind;
        if (kind.StartsWith("Assistant-", StringComparison.Ordinal))
        {
            livingOverlayPerformer = Array.IndexOf(Performers, kind.Substring(10));
            if (livingOverlayPerformer < 0) return;
            selectedPerformer = livingOverlayPerformer;
        }
        ClearGestureTarget();
        gesturePointing = false;
        livingPointerOwnsTarget = false;
        gestureCursor?.ClearPosition();
        // Preserve the existing Settings/Privacy suspension contract. Do not auto-grant on close.
        if (kind == "Settings" || kind == "Privacy") GestureCameraConsentGate.SetConsent(false);
        VisualElement panel = livingGlass.OpenOverlay(kind, language, livingOverlayPerformer);
        if (kind.StartsWith("Assistant-", StringComparison.Ordinal)) LivingGlassBindChat(panel, livingOverlayPerformer);
    }

    public void LivingGlassCloseOverlay(bool immediate = false)
    {
        if (livingGlass == null || !livingGlass.OverlayVisible) return;
        ClearBindings(livingChatUnbind);
        CancelAssistantRequest();
        bool returnSettings = !immediate && livingParentOverlay == "Settings";
        livingParentOverlay = null;
        livingOverlay = null;
        selectedPerformer = livingReturnPerformer;
        livingGlass.CloseOverlay(immediate);
        if (returnSettings) { LivingGlassOpenOverlay("Settings"); return; }
        bool eligible = currentScreen == AttractScreen || currentScreen == ColorReleaseScreen || IsPerformerStage(currentScreen, out _, out _)
            || (hasAwakenedThisSession && (currentScreen == "CLOSING---CELEBRATION" || currentScreen == "SOUVENIR---QR-DELIVERY"));
        GestureCameraConsentGate.SetConsent(consentAccepted && eligible);
    }

    public void LivingMinimize()
    {
        string kind = livingOverlay;
        LivingGlassCloseOverlay();
        if (kind == "Settings" || (kind != null && kind.StartsWith("Assistant-", StringComparison.Ordinal)))
            livingGlass.ShowResume(kind);
        // Requests and detached chat sessions are canceled; resume starts a fresh view, never a fictional transcript.
    }

    private void LivingGlassBindChat(VisualElement root, int performer)
    {
        ClearBindings(livingChatUnbind);
        CancelAssistantRequest();
        assistantSession = new AssistantSession
        {
            id = ++assistantSessionSequence, performerIndex = performer, performerId = Performers[performer],
            root = root, input = root.Q<TextField>("assistantChatInput"), send = root.Q<Button>("assistantChatSend"), cancel = root.Q<Button>("assistantCancel"),
            conversation = root.Q<ScrollView>("assistantConversation"), status = root.Q<Label>("assistantChatStatus")
        };
        AssistantSession session = assistantSession;
        session.cancel.SetEnabled(false);
        var speech = LivingGlassSpeechCapture.Bind(root, session.input, session.send, performer, language,
            () => session == assistantSession && IsAssistantSessionViewActive(session),
            () => environmentalMusicSource != null && environmentalMusicSource.isPlaying, livingGlass.Motion);
        livingSpeech = speech;
        if (speech != null) livingChatUnbind.Add(() =>
        {
            speech.Dispose();
            if (livingSpeech == speech) livingSpeech = null;
        });
        Action submit = () =>
        {
            speech?.CancelForContextChange();
            SubmitAssistantMessage(session, string.IsNullOrWhiteSpace(session.input.value) ? session.failedMessage : session.input.value);
        };
        session.send.clicked += submit;
        livingChatUnbind.Add(() => session.send.clicked -= submit);
        Button cancelButton = root.Q<Button>("assistantCancel");
        Action cancel = () =>
        {
            if (session != assistantSession || !session.requestInProgress) return;
            string draft = session.input.value;
            CancelAssistantRequest();
            session.id = ++assistantSessionSequence;
            session.requestInProgress = false;
            session.failedMessage = null;
            assistantSession = session;
            session.input.value = draft;
            session.send.SetEnabled(true);
            session.cancel.SetEnabled(false);
            session.send.text = language == AppLanguage.English ? "Ask" : "詢問";
            SetAssistantStatus(session, language == AppLanguage.English ? "Request canceled." : "已取消請求。", false);
        };
        cancelButton.clicked += cancel;
        livingChatUnbind.Add(() => cancelButton.clicked -= cancel);
        // Explicit Send is IME safe; no root Enter shortcut is installed on an editor.
        foreach (Button suggestion in root.Query<Button>(className: "lg-suggestion").ToList())
        {
            string prompt = suggestion.text;
            Action ask = () =>
            {
                speech?.CancelForContextChange();
                SubmitAssistantMessage(session, prompt);
            };
            suggestion.clicked += ask;
            livingChatUnbind.Add(() => suggestion.clicked -= ask);
        }
    }

    private void LivingGlassUpdateStatus()
    {
        if (livingGlass == null || !livingGlass.IsInitialized) return;
        string status;
        bool en = language == AppLanguage.English;
        if (!consentAccepted) status = en ? "Camera off · permission required" : "相機關閉 · 需要許可";
        else if (!GestureCameraConsentGate.ConsentGranted) status = en ? "Capture suspended" : "影像擷取已暫停";
        else if (gestureBridge == null) status = en ? "Tracking unavailable · use mouse or keyboard" : "追蹤無法使用 · 請使用滑鼠或鍵盤";
        else if (gestureBridge.HasCameraFailure) status = en ? "Camera error · stop capture in Privacy; ask exhibition staff for help" : "相機錯誤 · 請於隱私停止擷取，並聯絡展覽人員";
        else if (!gestureBridge.IsCameraReady) status = en ? "Camera starting · mouse and keyboard ready" : "相機啟動中 · 可使用滑鼠或鍵盤";
        else if (!gestureBridge.HasReceivedFrames) status = en ? "Waiting for tracking frames" : "等待追蹤影格";
        else if (gestureBridge.TrackedHandCount == 0) status = en ? "No hand detected · show your hand" : "未偵測到手部 · 請伸出手";
        else status = en ? "Hand detected · tracking ready" : "已偵測到手部 · 追蹤就緒";
        Texture texture = GestureCameraConsentGate.ConsentGranted && gestureBridge != null && !LivingProtected && !livingGlass.OverlayVisible
            && (currentScreen == AttractScreen || currentScreen == ColorReleaseScreen || (IsPerformerStage(currentScreen, out _, out int s) && (s == 0 || s == 3)))
            ? gestureBridge.CameraPreviewTexture : null;
        livingGlass.UpdateCamera(status, texture);
        if (IsPerformerStage(currentScreen, out _, out int stage) && stage == 2)
            livingGlass.UpdatePortraitStatus(placeholderFallbackRequested, livingPortraitFirstFrameReceived, automaticallyAdvancePlaceholderPortrait, placeholderPortraitPreviewSeconds);
    }
}
