using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>Bounded visual work only. Completion never advances a route or a media state.</summary>
public sealed class LivingGlassMotion
{
    private sealed class Track
    {
        public object key;
        public float time, duration;
        public Action<float> sample;
        public Action complete;
    }
    private readonly List<Track> tracks = new List<Track>(16);
    public bool Reduced { get; set; }
    public int ActiveCount { get { return tracks.Count; } }
    public void Cancel(object key) { tracks.RemoveAll(t => ReferenceEquals(t.key, key)); }
    public void Clear() { tracks.Clear(); }
    public void Run(object key, float seconds, Action<float> sample, Action complete = null)
    {
        Cancel(key);
        if (Reduced || seconds <= 0) { sample(1); complete?.Invoke(); return; }
        tracks.Add(new Track { key = key, duration = seconds, sample = sample, complete = complete });
        sample(0);
    }
    public void Tick(float delta)
    {
        for (int i = tracks.Count - 1; i >= 0; i--)
        {
            Track t = tracks[i];
            t.time += Mathf.Max(0, delta);
            float n = Mathf.Clamp01(t.time / t.duration);
            // Monotone spring-like settle: no overshoot, no detector timing dependency.
            float eased = 1 - Mathf.Pow(1 - n, 4);
            t.sample(eased);
            if (n >= 1) { tracks.RemoveAt(i); t.complete?.Invoke(); }
        }
    }
    public static void Pose(VisualElement visual, Vector2 offset, float scale)
    {
        visual.style.translate = new Translate(offset.x, offset.y);
        visual.style.scale = new Scale(new Vector3(scale, scale, 1));
    }
    public static void BridgeVisibility(float easedProgress, out float outgoing, out float incoming)
    {
        // Undo the shared quartic ease for timeline windows; the portrait still uses that ease.
        float progress = 1 - Mathf.Pow(1 - Mathf.Clamp01(easedProgress), .25f);
        outgoing = 1 - Mathf.Clamp01(progress / .28f);
        incoming = Mathf.Clamp01((progress - .38f) / .32f);
    }
    public static Rect VisibleCrop(Rect uv, Rect image, Rect aperture)
    {
        if (image.width <= 0 || image.height <= 0) return uv;
        return new Rect(uv.x+(aperture.xMin-image.xMin)/image.width*uv.width,
            uv.y+(image.yMax-aperture.yMax)/image.height*uv.height,
            aperture.width/image.width*uv.width, aperture.height/image.height*uv.height);
    }
}
