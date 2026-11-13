# Stage 3.1 — scoped visual stabilization

13 November 2026. Corrections are compiled and checked in isolation in Edit Mode. **A new Play Mode retest is NOT RUN and requires fresh authorization.** The original bounded runtime failures remain historical evidence; they are not relabelled as runtime passes. No original scene, production build entry, media importer, camera/gesture service, backend, package or deferred business operation was changed.

Review [static visual references](VisualReview.html), [Editor model/style results](EditorChecks.txt), [preservation checks](PreservationChecks.json), [source checkpoints](Before/), and the [exact proposed retest](RetestPlan.md). The previous [actual runtime motion evidence](../BoundedValidation/MotionFrames.jpg) remains intact.

## Root causes and corrections

| Defect | Evidence and root cause | Scoped correction | Current acceptance |
| --- | --- | --- | --- |
| LGV-05 portrait ghosting | Confirmed in previous runtime frames and current BuildBridge: proxy remained visible while destination art was faded in as part of the whole Focus content | Destination portrait uses visibility:hidden, retaining layout/UV calculation. One opaque non-picking proxy owns the portrait until its final650ms handoff; then the destination becomes visible and the proxy is removed in the same callback | PASS — isolated view ownership checks for all six×both locales; NOT RUN — corrected Game view transition |
| LGV-05 doubled copy | Old gallery opacity was1−t while new whole-page opacity was clamp((t−.35)/.65). Both were positive over part of the shared eased timeline; the two content roots also shared the ID content | Distinct outgoing decoration names, recursive non-picking/non-focusable subtree, exclusive copy windows; current Focus is the sole content page. Generation checks, explicit proxy cancellation and owned-element cleanup prevent a stale completion touching replacement content | PASS —1001 envelope samples plus12 integrated Editor tree scenarios; NOT RUN — real input during corrected transitions |
| Interrupted entrance brightness | New Editor scenario exposed ClearBridge resetting the current page opacity to1 during teardown, before it became outgoing decoration | Sample outgoing opacity **before** ClearRoute and pass it into the bridge. The fade starts from that captured opacity. The original failing case is retained in InterruptedEntranceFailure.txt | PASS — partial-opacity entrance case and subsequent cleanup checked for12 variants; NOT RUN — operational reproduction |
| LGV-06 scrollbar | Native ScrollView existed and overflow was reachable; child Scroller/Slider/RepeatButton nodes inherited the default bright track/thumb/arrows. Styling only the outer window did not theme these nodes | Supported USS selectors theme track, thumb, border, focus, hover, pressed and disabled states. Keep native visible vertical scroller, drag/wheel/repeat behavior, explicit ↑/↓ labels/tooltips. Shared focusable reading-scroll factory handles arrows/PageUp/PageDown/Home/End only when that scroll region owns keyboard input | PASS — computed dark styles, contained viewport and focused End/Home Editor event checks; NOT RUN — native runtime pointer/wheel/keyboard trials |
| LGV-07 typography | Families were already Source Serif4 / Source Sans3, but default font leading did not encode the Figma112%/130% display rhythm; labels added synthetic targeted bold, tracked wordmark/eyebrow too widely and forced the entire English mixed-language control to the TC face | Shared USS size tokens, explicit regular weight, neutral button centering, reduced tracking, consistent Latin identity/brand face, existing TC fallback for English mixed text. Immutable authored display/body/guidance copy gets supported em-based line leading; editable/service text remains plain | PASS — Editor EN/TC layout inspection and long mixed-script specimen; NOT RUN — Game view pixel fidelity, IME and physical readability |
| LGV-08 state/pixel mismatch | Confirmed helper defect: State() wrote metadata immediately, then asynchronous CaptureScreenshot ran later. Capture31 changed routes in the same command, producing Home metadata with Gallery pixels. Due actions also ran backwards if several became overdue during an Editor stall | End-of-frame coroutine captures readable pixels and same-frame metadata together; compare request route/locale/target/generation/overlay and reject changed presentations. One capture at a time, chronological/FIFO due actions, timeout, dimensions check, finally disposal/restoration, unique per-session evidence directory | PASS — compiled/source inspection; NOT RUN — capture repeatability in Play Mode |
| LGV-08 missing chrome | Several previous PNGs omitted chrome present in native inspection. Timing/repaint is plausible; available evidence does **not** establish whether all omissions are capture-only or renderer-specific | Use Unity's documented end-of-frame capture boundary, retain requested/captured frame/time and add header/transition visibility metadata. Preserve the application renderer and panel configuration | BLOCKED — needs paired native/Game view readback in separately authorized runtime pass; not declared resolved |

