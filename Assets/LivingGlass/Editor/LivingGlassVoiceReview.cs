#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public sealed partial class LivingGlassPreviewWindow
{
    private VisualElement voiceReviewToolbar;
    private PopupField<string> voicePerformer, voiceLocale, voiceState, voiceSurface;
    private Toggle voiceReduced;
    private Label voiceReviewCaption;

    [MenuItem("Awakening Celebrations/Living Glass/Voice composer design review")]
    public static LivingGlassPreviewWindow OpenVoiceReview()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Voice review requires Edit Mode.");

        var window = CreateInstance<LivingGlassPreviewWindow>();
        window.titleContent = new GUIContent("Living Glass · Voice design review");
        window.position = new Rect(60, 60, 1360, 840);
        window.ShowUtility();
        window.Setup();
        AssemblyReloadEvents.beforeAssemblyReload += window.Close;
        window.BuildVoiceReviewToolbar();
        window.ShowVoiceSpecimen();
        return window;
    }

    private void BuildVoiceReviewToolbar()
    {
        voiceReviewToolbar = new VisualElement();
        voiceReviewToolbar.style.position = Position.Absolute;
        voiceReviewToolbar.style.left = 12;
        voiceReviewToolbar.style.right = 12;
        voiceReviewToolbar.style.top = 6;
        voiceReviewCaption = new Label();
        voiceReviewCaption.style.unityFontStyleAndWeight = FontStyle.Bold;
        voiceReviewCaption.style.minHeight = 22;
        voiceReviewToolbar.Add(voiceReviewCaption);
        var controls = new VisualElement();
        controls.style.flexDirection = FlexDirection.Row;
        voiceReviewToolbar.Add(controls);
        voicePerformer = AddReviewChoice(controls, new List<string>(LivingGlassRoutes.Performers), 145);
        voiceLocale = AddReviewChoice(controls, new List<string> { "English", "繁體中文" }, 98);
        voiceState = AddReviewChoice(controls, new List<string>
        {
            "Idle / unavailable", "Preparing", "Listening", "Processing", "Editable transcript", "Spoken response"
        }, 154);
        voiceSurface = AddReviewChoice(controls, new List<string> { "Reflect & Converse", "Assistant window" }, 150);
        voiceReduced = new Toggle("Reduced effects");
        voiceReduced.RegisterValueChangedCallback(_ => ShowVoiceSpecimen());
        controls.Add(voiceReduced);
        rootVisualElement.Add(voiceReviewToolbar);
        FitCanvas();
    }

    private PopupField<string> AddReviewChoice(VisualElement parent, List<string> values, float width)
    {
        var choice = new PopupField<string>(values, 0);
        choice.style.width = width;
        choice.RegisterValueChangedCallback(_ => ShowVoiceSpecimen());
        parent.Add(choice);
        return choice;
    }

    public void SelectVoiceReview(int performer, AppLanguage locale, int specimen, bool overlay, bool reduced)
    {
        voicePerformer.SetValueWithoutNotify(voicePerformer.choices[performer]);
        voiceLocale.SetValueWithoutNotify(voiceLocale.choices[(int)locale]);
        voiceState.SetValueWithoutNotify(voiceState.choices[specimen]);
        voiceSurface.SetValueWithoutNotify(voiceSurface.choices[overlay ? 1 : 0]);
        voiceReduced.SetValueWithoutNotify(reduced);
        ShowVoiceSpecimen();
    }

    private void ShowVoiceSpecimen()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        int performer = voicePerformer.index;
        var locale = (AppLanguage)voiceLocale.index;
        bool chinese = locale == AppLanguage.Chinese;
        int specimen = voiceState.index;
        bool overlay = voiceSurface.index == 1;
        previewData.reducedMotion = voiceReduced.value;
        previewData.reducedEffects = voiceReduced.value;
        voiceReviewCaption.text = chinese
            ? "編輯器設計預覽｜無錄音、播放或服務連線"
            : "EDITOR DESIGN PREVIEW · No recording, playback or service connection";
        voiceReviewCaption.style.unityFontDefinition = FontDefinition.FromFont(chinese ? previewData.traditionalChinese : previewData.bodyEnglish);
        view.Render(overlay ? "COLOR-RELEASE" : LivingGlassRoutes.Chapter(performer, 4), locale, performer, false, true);
        if (overlay) view.OpenOverlay("Assistant-" + LivingGlassRoutes.Performers[performer], locale, performer);
        view.PreviewTick(1);
        if (specimen == 0) return;

        var composer = view.Root.Q("question-composer");
        var caption = composer.Q<Label>("voice-status");
        var accent = LivingGlassAssets.Accents[performer];
        if (specimen <= 3)
        {
            var microphone = composer.Q("microphone-unavailable");
            microphone.style.backgroundColor = Color.Lerp(LivingGlassAssets.Hex("#192633"), accent, .16f);
            microphone.style.borderTopColor = Color.Lerp(LivingGlassAssets.Hex("#526475"), accent, specimen == 2 ? 1 : .55f);
            caption.text = specimen == 1 ? (chinese ? "準備聆聽" : "Preparing")
                : specimen == 2 ? (chinese ? "聆聽中……" : "Listening…")
                : (chinese ? "轉錄中……" : "Transcribing…");
            microphone.Clear();
            microphone.Add(new LivingGlassVoiceSymbol(specimen != 2));
        }
        else if (specimen == 4)
        {
            caption.text = chinese ? "可編輯轉錄" : "Draft ready";
            composer.Q<TextField>("assistantChatInput").SetValueWithoutNotify(chinese
                ? "這件作品如何運用光線，引導我留意當下的感受？"
                : "How does the artwork use light to draw attention to this moment?");
        }
        else
        {
            var transcript = view.Root.Q<ScrollView>("assistantConversation");
            transcript.Q("assistant-empty").AddToClassList("lg-hidden");
            var response = new Label(chinese
                ? "回覆排版示例：留意畫面中最先吸引您目光的細節。"
                : "Response specimen: notice which detail first drew your eye.");
            response.AddToClassList("ac-assistant-chat-message");
            transcript.Add(response);
            transcript.Add(FixtureAction(chinese ? "朗讀這則回覆" : "Read this reply aloud"));
        }
        Repaint();
    }

    private static Label FixtureAction(string text)
    {
        var action = new Label(text) { pickingMode = PickingMode.Ignore };
        action.style.alignSelf = Align.Center;
        action.style.fontSize = 15;
        action.style.whiteSpace = WhiteSpace.Normal;
        action.style.unityTextAlign = TextAnchor.MiddleCenter;
        action.style.color = LivingGlassAssets.Hex("#f3f0e8");
        action.style.backgroundColor = LivingGlassAssets.Hex("#273b4b");
        action.style.paddingLeft = 6;
        action.style.paddingRight = 6;
        action.style.paddingTop = 4;
        action.style.paddingBottom = 4;
        return action;
    }
}
#endif
