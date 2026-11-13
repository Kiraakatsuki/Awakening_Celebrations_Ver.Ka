# Technical debt assessment

Stage 1 · 13 November 2026 · static analysis; no cleanup performed. Findings use the shared AC IDs in [PremiumExperienceAudit](PremiumExperienceAudit.md).

## Priorities and concrete work

| Finding / domain | Evidence and impact | Recommended work / size | Required validation |
| --- | --- | --- | --- |
| AC-01/03 global navigation correctness | Settings/AI reachability depends on page; `Back` applies awakening guard before overlay close | Policy/overlay ownership first; Medium–Large | Every route, topmost dismissal, gestures vs explicit controls, protected video |
| AC-02 controller coupling | 2,976-line controller combines view construction, domain state, media and transport | Incremental boundaries behind facade; Large | Compare routes/serialized state before and after each extraction; six performers/both locales |
| AC-04 product content | Privacy asks future implementation to define processing; video/Souvenir contain developer instructions | Approved bilingual editorial replacement, no fabricated functionality; Medium | Content review, actual device readability, delivered service/notice consistency |
| AC-05 layout/style duplication | 104 UXML; 97 USS, 91 empty; most appearance inline; 2,585 absolute-position XML elements; no exported token use found | Semantic components/tokens and responsive layout, not mass regex rewrite; Large | Per-family identity/binding checks, bilingual long text, 1080p and agreed alternate scales |
| AC-06 localization/type | 363 Han labels assigned Inter without those glyphs; no explicit panel TextSettings | Deliberate Traditional Chinese font stack, actual glyph/typography review; Medium | Cmap corpus, glyph forms, fallback and real rendered text; don't infer failure from SC filename alone |
| AC-07 focus/control reuse | `BindPath`, `RegisterClick`, Home `InteractionGroup` repeat event behavior; overlay suggestions surface/label separately focusable | Reusable semantic controls + view-owned disposal and focus scopes; Medium | Tab/Shift+Tab, Enter/Space, Escape, opener restoration, IME, no duplicate actions |
| AC-08 motion ownership | Repeated flags/coroutine handles, linear fades, mixed immediate replacement; no reduced-motion preference | Bounded TransitionCoordinator and cancellation contract; Medium | Interrupt/repeat/detach matrix; semantic completion unchanged |
| AC-09 mapping validator limitation | Existing prebuild validator compares manifest/snapshot/catalog and UXML file existence, not actual XML traversal | Extend action-role/reference validation; retain source-root provenance; Medium | Deliberate corrupted fixture must be detected without opening runtime/camera; validate remaps |
| AC-10 async lifecycle | `OnDisable` clears screen bindings but only closes overlays to cancel requests; PostVideo has no overlay | Explicit shared assistant cancellation on every lifecycle exit; Small–Medium | Disable/destroy/locale/re-entry with late response, request disposal, no UI updates after exit |
| AC-11 performance hypotheses | Bridge allocates copied landmark arrays every result; cursor uses left/top; status strings built per Update; preview repaint and video-frame repaint; RT created each portrait entry | Capture baseline first, then fix measured cause; no pooling framework by default; Unknown until measured | CPU/GPU/GC distributions and repeated-cycle memory on actual laptop |
| AC-12 reproducibility | Absolute Downloads tarball; manifest MCP main vs embedded/lock; Figma experimental package; Inter from transitive App UI | Document resolved dependency sources; make portable provenance proposal, later pin approved sources; Medium | Fresh clone/import on controlled workstation; package hashes and build, separately authorized |
| AC-02 related state transaction | `Navigate` clears bindings/history before document activation failure is known; multiple Home return paths have different reset semantics | Prepare/validate before commit; approve reset contract; Medium | Missing document/catalog case retains usable UI and correct history; no hidden state loss |

No Critical finding is asserted. Compilation, player build, runtime failure rates and actual resource leaks are not established. The absence of a first-party test suite is confirmed under `Assets` (excluding imported samples); it is a regression-risk gap, not proof that current features fail. Installed Unity Test Framework and tool/vendor test code do not cover app contracts automatically.

## Resource and event assessment

