# Living Glass — Refined Spatial Gallery

Stage 2B · 13 November 2026 · design convergence, with production implementation awaiting approval.

The user selected Living Glass as the creative direction and has now **approved Gallery Arc as the composition**. The [spatial depth and performer-reactive addendum](SpatialAddendum/README.md) is the current refinement: B is conditionally preferred, with an A comparison and exact proposed bounds awaiting review. Constellation/Salon remain historical studies, not active layout choices. The 70% gallery / 30% constellation brief guides artistic restraint, not a position formula.

Open the [standalone interactive prototype](prototype/index.html), its [review instructions](prototype/review-guide.html), or the [Living Glass Figma page](https://www.figma.com/design/9VQRtMDk8vFGer7gktEYsB?node-id=18-48). The running preview is `http://127.0.0.1:8767/LivingGlass/prototype/`. Keep the folder hierarchy intact: the prototype reuses six approved local images and two fonts from `../prototype/assets/` relative to this package. It also opens as a local HTML file without installing packages.

**Verification boundary:** native Figma structure and selected native renders were inspected. Browser verification is blocked by a saved permission/managed policy, including after user approval and a permission-change retry. The interactive prototype contains real CSS/Web Animations motion, but its visual fidelity and input behavior are not certified by a browser test. No alternate browser/address was used to bypass the denial. See [validation](ValidationReport.md) and [design QA](design-qa.md).

## Review sequence

Start with the [Arc spatial addendum](SpatialAddendum/README.md), [current A/B comparison](SpatialAddendum/Comparison.jpg), [all six reactive themes](SpatialAddendum/SixThemes.jpg) and [updated approval checklist](SpatialAddendum/ApprovalChecklist.md). The existing standalone prototype now defaults to Arc/B and exposes all-six-theme acquisition, loss and Focus-bridge review controls.

1. [Direction and rationale](DesignDirection.md), [gallery refinements](SpatialGallery.md), [indexed native designs](VisualIndex.md).
2. [Shell and overlays](GlobalShell.md), [glass materials](GlassMaterials.md), [performer themes](PerformerThemes.md).
3. [Motion and interaction contracts](MotionAndInteraction.md), [components, layout and accessibility](ComponentAndLayoutSpecification.md), [tokens](DesignTokens.json).
4. [Bilingual family coverage](ScreenFamilyCoverage.md), [104-record mapping](ScreenFamilyCoverage.csv), [deferred feature UX](DeferredFeatures.md).
5. [Unity feasibility and experiments](UnityFeasibility.md), [validation record](ValidationReport.md), [approval checklist](ApprovalChecklist.md).

The Figma page contains **43 editable 1920×1080 static boards**, plus **three separate native motion timelines**: gallery/overlays, reduced-motion comparison and a complete GAIA chapter event storyboard. Static references comprise 38 bilingual boards (three gallery arrangements, nine other family/conditional compositions, Settings, AI and five chapter states) plus five additional performer Focus themes. GAIA's shared Focus board completes the six-theme set. These boards use discrete original artwork, editable text, reusable controls and three presence-depth components. They are not flattened UI screenshots or Unity renders. The Stage 2A pages remain intact. No new artwork upload or Figma AI image generation was required.

All 15 existing screen families receive a unified composition and state contract. Tutorial substeps, long conversations and every recovery variant share patterns; this package does not falsely claim 104 independently finished, runtime-verified screens. Deferred business functionality remains postponed until after the main overhaul and cleanup and a separate implementation task.
