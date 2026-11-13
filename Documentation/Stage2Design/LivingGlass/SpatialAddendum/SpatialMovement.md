# Spatial movement — exact proposed bounds

The approved Arc geometry is fixed. Only the artwork surface, illumination and small reflection layers move. Names, captions, hit wrappers, identification controls, keyboard focus rings and dock stay in their existing positions. B is preferred for review; these are **proposed caps**, not retroactively approved values.

| Property | A · restrained optical | B · controlled spatial | Reduced motion |
| --- | --- | --- | --- |
| Aperture scale relative to rest | 1.006× | 1.020× maximum | 1.000× |
| Horizontal surface translation | 0px | ±4px | 0px |
| Vertical surface translation | −2px | −6px nominal, −7…−5px with pointer position | 0px |
| Apparent pitch / yaw | none | X ±1.0°, Y ±1.4° | none |
| Artwork parallax inside aperture | none | X ±3px, Y ±2px | none |
| Reflection displacement | none | up to±1.5px, subordinate to artwork | none |
| Non-target recession | unchanged | .996× on decorative surfaces only | unchanged |
| Perspective reference | none | one1200px CSS projection on the decorative surface only | none |
| Target response | 240ms | 240ms | immediate; color/opacity<=60ms |
| Return to rest | 340ms | 340ms | immediate |
| Theme color blend | 280ms | 280ms | <=60ms |
| Confirmation into Focus | existing650ms intent | existing650ms intent | immediate/no spatial proxy |

Use a non-overshooting settle curve `cubic-bezier(.18,.82,.22,1)`. Theme changes use `(.22,.76,.24,1)`. This feels spring-like through deceleration; no arbitrary bounce, oscillation, new physical camera or long input delay is introduced. The isolated numeric projection clamps outliers and non-finite pointer inputs. Combined surface/art displacement stays small; it does not add an independent moving silhouette, flying label or UI wobble.

The image has fixed overscan to hide edges during parallax. Overscan is a crop provision, not another animated scale multiplier. Gallery scale is capped relative to the resting **aperture**. The later art-to-Focus expansion is a separately scoped650ms route transition, not permission to exceed the gallery's hover scale during targeting. Per-art focal placement, including GAIA's right anchor, remains intact.

## One dominant event

| State | Dominant event | Supporting effects / ceiling | Suppression |
| --- | --- | --- | --- |
| Neutral | None | Static Arc/material hierarchy | No continuous drift or attention loop in this addendum |
| Acquiring | Local target feedback | One low-opacity rim; no environment change or depth translation | No global color response before stability |
| Targeted/changed | One selected artwork response | One color-light plane, identification rim/index, one bounded reflection; other art may recede .4% | No page-wide parallax, captions or dock movement |
| Tracking gap | Semantic suspension/status | Hold last atmosphere; cancel gesture acquisition | No confirmation, theme flash or invented loss timer |
| Semantic target lost | Calm return | 340ms rest; 280ms atmosphere→neutral | No detector threshold changes |
| Confirmed | Selected artwork→Focus | One non-picking art proxy + old gallery recession/fade and new Focus surface | Stop target-follow/idle effects; one transition scope,650ms total |
| Settings/AI opening | Panel expansion | Gallery pose settles within120ms; context color reduced to2.5% light and22% narrow panel rim | No tilt-follow, parallax, reflection or gallery entrance competition |
| Protected chapter | Performer artwork/video | Existing reduced shell and allowed media/status behavior | All gallery effects removed; no floating Settings/AI above playback |

At most one environmental light plane is used; its background color interpolates directly from the current computed color. It is not six stacked fullscreen theme textures. A native Figma radial material illustrates localized illumination. The prototype uses a single color surface with a static soft mask, not dynamic backdrop refraction. Opaque fallback removes this atmosphere/reflection while retaining names, target state and small semantic accent.

During very rapid pointer changes, earlier artworks may still be completing bounded return-to-rest decays. The worst case is the six existing artwork surfaces; only the current target owns pose/theme feedback, no extra scene or texture plane is created, and each old response ends within340ms. Profile this worst case rather than assuming a single visual node is active. There are no independent idle motion loops in the addendum.

## Confirmation continuity and interruption

Neutral → acquire GAIA → stable GAIA → explicit confirm commits once. The current themed gallery is retained as a temporary inert visual layer, the selected artwork expands continuously toward the Focus art window, and the gallery gently recedes/fades. The actual route is already Focus; the650ms proxy owns presentation only. The same data-driven path works for all six. No video preparation/completion is invented.

Opening an overlay cancels gallery/proxy animation scopes before panel expansion. Route replacement cancels detached animation handles; completion callbacks only remove their own visual copies and never navigate again. The external scripted bridge is cancelled by new pointer/keyboard/review intent. Its450/850/1700ms authoring beats illustrate the story; they are not MediaPipe dwell/loss/cooldown thresholds.

## Geometry and motion access

Each `.presence` is a fixed composite interaction wrapper. `.spatial-surface`, image, rim, reflection, atmosphere and transition copies ignore pointer input; future UI Toolkit equivalents require `PickingMode.Ignore`. Pointer position is normalized against the untransformed wrapper rectangle, not the tilted art. The prototype projects only decorative descendants and coalesces pointer-pose updates into animation frames.

Keyboard/gesture targets use a quiet deterministic centered pose, avoiding noisy hand-landmark tilt. Targeting semantics, numeric alternatives and confirmation do not depend on pose completion. Reduced motion produces the same model states/commands without tilt, translation, scale, parallax, recession, reflection or proxy expansion. The<=900px review layout also suppresses spatial transforms; it does not change the FHD kiosk target or input rules.
