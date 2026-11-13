---
name: figma-to-unity
description: "Translate approved Awakening Celebrations Figma references into bilingual UI Toolkit components, design tokens, and validated bindings. Use when a Figma frame or export is the design source rather than for general UI refactoring."
---

# Figma to Unity

Follow repository AGENTS.md. Confirm the approved reference and intended scope. Existing designs are references that may be adapted with a stated rationale for accessibility, responsiveness or runtime constraints; a frame does not prove a feature exists or is approved for implementation.

Inspect Assets/Figma/Awakening-Celebrations---Visual-Redesign/, including tokens.uss; consult Documentation/FigmaUnityFrameInventory.csv, FigmaUnityRuntimeRootInventory.csv and relevant screenshots/contact sheets as historical reference. Verify correspondence to current exports, Assets/Resources/RuntimeNodeBindings.json, the runtime catalog and controller bindings. Check installed com.unity.ui.figma before considering import changes. Do not reimport or overwrite exports as part of inspection.

Map colors, typography, spacing, radii, elevation and motion to reusable tokens with clear ownership. Separate stable component structure from performer, locale and interaction state. Preserve root/node provenance and logical route IDs; moving nested elements requires revalidating path/tag bindings. Do not bind by visible translated text or assume English and Chinese share identical child indices.

Adapt English and Traditional Chinese layouts for wrapping, glyph coverage, punctuation, line height and realistic assistant response lengths. Review all six performer variants and global Settings/assistant overlays. Explain reference inconsistencies; use feature-planning when a visual implies unconfirmed or deferred behavior.

Validate asset/USS references, binding integrity and locale coverage before visual comparison. Compare reference and implementation at matching viewport, scale and state when authorized evidence is available. Explain intentional deviations and remaining checks; do not claim pixel fidelity from static code. No Figma connection is mandatory: use supplied exports and report missing reference evidence.