Positive evidence to retain: Home groups unregister events on destroy; app uses reverse-order cleanup lists for page/overlay callbacks; bridge unsubscribes from runner; gate rejects duplicate owners and revokes on unregister; video player events are explicitly removed, target detached and RT released; navigation/overlay close abort assistant requests and invalidate sessions; chime is destroyed on controller destroy. Preserve those patterns while closing disable/asynchronous gaps.

Risks requiring focused validation: direct-page assistant work on disable; stale same-player callbacks during rapid re-entry; failure during document activation after old bindings are cleared; locale updates rejected during protected phases; scope of first-session flags after Done/Home; hidden documents retaining work; CJK IME Enter accidentally submitting. These are code-path hypotheses or omissions, not claimed reproduced runtime bugs.

The backend already validates input, limits body/message size, applies per-IP throttling and keeps API keys environment-side. Deployment health, venue privacy notice, response limits/operational budget and suitability of public access remain release decisions. No credential files or secrets were read or copied. Avoid speculative authentication infrastructure unless deployment requirements call for it.

## Legacy/unused candidate reference ledger

| Candidate | Evidence actually checked | What prevents deletion now | Recommendation |
| --- | --- | --- | --- |
| Generic `navigationDocument` fallback | App source supports manager-null fallback; SampleScene assigns manager and initial Privacy document | Other bootstrap/editor workflows may rely on public facade/fallback; serialized field still active | Retain through migration; propose removal only after caller/scene review |
| `interactionStylesAdded` | Field declared and assigned, no read in first-party source search | Reflection/tooling not comprehensively ruled out; tiny impact | Local cleanup candidate during P10, not a release priority |
| `GestureCursorController.ClickAtCurrentPosition` | No first-party caller found; target picking is used | Public helper may support tooling/planned interaction; user approved preserving gesture behavior | Keep until semantic global interaction plan resolved |
| Separate PerformerSelection design | Historical reports list it; current catalog/scene no longer does | Old report is not proof an unused asset still exists | Mark documentation superseded; do not invent deletion targets |
| MediaPipe sample trees/scenes | Only SampleScene enabled for production; production runner/gate/bootstrap/config live inside sample tree and sample asmdef | Transitive source/prefab/assembly/model and Resources dependencies; removing samples wholesale breaks integration | Protect integration; isolate only after explicit migration/cleanup approval |
| Noto Serif SC font | Source GUID referenced by `NotoSerifSC-Regular SDF.asset` | Transitive TMP/font fallback usage not exhaustively resolved; font assets are not proven unused | Retain; audit font system before removing |
| Figma images / blank USS | Local UXML references verified; blank USS still linked by UXML and import provenance | Resources/importer/generated ownership and future reimport behavior | Keep provenance; replace references deliberately during approved component migration |
| `Phase4RuntimeScreenshotCapture.cs` | Menu-only helper, checks Play Mode and writes screenshots | A useful historical tool; execution prohibited in Stage 1 | Do not run; retain until tooling cleanup explicitly approved |
| Old reports/contact sheets | Statements conflict with current source | Historical evidence; deletion loses context | Keep original reports; use dated Stage 1 index to identify superseded claims |
| Video clips under Resources | Six scene slots resolve directly and clips are also in Resources | Both scene references and dynamic resource behavior/packaging; filenames differ from logical names | Preserve; no relocation/compression change without media regression |

This is a triage ledger, not an exhaustive unused-asset proof. Source symbol search alone cannot establish asset non-use. Before each P10 deletion, attach GUID references across scenes/prefabs/assets/UXML/USS, string/Resources/editor/tool references, build inclusion, indirect dependencies, reason for obsolescence, concrete diff and restoration commit. Honor explicit destructive-cleanup approval even if an asset looks redundant.

## Cleanup sequencing

During redesign maintain code quality continuously through small ownership boundaries and regression checks. The major obsolete-code/assets cleanup remains after redesigned architecture and established experiences stabilize. Deferred C feature business logic starts only after that cleanup and a separately authorized task. Design merges (Guide→Help) and proposed omissions (unneeded recalibration/empty takeaway) do not authorize deleting the working prototype now.
