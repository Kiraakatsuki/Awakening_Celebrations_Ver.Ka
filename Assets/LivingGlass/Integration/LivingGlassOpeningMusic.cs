using System;
using UnityEngine;

// The existing flow owns these two background tracks; speech/video/clap audio are independent.
internal sealed class LivingGlassOpeningMusic : IDisposable
{
    private readonly Track opening;
    private readonly Track gallery;
    private bool home;
    private bool inGallery;
    private bool soundEnabled = true;
    private bool disposed;

    internal LivingGlassOpeningMusic(GameObject host)
    {
        opening = new Track(host, "Opening page music",
            "Audio/Opening/Awakening_Living_Gallery_Cinematic_Loop_72s", 0.16f);
        gallery = new Track(host, "Gallery music",
            "Audio/Gallery/Spring Will Come When the Snow Melts Away (From Fruits Basket) - Homura Records", 0.14f);
    }

    internal bool GalleryFadeComplete => gallery.Silent;

    internal void Show(bool isHome, bool isGallery)
    {
        home = isHome;
        inGallery = isGallery;
        if (home) opening.Load();
        if (inGallery) gallery.Load();
    }

    internal void SetSoundEnabled(bool enabled)
    {
        if (disposed || enabled == soundEnabled) return;
        soundEnabled = enabled;
        opening.SetSoundEnabled(enabled);
        gallery.SetSoundEnabled(enabled);
    }

    internal void ResetVisitor()
    {
        if (disposed) return;
        home = inGallery = false;
        soundEnabled = true;
        opening.Reset();
        gallery.Reset();
    }

    internal void Tick(bool pageUnobstructed, bool speechActive, float deltaTime, bool focused)
    {
        if (disposed) return;
        // Preserve opening overlay behavior. Gallery keeps its position beneath Settings.
        // Outgoing music reaches zero before a different background track starts.
        opening.Tick(home && pageUnobstructed && !speechActive && gallery.Silent,
            speechActive || !soundEnabled, deltaTime);
        gallery.Tick(inGallery && focused && !speechActive && opening.Silent,
            speechActive || !soundEnabled, deltaTime);
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        opening.Dispose();
        gallery.Dispose();
    }

    private sealed class Track : IDisposable
    {
        private readonly AudioSource source;
        private readonly string resource;
        private readonly float volume;
        private ResourceRequest loading;
        internal bool Silent => source.volume <= 0f;

        internal Track(GameObject host, string name, string resource, float volume)
        {
            this.resource = resource;
            this.volume = volume;
            var playback = new GameObject(name);
            playback.transform.SetParent(host.transform, false);
            source = playback.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.volume = 0f;
        }

        internal void Load()
        {
            if (loading == null && source.clip == null)
                loading = Resources.LoadAsync<AudioClip>(resource);
        }

        internal void SetSoundEnabled(bool enabled)
        {
            // Muting is immediate. Unmuting fades in without restarting the clip.
            source.mute = !enabled;
            if (enabled) source.volume = 0f;
        }

        internal void Tick(bool relevant, bool muted, float deltaTime)
        {
            if (loading != null && loading.isDone)
            {
                source.clip = loading.asset as AudioClip;
                loading = null;
            }
            if (source.clip == null) return;
            source.mute = muted;
            float target = relevant ? volume : 0f;
            float duration = relevant ? 1.8f : 1.25f;
            source.volume = Mathf.MoveTowards(source.volume, target,
                volume * Mathf.Max(0f, deltaTime) / duration);
            if (relevant && !source.isPlaying) source.Play();
            else if (!relevant && source.volume <= 0f && source.isPlaying) source.Stop();
        }

        internal void Reset()
        {
            source.Stop();
            source.volume = 0f;
            source.mute = false;
        }

        public void Dispose()
        {
            loading = null;
            source.Stop();
            UnityEngine.Object.Destroy(source.gameObject);
        }
    }
}
