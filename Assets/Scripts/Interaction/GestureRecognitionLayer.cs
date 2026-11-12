using System;
using System.Collections.Generic;
using System.Text;
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
        [Min(0f)] public float poseDwellSeconds = 0.14f;
        [Min(0f)] public float poseReleaseDwellSeconds = 0.15f;
        [Min(0f)] public float openPalmHoldSeconds = 2.0f;
        [Min(0f)] public float eventCooldownSeconds = 0.8f;
        [Min(0f)] public float trackingLossResetSeconds = 0.35f;
        [Min(0f)] public float noResultWarningSeconds = 5.0f;
        public bool diagnosticsEnabled;
        [Min(0.1f)] public float diagnosticsIntervalSeconds = 1.0f;
        [Min(0f)] public float openPalmMaxDrift = 0.06f;
        [Range(0f, 0.9f)] public float landmarkSmoothing = 0.35f;
        [Range(0f, 0.25f)] public float normalizedLandmarkMargin = 0.12f;
        [Min(2)] public int minimumPoseObservations = 3;
        [Min(0.02f)] public float maximumPoseObservationGapSeconds = 0.25f;

        [Min(1f)] public float fingerExtendedDistanceRatio = 1.12f;
        [Range(0f, 1f)] public float fingerCurledDistanceRatio = 0.96f;
        [Min(0f)] public float thumbExtensionPalmWidths = 0.65f;
        [Min(0f)] public float pointOtherFingerMaximumRatio = 1.04f;
        [Range(1, 4)] public int openPalmMinimumExtendedFingers = 3;
        [Range(1, 4)] public int fistMinimumCurledFingers = 3;

        [Min(0f)] public float waveWindowSeconds = 1.8f;
        [Min(0f)] public float waveMinimumHorizontalTravel = 0.58f;
        [Min(1)] public int waveDirectionReversals = 1;
        [Min(0f)] public float waveMinimumStep = 0.14f;
        [Min(3)] public int waveMinimumSamples = 4;
        [Min(0.01f)] public float waveMaximumObservationGapSeconds = 0.3f;
        [Min(0f)] public float waveMaximumVerticalDriftPalmWidths = 1.5f;
        [Min(0f)] public float waveQuickSweepPalmWidths = 1f;
        [Min(0f)] public float waveQuickSweepSeconds = 0.32f;

        [Min(0f)] public float clapArmDistancePalmWidths = 1.8f;
        [Min(0f)] public float clapContactDistancePalmWidths = 1.65f;
        [Min(0f)] public float clapMinimumClosingPalmWidths = 0.38f;
        [Min(0f)] public float clapMinimumFrameClosingPalmWidths = 0.015f;
        [Min(1)] public int clapMinimumClosingObservations = 1;
        [Min(0f)] public float clapSeparationDwellSeconds = 0f;
        [Min(0f)] public float clapClosingWindowSeconds = 0.8f;
        [Min(0f)] public float clapOpeningResetPalmWidths = 0.16f;
        [Min(0f)] public float clapCooldownSeconds = 0.35f;
        [Min(0f)] public float clapPairGraceSeconds = 0.26f;

        [Min(0f)] public float bothHandsRaisedDwellSeconds = 0.32f;
    }

    public readonly struct TrackedHand
    {
        public readonly string Id;
        public readonly Vector2[] Landmarks;

        public readonly Vector3[] WorldLandmarks;

        public TrackedHand(string id, Vector2[] landmarks, Vector3[] worldLandmarks = null)
        {
            Id = id ?? string.Empty;
            Landmarks = landmarks;
            WorldLandmarks = worldLandmarks;
        }
    }

    /// <summary>
    /// Converts copied hand landmarks into debounced semantic events. It has no
    /// MediaPipe, UI Toolkit, scene, or navigation dependency.
    /// </summary>
    public sealed class GestureRecognitionLayer
    {
        private const int LandmarkCount = 21;
        private const string VideoPoseKey = "both-hands-raised";
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
        private readonly Dictionary<string, LandmarkFilterState> landmarkFilters = new Dictionary<string, LandmarkFilterState>();
        private readonly Dictionary<SemanticGesture, double> lastEmitted = new Dictionary<SemanticGesture, double>();
        private readonly Dictionary<string, WaveHistory> waveHistories = new Dictionary<string, WaveHistory>();
        private readonly Dictionary<string, bool> diagnosticCandidateStates = new Dictionary<string, bool>();
        private readonly Dictionary<string, double> lastCandidateDiagnosticTimes = new Dictionary<string, double>();
        private readonly List<HandPose> analyzedHands = new List<HandPose>(2);
        private readonly HashSet<string> presentHandIds = new HashSet<string>();
        private readonly List<string> staleFilterIds = new List<string>(2);
        private readonly List<string> staleWaveIds = new List<string>();
        private bool clapArmed;
        private float lastClapScreenDistance;
        private float lastClapScale;
        private Vector2 lastClapMidpoint;
        private double lastClapPairTime;
        private bool clapHasPreviousDistance;
        private bool clapLatched;
        private double clapSeparatedSince = double.NaN;
        private double clapStartedAt;
        private double clapClosingDistance;
        private int clapClosingObservations;
        private bool hasValidHands;
        private double lastValidHandsTime;
        private string pointingHandId;
        private double pointingStarted;
        private bool pointing;
        private double lastVideoPoseTime = double.NegativeInfinity;
        public bool HasUsableTracking { get; private set; }
        public int UsableHandCount => HasUsableTracking ? analyzedHands.Count : 0;

        public event Action<GestureRecognitionEvent> GestureDetected;
        public event Action<bool, Vector2> PointingChanged;
        public event Action<string> DiagnosticMessage;

        public GestureRecognitionLayer(GestureRecognitionSettings settings)
        {
            this.settings = settings ?? new GestureRecognitionSettings();
        }

        public void ProcessFrame(IReadOnlyList<TrackedHand> hands, double timestampSeconds)
        {
            analyzedHands.Clear();
            if (hasValidHands && timestampSeconds - lastValidHandsTime >= settings.trackingLossResetSeconds)
                ResetTrackingState();

            if (hands == null || hands.Count == 0)
            {
                ProcessNoHands(timestampSeconds);
                return;
            }

            for (var i = 0; i < hands.Count; i++)
            {
                var hand = hands[i];
                if (string.IsNullOrEmpty(hand.Id) || !HasUsableLandmarks(hand.Landmarks))
                {
                    ReportCandidate(hand.Id ?? "unknown", "TrackingQuality", false, "invalid, out-of-frame, or degenerate landmarks");
                    continue;
                }

                var filteredLandmarks = FilterLandmarks(hand.Id, hand.Landmarks, timestampSeconds);
                var pose = Analyze(hand, filteredLandmarks);
                analyzedHands.Add(pose);
            }

            if (analyzedHands.Count == 0)
            {
                ProcessNoHands(timestampSeconds);
                return;
            }

            hasValidHands = true;
            HasUsableTracking = true;
            lastValidHandsTime = timestampSeconds;

            for (var i = 0; i < analyzedHands.Count; i++)
                ProcessSingleHandGestures(analyzedHands[i], analyzedHands.Count == 1, timestampSeconds);

            ProcessClap(analyzedHands, timestampSeconds);
            ProcessBothHandsRaised(analyzedHands, timestampSeconds);
            RemoveMissingHands(analyzedHands, timestampSeconds);
            UpdatePointing(analyzedHands, timestampSeconds);
        }

        private void ProcessNoHands(double now)
        {
            HasUsableTracking = false;
            pointingHandId = null;
            pointingStarted = 0;
            SetPointing(false, Vector2.zero);
            ProcessClap(analyzedHands, now);
            ProcessBothHandsRaised(analyzedHands, now);
            RemoveMissingHands(analyzedHands, now);
            if (hasValidHands && now - lastValidHandsTime >= settings.trackingLossResetSeconds)
                ResetTrackingState();
        }

        public void Reset()
        {
            poseStates.Clear();
            lastVideoPoseTime = double.NegativeInfinity;
            landmarkFilters.Clear();
            waveHistories.Clear();
            diagnosticCandidateStates.Clear();
            lastCandidateDiagnosticTimes.Clear();
            clapArmed = false;
            clapHasPreviousDistance = false;
            clapLatched = false;
            clapSeparatedSince = double.NaN;
            clapStartedAt = 0;
            clapClosingDistance = 0f;
            clapClosingObservations = 0;
            hasValidHands = false;
            HasUsableTracking = false;
            pointingHandId = null;
            pointingStarted = 0;
            SetPointing(false, Vector2.zero);
        }

        private HandPose Analyze(TrackedHand hand, Vector2[] p)
        {
            var palmCenter = GetPalmCenter(p);
            var palmWidth = Mathf.Max(Vector2.Distance(p[IndexMcp], p[PinkyMcp]),
                Vector2.Distance(p[Wrist], p[MiddleMcp]) * 0.7f, 0.001f);
            var extended = 0;
            var curled = 0;
            bool indexExtended = false;
            for (var i = 0; i < FingerTips.Length; i++)
            {
                var tipDistance = Vector2.Distance(p[FingerTips[i]], palmCenter);
                var pipDistance = Vector2.Distance(p[FingerPips[i]], palmCenter);
                bool fingerExtended = tipDistance > pipDistance * settings.fingerExtendedDistanceRatio;
                bool fingerCurled = tipDistance < pipDistance * settings.fingerCurledDistanceRatio;
                if (hand.WorldLandmarks != null)
                {
                    var world = hand.WorldLandmarks;
                    int pip = FingerPips[i];
                    Vector3 proximal = world[pip] - world[pip - 1];
                    Vector3 distal = world[FingerTips[i]] - world[pip];
                    if (proximal.sqrMagnitude > 0.000001f && distal.sqrMagnitude > 0.000001f)
                    {
                        float alignment = Vector3.Dot(proximal.normalized, distal.normalized);
                        fingerExtended = alignment >= 0.8f;
                        fingerCurled = alignment <= 0.55f;
                    }
                }

                if (i == 0)
                    indexExtended = fingerExtended;
                if (fingerExtended)
                {
                    extended++;
                }
                else if (fingerCurled)
                {
                    curled++;
                }
            }

            var thumbOpen = Vector2.Distance(p[ThumbTip], p[IndexMcp]) > palmWidth * settings.thumbExtensionPalmWidths;
            var openPalm = extended >= settings.openPalmMinimumExtendedFingers && (indexExtended || thumbOpen);
            var fist = curled >= settings.fistMinimumCurledFingers && !indexExtended && extended <= 1;
            var point = indexExtended && extended == 1 && (curled >= 2 ||
                (IsFingerCurled(p, 12, 10, palmCenter) && IsFingerCurled(p, 16, 14, palmCenter) && IsFingerCurled(p, 20, 18, palmCenter)));
            var direction = p[IndexTip] - p[IndexMcp];
            if (direction.sqrMagnitude > 0.0001f)
            {
                direction.Normalize();
            }

            // Motion needs the current frame. Keep filtering the pose and cursor independently.
            Vector2 motionPalmCenter = GetPalmCenter(hand.Landmarks);
            Vector2 wavePosition = (motionPalmCenter + hand.Landmarks[MiddleTip]) * 0.5f;
            return new HandPose(hand.Id, p, motionPalmCenter, wavePosition, palmWidth, openPalm, fist, point, direction,
                p[IndexTip], extended, curled);
        }

        private static Vector2 GetPalmCenter(Vector2[] points)
        {
            return (points[Wrist] + points[IndexMcp] + points[MiddleMcp] + points[RingMcp] + points[PinkyMcp]) / 5f;
        }

        private bool HasUsableLandmarks(Vector2[] points)
        {
            if (points == null || points.Length < LandmarkCount)
                return false;

            float margin = Mathf.Max(0f, settings.normalizedLandmarkMargin);
            for (var i = 0; i < LandmarkCount; i++)
            {
                Vector2 point = points[i];
                if (float.IsNaN(point.x) || float.IsNaN(point.y) || float.IsInfinity(point.x) || float.IsInfinity(point.y))
                    return false;
            }

            Vector2 wrist = points[Wrist];
            float palmSize = Mathf.Max(Vector2.Distance(points[IndexMcp], points[PinkyMcp]),
                Vector2.Distance(wrist, points[MiddleMcp]));
            return wrist.x >= -margin && wrist.x <= 1f + margin && wrist.y >= -margin && wrist.y <= 1f + margin &&
                palmSize >= 0.004f && palmSize <= 0.65f;
        }

        private Vector2[] FilterLandmarks(string identity, Vector2[] input, double now)
        {
            if (!landmarkFilters.TryGetValue(identity, out var filter) ||
                now - filter.LastObserved > settings.maximumPoseObservationGapSeconds ||
                filter.Points.Length != input.Length)
            {
                filter = new LandmarkFilterState(input.Length);
                Array.Copy(input, filter.Points, input.Length);
                landmarkFilters[identity] = filter;
            }
            else
            {
                float alpha = Mathf.Clamp01(1f - settings.landmarkSmoothing);
                for (var i = 0; i < LandmarkCount; i++)
                    filter.Points[i] = Vector2.Lerp(filter.Points[i], input[i], alpha);
            }

            filter.LastObserved = now;
            return filter.Points;
        }

        private bool IsFingerCurled(Vector2[] points, int tip, int pip, Vector2 palmCenter)
        {
            return Vector2.Distance(points[tip], palmCenter) < Vector2.Distance(points[pip], palmCenter) * settings.pointOtherFingerMaximumRatio;
        }

        private void ProcessSingleHandGestures(HandPose hand, bool singleHand, double now)
        {
            string geometry = settings.diagnosticsEnabled
                ? "extended=" + hand.ExtendedCount + "/4, curled=" + hand.CurledCount + "/4, palmWidth=" + hand.PalmWidth.ToString("0.000")
                : null;
            ReportCandidate(hand.Id, "Point", hand.Point, geometry);
            ReportCandidate(hand.Id, "Fist", hand.Fist, geometry);
            ReportCandidate(hand.Id, "OpenPalm", hand.OpenPalm, geometry);
            ProcessPose(hand.Id, "point", hand.Point, SemanticGesture.Point, now, settings.poseDwellSeconds,
                hand.PointDirection, hand.Points[Wrist], targetPosition: hand.PointPosition);
            ProcessPose(hand.Id, "fist", hand.Fist, SemanticGesture.Fist, now, settings.poseDwellSeconds,
                Vector2.zero, hand.Points[Wrist]);
            ProcessPose(hand.Id, "open-palm-held", singleHand && hand.OpenPalm, SemanticGesture.OpenPalmHeld, now,
                settings.openPalmHoldSeconds, Vector2.zero, hand.Points[Wrist], settings.openPalmMaxDrift);
            ProcessWave(hand, singleHand, now);
        }

        private void ProcessPose(string identity, string poseKey, bool active, SemanticGesture gesture,
            double now, float dwell, Vector2 direction, Vector2 anchor,
            float maximumDrift = float.PositiveInfinity, Vector2 targetPosition = default)
        {
            var key = identity + ":" + poseKey;
            if (!active)
            {
                ReportCandidate(identity, poseKey, false, "classifier no longer supports pose");
                if (poseStates.TryGetValue(key, out var inactiveState) && !inactiveState.HasInactiveTime)
                {
                    inactiveState.InactiveSince = now;
                    inactiveState.HasInactiveTime = true;
                    if (!inactiveState.Emitted)
                    {
                        ReportDiagnostic("Pose " + key + " ended before acceptance; release hysteresis started.");
                    }
                }
                return;
            }

            if (!poseStates.TryGetValue(key, out var state))
            {
                state = new PoseState(now, anchor);
                state.ConsecutiveObservations = 1;
                state.LastObserved = now;
                poseStates[key] = state;
                ReportDiagnostic("Pose " + key + " candidate started; requires " + dwell.ToString("0.00") + "s dwell.");
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

            if (now - state.LastObserved > settings.maximumPoseObservationGapSeconds)
            {
                state = new PoseState(now, anchor);
                state.ConsecutiveObservations = 1;
                poseStates[key] = state;
            }
            else
            {
                state.ConsecutiveObservations++;
                state.LastObserved = now;
            }

            if (!state.Emitted && maximumDrift < float.PositiveInfinity && Vector2.Distance(anchor, state.Anchor) > maximumDrift)
            {
                state = new PoseState(now, anchor);
                poseStates[key] = state;
            }

            if (state.Emitted || state.ConsecutiveObservations < settings.minimumPoseObservations ||
                now - state.Started < dwell || !CooldownElapsed(gesture, now))
            {
                if (!state.Emitted && now - state.Started < dwell && !state.DwellReported)
                {
                    state.DwellReported = true;
                    ReportDiagnostic("Pose " + key + " pending: dwell " + (now - state.Started).ToString("0.00") + "/" + dwell.ToString("0.00") + "s.");
                }
                else if (!state.Emitted && !CooldownElapsed(gesture, now) && !state.CooldownReported)
                {
                    state.CooldownReported = true;
                    ReportDiagnostic("Pose " + key + " held but rejected by " + gesture + " event cooldown.");
                }
                return;
            }

            state.Emitted = true;
            Emit(gesture, identity == "global" ? string.Empty : identity, direction, targetPosition, now);
            ReportDiagnostic("Pose " + key + " accepted after " + (now - state.Started).ToString("0.00") + "s.");
        }

        private void ProcessWave(HandPose hand, bool singleHand, double now)
        {
            waveHistories.TryGetValue(hand.Id, out var history);
            bool continuingOpenHand = history != null && !history.Emitted && hand.ExtendedCount >= 2 &&
                !hand.Fist && !hand.Point && history.SawOpenPalmRecently(now, settings.waveMaximumObservationGapSeconds);
            if (!hand.OpenPalm && !continuingOpenHand)
            {
                if (history != null && history.MarkOpenPalmInactive(now, settings.waveMaximumObservationGapSeconds))
                {
                    waveHistories.Remove(hand.Id);
                    ReportDiagnostic("Wave history released for " + hand.Id + " after stable open-palm release.");
                }
                return;
            }

            if (history == null)
            {
                history = new WaveHistory();
                waveHistories.Add(hand.Id, history);
            }

            history.MarkOpenPalmActive();
            if (hand.OpenPalm)
                history.LastOpenPalmTime = now;
            if (history.Emitted)
                return;

            bool resetForGap = history.Add(hand.WavePosition.x, hand.WavePosition.y, hand.PalmWidth, now,
                settings.waveWindowSeconds, settings.waveMaximumObservationGapSeconds,
                settings.waveMinimumStep);
            if (resetForGap)
                ReportDiagnostic("Wave history reset for " + hand.Id + " after an observation gap.");

            if (settings.diagnosticsEnabled)
                ReportWaveSummary(hand.Id, history, now);

            bool quickSweep = singleHand && history.HasQuickSweep(settings.waveQuickSweepPalmWidths, settings.waveQuickSweepSeconds);
            bool enoughMotion = quickSweep || (history.SampleCount >= settings.waveMinimumSamples &&
                                history.Reversals >= settings.waveDirectionReversals &&
                                history.HorizontalRange >= settings.waveMinimumHorizontalTravel &&
                                history.VerticalRange <= settings.waveMaximumVerticalDriftPalmWidths);
            ReportCandidate(hand.Id, "Wave", enoughMotion,
                settings.diagnosticsEnabled ? "samples=" + history.SampleCount + ", reversals=" + history.Reversals +
                ", horizontal=" + history.HorizontalRange.ToString("0.00") +
                ", vertical=" + history.VerticalRange.ToString("0.00") + " palm widths" : null);
            if (enoughMotion && !history.Emitted && CooldownElapsed(SemanticGesture.Wave, now))
            {
                Emit(SemanticGesture.Wave, hand.Id, Vector2.zero, Vector2.zero, now);
                history.Emitted = true;
                ReportDiagnostic("Wave accepted for " + hand.Id + (quickSweep ? " after a quick sweep." : " after a direction change."));
            }
            else if (enoughMotion && !CooldownElapsed(SemanticGesture.Wave, now))
            {
                if (!history.CooldownReported)
                {
                    history.CooldownReported = true;
                    ReportDiagnostic("Wave candidate rejected by event cooldown for " + hand.Id + ".");
                }
            }
            else
                history.CooldownReported = false;
        }

        private void ReportWaveSummary(string handId, WaveHistory history, double now)
        {
            if (now - history.LastDiagnosticAt < settings.diagnosticsIntervalSeconds)
                return;
            history.LastDiagnosticAt = now;
            ReportDiagnostic("Wave trajectory " + handId + ": samples=" + history.SampleCount +
                ", reversals=" + history.Reversals + ", horizontal=" + history.HorizontalRange.ToString("0.00") +
                ", vertical=" + history.VerticalRange.ToString("0.00") + " palm widths, trace=" +
                history.RecentTrace(8) + ", latched=" + history.Emitted + ".");
        }

        private void ProcessClap(List<HandPose> hands, double now)
        {
            if (clapHasPreviousDistance && now - lastClapPairTime > settings.clapPairGraceSeconds)
            {
                clapHasPreviousDistance = false;
                clapArmed = false;
                clapSeparatedSince = double.NaN;
                clapClosingDistance = 0;
                clapClosingObservations = 0;
            }
            if (hands.Count != 2)
            {
                // At contact the two palms can merge into a single detection.
                // Accept that only after a measured two-hand approach and when
                // the remaining detection is near the last midpoint.
                if (hands.Count == 1 && clapArmed && clapHasPreviousDistance &&
                    now - clapStartedAt <= settings.clapClosingWindowSeconds &&
                    lastClapScreenDistance <= settings.clapArmDistancePalmWidths * lastClapScale &&
                    HasClapApproach(now) &&
                    Vector2.Distance(hands[0].MotionPalmCenter, lastClapMidpoint) <= lastClapScale * 0.8f)
                {
                    EmitClap(now);
                }
                return;
            }

            float scale = (hands[0].PalmWidth + hands[1].PalmWidth) * 0.5f;
            float screenDistance = Vector2.Distance(hands[0].MotionPalmCenter, hands[1].MotionPalmCenter);
            float distance = screenDistance / scale;
            if (distance >= settings.clapArmDistancePalmWidths)
            {
                if (double.IsNaN(clapSeparatedSince))
                    clapSeparatedSince = now;
                if (now - clapSeparatedSince >= settings.clapSeparationDwellSeconds)
                {
                    clapArmed = true;
                    clapLatched = false;
                }
            }
            else
                clapSeparatedSince = double.NaN;

            if (clapArmed && !clapLatched && clapHasPreviousDistance)
            {
                float step = (lastClapScreenDistance - screenDistance) / scale;
                if (clapClosingObservations > 0 &&
                    (now - clapStartedAt > settings.clapClosingWindowSeconds || step < -settings.clapOpeningResetPalmWidths))
                {
                    clapClosingDistance = 0;
                    clapClosingObservations = 0;
                }

                if (step >= settings.clapMinimumFrameClosingPalmWidths)
                {
                    if (clapClosingObservations == 0)
                        clapStartedAt = now;
                    clapClosingDistance += step;
                    clapClosingObservations++;
                }

                if (distance <= settings.clapContactDistancePalmWidths && HasClapApproach(now))
                    EmitClap(now);
            }

            lastClapScreenDistance = screenDistance;
            lastClapScale = scale;
            lastClapMidpoint = (hands[0].MotionPalmCenter + hands[1].MotionPalmCenter) * 0.5f;
            lastClapPairTime = now;
            clapHasPreviousDistance = true;
        }

        private bool HasClapApproach(double now)
        {
            return clapClosingDistance >= settings.clapMinimumClosingPalmWidths &&
                clapClosingObservations >= settings.clapMinimumClosingObservations &&
                CooldownElapsed(SemanticGesture.Clap, now);
        }

        private void EmitClap(double now)
        {
            clapLatched = true;
            clapArmed = false;
            clapClosingDistance = 0;
            clapClosingObservations = 0;
            Emit(SemanticGesture.Clap, string.Empty, Vector2.zero, Vector2.zero, now);
        }

        private void ProcessBothHandsRaised(List<HandPose> hands, double now)
        {
            bool bothOpen = hands.Count == 2 && (hands[0].OpenPalm || hands[1].OpenPalm) &&
                hands[0].ExtendedCount >= 2 && !hands[0].Fist && !hands[0].Point &&
                hands[1].ExtendedCount >= 2 && !hands[1].Fist && !hands[1].Point;
            if (bothOpen)
                lastVideoPoseTime = now;
            else if (now - lastVideoPoseTime < 0.16)
                return;

            ProcessPose("global", VideoPoseKey, bothOpen, SemanticGesture.BothHandsRaised,
                now, settings.bothHandsRaisedDwellSeconds, Vector2.zero, Vector2.zero);
        }

        public void ResetVideoGesture()
        {
            poseStates.Remove("global:" + VideoPoseKey);
            lastEmitted.Remove(SemanticGesture.BothHandsRaised);
            lastVideoPoseTime = double.NegativeInfinity;
        }

        private void UpdatePointing(List<HandPose> hands, double now)
        {
            int candidate = -1;
            for (var i = 0; i < hands.Count; i++)
            {
                if (!hands[i].Point)
                    continue;

                if (candidate < 0)
                    candidate = i;
                if (hands[i].Id == pointingHandId)
                {
                    candidate = i;
                    break;
                }
            }

            if (candidate >= 0)
            {
                var hand = hands[candidate];
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

            staleFilterIds.Clear();
            foreach (var pair in landmarkFilters)
                if (!presentHandIds.Contains(pair.Key)) staleFilterIds.Add(pair.Key);
            for (var i = 0; i < staleFilterIds.Count; i++) landmarkFilters.Remove(staleFilterIds[i]);

            staleWaveIds.Clear();
            foreach (var pair in waveHistories)
            {
                if (!presentHandIds.Contains(pair.Key) &&
                    pair.Value.MarkOpenPalmInactive(now, settings.waveMaximumObservationGapSeconds))
                {
                    staleWaveIds.Add(pair.Key);
                }
            }
            for (var i = 0; i < staleWaveIds.Count; i++)
                waveHistories.Remove(staleWaveIds[i]);

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
                    if (!missingState.Emitted)
                        ReportDiagnostic("Pose " + key + " lost tracking before acceptance; release hysteresis started.");
                }
            }

        }

        private void ReportCandidate(string identity, string candidate, bool active, string detail = null)
        {
            if (!settings.diagnosticsEnabled)
                return;
            string key = identity + ":" + candidate;
            if (diagnosticCandidateStates.TryGetValue(key, out var previous) && previous == active)
                return;
            diagnosticCandidateStates[key] = active;
            double now = Time.realtimeSinceStartupAsDouble;
            if (lastCandidateDiagnosticTimes.TryGetValue(key, out var last) &&
                now - last < Math.Max(0.1f, settings.diagnosticsIntervalSeconds))
                return;
            lastCandidateDiagnosticTimes[key] = now;
            ReportDiagnostic(candidate + " candidate " + (active ? "active" : "inactive") +
                " for " + identity + (string.IsNullOrEmpty(detail) ? string.Empty : " (" + detail + ")") + ".");
        }

        private void ReportDiagnostic(string message)
        {
            if (settings.diagnosticsEnabled)
                DiagnosticMessage?.Invoke(message);
        }

        private static string HandIdFromPoseKey(string key)
        {
            var separator = key.LastIndexOf(':');
            return separator < 0 ? key : key.Substring(0, separator);
        }

        private bool CooldownElapsed(SemanticGesture gesture, double now)
        {
            float cooldown = gesture == SemanticGesture.Clap ? settings.clapCooldownSeconds : settings.eventCooldownSeconds;
            return !lastEmitted.TryGetValue(gesture, out var last) || now - last >= cooldown;
        }

        private void Emit(SemanticGesture gesture, string handId, Vector2 direction, Vector2 position, double now)
        {
            lastEmitted[gesture] = now;
            GestureDetected?.Invoke(new GestureRecognitionEvent(gesture, handId, direction, position, now));
        }

        private void ResetTrackingState()
        {
            poseStates.Clear();
            lastVideoPoseTime = double.NegativeInfinity;
            landmarkFilters.Clear();
            waveHistories.Clear();
            diagnosticCandidateStates.Clear();
            lastCandidateDiagnosticTimes.Clear();
            clapArmed = false;
            clapHasPreviousDistance = false;
            clapLatched = false;
            clapSeparatedSince = double.NaN;
            clapStartedAt = 0;
            clapClosingDistance = 0f;
            clapClosingObservations = 0;
            hasValidHands = false;
            HasUsableTracking = false;
            pointingHandId = null;
            pointingStarted = 0;
            SetPointing(false, Vector2.zero);
        }

        private readonly struct HandPose
        {
            public readonly string Id;
            public readonly Vector2[] Points;
            public readonly Vector2 MotionPalmCenter;
            public readonly Vector2 WavePosition;
            public readonly float PalmWidth;
            public readonly bool OpenPalm;
            public readonly bool Fist;
            public readonly bool Point;
            public readonly Vector2 PointDirection;
            public readonly Vector2 PointPosition;
            public readonly int ExtendedCount;
            public readonly int CurledCount;

            public HandPose(string id, Vector2[] points, Vector2 motionPalmCenter, Vector2 wavePosition, float palmWidth,
                bool openPalm, bool fist, bool point, Vector2 pointDirection, Vector2 pointPosition,
                int extendedCount, int curledCount)
            {
                Id = id;
                Points = points;
                MotionPalmCenter = motionPalmCenter;
                WavePosition = wavePosition;
                PalmWidth = palmWidth;
                OpenPalm = openPalm;
                Fist = fist;
                Point = point;
                PointDirection = pointDirection;
                PointPosition = pointPosition;
                ExtendedCount = extendedCount;
                CurledCount = curledCount;
            }
        }

        private sealed class PoseState
        {
            public double Started;
            public double LastObserved;
            public int ConsecutiveObservations;
            public Vector2 Anchor;
            public bool Emitted;
            public bool HasInactiveTime;
            public double InactiveSince;
            public bool DwellReported;
            public bool CooldownReported;

            public PoseState(double started, Vector2 anchor)
            {
                Started = started;
                LastObserved = started;
                Anchor = anchor;
            }
        }

        private sealed class LandmarkFilterState
        {
            public readonly Vector2[] Points;
            public double LastObserved;
            public LandmarkFilterState(int count) { Points = new Vector2[count]; }
        }

        private sealed class WaveHistory
        {
            private readonly List<Sample> samples = new List<Sample>();
            private bool hasPrevious;
            private float previousX;
            private float previousY;
            private float previousScale;
            private double previousTime;
            private float cumulativeX;
            private float cumulativeY;
            private bool hasOpenPalmInactiveTime;
            private double openPalmInactiveSince;

            public int Reversals { get; private set; }
            public float HorizontalRange { get; private set; }
            public float VerticalRange { get; private set; }
            public int SampleCount { get { return samples.Count; } }
            public bool Emitted { get; set; }
            public bool CooldownReported { get; set; }
            public double LastDiagnosticAt { get; set; }
            public double LastOpenPalmTime { get; set; } = double.NegativeInfinity;

            public bool SawOpenPalmRecently(double now, double maximumGap)
            {
                return now - LastOpenPalmTime <= maximumGap;
            }

            public bool HasQuickSweep(float minimumTravel, float maximumDuration)
            {
                if (samples.Count < 3)
                    return false;

                Sample last = samples[samples.Count - 1];
                float minY = Mathf.Min(last.Y, samples[samples.Count - 2].Y);
                float maxY = Mathf.Max(last.Y, samples[samples.Count - 2].Y);
                for (int i = samples.Count - 3; i >= 0; i--)
                {
                    Sample start = samples[i];
                    double duration = last.Time - start.Time;
                    if (duration > maximumDuration)
                        break;

                    minY = Mathf.Min(minY, start.Y);
                    maxY = Mathf.Max(maxY, start.Y);
                    float travel = Mathf.Abs(last.X - start.X);
                    if (duration >= 0.1 && travel >= minimumTravel && maxY - minY <= travel * 0.5f)
                        return true;
                }
                return false;
            }

            public bool Add(float x, float y, float scale, double time, float window, float maximumGap, float minimumStep)
            {
                scale = Mathf.Max(scale, 0.001f);
                bool resetForGap = hasPrevious && time - previousTime > maximumGap;
                if (resetForGap)
                    ClearMotion();

                if (!hasPrevious)
                {
                    hasPrevious = true;
                    previousX = x;
                    previousY = y;
                    previousScale = scale;
                    previousTime = time;
                    samples.Add(new Sample(0f, 0f, time));
                    Recalculate(minimumStep);
                    return resetForGap;
                }

                float normalizedScale = Mathf.Max((previousScale + scale) * 0.5f, 0.001f);
                cumulativeX += (x - previousX) / normalizedScale;
                cumulativeY += (y - previousY) / normalizedScale;
                samples.Add(new Sample(cumulativeX, cumulativeY, time));
                previousX = x;
                previousY = y;
                previousScale = scale;
                previousTime = time;

                while (samples.Count > 0 && time - samples[0].Time > window)
                {
                    samples.RemoveAt(0);
                }

                Recalculate(minimumStep);
                return resetForGap;
            }

            private void Recalculate(float minimumStep)
            {
                Reversals = 0;
                HorizontalRange = 0f;
                VerticalRange = 0f;
                if (samples.Count == 0)
                    return;

                float minX = float.MaxValue;
                float maxX = float.MinValue;
                float minY = float.MaxValue;
                float maxY = float.MinValue;
                for (var i = 0; i < samples.Count; i++)
                {
                    minX = Mathf.Min(minX, samples[i].X);
                    maxX = Mathf.Max(maxX, samples[i].X);
                    minY = Mathf.Min(minY, samples[i].Y);
                    maxY = Mathf.Max(maxY, samples[i].Y);
                }
                HorizontalRange = maxX - minX;
                VerticalRange = maxY - minY;

                float anchor = samples[0].X;
                int previousDirection = 0;
                for (var i = 1; i < samples.Count; i++)
                {
                    float delta = samples[i].X - anchor;
                    if (Mathf.Abs(delta) < minimumStep)
                        continue;
                    int direction = Math.Sign(delta);
                    if (previousDirection != 0 && direction != previousDirection)
                        Reversals++;
                    previousDirection = direction;
                    anchor = samples[i].X;
                }
            }

            public void MarkOpenPalmActive()
            {
                hasOpenPalmInactiveTime = false;
            }

            public bool MarkOpenPalmInactive(double time, float releaseDwell)
            {
                if (!hasOpenPalmInactiveTime)
                {
                    hasOpenPalmInactiveTime = true;
                    openPalmInactiveSince = time;
                }
                return time - openPalmInactiveSince >= releaseDwell;
            }

            public string RecentTrace(int maximumSamples)
            {
                var trace = new StringBuilder();
                int start = Mathf.Max(0, samples.Count - maximumSamples);
                for (var i = start; i < samples.Count; i++)
                {
                    if (i > start)
                        trace.Append(' ');
                    trace.Append(samples[i].X.ToString("0.0"));
                    trace.Append('/');
                    trace.Append(samples[i].Y.ToString("0.0"));
                }
                return trace.ToString();
            }

            private void ClearMotion()
            {
                samples.Clear();
                hasPrevious = false;
                previousX = 0f;
                previousY = 0f;
                previousScale = 0f;
                previousTime = 0;
                cumulativeX = 0f;
                cumulativeY = 0f;
                Reversals = 0;
                HorizontalRange = 0f;
                VerticalRange = 0f;
            }

            private readonly struct Sample
            {
                public readonly float X;
                public readonly float Y;
                public readonly double Time;
                public Sample(float x, float y, double time) { X = x; Y = y; Time = time; }
            }
        }
    }
}
