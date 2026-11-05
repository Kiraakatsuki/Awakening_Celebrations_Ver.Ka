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
        [Min(0f)] public float poseDwellSeconds = 0.18f;
        [Min(0f)] public float poseReleaseDwellSeconds = 0.15f;
        [Min(0f)] public float openPalmHoldSeconds = 1.0f;
        [Min(0f)] public float eventCooldownSeconds = 0.8f;
        [Min(0f)] public float trackingLossResetSeconds = 0.35f;
        [Min(0f)] public float noResultWarningSeconds = 5.0f;
        public bool diagnosticsEnabled;
        [Min(0.1f)] public float diagnosticsIntervalSeconds = 1.0f;
        [Min(0f)] public float openPalmMaxDrift = 0.06f;
        [Range(0f, 0.9f)] public float landmarkSmoothing = 0.35f;
        [Range(0f, 0.1f)] public float normalizedLandmarkMargin = 0.02f;
        [Min(2)] public int minimumPoseObservations = 5;
        [Min(0.02f)] public float maximumPoseObservationGapSeconds = 0.12f;

        [Min(1f)] public float fingerExtendedDistanceRatio = 1.12f;
        [Range(0f, 1f)] public float fingerCurledDistanceRatio = 0.96f;
        [Min(0f)] public float thumbExtensionPalmWidths = 0.65f;
        [Min(0f)] public float fistThumbMaximumPalmWidths = 1.15f;
        [Min(1f)] public float pointIndexExtensionRatio = 1.12f;
        [Min(0f)] public float pointOtherFingerMaximumRatio = 1.04f;
        [Range(1, 4)] public int openPalmMinimumExtendedFingers = 4;
        [Range(1, 4)] public int fistMinimumCurledFingers = 4;

        [Min(0f)] public float waveWindowSeconds = 1.4f;
        [Min(0f)] public float waveMinimumHorizontalTravel = 1.25f;
        [Min(1)] public int waveDirectionReversals = 2;
        [Min(0f)] public float waveMinimumStep = 0.18f;
        [Min(3)] public int waveMinimumSamples = 6;
        [Min(0.01f)] public float waveMaximumObservationGapSeconds = 0.2f;
        [Min(0f)] public float waveMaximumVerticalDriftPalmWidths = 0.9f;

        [Min(0f)] public float clapArmDistancePalmWidths = 1.8f;
        [Min(0f)] public float clapContactDistancePalmWidths = 0.9f;
        [Min(0f)] public float clapMinimumClosingPalmWidths = 0.3f;
        [Min(0f)] public float clapMinimumFrameClosingPalmWidths = 0.015f;
        [Min(1)] public int clapMinimumClosingObservations = 3;
        [Min(0f)] public float clapSeparationDwellSeconds = 0.25f;
        [Min(0f)] public float clapClosingWindowSeconds = 0.8f;
        [Min(0f)] public float clapOpeningResetPalmWidths = 0.08f;
        [Min(0f)] public float clapMinimumReboundPalmWidths = 0.12f;
        [Min(0.05f)] public float clapMaximumReboundSeconds = 0.35f;

        // Hand Landmarker has no shoulder/body landmarks. These frame-space rules
        // are provisional until a body-pose source and the approved definition exist.
        [Range(0f, 1f)] public float raisedWristMaximumY = 0.55f;
        [Min(0f)] public float bothHandsRaisedDwellSeconds = 0.45f;
        [Min(0f)] public float bothHandsRaiseMinimumTravel = 0.12f;
        [Min(0.1f)] public float bothHandsRaiseWindowSeconds = 1.5f;
        [Min(0f)] public float raiseGestureArmingMargin = 0.06f;
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
        private readonly Dictionary<string, LandmarkFilterState> landmarkFilters = new Dictionary<string, LandmarkFilterState>();
        private readonly Dictionary<SemanticGesture, double> lastEmitted = new Dictionary<SemanticGesture, double>();
        private readonly Dictionary<string, WaveHistory> waveHistories = new Dictionary<string, WaveHistory>();
        private readonly Dictionary<string, RaiseHistory> raiseHistories = new Dictionary<string, RaiseHistory>();
        private readonly Dictionary<string, bool> diagnosticCandidateStates = new Dictionary<string, bool>();
        private readonly Dictionary<string, double> lastCandidateDiagnosticTimes = new Dictionary<string, double>();
        private readonly List<HandPose> analyzedHands = new List<HandPose>(2);
        private readonly HashSet<string> presentHandIds = new HashSet<string>();
        private readonly List<string> staleFilterIds = new List<string>(2);
        private readonly List<string> staleRaiseIds = new List<string>(2);
        private readonly List<string> staleWaveIds = new List<string>();
        private bool clapArmed;
        private double lastClapDistance;
        private bool clapHasPreviousDistance;
        private bool clapLatched;
        private double clapSeparatedSince = double.NaN;
        private double clapStartedAt;
        private double clapClosingDistance;
        private int clapClosingObservations;
        private bool clapContactDetected;
        private bool clapContactAttempted;
        private double clapContactAt;
        private double clapContactDistance;
        private bool hasValidHands;
        private double lastValidHandsTime;
        private string pointingHandId;
        private double pointingStarted;
        private bool pointing;
        public bool HasUsableTracking { get; private set; }

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
                var pose = Analyze(new TrackedHand(hand.Id, filteredLandmarks));
                analyzedHands.Add(pose);
                ProcessSingleHandGestures(pose, timestampSeconds);
            }

            if (analyzedHands.Count == 0)
            {
                ProcessNoHands(timestampSeconds);
                return;
            }

            hasValidHands = true;
            HasUsableTracking = true;
            lastValidHandsTime = timestampSeconds;

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
            landmarkFilters.Clear();
            waveHistories.Clear();
            raiseHistories.Clear();
            diagnosticCandidateStates.Clear();
            lastCandidateDiagnosticTimes.Clear();
            clapArmed = false;
            clapHasPreviousDistance = false;
            clapLatched = false;
            clapSeparatedSince = double.NaN;
            clapStartedAt = 0;
            clapClosingDistance = 0f;
            clapClosingObservations = 0;
            clapContactDetected = false;
            clapContactAttempted = false;
            clapContactAt = 0;
            clapContactDistance = 0;
            hasValidHands = false;
            HasUsableTracking = false;
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
            var direction = p[IndexTip] - p[IndexMcp];
            if (direction.sqrMagnitude > 0.0001f)
            {
                direction.Normalize();
            }

            return new HandPose(hand.Id, p, palmCenter, palmWidth, openPalm, fist, point, direction,
                p[IndexTip], p[Wrist].y <= settings.raisedWristMaximumY, extended, curled);
        }

        private bool HasUsableLandmarks(Vector2[] points)
        {
            if (points == null || points.Length < LandmarkCount)
                return false;

            float margin = Mathf.Max(0f, settings.normalizedLandmarkMargin);
            for (var i = 0; i < LandmarkCount; i++)
            {
                Vector2 point = points[i];
                if (float.IsNaN(point.x) || float.IsNaN(point.y) || float.IsInfinity(point.x) || float.IsInfinity(point.y) ||
                    point.x < -margin || point.x > 1f + margin || point.y < -margin || point.y > 1f + margin)
                    return false;
            }

            float palmWidth = Vector2.Distance(points[IndexMcp], points[PinkyMcp]);
            return palmWidth >= 0.004f && palmWidth <= 0.35f;
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

        private void ProcessSingleHandGestures(HandPose hand, double now)
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

        private void ProcessWave(HandPose hand, double now)
        {
            if (!hand.OpenPalm)
            {
                if (waveHistories.TryGetValue(hand.Id, out var inactiveHistory) &&
                    inactiveHistory.MarkOpenPalmInactive(now, settings.poseReleaseDwellSeconds))
                {
                    waveHistories.Remove(hand.Id);
                    ReportDiagnostic("Wave history released for " + hand.Id + " after stable open-palm release.");
                }
                return;
            }

            if (!waveHistories.TryGetValue(hand.Id, out var history))
            {
                history = new WaveHistory();
                waveHistories.Add(hand.Id, history);
            }

            history.MarkOpenPalmActive();
            if (history.Emitted)
                return;

            bool resetForGap = history.Add(hand.Points[Wrist].x, hand.Points[Wrist].y, hand.PalmWidth, now,
                settings.waveWindowSeconds, settings.waveMaximumObservationGapSeconds,
                settings.waveMinimumStep);
            if (resetForGap)
                ReportDiagnostic("Wave history reset for " + hand.Id + " after an observation gap.");

            if (settings.diagnosticsEnabled)
                ReportWaveSummary(hand.Id, history, now);

            bool enoughMotion = history.SampleCount >= settings.waveMinimumSamples &&
                                history.Reversals >= settings.waveDirectionReversals &&
                                history.HorizontalRange >= settings.waveMinimumHorizontalTravel &&
                                history.VerticalRange <= settings.waveMaximumVerticalDriftPalmWidths;
            ReportCandidate(hand.Id, "Wave", enoughMotion,
                settings.diagnosticsEnabled ? "samples=" + history.SampleCount + ", reversals=" + history.Reversals +
                ", horizontal=" + history.HorizontalRange.ToString("0.00") +
                ", vertical=" + history.VerticalRange.ToString("0.00") + " palm widths" : null);
            if (enoughMotion && !history.Emitted && CooldownElapsed(SemanticGesture.Wave, now))
            {
                Emit(SemanticGesture.Wave, hand.Id, Vector2.zero, Vector2.zero, now);
                history.Emitted = true;
                ReportDiagnostic("Wave accepted for " + hand.Id + "; event latched until open palm is released.");
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
            if (hands.Count < 2)
            {
                clapArmed = false;
                clapHasPreviousDistance = false;
                clapSeparatedSince = double.NaN;
                clapClosingDistance = 0f;
                clapClosingObservations = 0;
                clapContactDetected = false;
                clapContactAttempted = false;
                ReportCandidate("global", "Clap", false, "two tracked hands required");
                return;
            }

            var averagePalmWidth = (hands[0].PalmWidth + hands[1].PalmWidth) * 0.5f;
            var distanceInPalmWidths = Vector2.Distance(hands[0].PalmCenter, hands[1].PalmCenter) / averagePalmWidth;
            if (distanceInPalmWidths >= settings.clapArmDistancePalmWidths)
            {
                if (double.IsNaN(clapSeparatedSince))
                    clapSeparatedSince = now;
                if (now - clapSeparatedSince >= settings.clapSeparationDwellSeconds)
                {
                    clapContactDetected = false;
                    clapContactAttempted = false;
                    if (clapLatched)
                    {
                        clapLatched = false;
                        ReportDiagnostic("Clap latch released after stable hand separation.");
                    }
                    if (!clapArmed)
                    {
                        clapArmed = true;
                        clapContactAttempted = false;
                        clapStartedAt = now;
                        clapClosingDistance = 0f;
                        clapClosingObservations = 0;
                        ReportDiagnostic("Clap armed after hands stayed separated.");
                    }
                }
            }
            else
                clapSeparatedSince = double.NaN;

            if (clapLatched)
            {
                lastClapDistance = distanceInPalmWidths;
                clapHasPreviousDistance = true;
                ReportCandidate("global", "Clap", false, "already emitted; awaiting separation");
                return;
            }

            if (clapArmed && now - clapStartedAt > settings.clapClosingWindowSeconds)
            {
                clapArmed = false;
                clapClosingDistance = 0f;
                clapClosingObservations = 0;
                ReportDiagnostic("Clap candidate rejected: closing window expired before contact.");
            }

            if (clapArmed && clapHasPreviousDistance)
            {
                double closingStep = lastClapDistance - distanceInPalmWidths;
                if (closingStep >= settings.clapMinimumFrameClosingPalmWidths)
                {
                    clapClosingDistance += closingStep;
                    clapClosingObservations++;
                }
                else if (-closingStep >= settings.clapOpeningResetPalmWidths)
                {
                    clapClosingDistance = 0f;
                    clapClosingObservations = 0;
                    clapStartedAt = now;
                }
            }

            bool clapCandidate = clapArmed && distanceInPalmWidths <= settings.clapContactDistancePalmWidths &&
                                 clapClosingDistance >= settings.clapMinimumClosingPalmWidths &&
                                 clapClosingObservations >= settings.clapMinimumClosingObservations &&
                                 hands[0].OpenPalm && hands[1].OpenPalm;
            ReportCandidate("global", "Clap", clapCandidate,
                settings.diagnosticsEnabled ? "distance=" + distanceInPalmWidths.ToString("0.00") + ", closing=" + clapClosingDistance.ToString("0.00") +
                ", observations=" + clapClosingObservations : null);
            if (clapCandidate && !clapContactAttempted)
            {
                clapContactDetected = true;
                clapContactAttempted = true;
                clapContactAt = now;
                clapContactDistance = distanceInPalmWidths;
                ReportDiagnostic("Clap contact candidate recorded; waiting for a quick rebound.");
            }

            if (clapContactDetected && now - clapContactAt > settings.clapMaximumReboundSeconds)
            {
                clapContactDetected = false;
                clapClosingDistance = 0f;
                clapClosingObservations = 0;
                ReportDiagnostic("Clap candidate rejected: no rebound after contact.");
            }

            bool clapRebounded = clapContactDetected &&
                distanceInPalmWidths - clapContactDistance >= settings.clapMinimumReboundPalmWidths;
            if (clapRebounded && CooldownElapsed(SemanticGesture.Clap, now))
            {
                Emit(SemanticGesture.Clap, string.Empty, Vector2.zero, Vector2.zero, now);
                clapLatched = true;
                clapArmed = false;
                clapContactDetected = false;
                ReportDiagnostic("Clap accepted after a closing contact candidate and quick rebound.");
            }

            lastClapDistance = distanceInPalmWidths;
            clapHasPreviousDistance = true;
        }

        private void ProcessBothHandsRaised(List<HandPose> hands, double now)
        {
            var raisedCount = 0;
            for (var i = 0; i < hands.Count; i++)
            {
                if (!hands[i].OpenPalm)
                {
                    raiseHistories.Remove(hands[i].Id);
                    continue;
                }

                if (UpdateRaiseHistory(hands[i], now))
                {
                    raisedCount++;
                }
            }

            ReportCandidate("global", "BothHandsRaised", raisedCount >= 2,
                settings.diagnosticsEnabled ? "raised open palms=" + raisedCount + "/2; image y <= " + settings.raisedWristMaximumY.ToString("0.00") : null);
            ProcessPose("global", "both-hands-raised", raisedCount >= 2,
                SemanticGesture.BothHandsRaised, now, settings.bothHandsRaisedDwellSeconds, Vector2.zero, Vector2.zero);
        }

        private bool UpdateRaiseHistory(HandPose hand, double now)
        {
            if (!raiseHistories.TryGetValue(hand.Id, out var history))
            {
                history = new RaiseHistory();
                raiseHistories.Add(hand.Id, history);
            }
            return history.Add(hand.Points[Wrist].y, now, settings.raisedWristMaximumY,
                settings.raiseGestureArmingMargin, settings.bothHandsRaiseMinimumTravel,
                settings.bothHandsRaiseWindowSeconds, settings.maximumPoseObservationGapSeconds);
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

            staleFilterIds.Clear();
            foreach (var pair in landmarkFilters)
                if (!presentHandIds.Contains(pair.Key)) staleFilterIds.Add(pair.Key);
            for (var i = 0; i < staleFilterIds.Count; i++) landmarkFilters.Remove(staleFilterIds[i]);
            staleRaiseIds.Clear();
            foreach (var pair in raiseHistories)
                if (!presentHandIds.Contains(pair.Key)) staleRaiseIds.Add(pair.Key);
            for (var i = 0; i < staleRaiseIds.Count; i++) raiseHistories.Remove(staleRaiseIds[i]);

            staleWaveIds.Clear();
            foreach (var pair in waveHistories)
            {
                if (!presentHandIds.Contains(pair.Key) &&
                    pair.Value.MarkOpenPalmInactive(now, settings.poseReleaseDwellSeconds))
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
            landmarkFilters.Clear();
            waveHistories.Clear();
            raiseHistories.Clear();
            diagnosticCandidateStates.Clear();
            lastCandidateDiagnosticTimes.Clear();
            clapArmed = false;
            clapHasPreviousDistance = false;
            clapLatched = false;
            clapSeparatedSince = double.NaN;
            clapStartedAt = 0;
            clapClosingDistance = 0f;
            clapClosingObservations = 0;
            clapContactDetected = false;
            clapContactAttempted = false;
            clapContactAt = 0;
            clapContactDistance = 0;
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
            public readonly Vector2 PalmCenter;
            public readonly float PalmWidth;
            public readonly bool OpenPalm;
            public readonly bool Fist;
            public readonly bool Point;
            public readonly Vector2 PointDirection;
            public readonly Vector2 PointPosition;
            public readonly bool Raised;
            public readonly int ExtendedCount;
            public readonly int CurledCount;

            public HandPose(string id, Vector2[] points, Vector2 palmCenter, float palmWidth,
                bool openPalm, bool fist, bool point, Vector2 pointDirection, Vector2 pointPosition, bool raised,
                int extendedCount, int curledCount)
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

        private sealed class RaiseHistory
        {
            private bool armed;
            private bool completed;
            private float startY;
            private double startedAt;
            private double lastObserved;

            public bool Add(float y, double now, float raisedMaximumY, float armingMargin,
                float minimumTravel, float windowSeconds, float maximumGap)
            {
                if (lastObserved > 0 && now - lastObserved > maximumGap)
                    Reset();
                lastObserved = now;

                if (completed)
                {
                    if (now - startedAt <= windowSeconds && y <= raisedMaximumY)
                        return true;
                    Reset();
                }

                if (!armed && y >= raisedMaximumY + armingMargin)
                {
                    armed = true;
                    startY = y;
                    startedAt = now;
                }
                else if (armed && now - startedAt <= windowSeconds &&
                         y <= raisedMaximumY && startY - y >= minimumTravel)
                {
                    completed = true;
                    return true;
                }
                else if (armed && now - startedAt > windowSeconds)
                    Reset();

                return false;
            }

            private void Reset()
            {
                armed = false;
                completed = false;
                startedAt = 0;
                startY = 0;
            }
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
