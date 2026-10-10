# Gesture Recognition Layer Audit

Date: 2026-10-11. Static implementation note; Unity compilation, camera operation, and gesture accuracy have not been runtime verified.

## Requirement source and boundary

The supplied `Awakening_Celebrations_Complete_Project_Documentation.docx` defines INT-01 through INT-06 as Wave, Clap, Point, Fist confirmation, both-hands raise, and Open Palm + Hold. It defines their semantic meanings and requires adjustable thresholds/dwell/cooldown. It does not define landmark geometry, exact durations, confidence thresholds, or a body-relative definition of “raised.” Those details remain provisional. The document also flags an older Wave/Clap/Sway/Raise-Hands proposal as historical; the current interaction table is the basis for these six event names.

This stage adds landmark recognition and semantic event output only. It does not connect gesture events to application navigation, modify Home screens, or add a second camera/model pipeline.

## Result path

The installed MediaPipe Unity Plugin 0.16.3 sample runner returns `HandLandmarkerResult` containing normalized hand landmarks, world landmarks, and handedness classifications. It does not classify the requested gestures. The runner now publishes a result event. `MediaPipeGestureBridge` copies normalized x/y landmarks and handedness during that callback, then sends the copied frame to `GestureRecognitionLayer` from Unity's `Update` loop. The recognizer raises `GestureDetected` events and has no screen/navigation references. Consumers can subscribe to `MediaPipeGestureBridge.GestureDetected` in a later integration stage.

The bridge must be attached to the same GameObject as the existing `HandLandmarkerRunner`. The sample configuration now requests two hands, which is needed for Clap and BothHandsRaised. No component was added to a scene in this stage.

## Recognition rules and initial values

| Gesture | Current rule | Initial configurable values |
|---|---|---|
| Wave | Open-palm hand with horizontal wrist motion, at least two direction reversals and sufficient travel inside a rolling time window. Emits once per detected wave cycle, with cooldown. | 1.0 s window; 0.14 normalized x travel; 2 reversals; 0.025 minimum x step; 0.8 s cooldown. |
| Clap | Two tracked hands approach: separation begins at or above the arm distance and crosses the contact distance with the required closing delta. Latched until hands separate again. | 1.2 palm widths to arm; 0.65 palm widths contact; 0.2 palm-width closing delta. |
| Point | Index tip extends relative to palm; the other three fingers satisfy the curled test; the pose persists through the pose dwell. The event includes normalized camera-space index direction. | 0.18 s pose dwell; finger extension/curl ratios exposed in settings. |
| Fist | At least three non-thumb fingers are curled, none are extended, and the thumb is near the palm. Emits once until the pose releases. | 0.18 s pose dwell; finger and thumb ratios exposed in settings. |
| BothHandsRaised | Two hand wrists remain in the upper 42% of normalized image coordinates. Emits once until the condition releases. | `raisedWristMaximumY = 0.42`; 0.18 s pose dwell. |
| OpenPalmHeld | Open-palm geometry remains present while the wrist stays within the configured normalized drift from its initial position. Emits once until the pose releases. | 1.0 s hold; 0.06 normalized-coordinate drift. |

All gesture thresholds, dwell values, and cooldowns are serialized in `GestureRecognitionSettings` on the bridge. Tracking resets recognition history after the configured loss interval (0.35 s by default). The sample runner's existing detection, presence, and tracking confidence defaults remain 0.5.

## Provisional assumptions and limits

- The DOCX specifies gesture meaning, not pose equations. Finger extension/curl ratios and wave/clap movement thresholds are initial engineering heuristics for calibration.
- Hand Landmarker has no shoulder or torso landmarks. BothHandsRaised therefore uses wrist height in the camera image, not actual height relative to the visitor's shoulders. It can false-trigger when hands are naturally held high and may miss a raised hand outside the configured image region.
- Clap uses normalized hand positions and palm width; it does not verify palm orientation, acoustic contact, or a physical clap sound.
- Wave uses normalized horizontal wrist movement. Camera mirroring/aspect effects and visitor-specific movement ranges require user testing.
- Point emits a direction only. Selecting a performer from that direction and confirming it with Fist are future navigation responsibilities.
- A callback with no valid hands or a stale result resets gesture continuity. The bridge reports tracking availability/loss and warns if the runner produces no results after the startup timeout.
- This layer is not connected to `SampleScene` or any awakening state. It cannot advance screens yet.

## Manual test setup

1. Open `Assets/Samples/MediaPipe Unity Plugin/0.16.3/Official Solutions/Scenes/Hand Landmark Detection/Hand Landmark Detection.unity`.
2. Add `MediaPipeGestureBridge` to the `Solution` GameObject that already has `HandLandmarkerRunner`.
3. Enter Play Mode, allow the existing sample camera/model setup to initialize, and watch the Console. Expect `Hand tracking available`, then one `Gesture recognized: ...` entry per gesture event. Stop tracking and expect `Hand tracking lost` after the configured timeout.
4. For initial threshold checks: wave side-to-side with an open hand; bring two visible hands together after starting apart; point with index extended and the other fingers curled; hold a fist; hold both wrists in the upper 42% of the camera image; and hold a steady open palm for about one second.
5. Send the Console output, which gestures were detected/missed, approximate camera framing, and whether the camera mirrored the preview. Do not interpret these defaults as final acceptance thresholds.

## Verification status

- Static source/API inspection: complete. Runner result event path, normalized landmark fields, handedness categories, and two-hand sample configuration were checked against the installed 0.16.3 source.
- C# compilation: not performed. `dotnet` is present but no .NET SDK or C# compiler is installed; the expected Unity Editor managed compiler path is absent.
- Unity Editor/Play Mode, camera initialization, and real gesture recognition: **Not verified — user testing required.**
