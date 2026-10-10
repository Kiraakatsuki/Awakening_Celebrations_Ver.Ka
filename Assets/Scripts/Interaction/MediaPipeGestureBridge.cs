using System;
using Stopwatch = System.Diagnostics.Stopwatch;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Unity.Sample.HandLandmarkDetection;
using UnityEngine;

namespace AwakeningCelebrations.Interaction
{
    /// <summary>
    /// Copies the existing sample runner's results and processes them on Unity's
    /// main thread. Attach beside HandLandmarkerRunner; this component does not
    /// create a camera, model, or navigation dependency.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MediaPipeGestureBridge : MonoBehaviour
    {
        [SerializeField] private GestureRecognitionSettings settings = new GestureRecognitionSettings();
        [SerializeField] private bool logRecognizedGestures = true;

        private readonly object resultLock = new object();
        private GestureRecognitionLayer recognizer;
        private HandLandmarkerRunner runner;
        private TrackedHand[] latestHands = Array.Empty<TrackedHand>();
        private long latestSequence;
        private long processedSequence;
        private double latestResultTime;
        private double lastNonEmptyHandsTime;
        private double bridgeEnabledTime;
        private bool tracking;
        private bool warnedNoResults;

        public event Action<GestureRecognitionEvent> GestureDetected;
        public event Action<bool> TrackingChanged;

        private void Awake()
        {
            recognizer = new GestureRecognitionLayer(settings);
            recognizer.GestureDetected += ForwardGesture;
        }

        private void OnEnable()
        {
            runner = GetComponent<HandLandmarkerRunner>();
            if (runner == null)
            {
                Debug.LogWarning("MediaPipeGestureBridge requires a HandLandmarkerRunner on the same GameObject.", this);
                return;
            }

            runner.ResultReceived += OnResultReceived;
            bridgeEnabledTime = NowSeconds();
            warnedNoResults = false;
        }

        private void OnDisable()
        {
            if (runner != null)
            {
                runner.ResultReceived -= OnResultReceived;
            }
            SetTracking(false);
            recognizer?.Reset();
        }

        private void Update()
        {
            TrackedHand[] frameHands = null;
            double frameTime = 0;
            lock (resultLock)
            {
                if (latestSequence != processedSequence)
                {
                    frameHands = latestHands;
                    frameTime = latestResultTime;
                    processedSequence = latestSequence;
                }
            }

            var now = NowSeconds();
            if (frameHands != null)
            {
                if (frameHands.Length > 0)
                {
                    lastNonEmptyHandsTime = frameTime;
                    SetTracking(true);
                }

                recognizer.ProcessFrame(frameHands, frameTime);
            }

            if (tracking && now - lastNonEmptyHandsTime >= settings.trackingLossResetSeconds)
            {
                recognizer.Reset();
                SetTracking(false);
            }
            else if (!warnedNoResults && runner != null && now - bridgeEnabledTime > settings.noResultWarningSeconds && latestSequence == 0)
            {
                warnedNoResults = true;
                Debug.LogWarning("No Hand Landmarker results received. Start the sample runner and check its camera/model setup.", this);
            }
        }

        private void OnResultReceived(HandLandmarkerResult result)
        {
            var count = result.handLandmarks == null ? 0 : result.handLandmarks.Count;
            var converted = new TrackedHand[count];
            var validCount = 0;
            for (var handIndex = 0; handIndex < count; handIndex++)
            {
                var source = result.handLandmarks[handIndex].landmarks;
                if (source == null || source.Count < 21)
                {
                    continue;
                }

                var points = new Vector2[source.Count];
                for (var pointIndex = 0; pointIndex < source.Count; pointIndex++)
                {
                    points[pointIndex] = new Vector2(source[pointIndex].x, source[pointIndex].y);
                }

                var handId = "hand-" + handIndex;
                if (result.handedness != null && handIndex < result.handedness.Count &&
                    result.handedness[handIndex].categories != null && result.handedness[handIndex].categories.Count > 0)
                {
                    var categoryName = result.handedness[handIndex].categories[0].categoryName;
                    if (!string.IsNullOrEmpty(categoryName))
                    {
                        handId = categoryName;
                    }
                }

                converted[validCount++] = new TrackedHand(handId, points);
            }

            if (validCount != converted.Length)
            {
                Array.Resize(ref converted, validCount);
            }

            lock (resultLock)
            {
                latestHands = converted;
                latestResultTime = NowSeconds();
                latestSequence++;
            }
        }

        private void ForwardGesture(GestureRecognitionEvent gesture)
        {
            if (logRecognizedGestures)
            {
                var direction = gesture.Gesture == SemanticGesture.Point ? " direction=" + gesture.PointDirection : string.Empty;
                Debug.Log("Gesture recognized: " + gesture.Gesture + direction, this);
            }
            GestureDetected?.Invoke(gesture);
        }

        private void SetTracking(bool isTracking)
        {
            if (tracking == isTracking)
            {
                return;
            }

            tracking = isTracking;
            TrackingChanged?.Invoke(tracking);
            if (logRecognizedGestures)
            {
                Debug.Log("Hand tracking " + (tracking ? "available" : "lost"), this);
            }
        }

        private static double NowSeconds()
        {
            return (double)Stopwatch.GetTimestamp() / Stopwatch.Frequency;
        }
    }
}
