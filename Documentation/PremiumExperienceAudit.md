# Premium experience audit — Stage 1

**Awakening Celebrations Ver.ka · 13 November 2026 · analysis and proposal only**

## Executive assessment

The project has substantial implemented experience logic, rather than only disconnected visual prototypes: a consent-gated MediaPipe pipeline, six performer flows, real VideoPlayer/RenderTexture integration, environmental audio fallback, bilingual routing, and an HTTPS assistant client/backend. Its production-quality gap is chiefly the coupling between experience flow and presentation, fragmented access to shared capabilities, fragile imported element identities, provisional product content, and an interface built largely from fixed coordinates.

Preserve these functional contracts while replacing the presentation deliberately. Do not treat current Unity screens or historical Figma exports as a complete product specification. No runtime failure, visual fidelity, smoothness, or measured performance result is claimed by this audit.

## Scope and evidence

Baseline commit: `b9f0972b38008b31e4bd2117ee88cb34ef9ce6fe`. Initial working tree already contained untracked `AGENTS.md` and `.agents/`; they are user guidance, not audit changes. New artifacts are confined to `Documentation/`. No Unity Editor calls, Play Mode, Player, builds, test runners, profiling, camera access, assistant requests, screenshots, or subjective visual inspection were performed.

The two supplied Stage 1 briefs authorize this audit and documentation, require preservation and a complete future feature inventory, and prohibit application changes. This is a source/serialized-asset UX assessment; rendered composition, actual contrast over video, runtime input and assistive technology remain pending. Historical screenshots were not used as present-day verification. The local Figma exports provide provenance and design coverage, not current approval of every frame.

Current static census: **104 catalog records, 52 logical routes with EN/ZH pairs, 104 UXML files, 97 USS files, 16 scene UIDocuments**. All catalog asset GUIDs resolve, XML parses, and local `Assets/` references in UXML resolve after URL decoding. The manifest and snapshot agree on their relevant mapping fields. Historical counts of 106 screens and 17 documents no longer describe this checkout. See [static evidence](TransformationAudit/StaticEvidence.json) and [full route inventory](TransformationAudit/RouteLocaleInventory.csv).

## Coordinated use of the nine project skills

| Skill | Applied analysis | Shared consequence |
| --- | --- | --- |
| [unity-architecture](../.agents/skills/unity-architecture/SKILL.md) | Controller, state, ownership, scene wiring, media/request cleanup | One flow owner; incremental extraction; preserve GUIDs |
| [unity-ui-toolkit](../.agents/skills/unity-ui-toolkit/SKILL.md) | Documents, catalog, path/tag bindings, focus and page families | Retain route IDs; shell and components replace coordinate bindings gradually |
| [premium-glass-rendering](../.agents/skills/premium-glass-rendering/SKILL.md) | Graphics/quality/URP renderer, panel, backdrop sources and resources | Tinted/preblurred baseline; bounded dynamic blur only after compatibility gate |
| [motion-interaction](../.agents/skills/motion-interaction/SKILL.md) | Coroutine fades, locks, input semantics and interruption | Presentation must not own navigation or video completion |
| [unity-performance](../.agents/skills/unity-performance/SKILL.md) | CPUAsync hand tracking, result allocations, texture/repaint lifecycle | Cost hypotheses and capture plan; no invented measurements |
| [regression-testing](../.agents/skills/regression-testing/SKILL.md) | Safe filesystem checks, validator scope, all-route runtime matrix | Static pass is separate from compile/runtime acceptance |
| [accessibility-localization](../.agents/skills/accessibility-localization/SKILL.md) | Assigned fonts, translated controls, focus, fallback input, readability | Explicit Traditional Chinese fonts, semantic controls and state-preserving locale updates |
| [feature-planning](../.agents/skills/feature-planning/SKILL.md) | A–F inventory, approval evidence and sequencing | Named deferred features need confirmation; functionality follows cleanup |
| [figma-to-unity](../.agents/skills/figma-to-unity/SKILL.md) | Export roots/tokens, manifest and runtime remaps | Provenance retained; frame presence does not imply feature approval |

The installed Unity project health-check skill supplements evidence, severity and confidence reporting. All nine project skills were read before source auditing.

## Prioritized findings

“Confirmed” below describes directly inspected source/configuration, not runtime reproduction. High = material product/release risk; Medium = meaningful usability or maintainability risk; Low = localized preventive improvement. IDs remain common across the blueprint.

