# Gesture Recognition and Scene Consolidation Report

Date: 2026-11-05

## Original architecture

- Primary application scene: `Assets/Scenes/SampleScene.unity`.
- Hand-recognition sample scene: `Assets/Samples/MediaPipe Unity Plugin/0.16.3/Official Solutions/Scenes/Hand Landmark Detection/Hand Landmark Detection.unity`.
- Build Settings already contained only `SampleScene.unity`. No application script referenced or loaded the hand-recognition scene, and no additive scene load was present. The second scene was a separate sample/editor scene, not an active build dependency.
- The hand-recognition scene contained one `HandLandmarkerRunner`, one `MediaPipeGestureBridge`, a MediaPipe bootstrap prefab, its own camera and sample preview/annotation UI. The main scene had one camera and no tracking components. The sample's nested annotation prefab dependency is unresolved in the project; its UI is not needed for recognition.
- Scene inventory: the primary scene roots include `HomeUI_ZH`, `HomeUI_EN`, `Main Camera`, `Global Light 2D`, `UIDocument`, and `HomeLanguageController`/`AwakeningApplicationController`. The hand scene roots include `EventSystem` (EventSystem and StandaloneInputModule), `Solution` (runner and bridge), `Directional Light`, and `Main Camera`, plus a sample Canvas/Screen prefab. The preview annotation components are `Screen`, `HandLandmarkerResultAnnotationController`, and `MultiHandLandmarkListAnnotation` on/from nested sample prefab references; that preview prefab dependency is unresolved in this checkout. The runner and bridge are the only sample scene components migrated.
- The runner's sample preview and annotation references were hard requirements in its frame loop, so simply moving it would fail when the sample UI was absent. The `AppSettings` asset selected `LocalResourceManager`, which is supported only in the Editor; player startup would stop before image-source initialization. Camera access itself was already consent-gated through `GestureCameraConsentGate`.

## Changes implemented

- Added one `GestureRecognitionRig` root to `SampleScene.unity`, with the existing MediaPipe `HandLandmarkerRunner` and `MediaPipeGestureBridge` components and their recognition settings. It uses a hand-specific bootstrap prefab and does not add a second camera, Canvas, sample UI, or annotation prefab.
- Serialized the rig's `MediaPipeGestureBridge` directly on `AwakeningApplicationController`. The controller subscribes once on enable and unsubscribes on disable; it does not search across scenes.
- Made preview and annotation drawing optional in `HandLandmarkerRunner`, allowing the application-scene pipeline to run headlessly while continuing to publish landmark results.
- Added a hand-landmarker-specific Bootstrap prefab and AppSettings asset configured for StreamingAssets, leaving the shared sample AppSettings unchanged. Copied the existing hand_landmarker.bytes model to Assets/StreamingAssets/hand_landmarker.bytes so this runner's configured model path resolves in a player build.
- Retained the original hand-recognition scene asset for rollback/reference. It is not in Build Settings and no runtime script loads it. Build Settings already pointed only to the primary application scene, so no change was needed there.
- Preserved the consent gate and existing gesture classifier/controller paths. Point remains a target operation; Fist confirms only a valid target; raised hands and held open palm remain mapped through the existing application state guards. `Back()` is gated until the awakening sequence is complete and applies post-awakening navigation restrictions.

## Consolidated architecture

`SampleScene` owns the application controller and the single gesture rig. The controller starts with camera consent denied. `GestureCameraConsentGate` is the session static gate and single-runner registry; it notifies the registered runner on consent changes. No scene objects were already marked `DontDestroyOnLoad`; the runner creates the MediaPipe Bootstrap prefab on demand and marks it `DontDestroyOnLoad`. The runner waits for consent and bootstrap readiness before starting the camera and hand-landmarker pipeline. The runner emits results; `MediaPipeGestureBridge` copies them and evaluates the recognition layer on Unity's main thread; the application controller receives semantic gestures and pointing/tracking state through the serialized bridge. Revocation, component disable, or scene shutdown stops the runner; its texture-frame pool is disposed. Preview is independently optional and remains hidden unless requested.

The existing pipeline owns the webcam and MediaPipe lifecycle through the runner and consent gate; the migration does not add another webcam manager or tracking pipeline. Existing application events are represented by the project's `SemanticGesture`/bridge callbacks and controller commands rather than a separate public event bus, so no replacement event signatures were introduced.

## Validation results

### Static inspection

