# Validation and preservation record

13 November 2026 · static checks and native Figma evidence only.

The current [spatial addendum validation](SpatialAddendum/ValidationReport.md) adds34 Arc-derived boards, unchanged native wrapper bounds and14 pure-model checks. Earlier results below apply to the original Stage2B package. Browser verification is still blocked; no previous screenshot or timing result is reused as new spatial browser proof.

## What was completed

- Created the isolated Living Glass HTML/CSS/JS prototype, using existing approved artwork/fonts and no dependency install. JavaScript syntax checks pass. CSP blocks network connections and media sources. No camera/service API is connected.
- Created 43 editable native 1920×1080 Figma boards on a new Stage 2B page. Text, artwork and component instances are separate layers; image hashes are limited to the six previously approved source artworks. Native TC font availability and final family names were read back.
- Inspected native Arc and Traditional Chinese Settings renders; corrected outer portrait-name clipping by replacing ineffective instance child sizing with reusable Far/Mid/Near masters. Readback confirms 340/370/400px artwork in 464px wrappers. Refined native assistant chooser to visibly include all six contexts.
- Exported all 43 native boards to local PNGs and produced comparison/overview sheets. `ExportLedger.json`, `NativeValidation.json` and `NativeRefinements.json` record provenance and dimensions. Contact-sheet visual review is distinct from browser execution.
- Contact-sheet review found that Salon's fixed-height auto-layout rows clipped its art into shallow strips. Both native locale boards now use 210px rows/cells and a 350px gallery start; exports were refreshed. `SalonRefinement.json` records the fix. The browser layout uses different CSS grid sizing and has not been visually verified.
- Refined GAIA's narrow crop to retain the face, using source-image placement only. The native Arc post-fix render was inspected; affected static exports were refreshed. The prototype specifies the matching right anchor but remains unverified. No source artwork pixels changed.
- Authored three separate native Figma motion timelines: 6s gallery/overlays, 6s reduced-motion version, 14s GAIA chapter event storyboard. Static boards remain intact. Native motion-only copies detach presence instances because the Plugin API cannot animate instance descendants; production components are not detached or changed. `MotionLedger.json` records affected IDs.
- Preserved all 104 route/locale records across 52 logical routes and 15 families, with 53 paired deferred-state entries and an additional Living Glass copy inventory. Checks and local-link results are in `PackageVerification.json`.
- Confirmed no tracked production diff in Assets, Packages, ProjectSettings or the backend. Source manifests document guidance, Stage 1/2A context and inspected runtime/configuration inputs. No production scene, UXML/USS, C#, runtime artwork, package, importer or MediaPipe settings changed.

## Browser verification blocked

The in-app browser declined opening `http://127.0.0.1:8767/LivingGlass/prototype/`. The user explicitly approved verification; retry still returned a saved permission or managed-policy denial. After the user reported changing the setting, the same origin was retried once and was still denied. No alternative port/browser, raw browser protocol or indirect execution was used to bypass that gate.

Therefore hover/keyboard targeting, interruption, minimize/restore, focus trapping/restoration, family layout/locale wrapping, reduced motion and actual browser animation remain **not run** for Stage 2B. The code includes those behaviors; that is not evidence that they pass. Stage 2A browser results apply to the earlier prototype only and are not reused as Stage 2B passes. The local server may be available for the user's own review; a server response is not visual verification.

## Native motion limits

The native timelines contain actual editable keyframes and documented durations, not a slideshow labelled as browser footage. `MotionValidation.json` records readback: 9 animated nodes in the standard 6s study, 3 in reduced mode, 5 chapter layers across14s. No local ffmpeg/frame extractor was available, so no server video render/export was requested and no sampled-motion visual sign-off is claimed. The timeline may be reviewed in Figma. The chapter is an authored sequence of **still artwork** and review events; it does not play video or demonstrate real first-frame/completion callbacks.

## Pending acceptance

Browser QA remains blocked as recorded in `design-qa.md`. Unity rendering/input/accessibility/media/performance verification, all-frame contrast, TC editorial review, Windows DPI/IME/screen reader and exhibition comfort remain pending. No Play Mode, Edit Mode runner, Player, camera test, live assistant, video decode or profiling session ran. FHD60 remains a target, not a measured guarantee. Production implementation should wait for visual/policy approval and an explicitly authorized implementation/validation task.