The old ghosting coefficients and source are preserved under Before/Assets/LivingGlass; the new integration checks exercise the actual new presentation builders, with flow=null. No application lifecycle, camera, player or HTTP service is initialized by those checks. The preview's manually advanced clock is **model testing**, not evidence of runtime fluidity or decoding.

## Transition ownership and timing

The route commits through the existing flow authority. A visual completion never navigates or triggers playback. The650ms portrait motion keeps the existing monotone easing; shared Motion.Run/Tick were not rewritten.

| Time within650ms | Outgoing text | New Focus copy | Portrait owner |
| --- | --- | --- | --- |
| 0–182ms | Fades from the sampled current opacity to0 | Hidden | Existing artwork proxy |
| 182–247ms | Hidden | Hidden | Proxy, with no duplicated text silhouette |
| 247–455ms | Hidden | Fades in, visibility enabled only above zero opacity | Proxy; destination art stays hidden |
| 455–650ms | Hidden | Fully visible | Proxy |
| Completion | Outgoing tree removed | Stable, normal opacity | Destination art visible; proxy removed |

The windows use real normalized time recovered from the existing quartic ease; the portrait still uses the original eased curve. Names in the outgoing tree are prefixed outgoing- so content/art queries cannot accidentally select stale elements. Every retained descendant ignores picking, has no focus stop and is disabled. Hidden destination copy is skipped by focus traversal. Clear/settle restores the owned destination visibility/opacity, cancels the proxy track and restores a valid default focus only when current focus is missing/invalid; shell or modal focus is not stolen.

Opening an overlay settles the visual bridge before panel motion. New routes/locales invalidate scheduled focus work by generation. Explicit settle, replacement and reduced mode leave no outgoing tree/proxy. Source and isolated model checks support these guarantees; their real runtime interruption behavior still needs the proposed pass.

## Visual precision preserved

| Contract | Result |
| --- | --- |
| Gallery geometry / artwork crop | PASS — existing BuildGallery, aperture layout, Art crop routine and six wrapper dimensions retained; no portrait asset changed |
| Controlled Spatial B targeting bounds | PASS — ApplyPose/TargetChanged bodies unchanged: aperture scale≤1.020, X≤4px, Y≤7px, parallax X≤3px/Y≤2px;240ms target/340ms return |
| Pitch/yaw | Existing optical fallback remains0°; within ±1°/±1.4° caps. No perspective/3D renderer was added or claimed |
| Six themes and lighting budget | PASS — palette/atmosphere data and ApplyTheme unchanged; no extra glow, capture camera or fullscreen pass |
| Glass / cinematic policy | Existing neutral tiers and reduced shell preserved; no blur/refraction assumption, offscreen renderer or duplicate video texture |
| Video and gesture semantics | PASS — protected source/configuration hashes and existing pure contracts unchanged; current runtime media/gesture regression NOT RUN |

The static artwork overscan remains1.035 to cover crop edges; it is not an additional animated hover scale. The approved cap applies to the aperture relative to rest. The650ms route expansion is distinct from hover targeting.

## Typography and component details

| Role | Current shared treatment | Remaining difference |
| --- | --- | --- |
| English display | Existing Source Serif4 Regular;1.12em leading; display72/gallery56/chapter60px values retained | Actual Game view weight/rasterization and baseline comparison pending |
| TC display | Existing bundled Noto Sans CJK TC Regular;1.3em leading;64/gallery54px retained | Figma's proposed Noto Serif TC is not bundled; this remains a visible family difference, not a completed serif match |
| Body/guidance | Existing Source Sans3 or TC face;1.5em authored copy leading; body25px | Dense service replies stay plain with their existing backend renderer. No untrusted text is enabled as rich markup |
| Status/control | Shared19/22px small/action tokens; regular weight; centered action text; existing36px consent line allowance retained | Screen-reader/IME/DPI/viewing-distance trials pending |
| Latin identities / brand | Source Sans3 even in TC; exact six names; wordmark1.5px tracking, eyebrow1px or0 in TC | No individual performer offsets introduced |
| Scroller | Native behavior and automatic visibility;24px track/repeat control,32px minimum thumb length,14px centered arrow type; neutral focus | Touch/exhibition pointer comfort and native focus/drag must be tested; scrollbars were not hidden |

The line-height tag is used only for authored immutable display/body/guidance strings. Dynamic identity/status text, assistant message rendering, editor contents and backend behavior are unchanged. Existing UI Toolkit has no IStyle.lineHeight property in this installed Editor; the supported rich-text em tag provides proportional leading without hard-coded per-label offsets or editing shared font assets.

## Evidence and checks

