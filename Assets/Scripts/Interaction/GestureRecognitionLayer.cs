using System;
using System.Collections.Generic;
using UnityEngine;

namespace AwakeningCelebrations.Interaction
{
    public enum SemanticGesture
    {
        Wave,
        Clap,
        Point,
        Fist,
        BothHandsRaised,
        OpenPalmHeld
    }

    public readonly struct GestureRecognitionEvent
    {
        public readonly SemanticGesture Gesture;
        public readonly string Handedness;
        // Normalized direction in the image passed to Hand Landmarker (x right, y down).
        // The runner may flip that image, so this is not necessarily physical camera space.
        public readonly Vector2 PointDirection;
        // Normalized index fingertip position in the transformed input image.
        public readonly Vector2 PointPosition;
        public readonly double TimestampSeconds;

        public GestureRecognitionEvent(SemanticGesture gesture, string handedness, Vector2 pointDirection,
            Vector2 pointPosition, double timestampSeconds)
        {
            Gesture = gesture;
            Handedness = handedness;
            PointDirection = pointDirection;
            PointPosition = pointPosition;
            TimestampSeconds = timestampSeconds;
        }
    }

    [Serializable]
    public sealed class GestureRecognitionSettings
    {
        [Min(0f)] public float poseDwellSeconds = 0.18f;
        [Min(0f)] public float poseReleaseDwellSeconds = 0.15f;
        [Min(0f)] public float openPalmHoldSeconds = 1.0f;
        [Min(0f)] public float eventCooldownSeconds = 0.8f;
        [Min(0f)] public float trackingLossResetSeconds = 0.35f;
        [Min(0f)] public float noResultWarningSeconds = 5.0f;
        [Min(0f)] public float openPalmMaxDrift = 0.06f;

        [Min(1f)] public float fingerExtendedDistanceRatio = 1.12f;
        [Range(0f, 1f)] public float fingerCurledDistanceRatio = 0.96f;
        [Min(0f)] public float thumbExtensionPalmWidths = 0.65f;
        [Min(0f)] public float fistThumbMaximumPalmWidths = 1.15f;
        [Min(1f)] public float pointIndexExtensionRatio = 1.12f;
        [Min(0f)] public float pointOtherFingerMaximumRatio = 1.04f;
        [Range(1, 4)] public int openPalmMinimumExtendedFingers = 3;
        [Range(1, 4)] public int fistMinimumCurledFingers = 3;

        [Min(0f)] public float waveWindowSeconds = 1.0f;
        [Min(0f)] public float waveMinimumHorizontalTravel = 0.14f;
        [Min(1)] public int waveDirectionReversals = 2;
        [Min(0f)] public float waveMinimumStep = 0.025f;

        [Min(0f)] public float clapArmDistancePalmWidths = 1.2f;
        [Min(0f)] public float clapContactDistancePalmWidths = 0.65f;
        [Min(0f)] public float clapMinimumClosingPalmWidths = 0.2f;

        // Hand Landmarker has no shoulder/body landmarks. These frame-space rules
        // are provisional until a body-pose source and the approved definition exist.
        [Range(0f, 1f)] public float raisedWristMaximumY = 0.42f;
    }

    public readonly struct TrackedHand
    {
        public readonly string Id;
        public readonly Vector2[] Landmarks;

        public TrackedHand(string id, Vector2[] landmarks)
        {
            Id = id ?? string.Empty;
            Landmarks = landmarks;
        }
    }

    /// <summary>
    /// Converts copied hand landmarks into debounced semantic events. It has no
    /// MediaPipe, UI Toolkit, scene, or navigation dependency.
    /// </summary>
    public sealed class GestureRecognitionLayer
    {
        private const int LandmarkCount = 21;
        private const int Wrist = 0;
        private const int ThumbTip = 4;
        private const int IndexMcp = 5;
        private const int IndexPip = 6;
        private const int IndexTip = 8;
        private const int MiddleMcp = 9;
        private const int MiddleTip = 12;
        private const int RingMcp = 13;
        private const int RingTip = 16;
        private const int PinkyMcp = 17;
        private const int PinkyTip = 20;
        private static readonly int[] FingerTips = { IndexTip, MiddleTip, RingTip, PinkyTip };
        private static readonly int[] FingerPips = { IndexPip, 10, 14, 18 };

