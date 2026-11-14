#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>Isolated layout/state specimens, never application sessions or assistant responses.</summary>
public static class LivingGlassConversationChecks
{
    private const string Folder="Documentation/Stage4VoiceExperience/Stage4_1";
    private const int ReflectionCases = 68;
    private const int CasesPerSurface = 48;
    private const int TotalCases = ReflectionCases + 2 * CasesPerSurface;
    private static LivingGlassPreviewWindow window;
    private static LivingGlassPresentation view;
    private static LivingGlassAssets data;
    private static int index;
    private static double due;
    private static bool checkChangedLayout;
    private static Rect initialAsk;
    private static Vector2 expectedViewport;
    private static readonly List<string> rows=new List<string>();
    private static readonly string[] states={"empty","suggestions","typing","sending","reply","long","failure","retry","cancelled","offline"};
    public static string Status { get; private set; }="NOT RUN";
    public static string Begin()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode only.");
        if(window!=null)throw new InvalidOperationException("Already checking.");
        window=LivingGlassPreviewWindow.Open();var flags=BindingFlags.Instance|BindingFlags.NonPublic;
        view=window.Presentation;
        data=window.PreviewAssets;
        if(typeof(LivingGlassPresentation).GetField("flow",flags).GetValue(view)!=null)throw new InvalidOperationException("Services forbidden.");
        index=0;rows.Clear();rows.Add("case,performer,locale,width,height,state,transcriptHeight,composerHeight,cancelDisplay,surface,askRightInset,askBottomInset");
        Status="RUNNING isolated Editor layouts; no runtime or service requests";
        Show();EditorApplication.update+=Tick;return Status;
    }
    private static void Show()
    {
        bool reflection = index < ReflectionCases;
        bool overlay = index >= ReflectionCases + CasesPerSurface;
        int layout = reflection ? index : (index - ReflectionCases) % CasesPerSurface;
        int p = reflection && layout >= 48 ? 0 : layout % 6;
        int locale = reflection && layout >= 48 ? (layout - 48) / 10 : (layout / 6) % 2;
        int size = reflection && layout >= 48 ? 0 : layout / 12;
        int[] widths={1920,1600,1280,1822},heights={1080,900,720,1138};
        view.Root.style.width=widths[size];view.Root.style.height=heights[size];
        expectedViewport = new Vector2(widths[size], heights[size]);
        data.reducedMotion=index==47;data.reducedEffects=index==47;
        string assistant = "Assistant-" + LivingGlassRoutes.Performers[p];
        view.Render(reflection ? LivingGlassRoutes.Chapter(p,4) : overlay ? "COLOR-RELEASE" : assistant,(AppLanguage)locale,p,false,true);
        if (overlay) view.OpenOverlay(assistant, (AppLanguage)locale, p);
        view.PreviewTick(1);
        string state=reflection && index>=48 ? states[(index-48)%10] : "empty";
        var input=view.Root.Q<TextField>("assistantChatInput");var send=view.Root.Q<Button>("assistantChatSend");var cancel=view.Root.Q<Button>("assistantCancel");
        if(state=="typing")input.SetValueWithoutNotify(locale==0?"A draft question":"尚未送出的問題");
        if(state=="sending"){send.SetEnabled(false);cancel.SetEnabled(true);view.Root.Q<Label>("assistantChatStatus").text=locale==0?"Thinking…":"思考中…";}
        if(state=="reply"||state=="long")
        {
            view.Root.Q("assistant-empty").AddToClassList("lg-hidden");
            var label=new Label(string.Concat(Enumerable.Repeat(locale==0?"EDITOR LAYOUT SPECIMEN — not an assistant reply. Mixed GAIA / WHITE-GUARDIAN text.\n":"編輯器排版示例，並非助理回覆。混合 GAIA / WHITE-GUARDIAN 文字。\n",state=="long"?24:2))){enableRichText=false};
            label.AddToClassList("ac-assistant-chat-message");view.Root.Q<ScrollView>("assistantConversation").Add(label);
        }
        if(state=="failure"||state=="retry"||state=="offline"||state=="cancelled")
        {
            view.Root.Q<Label>("assistantChatStatus").text=locale==0?"EDITOR STATE SPECIMEN: "+state:"編輯器狀態示例："+state;
            if(state=="failure"||state=="retry")send.text=locale==0?"Retry":"重試";
        }
        checkChangedLayout = false;
        due=EditorApplication.timeSinceStartup+.25;
    }
    private static void Tick()
    {
        if(EditorApplication.timeSinceStartup<due)return;
        try
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Runtime forbidden.");
            var root=view.Root;var left=root.Q("reflection-interaction");var side=root.Q("reflection-artwork");var scroll=root.Q<ScrollView>("assistantConversation");
            var composer=root.Q("question-composer");var ask=root.Q<Button>("assistantChatSend");var cancel=root.Q<Button>("assistantCancel");var next=root.Q<Button>("finish-chapter");
            float scale=root.worldBound.width/root.resolvedStyle.width;
            Check(Mathf.Abs(root.resolvedStyle.width - expectedViewport.x) < 1 && Mathf.Abs(root.resolvedStyle.height - expectedViewport.y) < 1, "viewport settled");
            Check(scroll.worldBound.yMax<=composer.worldBound.yMin+.5f,"transcript overlays composer");
            var surface = left ?? root.Q("glass-window") ?? root.Q("reading");
            Check(composer.worldBound.yMax<=surface.worldBound.yMax+.5f,"composer outside glass");
            Check(scroll.worldBound.height/scale>=(left != null ? 159 : 119),"transcript too small");
            if (left != null)
            {
                Check(left.worldBound.xMax<=side.worldBound.xMin,"zones overlap");
                Check(next.parent==side,"progression mixed with submission");
            }
            Check(root.Query<TextField>(name:"assistantChatInput").ToList().Count==1,"duplicate editor binding");
            var voice = composer.Q("microphone-unavailable");
            var input = composer.Q<TextField>("assistantChatInput");
            Check(voice != null && !voice.focusable && voice.pickingMode==PickingMode.Ignore && !(voice is Button),"voice marker is display-only");
            Check(voice.worldBound.xMax <= input.worldBound.xMin && input.worldBound.xMax <= ask.worldBound.xMin,"composer zones overlap");
            Check(input.worldBound.width / scale >= 175, "question editor too narrow");
            float rightInset = (composer.worldBound.xMax - ask.worldBound.xMax) / scale;
            float bottomInset = (composer.worldBound.yMax - ask.worldBound.yMax) / scale;
            Check(rightInset >= 12 && rightInset <= 14 && bottomInset >= 10 && bottomInset <= 12,"Ask corner insets incorrect");
            Check(ask.worldBound.height / scale >= 49 && ask.worldBound.width / scale >= 140, "Ask target too small");
            bool sending=checkChangedLayout || (index>=48 && index<ReflectionCases && states[(index-48)%10]=="sending");
            Check(cancel.resolvedStyle.display==(sending?DisplayStyle.Flex:DisplayStyle.None),"Cancel visibility does not match request-enabled state");
            if(sending)Check(cancel.worldBound.yMax<=ask.worldBound.yMin+.5f,"pending controls overlap");
            if (index >= 48 && index < ReflectionCases && states[(index-48)%10] == "long")
            {
                float end = scroll.verticalScroller.highValue;
                Check(end > 0, "long reply has no scroll range");
                scroll.Focus();
                using (var key = KeyDownEvent.GetPooled(new Event { type = EventType.KeyDown, keyCode = KeyCode.End }))
                {
                    key.target = scroll;
                    scroll.SendEvent(key);
                }
                Check(Mathf.Abs(scroll.scrollOffset.y - end) < .75f, "transcript End key does not reach the reply end");
                Check(ask.worldBound.yMax <= composer.worldBound.yMax, "scrolling displaced Ask");
            }
            if (checkChangedLayout)
                Check(Vector2.Distance(initialAsk.position, ask.worldBound.position) < .75f &&
                      Vector2.Distance(initialAsk.size, ask.worldBound.size) < .75f,
                      "Ask moved after draft, suggestions or pending change: " + initialAsk + " -> " + ask.worldBound);
            if (index < 48 || index >= ReflectionCases)
            {
                if (!checkChangedLayout)
                {
                    initialAsk = ask.worldBound;
                    input.SetValueWithoutNotify(string.Concat(Enumerable.Repeat(input.label + " · GAIA\n", 28)));
                    root.Q("suggestions").AddToClassList("lg-hidden");
                    ask.SetEnabled(false);
                    cancel.SetEnabled(true);
                    checkChangedLayout = true;
                    due = EditorApplication.timeSinceStartup + .25;
                    return;
                }
            }
            int layout = index < ReflectionCases ? index : (index - ReflectionCases) % CasesPerSurface;
            bool specimen = index >= 48 && index < ReflectionCases;
            string surfaceName = index < ReflectionCases ? "reflection" : index < ReflectionCases + CasesPerSurface ? "assistant-page" : "assistant-window";
            rows.Add(FormattableString.Invariant($"{index},{LivingGlassRoutes.Performers[specimen ? 0 : layout % 6]},{(specimen ? (layout - 48) / 10 : (layout / 6) % 2)},{root.resolvedStyle.width},{root.resolvedStyle.height},{(specimen ? states[(layout-48)%10] : "draft-hidden-suggestions-pending")},{scroll.worldBound.height/scale},{composer.worldBound.height/scale},{cancel.resolvedStyle.display},{surfaceName},{rightInset},{bottomInset}"));
            if(++index<TotalCases){Show();return;}
            Finish("PASS: 144 theme/locale/viewport layouts across three surfaces, plus the existing 20 Editor-only state specimens. Layout cases also checked Ask stability with a long draft, hidden suggestions and a pending request. No backend or native IME verified.");
        }
        catch(Exception e){Finish("FAIL case "+index+": "+e.Message);}
    }
    private static void Finish(string result)
    {
        Directory.CreateDirectory(Folder);File.WriteAllLines(Folder+"/LayoutChecks.csv",rows);File.WriteAllText(Folder+"/EditorChecks.txt",result);
        Status=result;EditorApplication.update-=Tick;if(window!=null)window.Close();window=null;view=null;data=null;
    }
    private static void Check(bool value,string reason){if(!value)throw new InvalidOperationException(reason);}
}
#endif