| Check | Status | Evidence |
| --- | --- | --- |
| Unity compilation and USS import | PASS | Editor import/domain reload and compiled checks completed; no new project C# or USS errors in Console |
| Repository static verification | PASS |24 checks, zero failures; StaticVerification.json |
| Additional protected/motion/theme checks | PASS |23 checks; protected scenes/build/packages/importers/shared modifications plus unchanged motion/crop/theme methods; PreservationChecks.json |
| Existing pure contracts | PASS |104 locale records/52 routes,30 chapters, interrupted motion, reduced pose and crop invariance; PureContractChecks.txt |
| Serialized scene validation | PASS | One flow/bridge/document, six video slots, catalog and required references; SerializedValidation.txt |
| Exclusive text envelope | PASS |1001 samples, no outgoing×incoming opacity overlap |
| Presentation tree scenarios | PASS | Six identities×two locales: early/final handoff, inert subtree, interrupted entrance, overlay, locale replacement, explicit settle and reduced mode; EditorChecks.txt |
| Scroll style / focused synthetic events | PASS | Computed arrow background #283747; overflow and viewport containment; End/Home event dispatch with explicit scroll focus. This is not native runtime keyboard evidence |
| Long Chinese / Latin specimen | PASS | Local18-paragraph specimen, plain text, scroll range987.2 in the scaled Editor preview; LongCopyCheck.txt. No assistant response was generated |
| Current Play Mode / camera / live AI / Player / profiling | NOT RUN | No new runtime authorization; final scene clean and Editor not playing |
| Corrected screenshot capture repeatability | NOT RUN | Requires rendered Game view; no Play Mode entered to test it |
| New runtime regressions | NOT RUN | None claimed absent; service behavior is preserved by source/contracts, not a new operational pass |

Existing Console warnings remain four obsolete API warnings and one unused-field warning in legacy source, plus the pre-existing Unity AI NoSubscription and intermittent MCP transport diagnostic. One in-memory diagnostic snippet initially omitted the UIElements extension namespace and was corrected; it created no project script. A synthetic End event initially targeted the wrong focus owner; that fixture was corrected to focus/target the scroll view and rerun. These are recorded separately from application compilation.

All previous bounded PNG/JSON/media results remain historical and untouched. Initial new desktop crop attempts were occluded by Codex; EditorPreviews/*.png are **rejected** and excluded from VisualReview.html. WindowCaptures contains the subsequently inspected unoccluded native static popup references. They use a1536×960 desktop surface with a1920×1080 logical canvas and are not full-HD Game view results. A system performance overlay in those desktop images is not an application performance measurement. Earlier Settings previews precede the final14px arrow-type adjustment; Settings-en-final-STATIC.png shows the final sizing.

## Exact corrected files

| File | Change |
| --- | --- |
| Assets/LivingGlass/Presentation/LivingGlassPresentation.cs | Exclusive bridge visibility/ownership, outgoing opacity sampled before teardown, unique inert outgoing IDs, generation cleanup/focus, shared scrollers and typography application |
| Assets/LivingGlass/Motion/LivingGlassMotion.cs | Added pure BridgeVisibility envelope; existing scheduler/easing/pose/crop algorithms unchanged |
| Assets/LivingGlass/UI/Styles/LivingGlass.uss | Shared type tokens/weight/tracking/alignment and native scroller child states |
| Assets/LivingGlass/Editor/LivingGlassBoundedValidation.cs | Same-frame rendered capture, consistency rejection, ordering/timeout/resource cleanup and fresh evidence output; never starts Play Mode automatically |
| Assets/LivingGlass/Editor/LivingGlassStabilizationChecks.cs (+meta) | New isolated Editor model/tree/style checks; flow always uninitialized, no services or test runner |

Four existing implementation files changed; the new checks are Editor-only. Before/ contains recoverable source/report checkpoints. No broad architecture, new package, destructive cleanup or production configuration change was made.

## Remaining acceptance

Corrected Game view transitions, native scrolling/keyboard/IME, corrected screenshot repeatability and pixel fidelity are **BLOCKED pending explicit authorization**. Camera/MediaPipe, live backend, full performance, Windows deployment and release/cutover gates remain outside this task. TC serif-display parity and enhanced glass/perspective remain documented differences; the established optical implementation was preserved.

Recommendation: approve only the [ten-minute offline, camera-denied retest](RetestPlan.md), with the same service interlocks. Stop at its deadline, record failures honestly, and do not infer release readiness from the current static/compilation results.

API references used for the bounded capture/leading correction: [Unity CaptureScreenshotAsTexture](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/ScreenCapture.CaptureScreenshotAsTexture.html), [WaitForEndOfFrame](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/WaitForEndOfFrame.html), and [supported UI Toolkit rich text tags](https://docs.unity3d.com/6000.0/Documentation/Manual/UIE-supported-tags.html). End-of-frame capture can wait indefinitely if Game view is not active in the Editor; the helper therefore rejects after five seconds rather than claiming success.