        private readonly GestureRecognitionSettings settings;
        private readonly Dictionary<string, PoseState> poseStates = new Dictionary<string, PoseState>();
        private readonly Dictionary<SemanticGesture, double> lastEmitted = new Dictionary<SemanticGesture, double>();
        private readonly Dictionary<string, WaveHistory> waveHistories = new Dictionary<string, WaveHistory>();
        private readonly List<HandPose> analyzedHands = new List<HandPose>(2);
        private readonly HashSet<string> presentHandIds = new HashSet<string>();
        private bool clapArmed;
        private double lastClapDistance;
        private bool clapHasPreviousDistance;
        private bool clapLatched;
        private bool hasValidHands;
        private double lastValidHandsTime;
        private string pointingHandId;
        private double pointingStarted;
        private bool pointing;

        public event Action<GestureRecognitionEvent> GestureDetected;
        public event Action<bool, Vector2> PointingChanged;

        public GestureRecognitionLayer(GestureRecognitionSettings settings)
        {
            this.settings = settings ?? new GestureRecognitionSettings();
        }

        public void ProcessFrame(IReadOnlyList<TrackedHand> hands, double timestampSeconds)
        {
            if (hands == null || hands.Count == 0)
            {
                pointingHandId = null;
                pointingStarted = 0;
                SetPointing(false, Vector2.zero);
                if (hasValidHands && timestampSeconds - lastValidHandsTime >= settings.trackingLossResetSeconds)
                {
                    ResetTrackingState();
                }
                return;
            }

            if (hasValidHands && timestampSeconds - lastValidHandsTime >= settings.trackingLossResetSeconds)
            {
                ResetTrackingState();
            }
            hasValidHands = true;
            lastValidHandsTime = timestampSeconds;

            analyzedHands.Clear();
            for (var i = 0; i < hands.Count; i++)
            {
                var hand = hands[i];
                if (hand.Landmarks == null || hand.Landmarks.Length < LandmarkCount)
                {
                    continue;
                }

                var pose = Analyze(hand);
                analyzedHands.Add(pose);
                ProcessSingleHandGestures(pose, timestampSeconds);
            }

            ProcessClap(analyzedHands, timestampSeconds);
            ProcessBothHandsRaised(analyzedHands, timestampSeconds);
            RemoveMissingHands(analyzedHands, timestampSeconds);
            UpdatePointing(analyzedHands, timestampSeconds);
        }

        public void Reset()
        {
            poseStates.Clear();
            waveHistories.Clear();
            clapArmed = false;
            clapHasPreviousDistance = false;
            clapLatched = false;
            hasValidHands = false;
            pointingHandId = null;
            pointingStarted = 0;
            SetPointing(false, Vector2.zero);
        }

