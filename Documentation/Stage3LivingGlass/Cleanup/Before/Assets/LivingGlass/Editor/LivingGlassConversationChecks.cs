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
    private const string Folder="Documentation/Stage3LivingGlass/ConversationRefinement";
    private static LivingGlassPreviewWindow window;
    private static LivingGlassPresentation view;
    private static LivingGlassAssets data;
    private static int index;
    private static double due;
    private static readonly List<string> rows=new List<string>();
    private static readonly string[] states={"empty","suggestions","typing","sending","reply","long","failure","retry","cancelled","offline"};
    public static string Status { get; private set; }="NOT RUN";
    public static string Begin()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode only.");
        if(window!=null)throw new InvalidOperationException("Already checking.");
        window=LivingGlassPreviewWindow.Open();var flags=BindingFlags.Instance|BindingFlags.NonPublic;
        view=(LivingGlassPresentation)typeof(LivingGlassPreviewWindow).GetField("view",flags).GetValue(window);
        data=(LivingGlassAssets)typeof(LivingGlassPreviewWindow).GetField("previewData",flags).GetValue(window);
        if(typeof(LivingGlassPresentation).GetField("flow",flags).GetValue(view)!=null)throw new InvalidOperationException("Services forbidden.");
        index=0;rows.Clear();rows.Add("case,performer,locale,width,height,state,transcriptHeight,composerHeight,cancelDisplay");
        Status="RUNNING isolated Editor layouts; no runtime or service requests";
        Show();EditorApplication.update+=Tick;return Status;
    }
    private static void Show()
    {
        int p=index<48?index%6:0;int locale=index<48?(index/6)%2:(index-48)/10;
        int size=index<48?index/12:0;
        int[] widths={1920,1600,1280,1822},heights={1080,900,720,1138};
        view.Root.style.width=widths[size];view.Root.style.height=heights[size];
        data.reducedMotion=index==47;data.reducedEffects=index==47;
        view.Render(LivingGlassRoutes.Chapter(p,4),(AppLanguage)locale,p,false,true);view.PreviewTick(1);
        string state=index<48?"empty":states[(index-48)%10];
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
            Check(left.worldBound.xMax<=side.worldBound.xMin,"zones overlap");
            Check(scroll.worldBound.yMax<=composer.worldBound.yMin+.5f,"transcript overlays composer");
            Check(composer.worldBound.yMax<=left.worldBound.yMax+.5f,"composer outside glass");
            Check(scroll.worldBound.height/scale>=159,"transcript too small");
            Check(next.parent==side,"progression mixed with submission");
            Check(root.Query<TextField>(name:"assistantChatInput").ToList().Count==1,"duplicate editor binding");
            Check(ask.worldBound.xMin>=composer.worldBound.xMin && ask.worldBound.xMax<=composer.worldBound.xMax+.5f,"Ask outside composer");
            bool sending=index>=48&&states[(index-48)%10]=="sending";
            Check(cancel.resolvedStyle.display==(sending?DisplayStyle.Flex:DisplayStyle.None),"Cancel visibility does not match request-enabled state");
            if(sending)Check(ask.worldBound.yMax<=cancel.worldBound.yMin+.5f,"pending controls overlap");
            rows.Add(index+","+LivingGlassRoutes.Performers[index<48?index%6:0]+","+(index<48?(index/6)%2:(index-48)/10)+","+root.resolvedStyle.width+","+root.resolvedStyle.height+","+(index<48?"empty":states[(index-48)%10])+","+scroll.worldBound.height/scale+","+composer.worldBound.height/scale+","+cancel.resolvedStyle.display);
            if(++index<68){Show();return;}
            Finish("PASS: 48 performer/locale/viewport layouts plus20 Editor-only state specimens. No backend behavior or IME runtime verified.");
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