| ID | Severity / confidence | Evidence and issue | Why quality suffers; concrete response |
| --- | --- | --- | --- |
| AC-01 | High / Confirmed restriction | `HomeLanguageController` Settings events; controller Settings/chooser/PostVideo bindings | Shared capabilities are tied to page flow. Introduce a persistent shell and an explicit capability policy; retain cinematic protection. |
| AC-02 | High / Confirmed concentration; risk Likely | `AwakeningApplicationController.cs`, 2,976 lines, owns routing, gesture command dispatch, video, audio, preview, assistant transport and UI | Changes cross unrelated lifecycles. Extract bounded presenters/services behind the current facade; require migration approval. |
| AC-03 | Medium / Confirmed control-flow inconsistency | `Back()` checks first-awakening restrictions before its overlay branch | The generic Back command cannot dismiss a pre-awakening assistant overlay through that path. Overlay-local Escape does close it. Make dismissal precede page guards; validate palm/keyboard/pointer equivalence. |
| AC-04 | High / Confirmed content gap | Privacy EN/ZH text says final implementation/venue policy must define data handling; Souvenir contains runtime-generation instructions | Visitor-facing authoring notes undermine trust and leave product/privacy policy incomplete. Obtain approved bilingual copy and distinguish camera processing from assistant network transmission. |
| AC-05 | Medium / Confirmed fixed layout; impact Likely | 2,585 inline absolute-position elements among 3,001 XML elements; fixed 1920×1080 roots; constant-pixel panel; assistant 1,200px conversation width | Alternative display sizes and long content can clip or become too small. Adopt constrained flex layouts, content-driven height and bilingual layout rules. |
| AC-06 | High / Confirmed font assignment risk; rendering Unverified | 363 Han-containing labels in 51 assets lack characters in explicitly assigned Inter; no explicit Panel TextSettings assignment | Chinese depends on an unverified fallback path. Specify a Traditional Chinese family/fallback and test actual rendered glyphs. This is not proof of 363 visibly missing labels. |
| AC-07 | Medium / Confirmed gaps | Close overlay restores first focusable element, not opener; suggestion surfaces/labels registered independently; generic label controls | Keyboard position and semantics vary. Use one semantic focus target per action, focus scope and opener restoration; screen-reader support requires verification. |
| AC-08 | Medium / Confirmed implementation; perceived effect Unverified | Linear 650ms fades; 3s Activation; no shared motion/reduced-motion settings | Routine navigation and cinematic timing lack a unified contract. Tokenize feedback, separate cinematic content duration, and provide a static/reduced-motion design. |
| AC-09 | Medium / Confirmed validator coverage gap | 251 snapshot path/tag mismatches across 15 localized assets, including decorative nodes; validator checks manifest/snapshot/catalog but never traverses UXML | Matching snapshots can still conceal changed runtime hierarchy. Validate actual action identities, including remaps and semantic role; do not call 251 controls broken. |
| AC-10 | Medium / Likely lifecycle risk | `OnDisable()` cancels assistant requests only indirectly while closing overlays; PostVideo uses a request without an overlay | Explicit transport cancellation is missing for a direct-page request on disable. Centralize idempotent cancellation and test completion after disable/navigation. |
| AC-11 | Medium / Confirmed cost paths; cost Unmeasured | Windows CPU inference, CPUAsync readback, per-result arrays, preview repaint, allocated video RT | User identifies ROG Strix G16 / RTX 4060 8GB / 16GB RAM; use provisional 1080p60 target and profile combined tracking/video/UI before accepting dynamic effects. |
| AC-12 | Medium / Confirmed portability risk | MediaPipe tarball references a Downloads path; MCP manifest Git main versus embedded 10.3.0 lock; experimental Figma importer | A fresh workstation may not reproduce this setup. Preserve versions now; propose portable package sourcing and document embedded precedence before later cleanup. |

## Visual and interaction assessment by area