        private HandPose Analyze(TrackedHand hand)
        {
            var p = hand.Landmarks;
            var palmCenter = (p[Wrist] + p[IndexMcp] + p[MiddleMcp] + p[RingMcp] + p[PinkyMcp]) / 5f;
            var palmWidth = Mathf.Max(Vector2.Distance(p[IndexMcp], p[PinkyMcp]), 0.001f);
            var extended = 0;
            var curled = 0;
            for (var i = 0; i < FingerTips.Length; i++)
            {
                var tipDistance = Vector2.Distance(p[FingerTips[i]], palmCenter);
                var pipDistance = Vector2.Distance(p[FingerPips[i]], palmCenter);
                if (tipDistance > pipDistance * settings.fingerExtendedDistanceRatio)
                {
                    extended++;
                }
                else if (tipDistance < pipDistance * settings.fingerCurledDistanceRatio)
                {
                    curled++;
                }
            }

            var thumbOpen = Vector2.Distance(p[ThumbTip], p[IndexMcp]) > palmWidth * settings.thumbExtensionPalmWidths;
            var openPalm = extended >= settings.openPalmMinimumExtendedFingers && thumbOpen;
            var fist = curled >= settings.fistMinimumCurledFingers && extended == 0 &&
                       Vector2.Distance(p[ThumbTip], palmCenter) < palmWidth * settings.fistThumbMaximumPalmWidths;
            var point = extended >= 1 && p[IndexTip] != Vector2.zero &&
                        Vector2.Distance(p[IndexTip], palmCenter) > Vector2.Distance(p[IndexPip], palmCenter) * settings.pointIndexExtensionRatio &&
                        IsFingerCurled(p, 12, 10, palmCenter) && IsFingerCurled(p, 16, 14, palmCenter) && IsFingerCurled(p, 20, 18, palmCenter);
            var direction = p[IndexTip] - p[Wrist];
            if (direction.sqrMagnitude > 0.0001f)
            {
                direction.Normalize();
            }

            return new HandPose(hand.Id, p, palmCenter, palmWidth, openPalm, fist, point, direction,
                p[IndexTip], p[Wrist].y <= settings.raisedWristMaximumY);
        }

        private bool IsFingerCurled(Vector2[] points, int tip, int pip, Vector2 palmCenter)
        {
            return Vector2.Distance(points[tip], palmCenter) < Vector2.Distance(points[pip], palmCenter) * settings.pointOtherFingerMaximumRatio;
        }

        private void ProcessSingleHandGestures(HandPose hand, double now)
        {
            ProcessPose(hand.Id, "point", hand.Point, SemanticGesture.Point, now, settings.poseDwellSeconds,
                hand.PointDirection, hand.Points[Wrist], targetPosition: hand.PointPosition);
            ProcessPose(hand.Id, "fist", hand.Fist, SemanticGesture.Fist, now, settings.poseDwellSeconds,
                Vector2.zero, hand.Points[Wrist]);
            ProcessPose(hand.Id, "open-palm-held", hand.OpenPalm, SemanticGesture.OpenPalmHeld, now,
                settings.openPalmHoldSeconds, Vector2.zero, hand.Points[Wrist], settings.openPalmMaxDrift);
            ProcessWave(hand, now);
        }

        private void ProcessPose(string identity, string poseKey, bool active, SemanticGesture gesture,
            double now, float dwell, Vector2 direction, Vector2 anchor,
            float maximumDrift = float.PositiveInfinity, Vector2 targetPosition = default)
        {
            var key = identity + ":" + poseKey;
            if (!active)
            {
                if (poseStates.TryGetValue(key, out var inactiveState) && !inactiveState.HasInactiveTime)
                {
                    inactiveState.InactiveSince = now;
                    inactiveState.HasInactiveTime = true;
                }
                return;
            }

            if (!poseStates.TryGetValue(key, out var state))
            {
                poseStates[key] = new PoseState(now, anchor);
                return;
            }

            if (state.HasInactiveTime)
            {
                if (now - state.InactiveSince > settings.poseReleaseDwellSeconds)
                {
                    state = new PoseState(now, anchor);
                    poseStates[key] = state;
                }
                else if (!state.Emitted)
                {
                    // A pose that has not fired must be continuously held for its
                    // dwell interval; a brief classifier dropout restarts that dwell.
                    state.Started = now;
                    state.Anchor = anchor;
                }
                state.HasInactiveTime = false;
            }

            if (!state.Emitted && maximumDrift < float.PositiveInfinity && Vector2.Distance(anchor, state.Anchor) > maximumDrift)
            {
                state = new PoseState(now, anchor);
                poseStates[key] = state;
            }

            if (state.Emitted || now - state.Started < dwell || !CooldownElapsed(gesture, now))
            {
                return;
            }

            state.Emitted = true;
            Emit(gesture, identity == "global" ? string.Empty : identity, direction, targetPosition, now);
        }

