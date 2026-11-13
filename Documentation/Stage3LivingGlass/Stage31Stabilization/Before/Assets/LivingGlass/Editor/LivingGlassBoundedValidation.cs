#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AwakeningCelebrations.Interaction;
using Mediapipe.Unity.Sample.HandLandmarkDetection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

/// <summary>Explicitly armed, offline Editor validation. Never saves scene/assets or grants consent.</summary>
[InitializeOnLoad]
public static class LivingGlassBoundedValidation
{
    private const string Arm = "LivingGlass.Bounded.Armed";
    private const string Folder = "Documentation/Stage3LivingGlass/BoundedValidation";
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private static AwakeningApplicationController flow;
    private static LivingGlassPresentation view;
    private static LivingGlassAssets previewAssets;
    private static readonly List<string> log = new List<string>();
    private static readonly List<TimedAction> actions = new List<TimedAction>();
    private sealed class TimedAction { public double due; public Action action; }
    static LivingGlassBoundedValidation()
    {
        EditorApplication.playModeStateChanged += Changed;
        EditorApplication.update += Tick;
        Application.logMessageReceived += OnLog;
    }
    private static object Get(object owner, string field) { return owner.GetType().GetField(field, Private).GetValue(owner); }
    private static void Set(object owner, string field, object value) { owner.GetType().GetField(field, Private).SetValue(owner,value); }
    private static object Call(object owner, string method, params object[] values) { return owner.GetType().GetMethod(method, Private).Invoke(owner,values); }

