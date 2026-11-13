# Transformation roadmap and approval register

Stage 1 · 13 November 2026 · planning only. All implementation below is proposed; no phase is automatically authorized by this document.

## Required order and review gates

**Completed here:** static application audit, current-source feature inventory, initial complete-system design/architecture specification and validation plan. Runtime/visual/performance acceptance remains not run by instruction.

1. **Gate A — review Stage 1:** approve the audit/architecture direction and identify retained C features. User confirmed all discussed unavailable/placeholder features were deliberately deferred; final inclusion of “most” is still a review decision.
2. **Stage 2 design completion, before functional overhaul:** develop/review full bilingual layouts and flows for existing plus retained deferred features using this design system; all six performer themes, global shell, protected phases, states and reduced-motion/effects alternatives. Approve exact references; current Figma exports are not the entire intended application. Do not implement deferred logic or expose false functional controls.
3. **Gate B — architecture/design approval:** accept shell/overlay policy, migration impact, preservation contracts, design reference set, target budgets and per-phase scope. Authorize implementation explicitly. Runtime/camera tests require their own explicit permission; implementation approval alone does not override a testing prohibition.
4. **Primary implementation P1–P9**, with safeguards throughout; **major cleanup P10**, followed by **established-feature integration P11**.
5. **Gate C — separate deferred-feature authorization:** only after overhaul, cleanup and established-function validation, authorize selected D1 capabilities and real service contracts.
6. **D1 deferred functional implementation**, then **D2 final all-feature integration, optimization and polish**. The main redesign must not need another visual overhaul when D1 lands.

Each phase ends in a reviewable commit/artifact and acceptance evidence. No arbitrary schedule is asserted without team capacity and runtime baseline. Use one representative end-to-end vertical slice before scaling to all variants.

## P1 — Architecture stabilization and regression safeguards

- **Scope / affected:** establish state/route/media contracts around `AwakeningApplicationController`, Home, document manager/catalog, gesture gate and backend interface; narrowly isolate deterministic policy/state where approved. Inspect and propose meaningful tests rather than rewrite the controller wholesale.
- **Dependencies:** Gate B, baseline commit/static evidence, approved privacy/reset/availability contracts and permitted test modes.
- **Risks / UX benefit:** subtle session semantics may change; protect existing behavior and reduce future regressions.
- **Acceptance:** catalog/mappings/reference baseline recorded; six ordered video slots intact; denial/single-owner and completion/cancellation invariants explicit; rejected navigation leaves usable UI/history; no deferred controls activated.
- **Validation:** filesystem checks first; inspected isolated Edit Mode tests only when authorized; runtime matrix stays pending until permission.
- **Rollback:** revert individual state-boundary commits while retaining existing serialized facade fields/GUIDs; no dependency migrations in this phase.

## P2 — Global shell, navigation and overlay policy

- **Scope / affected:** approved shell UXML/USS, `ScreenDocumentManager`, Home/app entry handlers, capability policy, overlay/focus owner and SampleScene assignments.
- **Dependencies:** P1; all-route policy approved; design layouts include retained C entry points/states without functional activation.
- **Risks / UX benefit:** root click capture, gesture commands, focus and capture suspension can conflict; delivers predictable global Settings/AI access and safe cinematic protection.
- **Acceptance:** one page and one ordinary modal; topmost dismissal before route guard; correct opener return; available/deferred/unavailable reasons; no protected video interruption, duplicate input owner or consent grant.
- **Validation:** route×locale input matrix, rapid open/close, same-route return, Settings capture suspension, safe stop and assistant late response under explicit permission.
- **Rollback:** restore previous scene+shell bindings+manager/controller commits together; disable new command path, never run two routers in parallel.

## P3 — Tokens and reusable components

