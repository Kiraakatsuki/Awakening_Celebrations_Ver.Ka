# Current repository status — Stage 3.4

The legacy demo has been retired after a verified, separate archival Git snapshot. **AwakeningLivingGlass is now the sole serialized development startup scene.** Read the [retirement report](Cleanup/FinalLegacyRetirement/Report.md), [active architecture](Cleanup/FinalLegacyRetirement/Architecture.md), [exact removal ledger](Cleanup/FinalLegacyRetirement/RemovalLedger.csv) and [current validation](Cleanup/FinalLegacyRetirement/Validation.json).

Current checks: Unity compilation and34 static regression checks PASS;68 isolated conversation cases and isolated transition/scroll checks PASS. Strict raw GUID resolution remains FAIL for the unchanged five classified support/recovery issues. No Play Mode, camera, application assistant request, Player build or profiling was performed in Stage3.4. Development-entry cutover is complete; operational/release acceptance remains pending.

`python -X utf8 Documentation/Stage3LivingGlass/verify_static.py` runs the current gate using fresh native snapshots. Its nonzero exit preserves the known raw GUID failures. Earlier24-check output is archived in `Cleanup/FinalLegacyRetirement/HistoricalChecks/`. The one-time demo-cloning scene builder and legacy Figma mapping validator are retired. Current safe Editor commands are **Validate primary scene (no runtime)**, **Run pure contract checks** and **Static visual review**.

The material below is the historical parallel-implementation handoff, preserved for design and validation provenance. Its rollback/build-entry/compatibility statements are superseded by Stage3.4.

---

# Stage 3 — Living Glass parallel implementation candidate

**Latest maintenance — Stage3.3B:** [Legacy reference retirement](Cleanup/LegacyRetirementReport.md) removed15 obsolete occurrences while preserving both scenes/build configuration and all runtime C#. Raw unresolved GUIDs decreased14→5; four support issues remain. No whole asset was safely removable. Static regressions pass; final operational acceptance and production cutover remain unapproved and unverified.

**Stage3.3 cleanup:** [Dependency inventory, scoped changes and validation](Cleanup/CleanupReport.md) are recorded. Two small code refactors passed compilation and isolated Editor checks; no files were proven eligible for deletion. Both scenes/build settings remain preserved. Fourteen pre-existing unresolved GUIDs prevent a clean-reference acceptance claim; runtime validation and cutover remain pending. Earlier implementation/validation summaries below are historical and must be read with the latest scoped reports.

**Conversation refinement:** Reflect & Converse now has separate interaction/artwork zones, a larger transcript, fixed composer and quiet Closing progression. [Review the change](ConversationRefinement/Report.md) and [static previews](ConversationRefinement/Review.html). The new family passes compilation/Editor checks; its runtime and IME validation remains pending separate authorization.

**Latest: authorized Stage3.1 runtime retest completed.** Unity is stopped. See the [updated acceptance matrix](ValidationAndAcceptance.md) and [actual runtime evidence](Stage31Stabilization/RuntimeRetest/20261113T122333804-557ac1/Review.html). Ghosting/scrolling/readback corrections passed bounded observations; skipped fixture commands, unverified replay cancellation and remaining visual differences are recorded. Older NOT RUN statements below are historical.

**Stage3.1 update:** scoped ghosting/typography/scrollbar/capture-helper corrections are compiled and pass isolated Editor checks. Open the [correction report](Stage31Stabilization/Report.md), [static visual references](Stage31Stabilization/VisualReview.html) and [proposed retest](Stage31Stabilization/RetestPlan.md). New Play Mode verification is NOT RUN; fresh authorization is required. The bounded runtime results below describe the earlier candidate.

Open the latest [bounded Play Mode report](ValidationAndAcceptance.md), [runtime visual review](BoundedValidation/Review.html), [runtime six-theme sheet](BoundedValidation/RuntimeSixThemes.jpg), or [actual motion samples](BoundedValidation/MotionFrames.jpg). The authorized offline, camera-denied pass is complete; Play Mode is stopped. Visual acceptance remains open, including transition ghosting and screenshot readback discrepancies.

The earlier [static visual review](Review.html), [bilingual overview](BilingualOverview.png), [six-theme sheet](SixThemes.png), and [Arc comparison](GalleryComparison.png) remain historical **static Unity Editor views**, not runtime or performance evidence.

The new scene is **`Assets/Scenes/AwakeningLivingGlass.unity`**. Production build entry remains SampleScene. No original scene, active legacy UXML/USS, media importer or package dependency was replaced. This is an implemented and compiled candidate; it is **not declared complete or accepted**. The bounded runtime pass verified real local media for all six and representative UI operations, while hardware/service parity and final visual refinement remain open.

## Implementation coverage

One persistent UI Toolkit document renders15 shared families covering the current104 localized records /52 logical routes: Home, Privacy, all tutorial substeps and return route, Attract, Color Release / Gallery Arc, six performers×five chapter stages, Closing, Settings, assistant chooser, six assistant conversations and the unavailable Souvenir surface. The Gallery Arc retains Far/Mid/Near/Near/Mid/Far art hierarchy and stable interaction wrappers. Performer artwork, names, palette data and video ordering are preserved.