    public static string Prepare()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || ScenePath() != LivingGlassSceneBuilder.ScenePath || UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)
            throw new InvalidOperationException("Requires the saved new scene in Edit Mode.");
        Directory.CreateDirectory(Folder + "/Screenshots");
        flow = UnityEngine.Object.FindAnyObjectByType<AwakeningApplicationController>();
        view = UnityEngine.Object.FindAnyObjectByType<LivingGlassPresentation>();
        if (flow == null || view == null || GestureCameraConsentGate.ConsentGranted) throw new InvalidOperationException("Unsafe baseline.");
        SessionState.SetBool("LG.PrefHad", PlayerPrefs.HasKey("AwakeningCelebrations.Language"));
        SessionState.SetString("LG.Pref", PlayerPrefs.GetString("AwakeningCelebrations.Language","en"));
        foreach (HandLandmarkerRunner runner in UnityEngine.Object.FindObjectsByType<HandLandmarkerRunner>(FindObjectsInactive.Include)) runner.gameObject.SetActive(false);
        var serialized = new SerializedObject(flow);
        serialized.FindProperty("assistantBackendBaseUrl").stringValue = "";
        serialized.ApplyModifiedPropertiesWithoutUndo();
        if ((string)Get(flow,"assistantBackendBaseUrl") != "" || UnityEngine.Object.FindObjectsByType<HandLandmarkerRunner>(FindObjectsInactive.Exclude).Length != 0)
            throw new InvalidOperationException("Isolation failed.");
        ConfigureGameView();
        SessionState.SetString("LG.Deadline", DateTime.UtcNow.AddMinutes(10).ToString("O"));
        SessionState.SetBool(Arm,true);
        File.WriteAllText(Folder+"/Preflight.txt", "PASS: saved candidate scene verified; camera gate denied; rig inactive in unsaved scene; backend endpoint blank; sender/consent controls disabled by guard. No production scene/asset saves. Ten-minute stop deadline per session.\nGallery/Attract downstream fixtures are presentation setup, never gesture recognition results.");
        return "Prepared unsaved isolation overrides. No Play Mode started yet. Game view requests 1920x1080.";
    }
    private static string ScenePath() { return UnityEngine.SceneManagement.SceneManager.GetActiveScene().path; }
    private static void Changed(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(Arm,false)) return;
        if (state == PlayModeStateChange.EnteredPlayMode) { flow = null; view = null; log.Clear(); actions.Clear(); }
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            SessionState.SetBool(Arm,false);
            File.WriteAllLines(Folder+"/RuntimeLog.txt", log);
            actions.Clear();
            if (SessionState.GetBool("LG.PrefHad",false)) PlayerPrefs.SetString("AwakeningCelebrations.Language", SessionState.GetString("LG.Pref","en"));
            else PlayerPrefs.DeleteKey("AwakeningCelebrations.Language");
            PlayerPrefs.Save();
            RestoreGameView();
            // Discard only our known unsaved rig/endpoint overrides; source scene on disk is untouched.
            EditorSceneManager.OpenScene(LivingGlassSceneBuilder.ScenePath, OpenSceneMode.Single);
        }
    }
    private static void OnLog(string message, string stack, LogType type)
    {
        if (SessionState.GetBool(Arm,false) && EditorApplication.isPlaying)
            log.Add(DateTime.UtcNow.ToString("O") + " | " + type + " | " + message + (type == LogType.Exception ? "\n"+stack : ""));
    }
    private static void Tick()
    {
        if (!SessionState.GetBool(Arm,false) || !EditorApplication.isPlaying) return;
        if (flow == null) flow = UnityEngine.Object.FindAnyObjectByType<AwakeningApplicationController>();
        if (view == null) view = UnityEngine.Object.FindAnyObjectByType<LivingGlassPresentation>();
        bool unsafeState = GestureCameraConsentGate.ConsentGranted || (flow != null && (flow.LivingConsent || (string)Get(flow,"assistantBackendBaseUrl") != "" || Get(flow,"assistantWebRequest") != null));
        foreach (HandLandmarkerRunner runner in UnityEngine.Object.FindObjectsByType<HandLandmarkerRunner>(FindObjectsInactive.Include)) unsafeState |= runner.isActiveAndEnabled || runner.IsPipelineStarted;
        if (unsafeState)
        {
            GestureCameraConsentGate.SetConsent(false);
            if (flow != null) Call(flow,"CancelAssistantRequest");
            log.Add("FAIL: isolation guard stopped the pass.");
            EditorApplication.isPlaying = false; return;
        }
        if (DateTime.UtcNow > DateTime.Parse(SessionState.GetString("LG.Deadline",DateTime.UtcNow.ToString("O"))))
        { log.Add("Bounded session deadline reached."); EditorApplication.isPlaying=false; return; }
        if (view != null && view.Root != null)
        {
            foreach (string key in new[] { "consent", "privacy-consent", "assistantChatSend" }) view.Root.Q(key)?.SetEnabled(false);
            view.Root.Query<Button>(className:"lg-suggestion").ForEach(button => button.SetEnabled(false));
        }
        double now = EditorApplication.timeSinceStartup;
        for (int i=actions.Count-1;i>=0;i--) if (actions[i].due <= now) { Action action=actions[i].action; actions.RemoveAt(i); action(); }
    }
    public static string Gallery()
    {
        Require();
        if (flow.LivingProtected) throw new InvalidOperationException("Exit protected media through its real Back/completion path first.");
        Set(flow,"hasAwakenedThisSession",true); Set(flow,"awakeningSequenceCompletedThisSession",true); Set(flow,"colorReleaseSelectionReady",true);
        bool result = flow.Navigate("COLOR-RELEASE",flow.Language,false);
        return "Presentation fixture only (no gesture/consent simulation): Gallery navigate="+result;
    }
    public static string Route(string route)
    {
        Require();
        if (flow.LivingProtected) throw new InvalidOperationException("Protected route active.");
        Set(flow,"hasAwakenedThisSession",false); Set(flow,"awakeningSequenceCompletedThisSession",false); Set(flow,"colorReleaseSelectionReady",false);
        if (route == "Home") { flow.ShowHome(false); return "Home fixture."; }
        if (route == "ATTRACT---MONOCHROME---FALLBACK-INPUT")
        {
            Set(flow,"currentScreen",route); Set(flow,"activeBindingScreen",route); Call(flow,"LivingGlassRender");
            return "Attract presentation fixture; normal consent-gated entry remains blocked.";
        }
        return "Route fixture navigate="+flow.Navigate(route,flow.Language,false);
    }
    public static void Modes(bool reduced, bool opaque)
    {
        Require(); if (flow.LivingProtected) throw new InvalidOperationException("Stable state required.");
        if (previewAssets == null) { previewAssets=UnityEngine.Object.Instantiate((LivingGlassAssets)Get(view,"assets")); previewAssets.hideFlags=HideFlags.DontSave; Set(view,"assets",previewAssets); }
        previewAssets.reducedMotion=reduced; previewAssets.reducedEffects=opaque; Call(flow,"LivingGlassRender");
    }
    public static void Capture(string label)
    {
        Require(); State(label);
        ScreenCapture.CaptureScreenshot(Folder+"/Screenshots/"+label+".png");
    }
    public static void After(float seconds, Action action) { actions.Add(new TimedAction { due=EditorApplication.timeSinceStartup+seconds, action=action }); }
    public static string Burst(string label)
    {
        Require();
        foreach (float time in new[] { .02f,.12f,.28f,.48f,.75f })
        { float at=time; After(at,()=>Capture(label+"-"+Mathf.RoundToInt(at*1000))); }
        return "Queued actual rendered frame snapshots; no semantic/media events are synthesized.";
    }
    [Serializable] private sealed class ElementRecord { public string name; public Rect bounds; public float opacity,scale; public Vector3 translation; public bool enabled; public string text; }
    [Serializable] private sealed class Snapshot
    {
        public string label,utc,route,locale,focus,videoState; public int width,height,frame,performer,players,playingPlayers;
        public bool cameraConsent,sessionConsent,rigInactive,requestAbsent,overlay; public ElementRecord[] elements;
    }
    public static string State(string label)
    {
        Require();
        var names = new List<string> { "content","gallery-arc","identification","shell","glass-window","camera","assistantChatStatus","resume-window" };
        for (int i=0;i<6;i++) { names.Add("presence-"+i); names.Add("aperture-"+i); names.Add("performer-name-"+i); names.Add("art-"+i); }
        var elements = new List<ElementRecord>();
        foreach (string name in names)
        {
            VisualElement e=view.Root.Q(name); if(e==null)continue;
            elements.Add(new ElementRecord { name=name,bounds=e.worldBound,opacity=e.resolvedStyle.opacity,scale=e.resolvedStyle.scale.value.x,translation=e.resolvedStyle.translate,enabled=e.enabledInHierarchy,text=(e as TextElement)?.text });
        }
        var players=UnityEngine.Object.FindObjectsByType<VideoPlayer>(FindObjectsInactive.Include);
        var p=players.FirstOrDefault();
        var focused=view.Root.focusController?.focusedElement as VisualElement;
        var data=new Snapshot { label=label,utc=DateTime.UtcNow.ToString("O"),route=flow.CurrentScreen,locale=flow.Language.ToString(),focus=focused?.name,
            width=Screen.width,height=Screen.height,frame=Time.frameCount,performer=flow.SelectedPerformer,cameraConsent=GestureCameraConsentGate.ConsentGranted,sessionConsent=flow.LivingConsent,
            rigInactive=UnityEngine.Object.FindObjectsByType<HandLandmarkerRunner>(FindObjectsInactive.Include).All(r=>!r.isActiveAndEnabled&&!r.IsPipelineStarted),requestAbsent=Get(flow,"assistantWebRequest")==null,
            overlay=view.OverlayVisible,players=players.Length,playingPlayers=players.Count(v=>v.isPlaying),videoState=p==null?"none":"prepared="+p.isPrepared+",playing="+p.isPlaying+",frame="+p.frame+",time="+p.time+",clip="+(p.clip==null?"none":p.clip.name),elements=elements.ToArray() };
        string json=JsonUtility.ToJson(data,true); File.WriteAllText(Folder+"/"+label+".json",json);
        return "route="+data.route+";locale="+data.locale+";focus="+data.focus+";size="+data.width+"x"+data.height+";overlay="+data.overlay+";consent="+data.cameraConsent+";rigInactive="+data.rigInactive+";requestAbsent="+data.requestAbsent+";"+data.videoState;
    }
    private static void Require()
    {
        if (!SessionState.GetBool(Arm,false)||!EditorApplication.isPlaying)throw new InvalidOperationException("Bounded session not active.");
        if(flow==null)flow=UnityEngine.Object.FindAnyObjectByType<AwakeningApplicationController>();
        if(view==null)view=UnityEngine.Object.FindAnyObjectByType<LivingGlassPresentation>();
        if(flow==null||view==null||view.Root==null)throw new InvalidOperationException("UI not initialized yet.");
    }
    public static string RunMediaSequence()
    {
        Require();
        int actor=0; bool first=false,loop=false,error=false,activation=false,advancing=false,cancelCase=false;
        var rows=new List<string>{"actor,firstFrame,realLoopEvent,error,completionRoute"};
        VideoPlayer hooked=null;
        Action kick=null;
        kick=()=>
        {
            first=loop=error=activation=advancing=false;
            Gallery();
            After(.8f,()=>flow.LivingConfirm(actor));
            After(1.7f,()=>flow.LivingContinue());
        };
        EditorApplication.CallbackFunction watch=null;
        watch=()=>
        {
            if(!EditorApplication.isPlaying||!SessionState.GetBool(Arm,false)){EditorApplication.update-=watch;return;}
            var player=UnityEngine.Object.FindAnyObjectByType<VideoPlayer>();
            if(player!=null&&player!=hooked)
            {
                hooked=player;
                player.loopPointReached+=source=>{loop=true;State("Media-real-loop-"+actor);};
                player.errorReceived+=(source,message)=>{error=true;File.AppendAllText(Folder+"/MediaErrors.txt",actor+": "+message+"\n");};
            }
            string route=flow.CurrentScreen;
            if(route.Contains("---ACTIVATION---")&&!activation){activation=true;Capture("Media-activation-"+actor);}
            if(route.Contains("---LIVING-PORTRAIT---")&&player!=null&&player.isPlaying&&player.frame>2&&!first)
            {
                first=true;Capture((cancelCase?"Cancel":"Media")+"-decoded-"+actor);
                if(cancelCase)
                {
                    advancing=true;
                    After(.3f,()=>flow.Back());
                    After(1.2f,()=>
                    {
                        bool stopped=!player.isPlaying&&Get(flow,"livingPortraitRenderTexture")==null&&!(bool)Get(flow,"livingPortraitPlayerEventsRegistered");
                        File.WriteAllText(Folder+"/MediaCancellation.txt",(stopped?"PASS":"FAIL")+": actual decoded portrait canceled through Back; player stopped, owned texture released, handlers unregistered. Route="+flow.CurrentScreen);
                        Capture("Media-cancel-return"); EditorApplication.update-=watch;
                        File.WriteAllLines(Folder+"/MediaResults.csv",rows);
                    });
                }
            }
            if(route.Contains("---RELEASE---")&&!flow.LivingProtected&&!advancing&&!cancelCase)
            {
                advancing=true; rows.Add(LivingGlassRoutes.Performers[actor]+","+first+","+loop+","+error+","+route);
                Capture("Media-release-"+actor);
                After(.2f,()=>flow.LivingContinue());
                After(.7f,()=>Capture("Media-postvideo-"+actor));
                After(1.0f,()=>
                {
                    if(actor<5){actor++;kick();}
                    else
                    {
                        File.WriteAllLines(Folder+"/MediaResults.csv",rows);
                        flow.LivingContinue();
                        After(.6f,()=>Capture("Closing-live"));
                        After(.8f,()=>flow.LivingSouvenir());
                        After(1.4f,()=>Capture("Souvenir-unavailable-live"));
                        After(1.7f,()=>{actor=0;cancelCase=true;kick();});
                    }
                });
            }
        };
        EditorApplication.update+=watch;
        kick();
        return "Six local 20-second clips queued using actual production playback and loopPointReached; no completion simulation or speed changes. One real cancellation follows.";
    }
    private static void ConfigureGameView()
    {
        var assembly=typeof(Editor).Assembly; Type type=assembly.GetType("UnityEditor.GameView");
        var window=EditorWindow.GetWindow(type);
        SessionState.SetInt("LG.SizeIndex",(int)type.GetProperty("selectedSizeIndex",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).GetValue(window));
        SessionState.SetBool("LG.Maximized",window.maximized);
        Type sizesType=assembly.GetType("UnityEditor.GameViewSizes");
        object sizes=typeof(ScriptableSingleton<>).MakeGenericType(sizesType).GetProperty("instance").GetValue(null);
        Type groupType=assembly.GetType("UnityEditor.GameViewSizeGroupType");
        object group=sizesType.GetMethod("GetGroup").Invoke(sizes,new[]{Enum.Parse(groupType,"Standalone")});
        int count=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null); int chosen=-1;
        for(int i=0;i<count;i++)
        {
            object size=group.GetType().GetMethod("GetGameViewSize").Invoke(group,new object[]{i});
            if((int)size.GetType().GetProperty("width").GetValue(size)==1920&&(int)size.GetType().GetProperty("height").GetValue(size)==1080){chosen=i;break;}
        }
        SessionState.SetInt("LG.AddedSize",-1);
        if(chosen<0)
        {
            Type sizeType=assembly.GetType("UnityEditor.GameViewSize"); Type kind=assembly.GetType("UnityEditor.GameViewSizeType");
            object size=Activator.CreateInstance(sizeType,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic,null,new object[]{Enum.Parse(kind,"FixedResolution"),1920,1080,"Living Glass bounded test"},null);
            int custom=(int)group.GetType().GetMethod("GetCustomCount").Invoke(group,null);
            group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size}); SessionState.SetInt("LG.AddedSize",custom); chosen=count;
        }
        type.GetProperty("selectedSizeIndex",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).SetValue(window,chosen);
        window.maximized=true; window.Focus(); window.Repaint();
    }
    private static void RestoreGameView()
    {
        Type type=typeof(Editor).Assembly.GetType("UnityEditor.GameView"); var window=EditorWindow.GetWindow(type);
        type.GetProperty("selectedSizeIndex",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).SetValue(window,SessionState.GetInt("LG.SizeIndex",0));
        window.maximized=SessionState.GetBool("LG.Maximized",false);
        int added=SessionState.GetInt("LG.AddedSize",-1);
        if(added>=0)
        {
            Type sizesType=typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes"); object sizes=typeof(ScriptableSingleton<>).MakeGenericType(sizesType).GetProperty("instance").GetValue(null);
            Type groupType=typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeGroupType"); object group=sizesType.GetMethod("GetGroup").Invoke(sizes,new[]{Enum.Parse(groupType,"Standalone")});
            group.GetType().GetMethod("RemoveCustomSize").Invoke(group,new object[]{added});
        }
    }
}
#endif