- **Scope / affected:** approved token USS, semantic buttons/cards/status/reading panels, bilingual string/font presentation, first family presenters; catalog/binding provenance retained.
- **Dependencies:** P2 interaction contracts; complete design approval; Traditional Chinese font choice.
- **Risks / UX benefit:** moving hierarchy breaks Figma paths; component overgeneralization; provides consistent readable controls and responsive content.
- **Acceptance:** one focus target per action; clear hover/press/focus/disabled states; token-driven styles; long EN/ZH strings fit; no untranslated/fake controls; existing routes unchanged.
- **Validation:** actual action path/tag/role and local-reference checks; approved viewport/state comparisons later, font corpus then rendered fallback checks.
- **Rollback:** restore a family's UXML/USS/catalog/binding/presenter set as one unit; retain old export snapshot for provenance.

## P4 — Glass and material system

- **Scope / affected:** material variants; tint/preblur baseline; optional small dynamic-backdrop spike; PanelSettings, approved UI shaders/renderer resources only if required.
- **Dependencies:** P3; confirmed compositing source, laptop/API baseline and separate approval for any offscreen-panel/render-pass change.
- **Risks / UX benefit:** missing UI in camera capture, alpha/color-space errors, feedback loop, bandwidth/overdraw; provides depth without compromising text or playback.
- **Acceptance:** readable opaque fallback; shared bounded resources; text sharp; no second camera/video; no enhancement required for basic UX; selected performance tier meets provisional FHD60 target.
- **Validation:** bright/dark art/video, ordering/alpha, GPU/memory capture and repeated overlay cycles only when authorized; source/budget arithmetic alone cannot pass quality.
- **Rollback:** switch to baseline materials and revert enhancement pass/panel changes together; release allocated resources and restore original camera/panel wiring.

## P5 — Unified motion and interruption

- **Scope / affected:** transition coordinator/scopes, component USS feedback, performer coroutine presentation adapter; existing semantic input and playback ownership preserved.
- **Dependencies:** P2/P3; approved motion/state matrix; P4 surface changes can remain baseline during motion integration.
- **Risks / UX benefit:** competing animation owners or stale callbacks; supplies immediate feedback and coherent transitions.
- **Acceptance:** cancellation resolves input/focus/opacity; no double navigation; gesture dwell unchanged; media completion remains event-driven; reduced-motion design demonstrated without activating deferred preference storage.
- **Validation:** interrupted/repeated input, detach/disable, locale change, safe stop and missing-video hold; frame-pacing capture under permission.
- **Rollback:** revert coordinator and associated presenter adapters together; restore existing timings/locks without leaving mixed owners.

## P6 — Core experience redesign

- **Scope / affected:** Home/Privacy/Tutorial/Attract/Color Release/Closing and existing Souvenir navigation, Home/app family presenters and exported sources under approved redesign scope.
- **Dependencies:** P2/P3/P5; approved copy, input alternatives, first-session and next-visitor policy.
- **Risks / UX benefit:** awakening flags, consent, target mapping and reset copy; produces a clear entry, learned interaction and coherent completion.
- **Acceptance:** consent denied by default; first awakening only as approved; six choices map correctly; no new QR service; placeholder delivery unavailable honestly; all locale paths remain usable.
- **Validation:** new/repeated visitor cycles, decline/revoke, tutorial skip/back, pointer/keyboard and existing gesture alternatives, both locales at agreed sizes.
- **Rollback:** revert core families plus bindings/flow changes as coordinated commits; no asset deletion.

## P7 — Six performer experiences

- **Scope / affected:** shared performer presenter and all six×five×two localized variants, VideoPlayer/image/RT ownership, stage transitions/audio feedback.
- **Dependencies:** P1–P6 relevant contracts; all clips/art and design themes approved; VP8 compatibility preserved.
- **Risks / UX benefit:** clip swaps, first-frame flash, callback re-entry, interrupt/cancel errors; delivers cinematic continuity and unmistakable performer identity.
- **Acceptance:** all 60 stage/locale presentations covered; actual preparation→first frame→play→finish; explicit Back cancellation; no click skip of automatic states; configured missing/error-video behavior; audio remains current policy unless approved.
- **Validation:** complete all-six media matrix including missing clip/error, repeated re-entry, timeout/hold setting variants in isolated authorized scenarios; memory returns to stable level.
- **Rollback:** retain original clip GUID ordering and importer values; revert presenter/player/variant set together, avoiding a second active player.

