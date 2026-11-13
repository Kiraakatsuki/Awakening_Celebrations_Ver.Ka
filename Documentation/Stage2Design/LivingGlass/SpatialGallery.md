# Refined spatial gallery

Gallery Arc is now approved. [The spatial addendum](SpatialAddendum/README.md) refines this composition in place; the prototype compares restrained A and controlled-spatial B on Arc only. The three original native studies below remain historical evidence. [Visual index](VisualIndex.md) links their original locale references; the addendum index is the current review entry.

## Arc geometry at 1920×1080

Reserve the upper 28–30% for identity, title and two-line guidance, and the bottom region for identification/status and floating navigation. Begin with 96px side gutters. The native board places the six-region row at x=96, y=340, width=1728, height=464. Six tracks are approximately 261.3px wide with 32px gaps. The browser uses proportional placement and a responsive row; it is a motion reference rather than a pixel-identical Figma export.

Use three artwork-depth variants inside the same **464px-tall interaction column**: Far (340px art, 38px top inset), Mid (370px, 16px), Near (400px, 0px). Arc order is Far, Mid, Near, Near, Mid, Far. Names occupy the remainder and wrap without clipping. Depth is conveyed through art aperture, inset, luminance and bounded shadow; no perspective transform alters the hit column. The prototype's closely related continuous proportions are an approximation pending rendered QA.

The identification plate is centered below the artworks, approximately 820×110px on the native FHD reference, with index, name, target state and a confirmation action. No target means “Six artistic presences / 六種藝術存在” and a disabled confirmation action. The plate does not cover the six art regions. Global navigation is below it. Camera/tracking text occupies a separate lower-left status area; neither overlaps a portrait label.

## Stable targeting contract

1. Each performer has one composite focus/picking region keyed by PER-001…PER-006. Decorative descendants ignore picking. No separate Tab stop for image and name.
2. Gallery reflow establishes six stable, nonoverlapping panel-space rectangles. Pointing uses the existing mapping/target selection authority; no new nearest-neighbour algorithm is silently introduced.
3. Only artwork children reveal/drift. Grid tracks, hit wrappers, navigation, focus outlines and identification controls remain stationary. Do not animate width, flex basis or hit-region bounds while input is active.
4. Hover, focus or pointing may identify a target. **Pointing never commits selection.** Existing MediaPipe fist confirmation remains the commit event; existing detector dwell/stability/release/cooldown values remain unchanged.
5. Pointer click and Enter/Space activation of a performer remain explicit confirmation alternatives; supported number keys 1–6 preserve their direct-choice meaning. The plate offers the same explicit confirmation without inventing a new gesture.
6. Tracking loss clears gesture acquisition, not the committed performer/session. Recovery requires a fresh point target before fist confirmation. Pointer ownership clears a stale illustrated gesture target in the prototype.
7. Reflow, locale change, overlay opening or page replacement invalidates old gesture acquisition and cached rectangles. Recalculate after committed layout, then use current source readiness rules. Never retain an old target through an interrupted route transition.

Native Figma variants initially clipped outer captions because instance child resizing did not persist. That was corrected with reusable Far/Mid/Near component masters, and dimensions were read back. `NativeRefinements.json` records the correction.

## Alternate compositions

Constellation preserves the six-column order and stable region grid, but alternates top offsets and art prominence. Treat the greater asymmetry as a design option; do not randomly drift columns. Salon uses three horizontally paired image/label compositions in each of two rows, with fixed row heights. The smaller-width study may use a 3×2 or 2×3 arrangement only after input mapping is recalculated and decorative motion is stopped for that reflow.

At the target exhibition display, all six must be visible without horizontal scrolling. No hover-only discovery, hidden carousel pages or obscured peripheral performer is acceptable. Below 900px the authoring prototype offers a vertically taller review layout; that is not an approved kiosk target.

## Verification still required

Overlay hit-region outlines in Unity and compare actual pointer/MediaPipe coordinates under Windows DPI scaling, panel scaling, fullscreen letterboxing and both locales. Confirm no target changes while art drifts, no gap activates a neighbouring performer, and no overlay input propagates into selection. Test all six, rapid target changes and tracking loss. Browser hit-region visualization is implemented but has not been exercised because access is blocked. No camera experiment is authorized here.