        private void ProcessWave(HandPose hand, double now)
        {
            if (!hand.OpenPalm)
            {
                waveHistories.Remove(hand.Id);
                return;
            }

            if (!waveHistories.TryGetValue(hand.Id, out var history))
            {
                history = new WaveHistory();
                waveHistories.Add(hand.Id, history);
            }

            history.Add(hand.Points[Wrist].x, now, settings.waveWindowSeconds, settings.waveMinimumStep);
            if (history.Reversals >= settings.waveDirectionReversals &&
                history.HorizontalRange >= settings.waveMinimumHorizontalTravel &&
                CooldownElapsed(SemanticGesture.Wave, now))
            {
                Emit(SemanticGesture.Wave, hand.Id, Vector2.zero, Vector2.zero, now);
                history.Clear(now, hand.Points[Wrist].x);
            }
        }

        private void ProcessClap(List<HandPose> hands, double now)
        {
            if (hands.Count < 2)
            {
                clapArmed = false;
                clapHasPreviousDistance = false;
                clapLatched = false;
                return;
            }

            var averagePalmWidth = (hands[0].PalmWidth + hands[1].PalmWidth) * 0.5f;
            var distanceInPalmWidths = Vector2.Distance(hands[0].PalmCenter, hands[1].PalmCenter) / averagePalmWidth;
            if (distanceInPalmWidths >= settings.clapArmDistancePalmWidths)
            {
                clapArmed = true;
                clapLatched = false;
            }

            var closedFromPrevious = clapHasPreviousDistance &&
                                     lastClapDistance - distanceInPalmWidths >= settings.clapMinimumClosingPalmWidths;
            if (!clapLatched && clapArmed && closedFromPrevious &&
                distanceInPalmWidths <= settings.clapContactDistancePalmWidths &&
                CooldownElapsed(SemanticGesture.Clap, now))
            {
                Emit(SemanticGesture.Clap, string.Empty, Vector2.zero, Vector2.zero, now);
                clapLatched = true;
                clapArmed = false;
            }

            lastClapDistance = distanceInPalmWidths;
            clapHasPreviousDistance = true;
        }

        private void ProcessBothHandsRaised(List<HandPose> hands, double now)
        {
            var raisedCount = 0;
            for (var i = 0; i < hands.Count; i++)
            {
                if (hands[i].Raised)
                {
                    raisedCount++;
                }
            }

            ProcessPose("global", "both-hands-raised", raisedCount >= 2 && !clapLatched,
                SemanticGesture.BothHandsRaised, now, settings.poseDwellSeconds, Vector2.zero, Vector2.zero);
        }

        private void UpdatePointing(List<HandPose> hands, double now)
        {
            for (var i = 0; i < hands.Count; i++)
            {
                var hand = hands[i];
                if (!hand.Point)
                {
                    continue;
                }

                if (pointingHandId != hand.Id)
                {
                    pointingHandId = hand.Id;
                    pointingStarted = now;
                }

                SetPointing(now - pointingStarted >= settings.poseDwellSeconds, hand.PointPosition);
                return;
            }

            pointingHandId = null;
            pointingStarted = 0;
            SetPointing(false, Vector2.zero);
        }

        private void SetPointing(bool isPointing, Vector2 position)
        {
            if (pointing == isPointing && !isPointing)
            {
                return;
            }

            pointing = isPointing;
            PointingChanged?.Invoke(isPointing, position);
        }

        private void RemoveMissingHands(List<HandPose> hands, double now)
        {
            presentHandIds.Clear();
            for (var i = 0; i < hands.Count; i++)
            {
                presentHandIds.Add(hands[i].Id);
            }

            foreach (var key in poseStates.Keys)
            {
                if (key.StartsWith("global:", StringComparison.Ordinal) || presentHandIds.Contains(HandIdFromPoseKey(key)))
                {
                    continue;
                }
                if (poseStates.TryGetValue(key, out var missingState) && !missingState.HasInactiveTime)
                {
                    missingState.InactiveSince = now;
                    missingState.HasInactiveTime = true;
                }
            }

        }