## P8 — Settings, privacy, localization and assistant redesign

- **Scope / affected:** Settings/current Language/Privacy/chooser/overlay/PostVideo presenters; assistant session/transport extraction; runtime chat USS and backend contract documentation.
- **Dependencies:** P2/P3/P7; approved notice/content; actual endpoint contract and error requirements. Retained C layouts completed already; their business logic still excluded.
- **Risks / UX benefit:** text editing keys/IME, stale replies, privacy wording and theme/performer coupling; clear global assistance and settings return context.
- **Acceptance:** current request/suggestion/retry/cancel behavior preserved; offline/unconfigured/invalid reply states; credentials backend-only; no late update; bilingual text/font coverage; no Sound/Accessibility/Tracking/Reset/QR logic activated.
- **Validation:** controlled stub responses before authorized live tests; all theme/locale combinations, long replies, server/connection errors, rate limit, timeout, close/disable/re-entry and language changes.
- **Rollback:** restore client/presenter/USS compatible set; keep deployed backend changes separate with explicit authorization and rollback version if any later task changes it.

## P9 — Accessibility and performance refinement

- **Scope / affected:** focus/semantic controls, font/layout corrections, baseline static motion/effects alternatives, measured UI/bridge/video hotspots and quality configuration.
- **Dependencies:** integrated primary features, target laptop and explicit runtime/profiling/camera permission; no deferred preference activation.
- **Risks / UX benefit:** tuning recognizer semantics for performance, hiding bad contrast with blur, unmeasured “optimizations”; provides readable alternatives and stable frame pacing.
- **Acceptance:** foundation input/contrast/type requirements verified; no critical text truncation; actual Chinese glyph forms; FHD60 target assessed with p95/p99 and thermal/power context; no progressive memory/capture leak.
- **Validation:** [detailed plan](TransformationAudit/ValidationAndPerformancePlan.md); one measured cause at a time, same workload baseline, physical viewing trials, screen-reader support reported honestly.
- **Rollback:** revert individual measured optimization or effects configuration; retain readable baseline and original gesture thresholds.

## P10 — Major cleanup and refactoring

- **Scope / affected:** only evidenced obsolete fallback/helpers/assets and superseded presentation code; affected source/scene/catalog/importer references documented per concrete proposal.
- **Dependencies:** primary redesign stable; complete transitive reference ledger; separately approve destructive changes and any additional major restructuring.
- **Risks / UX benefit:** Resources/string/sample/bootstrap/font references may be missed; removes maintenance burden without functional loss.
- **Acceptance:** no required asset or integration removed; serialized GUIDs intentional; no unexpected catalog/build/reference changes; regression contract still passes; historical evidence retained or archived by approval.
- **Validation:** before/after GUID/string/build/Resources graph; compile/build/runtime regression only under granted permissions; verify clean controlled import later.
- **Rollback:** reversible cleanup commits with original `.meta` files/assets preserved in version control; no cache deletion as a fix.

## P11 — Established-feature integration and validation

- **Scope / affected:** full primary application; release evidence, known-limitations and packaging/runbook; no unapproved deferred business logic.
- **Dependencies:** P1–P10 acceptance and explicit test/build permissions.
- **Risks / UX benefit:** individually passing families can fail together; establishes a reliable baseline before scope expands.
- **Acceptance:** all existing flows, six performers, both locales, camera privacy, video cancellation/completion, AI lifecycle, navigation/focus and fallback matrix pass; measured performance at chosen quality; no blocker hidden behind a “static pass.”
- **Validation:** clean compilation/import, authorized Windows build and device matrix; distinguish pre-existing vs introduced defects, collect evidence and resolve release blockers.
- **Rollback:** return to last accepted integration commit and keep deferred implementations out until this gate passes.

## D1 — Separately authorized deferred functionality