- Confirmed the primary scene contains exactly one `HandLandmarkerRunner`, one `MediaPipeGestureBridge`, and one application camera; the rig is a scene root and the controller points to its bridge.
- Confirmed the primary scene remains the only enabled Build Settings scene and source code has no load/access references to the hand-recognition scene.
- Confirmed the moved rig uses the existing bootstrap prefab, has null optional preview/annotation references, and has no reference to the unresolved sample annotation UI.
- Confirmed the packaged hand model exists at the exact model path used by `HandLandmarkDetectionConfig` and the hand-specific AppSettings selects StreamingAssets while the shared sample AppSettings remains unchanged.
- Confirmed the controller's consent and Back guards remain in place. `git diff --check` completed without errors. The primary scene has no duplicate local fileIDs, exactly one runner/bridge/main camera, one root for the rig, and a valid serialized bridge reference.

### Compilation and runtime

- C# compilation and Unity Play Mode were unavailable during this migration; the current workspace session has no connected Unity Editor control surface. No final compile is claimed.
- No actual webcam, permission prompt, camera failure/recovery, application restart, or real gesture run was performed.
- A previous Unity Editor session ran deterministic synthetic classifier fixtures for all six gesture categories (positive and negative cases); those were classifier/state fixtures, not end-to-end camera or scene tests, and the final consolidated scene was not part of that run.

## Regression assessment and remaining limitations

The migration leaves the application UI, Figma assets, English/Traditional Chinese content, navigation, assistant backend, and keyboard/mouse input untouched. The gesture classifier and existing controller state gates remain in place. Static inspection supports preservation, but those experiences and the six gestures have not been retested end-to-end in the consolidated scene. Webcam orientation, mirror behavior, device availability, model initialization, consent acceptance/revocation, shutdown, tracking loss, and recognition quality still require Unity Editor/player and camera validation.

## Final status

**IMPLEMENTED — RUNTIME VALIDATION PENDING**

## Phase 2 — Runtime Integration and Gesture Regression Validation

Date: 2026-11-05

### A. Compilation

- Unity Editor: **Unity 6.4 (6000.4.7f1), Windows, DX12**.
- The Editor compiled the project scripts successfully after the runner, bootstrap lifecycle, and diagnostics changes. The latest recorded Tundra build completed successfully (1.80 seconds; 6 items updated; 1826 evaluated). No C# compilation errors were reported.
- No automated Unity test suite or test assembly was present in the project, so no automated tests were run in this phase. The prior report records synthetic classifier fixtures; those are not live-camera tests.
- Existing warnings remain: obsolete `Resolution.refreshRate` uses in MediaPipe `ImageSource.cs`, obsolete `GetInstanceID()` uses in MediaPipe `GraphRunner.cs`, and obsolete object-search calls in `AwakeningApplicationController.cs`. Unity also reports duplicate `System.Runtime.CompilerServices.Unsafe.dll` versions and uses the Collections test package's version. These did not prevent compilation; unrelated package upgrades were not made.
- The Editor initially crashed when a script-domain reload re-enabled the persistent MediaPipe Bootstrap and called native `InitGoogleLogging()` a second time. `Bootstrap` now shuts down GPU/glog/protobuf resources on Play Mode disable as well as application quit, with an initialization guard. Subsequent clean Play Mode stop/restart cycles completed without the duplicate-glog crash. Hot-reloading scripts while the camera is actively running was not repeated after this fix.

### B. Model Initialization

- Model: `Assets/StreamingAssets/hand_landmarker.bytes` (7,819,041 bytes; non-zero).
- Hand-specific Bootstrap configuration uses `StreamingAssetsResourceManager`. On Windows Editor, the model path resolved and the logs confirmed asset preparation, successful `HandLandmarker.CreateFromOptions`, and CPU/TFLite initialization.
- Windows selected CPU inference because GPU inference is unsupported in this Editor configuration. Android, iOS, WebGL and standalone player packaging were not built or tested; their StreamingAssets extraction/access behavior remains unverified here.

### C. Camera

- The app consent checkbox and privacy screen were exercised. Before consent, the scene had no runtime Bootstrap object. Declining consent returned to Home with the consent checkbox cleared and did not start the pipeline. Accepting consent did not start capture during Home or the tutorial; capture began after entering Attract.
- Play Mode reported `USB2.0 HD UVC WebCam`, 1280×720, requested 30 FPS, rotation 0°, mirrored. The webcam texture became prepared. Actual delivered FPS was not measured; MediaPipe only exposes the requested frame rate through the current source abstraction.
- Windows Editor did not show an OS camera permission prompt. The app-level consent gate was observed. Revoking consent through an in-experience UI while the camera was active could not be exercised: the current Attract screen has no route to the privacy controls. The runner's false-consent stop path and gate unregistration were inspected, and active-pipeline shutdown was validated by stopping Play Mode.

### D. MediaPipe Tracking

