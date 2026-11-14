using System;
using System.Collections.Generic;
using UnityEngine;

internal static class GuideCues
{
    internal static string For(string route)
    {
        switch (route)
        {
            case "Home": return "welcome";
            case "Privacy": return "privacy";
            case "TUTORIAL---ENTRY---DIM": return "overview";
            case "TUTORIAL---INTRODUCTION":
            case "TUTORIAL---RETURN---DISMISS": return "tutorial-move";
            case "TUTORIAL---LOOK---MOVE": return "tutorial-colour";
            case "TUTORIAL---POINT": return "gestures";
            case "TUTORIAL---HOLD---SELECT": return "tutorial-confirm";
            case "TUTORIAL---EXPERIENCE": return "tutorial-portrait";
            case "TUTORIAL---READY": return "tutorial-ready";
            case "ATTRACT---MONOCHROME---FALLBACK-INPUT": return "awakening";
            case "COLOR-RELEASE": return "discovery";
            case "CLOSING---CELEBRATION": return "closing";
        }
        return LivingGlassRoutes.Family(route) == "POST-VIDEO-AI" ? "reflection" : null;
    }
}

// One attempt per cue and locale in a visit. Only an explicit replay bypasses
// this policy, and only for the currently visible context.
internal sealed class GuideVisit
{
    private readonly HashSet<string> consumed = new HashSet<string>();
    internal bool Enabled { get; private set; }

    internal void Set(bool enabled) { Enabled = enabled; }
    internal void Reset() { Enabled = false; consumed.Clear(); }

    internal bool TryConsume(string cue, AppLanguage language, bool replay = false)
    {
        if (!Enabled || cue == null) return false;
        bool fresh = consumed.Add(cue + ":" + language);
        return fresh || replay;
    }
}

// A superseded asynchronous resource load cannot start on a new view, even
// when that view uses the same cue (for example, another performer).
internal sealed class GuideLoadAuthority
{
    private uint revision;
    internal uint Begin() => ++revision;
    internal void Invalidate() { revision++; }
    internal bool Accepts(uint ticket) => revision == ticket;
}

[Serializable]
internal sealed class GuideClipEntry
{
    public string id;
    public string en;
    public string zh;
}

[Serializable]
internal sealed class ApprovedGuideCatalog
{
    public bool approved;
    public string revision;
    public GuideClipEntry[] clips;

    internal string PathFor(string cue, AppLanguage language)
    {
        if (!approved || string.IsNullOrWhiteSpace(revision) || clips == null || cue == null) return null;
        foreach (var item in clips)
        {
            if (item == null || item.id != cue) continue;
            string path = language == AppLanguage.Chinese ? item.zh : item.en;
            string expected = "LivingGlassGuide/Approved/" + cue + (language == AppLanguage.Chinese ? "-zh" : "-en");
            return path == expected ? path : null;
        }
        return null;
    }
}

// A visit owns one playback source for both kinds of speech. Existing
// environmental and video owners are observed and never retuned or ducked.
internal sealed class LivingGlassVoiceCoordinator : IDisposable
{
    private readonly UnityReplyAudio playback;
    private readonly Func<bool> blocked;
    private readonly Func<bool> overlayPresented;
    private readonly GuideVisit visit = new GuideVisit();
    private readonly GuideLoadAuthority loadAuthority = new GuideLoadAuthority();
    private ApprovedGuideCatalog catalog;
    private string route;
    private AppLanguage locale;
    private bool overlay, galleryReady, guidePlaying;
    private ResourceRequest loading;
    private string loadingPath;
    private uint loadTicket;
    private int performer = -1;
    private bool tutorialOptInInOverlay;

    internal LivingGlassVoiceCoordinator(GameObject host, Func<bool> blocked, Func<bool> overlayPresented)
    {
        playback = new UnityReplyAudio(host);
        this.blocked = blocked;
        this.overlayPresented = overlayPresented;
        var index = Resources.Load<TextAsset>("LivingGlassGuide/ApprovedCatalog");
        if (index != null)
        {
            try { catalog = JsonUtility.FromJson<ApprovedGuideCatalog>(index.text); }
            catch (ArgumentException) { }
        }
    }

