# Gallery Arc — spatial depth and performer-reactive theming

Stage 2B addendum · 13 November 2026. **Gallery Arc is approved as the composition. B is the user's preferred treatment, conditional on balance and precise targeting. The exact bounds below remain proposals for final review.** Production implementation is not authorized.

Open the [updated interactive prototype](../prototype/index.html), [review instructions](ReviewGuide.md), or [Arc spatial Figma page](https://www.figma.com/design/9VQRtMDk8vFGer7gktEYsB?node-id=33-2). The existing preview address remains `http://127.0.0.1:8767/LivingGlass/prototype/`. Keep the existing folder hierarchy for local-file viewing. No package installation is needed.

The approved Arc supplies the six positions, depth apertures, typography, identification plate and global dock. The live review now compares **A — restrained optical** and **B — controlled spatial** on that same Arc. Constellation/Salon remain historical exploration artifacts only; they are not choices in this prototype.

## Review artifacts

- [Native visual index](VisualIndex.md): **34 editable 1920×1080 Figma boards**, including A/B × six performers × both locales, neutral/acquisition/loss specimens and common-material Settings/AI.
- [A/B comparison](Comparison.jpg) and [six-theme overview](SixThemes.jpg). These derive from native Figma renders, not browser screenshots. Native B poses use 2D optical proxies; CSS perspective is a distinct unverified prototype effect.
- [Spatial bounds and motion budgets](SpatialMovement.md), [theming/input state specification](ThemeAndInputSpecification.md), [state matrix](SelectionStateMatrix.csv), [theme/bounds tokens](SpatialTokens.json).
- [Unity feasibility](UnityFeasibility.md), [validation record](ValidationReport.md), [approval checklist](ApprovalChecklist.md).

The prototype implements real visual transitions, direct theme changes, pointer-based portrait response, a scripted selected-theme→Focus bridge, interruption/cancellation, overlay suppression, both locales and reduced-motion/reduced-effects views. Review events are authoring inputs; they do not execute camera detection or real media callbacks. Gesture acquisition timing and target loss come from explicit review/source-semantic events, never new detector timers.

**Verification:** the pure presentation model passed 14 focused checks. Native wrappers were read back unchanged across all34 boards and representative native renders inspected. The browser's existing saved-policy denial remains unresolved; no new attempt or alternative browser/address was used. Actual DOM geometry, browser appearance/input/motion, Unity compatibility and frame pacing remain unverified. See the validation record for exact limits. No production Unity files, gesture code, consent behavior, media assets or video-completion rules changed.
