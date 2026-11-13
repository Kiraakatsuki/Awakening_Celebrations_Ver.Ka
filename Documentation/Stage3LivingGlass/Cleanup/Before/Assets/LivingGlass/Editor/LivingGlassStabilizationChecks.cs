#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>Isolated Editor view/model checks. Never initializes application flow or enters Play Mode.</summary>
public static class LivingGlassStabilizationChecks
{
    private const string Folder = "Documentation/Stage3LivingGlass/Stage31Stabilization";
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly List<string> results = new List<string>();
    private static LivingGlassPreviewWindow window;
    private static LivingGlassPresentation view;
    private static LivingGlassAssets data;
    private static int step;
    private static double due;
    public static string Status { get; private set; } = "NOT RUN";

    public static string Begin()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || window != null) throw new InvalidOperationException("Requires an idle Edit Mode preview.");
        Directory.CreateDirectory(Folder); results.Clear();
        for (int i=0;i<=1000;i++)
        {
            float eased=1-Mathf.Pow(1-i/1000f,4), outgoing,incoming;
            LivingGlassMotion.BridgeVisibility(eased,out outgoing,out incoming);
            Check(outgoing*incoming == 0,"text ownership at sample "+i);
        }
        results.Add("PASS: 1001 shared-ease samples never show outgoing and incoming copy together.");
        window=LivingGlassPreviewWindow.Open();
        view=(LivingGlassPresentation)typeof(LivingGlassPreviewWindow).GetField("view",Private).GetValue(window);
        data=(LivingGlassAssets)typeof(LivingGlassPreviewWindow).GetField("previewData",Private).GetValue(window);
        Check(typeof(LivingGlassPresentation).GetField("flow",Private).GetValue(view)==null,"preview has no application/service owner");
        step=0; Status="RUNNING: isolated Editor view checks, not Play Mode";
        PrepareGallery(); due=EditorApplication.timeSinceStartup+.25;
        EditorApplication.update+=Tick;
        return Status;
    }
    private static void PrepareGallery()
    {
        int p=step%6; AppLanguage locale=step<6?AppLanguage.English:AppLanguage.Chinese;
        data.reducedMotion=false; data.reducedEffects=false;
        view.Render("COLOR-RELEASE",locale,-1,false,true); view.TargetChanged(p); view.PreviewTick(1);
    }
    private static void Tick()
    {
        if (EditorApplication.timeSinceStartup<due) return;
        try
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Runtime not authorized by this checker.");
            if (step<12)
            {
                CheckTransition(step%6,step<6?AppLanguage.English:AppLanguage.Chinese);
                step++;
                if(step<12) {PrepareGallery();due=EditorApplication.timeSinceStartup+.25;return;}
                Show("Home",AppLanguage.English); return;
            }
            if(step==12) {CaptureStatic("Home-en"); step++; Show("Home",AppLanguage.Chinese);return;}
            if(step==13) {CaptureStatic("Home-zh");step++;Show("COLOR-RELEASE",AppLanguage.Chinese,"Settings");return;}
            if(step==14) {CheckScroll();CaptureStatic("Settings-zh");step++;Show("COLOR-RELEASE",AppLanguage.English,"Settings");return;}
            if(step==15) {CheckScroll();CaptureStatic("Settings-en");step++;Show("COLOR-RELEASE",AppLanguage.Chinese,"Privacy");return;}
            if(step==16) {CheckScroll();CaptureStatic("Privacy-zh");step++;Show("COLOR-RELEASE",AppLanguage.Chinese,null,true);return;}
            CaptureStatic("Gallery-reduced-zh");
            results.Add("PASS: isolated bilingual static views and reduced/opaque Gallery constructed without services. Visual references use separately inspected native-window captures, not automatic desktop crops.");
            Finish("PASS: Editor-only model/tree/style checks; runtime retest NOT RUN.");
        }
        catch(Exception error) {results.Add("FAIL: "+error);Finish("FAIL: "+error.Message);}
    }
    private static void CheckTransition(int performer, AppLanguage locale)
    {
        Rect[] before=view.PerformerTargets.Select(e=>e.worldBound).ToArray();
        Check(before.Length==6 && before.All(b=>b.width>0 && b.height>0),"six laid-out wrappers");
        view.Content.style.opacity=.4f;
        view.Render(LivingGlassRoutes.Chapter(performer,0),locale,performer,false,true);
        var outgoing=view.Root.Q("outgoing-content"); var art=view.Content.Q<Image>("art-"+performer);var copy=view.Content.Q("chapter-copy");
        Check(view.Root.Query<VisualElement>(name:"content").ToList().Count==1,"one authoritative page");
        Check(outgoing!=null && outgoing.Query<VisualElement>().ToList().All(e=>e.pickingMode==PickingMode.Ignore&&!e.focusable),"outgoing subtree inert");
        Check(outgoing.style.opacity.value<=.4f,"interrupted entrance does not brighten the outgoing page");
        view.PreviewTick(.13f);
        Check(art.style.visibility.value==Visibility.Hidden && copy.style.visibility.value==Visibility.Hidden,"one portrait and no overlapping copy at early sample");
        view.PreviewTick(.20f);
        Check(outgoing.style.opacity.value==0 && copy.style.opacity.value>0 && art.style.visibility.value==Visibility.Hidden,"outgoing text gone before new copy reveal");
        view.PreviewTick(.32f);
        Check(view.Root.Q("transition-portrait")==null && view.Root.Q("outgoing-content")==null && art.style.visibility.value==Visibility.Visible,"final portrait handoff cleans copies");
        view.PreviewTick(1);
        Check(view.Content.style.opacity.value==1,"no stale page opacity");
        // Repeated navigation, overlay interruption, explicit settle, locale replacement and reduced motion.
        view.Render("COLOR-RELEASE",locale,-1,false,true);view.PreviewTick(1);
        view.Render(LivingGlassRoutes.Chapter(performer,0),locale,performer,false,true);view.PreviewTick(.10f);
        view.OpenOverlay("Settings",locale,performer);view.PreviewTick(1);
        Check(view.Root.Q("transition-portrait")==null && view.Content.Q<Image>("art-"+performer).style.visibility.value==Visibility.Visible,"overlay settles bridge");
        view.CloseOverlay(true);
        view.Render("COLOR-RELEASE",locale,-1,false,true);view.PreviewTick(1);
        view.Render(LivingGlassRoutes.Chapter(performer,0),locale,performer,false,true);view.PreviewTick(.10f);
        view.Render("Home",locale==AppLanguage.English?AppLanguage.Chinese:AppLanguage.English,-1,false,false);view.PreviewTick(1);
        Check(view.Root.Q("transition-portrait")==null && view.Root.Q("outgoing-content")==null && view.Content.style.opacity.value==1,"replacement cancels old generation");
        view.Render("COLOR-RELEASE",locale,-1,false,true);view.PreviewTick(1);
        view.Render(LivingGlassRoutes.Chapter(performer,0),locale,performer,false,true);view.PreviewTick(.10f);view.SettleRoutePresentation();view.PreviewTick(1);
        Check(view.Root.Q("transition-portrait")==null && view.Content.style.opacity.value==1,"explicit settle clears visual work");
        data.reducedMotion=true;
        view.Render("COLOR-RELEASE",locale,-1,false,true);
        view.Render(LivingGlassRoutes.Chapter(performer,0),locale,performer,false,true);
        Check(view.Root.Q("transition-portrait")==null && view.Root.Q("outgoing-content")==null,"reduced mode creates no proxy");
        data.reducedMotion=false;
        results.Add("PASS: "+LivingGlassRoutes.Performers[performer]+" / "+locale+" / exclusive copy/portrait; final cleanup; overlay, locale, settle and reduced interruption contracts. Editor model checks only.");
    }
    private static void Show(string route,AppLanguage locale,string overlay=null,bool reduced=false)
    {
        data.reducedMotion=reduced;data.reducedEffects=reduced;
        view.Render(route,locale,-1,false,true);view.PreviewTick(1);
        if(overlay!=null){view.OpenOverlay(overlay,locale,0);view.PreviewTick(1);}
        window.Focus();window.Repaint();due=EditorApplication.timeSinceStartup+.35;
    }
    private static void CheckScroll()
    {
        var scroll=view.Root.Q<ScrollView>("window-scroll");
        Check(scroll!=null && scroll.focusable && scroll.verticalScrollerVisibility==ScrollerVisibility.Auto,"visible native scroll view keyboard entry");
        Check(scroll.verticalScroller.lowButton.text=="↑" && scroll.verticalScroller.highButton.text=="↓","native repeat controls remain named/visible");
        Color color=scroll.verticalScroller.lowButton.resolvedStyle.backgroundColor;
        Check(color.r<.3f && color.g<.4f && color.b<.5f,"computed native arrow uses dark glass style");
        Check(scroll.contentViewport.worldBound.xMax<=view.Root.Q("glass-window").worldBound.xMax,"viewport contained by reading window");
        scroll.Focus();
        using (var end = KeyDownEvent.GetPooled(new Event { type=EventType.KeyDown, keyCode=KeyCode.End })) { end.target=scroll; scroll.SendEvent(end); }
        Check(Mathf.Abs(scroll.scrollOffset.y-Mathf.Max(0,scroll.verticalScroller.highValue))<.5f,"isolated End event reaches final content");
        using (var home = KeyDownEvent.GetPooled(new Event { type=EventType.KeyDown, keyCode=KeyCode.Home })) { home.target=scroll; scroll.SendEvent(home); }
        Check(scroll.scrollOffset.y==0,"isolated Home event returns to start");
        results.Add("PASS: "+scroll.tooltip+" / computed arrow color="+color+"; native scroll high="+scroll.verticalScroller.highValue+"; Editor styles, not native runtime input.");
    }
    private static void CaptureStatic(string label)
    {
        results.Add("PASS: "+label+" isolated Editor layout completed. Automatic desktop capture excluded because occlusion is not controlled.");
    }
    private static void Finish(string status)
    {
        Status=status;EditorApplication.update-=Tick;
        File.WriteAllLines(Folder+"/EditorChecks.txt",results.Concat(new[]{status}));
        if(window!=null)window.Close();window=null;view=null;data=null;
    }
    private static void Check(bool condition,string message) {if(!condition)throw new InvalidOperationException(message);}
}
#endif