- The task initialized and the live-stream result callback was exercised. The latest run logged `MediaPipe first hand-landmark result received: 0 hand(s).` This confirms a current no-hand result was delivered; it does not establish recognition accuracy with hands present.
- Camera orientation diagnostics reported rotation 0° and mirrored input. The app keeps the sample preview hidden. No deliberate one-hand, two-hand, occlusion, lighting, distance, or reacquisition scenario was tested with a visible live preview.
- An earlier runtime log recorded tracking available/lost transitions, but this was not a controlled hand-in-frame regression. Stale-landmark and tracking-loss protections remain in the bridge/classifier and were reviewed statically; no sustained gesture was used to test them with a person in view.
- The bridge's no-result warning previously started its timer when the bridge was enabled, so it could warn during the tutorial before capture began. It now starts timing only after the runner and camera source are ready. The first-result callback emits one low-volume diagnostic with the detected hand count.

### E. Gesture Regression

Results below distinguish classifier-level fixtures recorded in the Phase 1 history from live camera validation in this phase. No live gesture was performed for this phase.

| Gesture | Result | Evidence and limitation |
|---|---|---|
| Wave | PARTIALLY VERIFIED | Classifier fixtures were recorded in Phase 1; live motion-versus-stationary-wave behavior was not tested. |
| Clap | PARTIALLY VERIFIED | Classifier fixtures were recorded in Phase 1; hands-approaching, contact, and sustained-contact behavior were not tested live. |
| Point | PARTIALLY VERIFIED | Point/target classification and separate confirmation path remain in code; no live performer target was tested. |
| Fist | PARTIALLY VERIFIED | Valid-target guard remains in the controller; no live confirmation or no-target rejection was tested. |
| Raise Both Hands | PARTIALLY VERIFIED | Two-hand classifier and selected-performer dispatch remain in code; no live one-hand/two-hand comparison was tested. |
| Open Palm + Hold | PARTIALLY VERIFIED | Hold timing and awakening/back guards remain in code; no live duration or completed-awakening test was performed. |

### F. Application Integration

- The existing consent/privacy screen, tutorial sequence, keyboard Return progression, mouse controls, and Attract screen ran in Play Mode. The serialized application-controller-to-bridge reference connected successfully.
- The camera remained gated until the accepted user journey reached Attract. Decline did not instantiate the Bootstrap. Gesture callbacks were not used to drive application state in this phase, so semantic event delivery to each experience controller remains unverified at runtime.
- Existing event signatures and state checks were not changed. Point and Fist remain separate semantic operations; the controller still rejects confirmation without a valid target and guards Back until the awakening sequence is complete. Static inspection only.
- No regression was observed in the tested consent/tutorial/Attract path or keyboard/mouse navigation. Settings, performer variants, assistant overlay, localization, Closing, and Souvenir were not retested.

### G. Resource and Performance Assessment

- Two clean Play Mode start/stop cycles after the Bootstrap fix each initialized the StreamingAssets model, the Hand Landmarker task, and the same webcam. Logs confirmed native resource shutdown on each stop; the following start succeeded without a duplicate Bootstrap or duplicate camera owner.
- The runner still uses one reusable ten-frame texture pool and disposes it on stop or guarded pipeline failure. A guarded nested-coroutine wrapper now catches model/camera/inference exceptions, logs the failure, stops the image source/task, disposes the pool, and clears the started flags so a consent toggle can retry. Bootstrap initialization now has a 60-second readiness timeout.
- The one-time camera-ready diagnostic reports source name, dimensions, requested FPS, rotation and mirror state. The first landmark result reports hand count. No per-frame raw image logging was added. No profiler capture, allocation benchmark, long-duration stress test, camera-disconnect test, or forced model/inference failure test was run.
- `MediaPipeGestureBridge` copies MediaPipe's reused result data into fresh hand/landmark arrays per callback so the main-thread recognizer never retains runner-owned buffers. This is a bounded per-result allocation hotspot; it was not changed without profiling evidence and remains a performance measurement item.

### H. Outstanding Issues

- Actual live gesture behavior for all six gestures, false-positive cases, tracking loss/reacquisition with a visible hand, and gesture-to-controller state transitions still need a controlled in-person test.
- Active-session consent revocation was not reachable from the tested Attract UI; the code path was statically checked, and Play Mode shutdown was exercised instead.
- Standalone player builds and non-Windows platform camera/model loading were not tested. Actual delivered FPS and performance under prolonged operation remain unmeasured.
- Editor hot-reload while the recognition pipeline is actively running was the trigger for the earlier glog double-initialization crash. Clean Play Mode shutdown/restart now passes, but that exact hot-reload sequence was not re-run after the shutdown change.
- The Editor-only Unity AI Assistant `NoSubscription` message appeared in its status area; it is unrelated to the recognition pipeline. Existing compiler/package warnings are listed above.

### I. Final Status

**PARTIALLY VERIFIED** — Unity compilation, consent gating, model loading, camera readiness, first no-hand landmark callback, and two clean Play Mode shutdown/restart cycles were observed. The six live gesture regressions and controlled tracking scenarios remain untested.