The new presentation supplies English/Traditional Chinese copy, gesture illustrations, pointer/keyboard actions, identification/confirmation, contextual Help, privacy/camera status, named video surface, assistant input/suggestions/loading/error/retry/cancel bindings, window entry/close/minimize/resume, focus scopes and reduced-motion/effects configuration. Settings includes truthful unavailable surfaces for sound preferences, saved accessibility preferences, calibration and full Restart/End Experience. No deferred business service, QR code or fake successful setting was added.

The new code and assets are under `Assets/LivingGlass/`: `Core`, `Integration`, `Presentation`, `Motion`, `Themes`, `Rendering`, `UI/Components`, `UI/Styles`, `Assets/Fonts` and `Editor`. The semantic port joins the existing single flow/video/assistant authority; the new view has no imported Figma path bindings. See [architecture](Architecture.md), [classification](CodeClassification.csv) and [exact shared changes/wiring](SceneAndSharedChanges.md).

## Evidence

- **PASS:** Unity6000.4.7f1 compilation with no introduced C# errors; existing warnings and plugin diagnostics are recorded.
- **PASS:**24 repository checks, including byte-preserved baseline scene/build/packages, protected method-body preservation, unchanged recognizer/runner logic except read-only failure diagnostics, font coverage and media ordering. See [static report](StaticVerification.json).
- **PASS:** compiled pure route/motion checks:104 records,52 paired routes, all30 chapters, rejected unknown routes, interruption/cancellation and reduced final pose. These are inspected Editor-only checks, not runtime tests. See [results](PureContractChecks.txt).
- **PASS, limited:** static Editor reference/ownership validation and42 representative rendered views; ten gallery layout samples confirm nonoverlapping wrappers and vertically contained names across locales, six targets and reduced modes. See [serialized report](SerializedValidation.txt), [geometry](EditorGeometryChecks.txt) and [visual manifest](VisualManifest.json).
- **PASS, bounded runtime:** all-six local video decoding and real completion, GAIA replay cancellation, six target-command themes, representative native pointer/keyboard operations, window motion/interruption, bilingual presentation and reduced/opaque Gallery. Keyboard and backdrop-focus defects were corrected and retested.
- **FAIL / BLOCKED / NOT RUN:** transient Gallery-to-Focus portrait/text ghosting, scrollbar/type fidelity, inconsistent screenshot readback, full visitor/original-scene parity, actual camera/gestures, live AI, IME, Player builds and performance. See the complete [acceptance matrix](ValidationAndAcceptance.md).

The original static package used no Play Mode. The subsequent explicitly authorized validation used Editor Play Mode with the capture rig inactive, consent denied and assistant backend isolated. No camera activation or live assistant request occurred. There is no measured60fps claim. The hardware target remains the documented ROG Strix G16 / RTX4060 Laptop8GB / RAM16GB,1920×1080 at steady60fps.

## Visual fidelity and limits

The approved Gallery Arc and six palettes are implemented with native UI Toolkit composition. Small 2D transforms, crop parallax, a single48-triangle vertex-color atmosphere and tinted reading surfaces form the baseline. CSS perspective pitch/yaw and true backdrop blur/refraction are not claimed. The comparison identifies a supported optical fallback rather than recreating an unvalidated 3D camera system. Source Serif4 is used for English display; bundled Noto Sans CJK TC is the readable Traditional Chinese baseline. The gallery heading is adjusted for Unity font metrics to avoid overlapping the art. Performer thematic copy lives on the identification panel, preserving the reference's restrained name-only captions.

See [motion/rendering contracts](MotionAndRendering.md), [coverage](ScreenCoverage.csv), [rollback and cutover gates](RollbackAndCutover.md) and the recoverable source checkpoints under `Checkpoints/`. Remaining service extraction is explicitly recorded; it was not disguised as completed architectural migration.

## Review without runtime

Open the new scene in Edit Mode. Use **Awakening Celebrations → Living Glass → Static visual review** for the isolated native preview. It does not initialize flow, capture, media or network services. The UI is composed at runtime in the actual scene, so an inactive Edit Mode Game view is not an operational preview.

Safe inspected menu checks are **Validate serialized scene (no runtime)** and **Run pure contract checks**. The one-time scene creation command refuses to overwrite an existing candidate or save a dirty user scene. Do not rerun it to replace this scene. The capture command is a developer helper that captures the preview's desktop rectangle; bring the preview forward and reject any occluded images. The final review set excludes the rejected provisional captures.

Stage3 checkpoints: A baseline/classification recorded; B scene/shared presentation created; C all shared families implemented; D service/semantic wiring present but operational verification pending; E bounded motion/material baseline implemented with documented optical compromise; F static/pure/Editor checks passed, operational tests blocked; G candidate handed off without cutover or cleanup.

The bounded Play Mode pass is now complete; the latest matrix supersedes historical operational-blocked statements above. Camera capture, live assistant requests, Windows builds and extended profiling remain separately scoped. Cutover and destructive cleanup must wait for actual parity, final visual approval and performance evidence. No automatic follow-on work was started.
