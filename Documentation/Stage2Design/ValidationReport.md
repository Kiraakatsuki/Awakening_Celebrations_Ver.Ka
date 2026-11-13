# Design package validation

13 November 2026. This validates isolated design artifacts and static preservation evidence. It is not a Unity build, runtime regression result or performance measurement.

| Check | Evidence / result |
| --- | --- |
| Project guidance and Stage 1 reconciliation | `SourceManifest.json` records AGENTS.md, all nine project skills and ten requested Stage 1 documents; reconciliation identifies unapproved policy differences |
| Native Figma deliverable | 30 editable 1920×1080 frames, five views × three directions × two locales; `figma/ScreenLedger.json` and `figma/NativeValidation.json` record dimensions, editable text, instances and fonts |
| Artwork provenance | Six existing performer images only; explicit approval obtained for upload to the new Figma exploration; `figma/AssetUploadLedger.json` records sources |
| Browser visual coverage | All 30 concept/view/locale combinations inspected; image readiness and expected dialogs recorded in `review-evidence/BrowserCoverage.json`; full-page PNGs indexed in `VisualMockupIndex.md` |
| Keyboard / modal behavior | Settings Escape dismissed and restored `shell-settings`; AI Tab remained within the modal's enabled Close action; background inert and focus scope are prototype behavior, not Unity certification |
| Motion/state illustration | Ready, tracking loss, point target, confirmation, Activation, preparation, first-frame reveal, explicit completion and cancel inspected; `review-evidence/BehaviorChecks.json` |
| Deferred / optional illustration | Components, unavailable deferred controls, conditional calibration and Souvenir inspected in both locales; reduced-motion and opaque review overrides inspected; `review-evidence/ExtraChecks.json` |
| Complete route coverage | 104 localized records, 52 route identities, 15 families; each family requires state/localization refinement after direction selection |
| Deferred copy | 53 paired English/Traditional Chinese state entries across seven feature groups; successful outcomes are specifications requiring real service/state confirmation |
| Artifact integrity | Local links, PNG/JPG decoding, JSON/CSV counts, source hashes, JS syntax and tracked production diff checked; see `PackageVerification.json` |

## Corrections during review

Constrained Home copy to its reading column, then narrowed Living Light's column to retain a clear gap from artwork at the captured 1264px browser width. Both locales were recaptured after the final spacing change. Component/deferred specimens expand vertically so their footer does not cover content. Fixed the package-index link for localhost viewing. Cancelled stale modal-close callbacks when another view opens. Restricted gesture status to its relevant views. One selection capture initially preceded image readiness; it was repeated after all images loaded and replaced.

`ConceptComparison.jpg` and `ConceptScreenOverview.jpg` are crops/contact sheets derived from the final browser captures. They intentionally omit the external review toolbar for visual comparison; the complete screenshots retain that context. Browser studies use a responsive review canvas and are not the native 1920×1080 Figma frame exports. `mockups/A-Home-en.png` is a native Figma composition export. `review-evidence/Prototype-A-Home.png` is an earlier first-pass capture retained as historical evidence; use the indexed captures for review.

## Limits and remaining validation

No Unity Play Mode, Edit Mode test runner, standalone Player, camera test, live assistant request, video decode test or profiling session was run. No production C#, UXML/USS, scene, package, MediaPipe or importer settings changed. The 1080p/60fps target remains provisional. Browser material effects, focus APIs and keyboard events do not establish Unity parity.

Traditional Chinese was visually checked in representative captures, but native-speaker editorial approval, CJK IME submission, screen-reader behavior, actual Unity font fallback/atlas cost and physical exhibition readability remain pending. No measured contrast pass over every future video frame is claimed. Optional features show truthful unavailable/specification states; no calibration success, audio setting, reset, assistant response or QR delivery was fabricated. Final pixel refinement for all state variants follows selection of a direction and approval of the relevant policies.