        private static string HandIdFromPoseKey(string key)
        {
            var separator = key.LastIndexOf(':');
            return separator < 0 ? key : key.Substring(0, separator);
        }

        private bool CooldownElapsed(SemanticGesture gesture, double now)
        {
            return !lastEmitted.TryGetValue(gesture, out var last) || now - last >= settings.eventCooldownSeconds;
        }

        private void Emit(SemanticGesture gesture, string handId, Vector2 direction, Vector2 position, double now)
        {
            lastEmitted[gesture] = now;
            GestureDetected?.Invoke(new GestureRecognitionEvent(gesture, handId, direction, position, now));
        }

        private void ResetTrackingState()
        {
            poseStates.Clear();
            waveHistories.Clear();
            clapArmed = false;
            clapHasPreviousDistance = false;
            clapLatched = false;
            hasValidHands = false;
            pointingHandId = null;
            pointingStarted = 0;
            SetPointing(false, Vector2.zero);
        }

        private readonly struct HandPose
        {
            public readonly string Id;
            public readonly Vector2[] Points;
            public readonly Vector2 PalmCenter;
            public readonly float PalmWidth;
            public readonly bool OpenPalm;
            public readonly bool Fist;
            public readonly bool Point;
            public readonly Vector2 PointDirection;
            public readonly Vector2 PointPosition;
            public readonly bool Raised;

            public HandPose(string id, Vector2[] points, Vector2 palmCenter, float palmWidth,
                bool openPalm, bool fist, bool point, Vector2 pointDirection, Vector2 pointPosition, bool raised)
            {
                Id = id;
                Points = points;
                PalmCenter = palmCenter;
                PalmWidth = palmWidth;
                OpenPalm = openPalm;
                Fist = fist;
                Point = point;
                PointDirection = pointDirection;
                PointPosition = pointPosition;
                Raised = raised;
            }
        }

        private sealed class PoseState
        {
            public double Started;
            public Vector2 Anchor;
            public bool Emitted;
            public bool HasInactiveTime;
            public double InactiveSince;

            public PoseState(double started, Vector2 anchor)
            {
                Started = started;
                Anchor = anchor;
            }
        }

        private sealed class WaveHistory
        {
            private readonly List<Sample> samples = new List<Sample>();
            private int lastDirection;

            public int Reversals { get; private set; }
            public float HorizontalRange { get; private set; }

            public void Add(float x, double time, float window, float minimumStep)
            {
                samples.Add(new Sample(x, time));
                while (samples.Count > 0 && time - samples[0].Time > window)
                {
                    samples.RemoveAt(0);
                }

                if (samples.Count >= 2)
                {
                    var delta = samples[samples.Count - 1].X - samples[samples.Count - 2].X;
                    var direction = Mathf.Abs(delta) >= minimumStep ? Math.Sign(delta) : 0;
                    if (direction != 0)
                    {
                        if (lastDirection != 0 && direction != lastDirection)
                        {
                            Reversals++;
                        }
                        lastDirection = direction;
                    }
                }

                var min = float.MaxValue;
                var max = float.MinValue;
                for (var i = 0; i < samples.Count; i++)
                {
                    min = Mathf.Min(min, samples[i].X);
                    max = Mathf.Max(max, samples[i].X);
                }
                HorizontalRange = samples.Count > 1 ? max - min : 0f;
            }

            public void Clear(double time, float x)
            {
                samples.Clear();
                samples.Add(new Sample(x, time));
                lastDirection = 0;
                Reversals = 0;
                HorizontalRange = 0f;
            }

            private readonly struct Sample
            {
                public readonly float X;
                public readonly double Time;
                public Sample(float x, double time) { X = x; Time = time; }
            }
        }
    }
}