    internal bool Enabled => visit.Enabled;
    internal IReplyAudio BindReply() => new ReplyChannel(playback);
    private string CurrentCue => route == "COLOR-RELEASE" && !galleryReady ? null : GuideCues.For(route);

    internal void Set(bool enabled)
    {
        if (visit.Enabled == enabled) return;
        visit.Set(enabled);
        tutorialOptInInOverlay = enabled && overlay && LivingGlassRoutes.Family(route) == "Tutorial";
        if (!enabled) CutSpeech();
        else QueueGuide();
    }

    internal void Show(string route, AppLanguage language, bool galleryReady, int performer)
    {
        bool changed = this.route != route || locale != language || this.galleryReady != galleryReady
            || this.performer != performer;
        if (!changed) return;
        tutorialOptInInOverlay = false;
        CutSpeech();
        this.route = route;
        locale = language;
        this.galleryReady = galleryReady;
        this.performer = performer;
        QueueGuide();
    }

    internal void Overlay(bool open)
    {
        if (overlay == open) return;
        overlay = open;
        if (open) CutSpeech();
    }

    private void QueueGuide()
    {
        string path = catalog?.PathFor(CurrentCue, locale);
        if (!GuidePlaybackPolicy.CanLoad(path, overlay, blocked(), visit, CurrentCue, locale)) return;
        loadingPath = path;
        loadTicket = loadAuthority.Begin();
        loading = Resources.LoadAsync<AudioClip>(path);
    }

    internal void Tick()
    {
        playback.Tick();
        if (guidePlaying && !playback.IsPlaying) guidePlaying = false;
        if (blocked() || overlay || !visit.Enabled)
        {
            if (loading != null || guidePlaying) InterruptGuide();
            return;
        }
        // An explicit tutorial opt-in waits for the Settings exit animation.
        if (tutorialOptInInOverlay && !overlayPresented())
        {
            tutorialOptInInOverlay = false;
            QueueGuide();
        }
        if (loading == null || !loading.isDone) return;
        var clip = loading.asset as AudioClip;
        if (clip != null && clip.loadState == AudioDataLoadState.Loading) return;
        bool current = loadAuthority.Accepts(loadTicket) && catalog?.PathFor(CurrentCue, locale) == loadingPath;
        loading = null;
        loadingPath = null;
        loadAuthority.Invalidate();
        if (current && clip != null && clip.loadState == AudioDataLoadState.Loaded)
            guidePlaying = playback.TryPlayGuide(clip);
    }

    internal void InterruptGuide()
    {
        // ResourceRequest cannot be cancelled; clearing this reference prevents
        // a completed obsolete load from gaining playback ownership.
        loading = null;
        loadingPath = null;
        loadAuthority.Invalidate();
        if (!guidePlaying) return;
        playback.Stop();
        guidePlaying = false;
    }

    internal void CutSpeech()
    {
        InterruptGuide();
        playback.Stop();
    }

    internal void Reset()
    {
        CutSpeech();
        visit.Reset();
        overlay = false;
        tutorialOptInInOverlay = false;
        route = null;
        performer = -1;
    }

    public void Dispose() { Reset(); playback.Dispose(); }

    // A detached chat only releases its own dynamic playback, never the guide
    // source or the coordinator that outlives it.
    private sealed class ReplyChannel : IReplyAudio
    {
        private readonly UnityReplyAudio owner;
        private bool owns;
        internal ReplyChannel(UnityReplyAudio owner) { this.owner = owner; }
        public bool IsPlaying => owns && owner.IsPlaying;
        public bool TryPlay(float[] samples) { owns = owner.TryPlay(samples); return owns; }
        public void Stop() { if (owns) owner.Stop(); owns = false; }
        public void Dispose() => Stop();
        public void Fade()
        {
            if (owns) owner.Fade();
            owns = false;
        }
    }

    internal static void FadeReply(IReplyAudio channel)
    {
        if (channel is ReplyChannel reply) reply.Fade();
        else channel.Stop();
    }
}

internal static class GuidePlaybackPolicy
{
    internal static bool CanLoad(string path, bool overlay, bool blocked, GuideVisit visit, string cue, AppLanguage locale) =>
        path != null && !overlay && !blocked && visit.TryConsume(cue, locale);
}
