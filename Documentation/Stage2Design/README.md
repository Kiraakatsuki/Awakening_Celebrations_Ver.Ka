# Stage 2A — Premium experience design review

**13 November 2026 · Proposed, not approved · Production implementation has not begun.**

Open the [editable Figma exploration](https://www.figma.com/design/9VQRtMDk8vFGer7gktEYsB) or the [standalone interactive prototype](prototype/index.html). The prototype can be opened as a local file; its included artwork/fonts need to remain beside it. For the currently running localhost preview, see the completion message. No build system, dependency install, Unity modification or external service is needed.

The Figma file contains **30 editable 1920×1080 frames**: three concepts × Home, Color Release/selection, Global Settings, Global AI and a GAIA cinematic view × English/Traditional Chinese. Discrete existing artwork is used as image content; the interface/text is editable. The six source images were uploaded with explicit user approval during this task. No Figma AI credits or newly generated performer artwork were used.

## Review order

Start with the [three-direction comparison sheet](ConceptComparison.jpg) and [fifteen-view overview](ConceptScreenOverview.jpg). These are browser visual studies; native editable Figma references are indexed separately.

1. [Visual direction comparison](VisualDirectionComparison.md) and [indexed mockups](VisualMockupIndex.md).
2. [Global shell](GlobalShellSpecification.md), [components/states](ComponentStateLibrary.md), [motion storyboard](MotionStoryboard.md).
3. [Screen-family coverage](ScreenFamilyCoverage.md) and [104-record route/locale mapping](ScreenFamilyCoverage.csv).
4. [Deferred feature UX](DeferredFeatureUX.md) and [bilingual state copy](DeferredFeatureStates.csv).
5. [Revised tokens/type](TokensAndTypography.md), [machine-readable tokens](DesignTokens.json), [Unity feasibility](UnityFeasibilityAndRisks.md).
6. [Stage 1 reconciliation](SourcesAndReconciliation.md), [validation evidence](ValidationReport.md), [approval checklist](DesignApprovalChecklist.md).

## What to compare

| Direction | Composition | Primary tradeoff |
| --- | --- | --- |
| A — Living Light | Mineral-dark split composition; six equally weighted portrait cards; right-side overlays | Clearest readability and lowest visual-system risk; restrained rather than spectacular glass |
| B — Fluid Glass | Overlapping hero surface, floating dock, two-row spatial cards, central broad overlays | Strongest material expression; highest compositing, overdraw and contrast risk |
| C — Cinematic Spatial | Artwork field, vertical performer index, left reading panel, quiet edge shell | Strongest immersion; requires disciplined contrast fields and selection discoverability |

**Recommendation:** A as the primary everyday language, C’s reduced cinematic shell during protected playback, and only a bounded amount of B’s edge-light/material treatment. This is a design recommendation, not approval to combine concepts or implement a renderer.

## Prototype boundaries

The review toolbar changes the illustrated concept, locale, view and accessibility/effects appearance. The exhibition canvas supports navigation, modal opening/dismissal, focus scope, keyboard selection and animation studies. A separate event panel illustrates readiness/loss/target/confirm, Focus→Activation, preparation, first-frame reveal, completion and cancellation. It never accesses a camera, decodes a video, sends a network request or applies a deferred business setting. Browser blur/shadows are visual approximations, not proof of Unity support or 60fps performance.

Sound/accessibility/camera/reset actions remain disabled with reasons. Calibration and Souvenir have optional layouts, without success simulation or generated QR. No automatic timer is allowed to stand in for a real video completion event. Source pose/dwell thresholds and VP8 importers are unchanged.

Review the direction and policy decisions before production work. Full post-selection screen refinement remains a subsequent design step; the coverage matrix specifies every route without claiming 104 finished unrelated screens.
