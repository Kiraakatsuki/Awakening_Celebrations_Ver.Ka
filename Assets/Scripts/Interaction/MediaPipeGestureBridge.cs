using System;
using Stopwatch = System.Diagnostics.Stopwatch;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Unity.Sample;
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
        [SerializeField, Range(0f, 1f)] private float minimumHandednessConfidence = 0.55f;

        private readonly object resultLock = new object();
        private GestureRecognitionLayer recognizer;
        private HandLandmarkerRunner runner;
        private TrackedHand[] latestHands = Array.Empty<TrackedHand>();
        private long latestSequence;
        private long processedSequence;
        private double latestResultTime;
        private double lastNonEmptyHandsTime;
        private bool tracking;
        private bool warnedNoResults;
        private int lastDiagnosticHandCount = -1;
        private double nextDiagnosticTime;
        private double lastProcessedResultTime;
        private double lastFrameIntervalMilliseconds;
        private double lastResultQueueDelayMilliseconds;
        private double lastRecognitionProcessingMilliseconds;
        private double pipelineStartedTime;
        private long droppedResultCount;
        private int trackingInterruptionCount;
        private bool reportedPipelineConfiguration;
        private bool reportedFirstResult;
        private bool reportedTwoHands;
        private bool wasConsentGranted;

        public int TrackedHandCount { get; private set; }
        public bool IsCameraReady => runner != null && runner.IsImageSourcePrepared;
        public bool HasReceivedFrames { get; private set; }
        public bool HasCameraFailure => runner != null && !string.IsNullOrEmpty(runner.LastPipelineFailureType);
        public Texture CameraPreviewTexture => IsCameraReady ? runner.CameraPreviewTexture : null;

        public event Action<GestureRecognitionEvent> GestureDetected;
        public event Action<bool> TrackingChanged;
        public event Action<bool, Vector2> PointingChanged;

        public void ResetVideoGesture()
        {
            recognizer?.ResetVideoGesture();
        }

        private void OnEnable()
        {
            recognizer = new GestureRecognitionLayer(settings);
            recognizer.GestureDetected += ForwardGesture;
            recognizer.PointingChanged += ForwardPointing;
            recognizer.DiagnosticMessage += ForwardDiagnostic;
            runner = GetComponent<HandLandmarkerRunner>();
            if (runner == null)
            {
                Debug.LogWarning("MediaPipeGestureBridge requires a HandLandmarkerRunner on the same GameObject.", this);
                return;
            }

            runner.ResultReceived += OnResultReceived;
            pipelineStartedTime = 0;
            warnedNoResults = false;
            reportedFirstResult = false;
            reportedTwoHands = false;
        }

        private void OnDisable()
        {
            if (runner != null)
            {
                runner.ResultReceived -= OnResultReceived;
            }
            SetTracking(false);
            TrackedHandCount = 0;
            HasReceivedFrames = false;
            recognizer?.Reset();
            PointingChanged?.Invoke(false, Vector2.zero);
        }

        private void Update()
        {
            if (!GestureCameraConsentGate.ConsentGranted)
            {
                if (wasConsentGranted)
                {
                    wasConsentGranted = false;
                    lock (resultLock)
                        processedSequence = latestSequence;
                    recognizer.Reset();
                    SetTracking(false);
                    TrackedHandCount = 0;
                    HasReceivedFrames = false;
                    lastNonEmptyHandsTime = 0;
                    pipelineStartedTime = 0;
                    reportedFirstResult = false;
                    reportedTwoHands = false;
                }
                return;
            }
            wasConsentGranted = true;

            TrackedHand[] frameHands = null;
            double frameTime = 0;
            long sequence = 0;
            long skippedResults = 0;
            lock (resultLock)
            {
                if (latestSequence != processedSequence)
                {
                    frameHands = latestHands;
                    frameTime = latestResultTime;
                    sequence = latestSequence;
                    skippedResults = Math.Max(0, sequence - processedSequence - 1);
                    processedSequence = sequence;
                }
            }

            var now = NowSeconds();
            if (runner == null || !runner.IsPipelineStarted || !runner.IsImageSourcePrepared)
            {
                pipelineStartedTime = 0;
                warnedNoResults = false;
            }
            else if (pipelineStartedTime <= 0)
            {
                pipelineStartedTime = now;
            }

            if (frameHands != null)
            {
                HasReceivedFrames = true;
                droppedResultCount += skippedResults;
                if (lastProcessedResultTime > 0)
                    lastFrameIntervalMilliseconds = Math.Max(0, frameTime - lastProcessedResultTime) * 1000.0;
                lastProcessedResultTime = frameTime;
                double processStarted = NowSeconds();
                // Keep the cursor and gesture histories across brief detector dropouts.
                // Longer gaps still cancel holds and release the cursor.
                if (frameHands.Length > 0 || frameTime - lastNonEmptyHandsTime >= 0.16)
                {
                    recognizer.ProcessFrame(frameHands, frameTime);
                    if (recognizer.HasUsableTracking)
                    {
                        TrackedHandCount = recognizer.UsableHandCount;
                        lastNonEmptyHandsTime = frameTime;
                        if (TrackedHandCount == 2 && !reportedTwoHands)
                        {
                            reportedTwoHands = true;
                            Debug.Log("Hand tracker detected two hands. Clap recognition is available.", this);
                        }
                    }
                }
                double processedAt = NowSeconds();
                if (recognizer.HasUsableTracking)
                    SetTracking(true);
                lastResultQueueDelayMilliseconds = Math.Max(0, processStarted - frameTime) * 1000.0;
                lastRecognitionProcessingMilliseconds = Math.Max(0, processedAt - processStarted) * 1000.0;
                ReportFrameDiagnostics(frameHands.Length, processedAt);
            }

            if (tracking && now - lastNonEmptyHandsTime >= settings.trackingLossResetSeconds)
            {
                recognizer.Reset();
                TrackedHandCount = 0;
                SetTracking(false);
            }
            else if (!warnedNoResults && runner != null && runner.IsPipelineStarted && runner.IsImageSourcePrepared &&
                     pipelineStartedTime > 0 && now - pipelineStartedTime > settings.noResultWarningSeconds && !HasReceivedFrames)
            {
                warnedNoResults = true;
                Debug.LogWarning("No Hand Landmarker results received after the camera became ready. Check inference callbacks and model/runtime logs.", this);
            }
        }

        private void OnResultReceived(HandLandmarkerResult result)
        {
            var count = result.handLandmarks == null ? 0 : result.handLandmarks.Count;
            if (!reportedFirstResult)
            {
                reportedFirstResult = true;
                Debug.Log($"MediaPipe first hand-landmark result received: {count} hand(s).", this);
            }

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
                    var handednessCategory = result.handedness[handIndex].categories[0];
                    var categoryName = handednessCategory.categoryName;
                    if (!string.IsNullOrEmpty(categoryName) && handednessCategory.score >= minimumHandednessConfidence)
                    {
                        handId = categoryName;
                    }
                }

                for (var assigned = 0; assigned < validCount; assigned++)
                {
                    if (converted[assigned].Id == handId)
                    {
                        handId = "hand-" + handIndex;
                        break;
                    }
                }

                Vector3[] worldPoints = null;
                if (result.handWorldLandmarks != null && handIndex < result.handWorldLandmarks.Count)
                {
                    var world = result.handWorldLandmarks[handIndex].landmarks;
                    if (world != null && world.Count >= 21)
                    {
                        worldPoints = new Vector3[21];
                        for (var i = 0; i < worldPoints.Length; i++)
                            worldPoints[i] = new Vector3(world[i].x, world[i].y, world[i].z);
                    }
                }

                converted[validCount++] = new TrackedHand(handId, points, worldPoints);
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
            if (logRecognizedGestures || settings.diagnosticsEnabled)
            {
                var prefix = settings.diagnosticsEnabled ? "[GestureDiag] event emitted=" : "Gesture recognized: ";
                var detail = gesture.Gesture == SemanticGesture.Point
                    ? " direction=" + gesture.PointDirection + " fingertip=" + gesture.PointPosition
                    : string.Empty;
                Debug.Log(prefix + gesture.Gesture + " hand=" + gesture.Handedness + detail +
                    " timestamp=" + gesture.TimestampSeconds.ToString("0.000") + ".", this);
            }
            GestureDetected?.Invoke(gesture);
        }

        private void ForwardPointing(bool isPointing, Vector2 normalizedPosition)
        {
            PointingChanged?.Invoke(isPointing, normalizedPosition);
        }

        private void ForwardDiagnostic(string message)
        {
            if (settings.diagnosticsEnabled)
                Debug.Log("[GestureDiag] " + message, this);
        }

        private void ReportFrameDiagnostics(int handCount, double now)
        {
            if (!settings.diagnosticsEnabled)
                return;

            if (!reportedPipelineConfiguration && runner != null)
            {
                reportedPipelineConfiguration = true;
                var source = ImageSourceProvider.ImageSource;
                string sourceDetails = source == null ? "image source not prepared" :
                    source.resolution.width + "x" + source.resolution.height + " @ " + source.frameRate.ToString("0.0") + " fps";
                Debug.Log("[GestureDiag] MediaPipe mode=" + runner.config.RunningMode +
                    ", delegate=" + runner.config.Delegate + ", read=" + runner.config.ImageReadMode +
                    ", hands=" + runner.config.NumHands + ", detection/presence/tracking confidence=" +
                    runner.config.MinHandDetectionConfidence.ToString("0.00") + "/" +
                    runner.config.MinHandPresenceConfidence.ToString("0.00") + "/" +
                    runner.config.MinTrackingConfidence.ToString("0.00") + ", source=" + sourceDetails + ".", this);
            }

            if (handCount != lastDiagnosticHandCount)
            {
                lastDiagnosticHandCount = handCount;
                Debug.Log("[GestureDiag] tracked hand count=" + handCount + ", tracking=" + tracking + ".", this);
            }

            if (now < nextDiagnosticTime)
                return;
            nextDiagnosticTime = now + Math.Max(0.1f, settings.diagnosticsIntervalSeconds);
            Debug.Log("[GestureDiag] interval=" + lastFrameIntervalMilliseconds.ToString("0.0") +
                "ms, result-to-recognition=" + lastResultQueueDelayMilliseconds.ToString("0.0") +
                "ms, recognition cost=" + lastRecognitionProcessingMilliseconds.ToString("0.00") +
                "ms, skipped results=" + droppedResultCount + ", tracking interruptions=" +
                trackingInterruptionCount + ", failed texture readbacks=" +
                (runner == null ? 0 : runner.FailedTextureReadbackCount) + ", texture-pool skips=" +
                (runner == null ? 0 : runner.SkippedTexturePoolFrameCount) + ".", this);
        }

        private void SetTracking(bool isTracking)
        {
            if (tracking == isTracking)
            {
                return;
            }

            tracking = isTracking;
            if (!isTracking)
                trackingInterruptionCount++;
            TrackingChanged?.Invoke(tracking);
            if (settings.diagnosticsEnabled)
                Debug.Log("[GestureDiag] tracking=" + tracking + ", interruptions=" + trackingInterruptionCount + ".", this);
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
