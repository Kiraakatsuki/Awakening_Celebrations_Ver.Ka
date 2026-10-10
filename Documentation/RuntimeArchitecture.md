# Runtime Architecture and Implementation Plan

## Verified constraints

- Unity project version is 6000.4.7f1. UI Toolkit package is present, with 106 localized UXML assets and 99 imported USS files.
- URP 17.4.0 packages are installed and the scene has a 2D Global Light. Graphics settings have no custom pipeline default, while quality profiles reference the project's URP asset. Rendering is UI Toolkit through `UIDocument` and `PanelSettings`.
- The shared `PanelSettings.asset` uses Constant Pixel Size, scale 1, and a 1200×800 reference resolution; Player default window resolution is 1920×1080. No setting was changed.
- The original Home interaction component bound connector snapshot paths and added hover/pressed/checkbox state; this implementation extends its actions and adds a navigation controller.
- The saved scene now activates both Home GameObjects and attaches the navigation controller, which disables the generic Privacy document until navigation requests it.
- No Unity Editor executable/process was available during this task, so Play Mode and compiler validation are not available here.

## Implemented runtime services

1. **Screen catalog** (`RuntimeScreenCatalog.asset`): explicit logical screen key + language → `VisualTreeAsset`, with source root provenance. No raw file paths or translated label text in navigation logic.
2. **Application flow controller**: one active navigation `UIDocument`, a back stack, and an assistant overlay stack. The existing Home documents stay alive; the navigation document is disabled on Home and receives the current UXML on navigation.
3. **Session state**: language, session-only consent, selected performer (starts unset), current screen, and `hasAwakenedThisSession`. Awakening is set when Color Release completes to Performer Select and stays memory-only until process exit.
4. **Control bindings**: `RuntimeNodeBindings.json` validates Figma node ID → imported root ID/path/tag; the controller then validates path bounds and runtime tag. Invalid bindings log an error and do not bind.
5. **Input**: pointer hover/pressed/click, Tab focus, Enter/Space activation on mapped controls, Escape/Backspace back/close, and 1–6 selection on performer-select/assistant-chooser screens. Click/keyboard on performer stage views is a desktop fallback for gesture cues, not sensor gesture recognition.
6. **Interaction state**: `HomeInteraction.uss` supplies opacity-only transient feedback so imported screen palettes remain intact. Focus is neutral white on imported screens and gold only on Home. Home language/checkbox state remains a persistent selected/checked state; unavailable controls are disabled and removed from hit-testing and keyboard focus.

## Implementation plan

1. Preserved and integrated Home language, hover/pressed, consent and focus behavior; repaired startup by enabling both Home UIDocuments and hiding the former active Privacy document until navigation.
2. Added explicit localized screen registry and flow/session controller.
3. Added English tutorial UXML variants from exact English Figma node text/path data.
4. Bound verified routes for Home, Privacy, tutorial, Attract, Color Release, Performer Select, performer stage chains, Settings, assistant chooser/overlays, Closing and Souvenir. Settings Language, Privacy, Ask assistant and Back work across both languages; unverified Sound, Accessibility, Tracking, Restart, and Guide actions are marked unavailable.
5. Added fail-closed ID/path/tag/root checks; no visible-text selectors.
6. Added consistent transient interaction states, one focus stop per multi-layer action, cleanup on screen replacement, and Home reset/focus behavior on navigation.
7. Static file/mapping checks completed. `Tools > Awakening Celebrations > Validate Figma Runtime Mapping` checks connector snapshot/catalog consistency in the Editor and the same check runs before builds. Unity compilation, Play Mode navigation/lifecycle tests, and visual comparisons remain unverified because Unity Editor is unavailable.

## Interaction and integration limits

- `Documentation/InteractionAudit.md` records the static control inventory, state model, unavailable controls, and manual checks.
- Sound has no audio runtime to control; Accessibility has no verified options; Tracking has no camera/calibration service; Restart has no confirmed destination/reset scope. Those Settings rows are explicitly disabled and do not change application state.
- Assistant prompts/Ask are disabled because there is no assistant service. The Focus screen's prototype Assistant entry has no corresponding imported UXML node. Souvenir navigation works, but QR delivery has no runtime generator/destination.

## Re-import behavior

Snapshot paths are conditional selectors, not permanent IDs. Every bound action checks its Figma ID, expected path/tag, owning root, and runtime tag. Adding or moving an element can invalidate paths; styling/text-only changes usually leave hierarchy paths intact but must still be revalidated. English/Chinese screens use explicit registry pairs; no positional parity is used without the imported per-language node records.
