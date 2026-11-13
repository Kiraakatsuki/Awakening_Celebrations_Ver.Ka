---
name: unity-ui-toolkit
description: Inspect or change Awakening Celebrations UI Toolkit structure, UXML, USS, UIDocuments, responsive components, navigation, or global overlay layering. Use for UI structure and behavior rather than glass shaders or Figma-only planning.
---

# Unity UI Toolkit for Awakening Celebrations

Follow repository `AGENTS.md`. This skill supports an authorized UI task; loading it does not authorize a redesign or Play Mode.

## Inspect before editing

- Confirm Unity version in `ProjectSettings/ProjectVersion.txt` (observed 6000.4.7f1). Read `Documentation/UIDocumentArchitecture.md` and verify against `Assets/Scenes/SampleScene.unity`, `Assets/Scripts/ScreenDocumentManager.cs`, `AwakeningApplicationController.cs`, and `HomeLanguageController.cs` in the same Scripts directory.
- Trace `Assets/Scripts/RuntimeScreenCatalog.cs`, its asset under `Assets/Resources/`, and `Assets/Resources/RuntimeNodeBindings.json`. Inspect the actual bound UXML/USS in `Assets/Figma/Awakening-Celebrations---Visual-Redesign/` before moving elements.
- Inspect `Assets/UI Toolkit/PanelSettings.asset`, window sizing, picking, focus, and enabled documents. Existing sorting orders are Home 0, main pages 10, assistant overlay 20; verify before relying on them. Language and performer variants share page-family documents.

## Make compatible changes

Preserve logical screen IDs, catalog EN/ZH mappings, and validated Figma root/path/tag bindings. A hierarchy change can invalidate bindings even if it looks identical. Reuse components only where their state and locale contracts match; avoid creating a second router or assistant overlay stack.

For responsive layouts, check flex sizing, min/max dimensions, text wrapping, image aspect ratios, overflow, and pointer/gesture hit areas at the exhibition resolution and supported window sizes. Maintain one focus target per composite action and restore focus after overlays close. Inspect global Settings and assistant access from each affected page family.

Verify UXML references, USS imports, binding paths, locale parity, document activation and event cleanup statically. If available, inspect the existing Editor mapping validator before running it. Report visual and runtime checks as pending when runtime testing is not authorized. Use the installed `unity:ui-uitk` skill for general framework mechanics when available instead of duplicating its guidance.
