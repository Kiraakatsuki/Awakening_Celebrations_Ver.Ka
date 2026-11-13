# Glass material system

The [Arc spatial addendum](SpatialAddendum/ThemeAndInputSpecification.md) now defines restrained performer-reactive accents and overlay suppression. Common window/reading materials stay neutral; the addendum's opacity caps and one-light-plane budget govern selection. Enhanced backdrop rendering is still a separate unapproved experiment.

The material system is defined by tint, edge, reading contrast and depth before blur. Figma/browser effects are reference treatments. Native UI Toolkit backdrop filtering is not assumed. Materials must retain their hierarchy in the opaque fallback.

| Tier | Uses | Base and light | Contrast/depth contract |
| --- | --- | --- | --- |
| G0 · Environment | Gallery/chapter base | #0C1117; original art; extremely subdued existing-art diffusion | No animated synthetic fog needed; full-resolution art remains sharp |
| G1 · Navigation | Floating dock, quiet controls | #23303F at ~91% opacity; 1px neutral edge, brighter upper rim | One bounded shadow, 28px dock radius; stable hit plane |
| G2 · Identification | Performer plate, contextual hint | #243343 at ~94%; performer light limited to index, lower edge and adjacent art | 24px radius; text stays neutral; no moving/floating panel position during targeting |
| G3 · Reading window | Settings, assistant, Help, Privacy | #243343 at >=94% over controlled scrim; #17212C opaque inputs/long-text regions | 30px outer radius, 14–15px controls; primary reading targets contrast rather than maximum transparency |
| G4 · Decision | Consent/end/exit confirmation | Opaque #17212C or #243343 with strong modal separation | Highest input priority, clear Cancel; no refracted text or translucent low-contrast affirmative action |

Initial highlights use a 1px white edge at 12–16% opacity and upper rim at 26–30%. Do not outline every nested group. Native boards use a conservative tinted baseline; the prototype adds browser blur as an explicitly unvalidated enhancement. Shadow proposals: dock 18px offset / 45px softness / ~38% black, panel 30px / 80px / ~50%, artwork 25px / 44px / ~30%. These describe appearance, not a promise that USS provides matching box shadows. Unity may use bounded nine-slice authored textures or a validated custom element/material.

## Baseline implementation path

Use UI Toolkit fills, borders, radii, source artwork and reusable shadow/rim assets where needed. Sustained text receives a reliable opaque/tinted reading zone. Static diffusion may be an approved preblurred artwork derivative, generated during an authorized asset step; no new runtime artwork was made here. This baseline needs no camera capture, renderer feature or dependency. It must be accepted on its own merits before adding enhanced glass.

## Separately validated enhanced path

If moving scene/video content genuinely needs diffusion, investigate one shared lower-layer source and bounded downsample/blur pair. At FHD begin with a quarter-dimension 480×270 experiment. Mask only the dock/window area, keep the upper UI/text layer sharp, and avoid feedback of the glass into its own source. The current overlay UIDocuments are not necessarily present in a camera color texture. Blurring lower UI would require explicit offscreen composition and input/sorting changes, which need architecture approval.

Edge refraction, if retained, is limited to a few pixels in a narrow perimeter and disabled during critical text/target feedback. It never changes hit geometry, warps art identity, duplicates video, or captures the webcam again. Large chromatic fringes and full-window liquid distortion are excluded. Compare straight/premultiplied alpha and Linear-space color explicitly.

## Fallback and validation

Opaque mode replaces diffusion with #243343/#17212C, keeps layout/rims/status and removes idle background diffusion. Reduced motion independently disables drift/spatial expansion. User preference functionality remains deferred; prototype switches are authoring overrides.

Measure bright/dark/moving art, both locales, six accents, focus/disabled/error states and font edges. Target >=4.5:1 normal text, >=3:1 large text/meaningful UI boundaries, and preferably >=7:1 primary kiosk reading. These are goals, not an all-frame contrast certificate. Never use blur as the sole readability mechanism. Performance/contrast/resource experiments are listed in [Unity feasibility](UnityFeasibility.md).