| Area | Static evidence / limit | Proposed direction and acceptance |
| --- | --- | --- |
| Identity, color, depth | Six recognizable accents and dark stages exist; glass tokens are opaque colors, most generated USS files are empty | “Living light”: restrained dark exhibition space, warm neutral typography, performer accents as identity, opaque reading surfaces with optional glass chrome. Review both locales over bright/dark content. |
| Typography | 847 explicitly sized labels under 18px; assistant replies 14px/status 13px; inline authoring labels remain | Raise visitor reading size, reduce metadata, establish display/body/caption hierarchy. Physical viewing distance determines final scale. |
| Spacing/alignment/composition | Fixed coordinates and manually altered EN Settings differ from canonical paths | Rebuild approved families from shared spacing/container tokens; compare at agreed viewport only during authorized validation. No present alignment defect is inferred solely from coordinates. |
| Glass/background | Tinted rectangles and artwork; no project glass renderer feature or custom glass shader found | Keep text sharp; share a backdrop texture only where moving content requires it. Never blur the controls themselves. |
| Icons | Multiple actions are separate visual/label layers rather than semantic controls | Establish a consistent optical grid, stroke weight, labels and state shapes. Do not convey state through color alone. |
| Hover/pressed/focus | Shared feedback mainly reduces whole-element opacity to .94/.82; white/gold outlines | Use surface/border response, visible focus, immediate activation and distinct disabled reasons. Verify outline rendering in this Unity version. |
| Gesture feedback | Tracked-hand status, cursor, targeting outline, dwell/release logic exist | Explain “point → fist”, show acquiring/targeted/confirmed/lost states, preserve recognition thresholds. Global gestures require separate semantic design and approval. |
| Navigation/modal | Screen locks, history filtering and overlay stack exist; no shell | One overlay authority, topmost dismissal, deferred global requests during cinematic protection, visible privacy stop. |
| Loading/error/empty | Camera waiting/no-hand labels and assistant retry exist; video failures log and restore artwork | Localized visible status, safe retry/cancel and truthful capability availability; distinguish missing clip, prepare delay, decoder error and service outage. |
| Motion | Performer fades and waits exist; most other screen changes replace trees immediately | Consistent short presentation transitions; deterministic cancellation, no delayed button actions, no fade-driven playback completion. |
| Bilingual delivery | Route parity is complete statically; content correctness and fonts are not established by file pairs | Editorial review, CJK glyph coverage, long-text layouts, locale changes without restarting experience. |

## Feature status and product completeness

The [FeatureImplementationMatrix](FeatureImplementationMatrix.md) separates A implemented, B partial, C deliberately deferred, D design-only, E legacy candidates, F unconfirmed. A means a source/serialized functional path exists, with runtime verification still pending. Broken behavior is tracked independently under findings rather than added as a misleading seventh product status.

During this audit the user confirmed that all the unavailable/placeholder candidates discussed were deliberately deferred, explicitly including the placeholder Souvenir section, and that most are suitable for inclusion. This supplies postponement evidence for those candidates. Their status is C with any implemented navigation portion recorded separately. The recommendation is to include sound, accessibility, privacy/tracking status and Souvenir design; combine Guide with contextual help; keep calibration and restart behind product decisions. “Most” is not blanket approval of every historical Figma frame or a new service. The matrix distinguishes confirmed deferral from proposed inclusion and functional authorization. No deferred business logic is authorized in this stage or during the primary overhaul.

## Blueprint and review order

1. [CurrentArchitectureAssessment](CurrentArchitectureAssessment.md) — verified baseline and preservation contracts.
2. [FeatureImplementationMatrix](FeatureImplementationMatrix.md) — capability/approval inventory.
3. [GlobalNavigationRedesign](GlobalNavigationRedesign.md) — shell, route and overlay policies.
4. [PremiumDesignSystem](PremiumDesignSystem.md) — visual tokens, glass/compositing and bilingual components.
5. [MotionInteractionSpecification](MotionInteractionSpecification.md) — timing and interruption contracts.
6. [TargetRuntimeArchitecture](TargetRuntimeArchitecture.md) — bounded migration and resource ownership.
7. [TechnicalDebtAssessment](TechnicalDebtAssessment.md) — evidence, cleanup candidates, validation.
8. [TransformationRoadmap](TransformationRoadmap.md) — gated phases and approval register.
9. [Validation and performance plan](TransformationAudit/ValidationAndPerformancePlan.md) — exact static results and pending runtime coverage.

Review the audit, named future scope, shell behavior and migration impact before authorizing Stage 2. Nothing in these proposals authorizes implementation, destructive cleanup, hardware capture, or a Unity upgrade.
