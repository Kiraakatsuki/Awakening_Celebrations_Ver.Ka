# Proposed bounded Stage 3.1 retest — NOT AUTHORIZED / NOT RUN

The Stage 3.1 prompt explicitly says the prior Play Mode authorization is exhausted. This plan requests a **new maximum ten-minute Editor Play Mode pass**, candidate scene only. Do not enter Play Mode until the user explicitly approves it. No build, profiling, camera consent, capture pipeline, live assistant request or deferred business operation is included.

## Preflight outside Play Mode

1. Record Git/Console and protected hashes. Require the saved, clean `Assets/Scenes/AwakeningLivingGlass.unity`; preserve the original scene and build settings.
2. Inspect and explicitly arm `LivingGlassBoundedValidation.Prepare()` only after authorization. It deactivates the MediaPipe runner and blanks the backend URL in memory, keeps consent denied, blocks consent/Send/suggestions and sets a ten-minute stop deadline. Never save those overrides.
3. Confirm endpoint empty, request absent, runner inactive/not started, gate/session consent false. If isolation cannot be established, do not enter Play Mode.
4. Keep the Game view visible at 1920×1080. Capture goes to a unique `RuntimeRetest/<UTC>-<id>/` directory; never overwrite the previous bounded evidence. A capture waiting five seconds for a rendered frame must be rejected.

## Ten-minute maximum, after entering Play Mode

| Window | Exact checks | Required observation |
| --- | --- | --- |
| 0–1 min | Home English/TC, initial entrance, privacy text and mixed Latin/CJK language control | No clipping/markup, neutral focus, denied-camera state; compare actual Game view with reference and static preview |
| 1–3 min | Use labelled Gallery readiness fixture; target all six, direct theme changes, physical hover/Tab/Shift+Tab/Enter/click | Fixed six hit regions; same bounded optical pose and themes; no selection before confirmation; pointer/keyboard behavior remains correct |
| 3–5 min | Gallery→Focus for six identities, both locales; sample approximately20/120/280/480/750ms; confirm during entrance; interrupt by Back, overlay and locale replacement | One portrait owner; old copy gone before new copy appears; no duplicated content ID; no residue or hidden input target; 650ms intent preserved |
| 5–7 min | Settings and AI chooser/conversation open/close/reversal/minimize; native Tab into scroll region, arrows/PageUp/PageDown/Home/End, wheel/track/thumb/repeat arrows; local long TC/Latin text specimen | End content reachable, visible neutral scrollbar/focus, viewport contained; fields retain editing keys; no live requests; modal blocks underlying page and restores opener |
| 7–8 min | Reduced-motion + opaque review override; one selection/return and overlay cycle; revert override | Same functional commands, immediate stable poses, no spatial proxy, readable neutral material |
| 8–9 min | One real local GAIA video completion and a decoded replay cancellation, reached via existing UI commands | Actual decoder/loop event, ordinary Release; Back stops playback and releases owned texture. Do not simulate completion or retune timing |
| 9–10 min | Repeated settled Home/Gallery/Settings end-of-frame captures, paired with native Game view observations; intentionally request a capture then replace route once | Accepted pairs match route/locale/target/frame and all visible chrome; changed presentation rejected without a PNG success. Stop and restore at deadline |

The fixtures are not gesture recognition. Where time is insufficient, mark cases NOT RUN and stop at the deadline; do not extend into release testing. All-six decoding from the prior pass remains historical evidence, not a new all-six runtime result.

## Capture acceptance and diagnostics

- Metadata and `CaptureScreenshotAsTexture` pixels must be collected at the same rendered end-of-frame; record requested and captured frame/time. No asynchronous PNG capture paired with earlier state.
- Test settled scenes separately from transition samples. Compare visible header, dock, status and overlay against native inspection; geometry alone cannot establish visual fidelity. Native desktop scaling may prevent a pixel-for-pixel comparison; record that limitation.
- Check readability, dimensions1920×1080, texture disposal and restoration of `RenderTexture.active`. Reject route/locale/target/generation/overlay changes, timeout or wrong dimensions. Do not create a replacement video/camera pipeline.
- If chrome still differs, retain both observations and investigate the renderer/capture path separately. Do not declare the issue fixed from the timing correction alone.

## Stop and record

Stop immediately if isolation fails, an application exception appears or a stuck transition prevents safe navigation. On normal stop, discard unsaved overrides, restore locale/Game view and verify scene clean/Play Mode false. Compare protected hashes, record Console changes and update each acceptance item PASS/FAIL/BLOCKED/NOT RUN. This pass cannot authorize production cutover, legacy cleanup, live service/hardware work or a60fps claim.