- **Scope / affected:** only retained/approved F27–F33 capabilities (plus separately confirmed new scope), one at a time; their already-approved components and real audio/settings/camera/reset/delivery dependencies.
- **Dependencies:** Gate C, P10/P11 accepted, final inclusion and actual business/service/privacy contracts. No guessed QR destination, calibration algorithm or reset semantics.
- **Risks / UX benefit:** scope creep and premature backend infrastructure; completes useful capabilities without visual fragmentation.
- **Acceptance:** implement each real state and service outcome from the complete design; no fake controls; both locales, alternatives, cancellation and shell policy correct; approved omitted features stay absent.
- **Validation:** feature-specific meaningful tests, authorized integration/error/privacy/performance checks and regression of existing experiences after each capability.
- **Rollback:** isolate each feature's activation and versioned business integration; restore truthful unavailable/omitted state, cancel owned resources, never leave a pretend working action.

## D2 — Final integration, optimization and polish

- **Scope / affected:** full intended application including approved deferred capabilities and final content/assets.
- **Dependencies:** D1 accepted and explicit release validation authorization.
- **Risks / UX benefit:** cumulative resources/services and venue conditions; final coherent exhibition delivery.
- **Acceptance:** complete all-feature regression, performance/thermal repeatability on deployment hardware, approved visitor/session policies, localization/accessibility review and operator handoff. Any unresolved unsupported behavior is documented explicitly.
- **Validation:** repeat full matrix, long-session cycles, network/device faults, fresh restart and installation/build provenance with captured evidence.
- **Rollback:** last accepted release candidate with compatible assets/configuration and backend versions; feature-level rollback for external integration failure.

## Decisions requiring review before implementation

| Decision | Recommendation / reason | Approval point |
| --- | --- | --- |
| Scope and major architecture | Incremental shell/policy/presenter extraction, not full replacement | Gate A/B |
| Deferred inclusion | Include Sound/Accessibility/tracking status; combine Guide with Help; calibration conditional; Souvenir only with useful defined takeaway; safe restart conditional on reset rules | Gate A/design freeze; D1 functionality later |
| Display/performance | Keep current FHD; provisional 60fps on RTX4060 laptop/16GB; measure CPU/GPU/thermal conditions; higher resolution optional after baseline | Gate B/P4/P9 |
| Viewing setup | Determine comfortable distance through actual target recognition/reading/gesture trials; exact CPU, laptop power profile, DPI/monitor to record | Before visual/performance acceptance |
| Privacy and participation | Final bilingual camera/AI data notice; decide whether consent-free pointer/keyboard experience is allowed rather than current entry gate | Before P6/P8 |
| Visitor reset/Home/Done | Define which state clears, next route, consent revocation and language persistence; preserve current semantics until approved | Before P2/P6; reset business logic D1 |
| Global overlay policy | Protected phases defer Settings/AI; explicit safe Stop/Back; topmost dismissal, opener restoration; capture suspension expectations | Before P2 |
| Global gesture access | Preserve existing recognizer; new generic shell activation gesture requires approved meaning; keep pointer/keyboard alternatives | Before any gesture behavior change |
| Content/assistant | Approved performer content and prompts; decide curated knowledge/history; backend availability/privacy; no inference that one-turn chat is a knowledge system | Before P8/new services |
| Video/audio behavior | Preserve VP8 intent, completion event, missing-video configured hold/timer and muted video audio; approve soundtrack and any timing change separately | Before P7 |
| Typography/design sources | Approve new bilingual design references, Traditional Chinese fonts and token/state system; current exports are references | Design freeze/P3 |
| Dynamic glass | Tint/preblur first; explicit lower-layer composition only if necessary and profiled; no Unity upgrade/dependency assumed | Before rendering spike/integration |
| Cleanup | Exact removal diff + dependency evidence; no wholesale sample/assets purge | P10 separate approval |
| Runtime verification | Explicit permission for Play Mode/Player/build/profiling/camera scenarios; no test runner bypass | Before any runtime work |

This document ends Stage 1 with a reviewable proposal. It does not start Stage 2.
