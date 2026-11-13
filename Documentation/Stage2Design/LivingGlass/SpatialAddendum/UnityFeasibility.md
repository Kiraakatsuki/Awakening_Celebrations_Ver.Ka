# Transfer to Unity6000.4.7f1 / URP2D / UI Toolkit

The common dark glass, 2D translate/scale, bounded optical lighting, neutral text and fixed picking wrappers form the baseline. The browser's CSS perspective/pitch/yaw is **not** evidence of native UI Toolkit overlay perspective. Unity's6000.4 transform manual describes layout-independent2D transforms; a Unity UI Toolkit engineer specifically distinguishes perspective from overlay mode. [Unity6000.4 USS transforms](https://docs.unity3d.com/6000.4/Documentation/Manual/UIE-Transform.html), [Unity staff explanation of overlay perspective](https://discussions.unity.com/t/ui-toolkit-3d-like-card-tilt-x-y-rotation-without-clipping/1706726).

## Feasible baseline and optional enhancement

| Effect | Baseline proposal | Additional authorization/validation |
| --- | --- | --- |
| Stable selection geometry | One unchanged hit wrapper per performer; non-picking decorative subtree | Verify actual panel mapping, Windows DPI, source target authority and region invalidation |
| Scale/lift/recession | 2D transform of art child only; same proposed magnitude ceilings | Check bounds/clipping and transform order; do not animate flex/layout dimensions |
| Parallax | Move an overscanned art child2–3px inside a fixed aperture | Validate crop edges and texture sampling; avoid hand-landmark-driven noise |
| Pitch/yaw | Approximate depth with 2D placement, minor Z rotation, rim/light and scale if visually sufficient | Exact perspective/quad deformation is an optional bounded custom mesh/shader experiment; browser rotateX/Y cannot be copied as a guarantee |
| Atmosphere | One localized alpha surface, authored soft mask/tint; static neutral stage | Measure color interpolation/repaint/compositing and Linear-space result; no new full-screen post stack |
| Reflections/rims | One selected art rim/reflection and tiny contextual strips | May require authored texture/custom element; no text/hit-geometry refraction |
| Glass | Existing tint/opaque baseline and readable windows | True backdrop diffusion remains a separately approved source/pass experiment, not a requirement for movement |
| Confirmation | One non-picking artwork presentation proxy, old gallery fade, existing Focus view | Reuse single transition scope and generation cancellation; presentation never advances media |

Native Figma B boards use a2D optical pose as a design proxy, not true pitch/yaw or a shader proof. The standard fallback should be strong enough to keep the chosen language even if the perspective enhancement is rejected. Do not add another camera, world-space conversion, duplicate video decoder or gesture pipeline to reproduce a browser illusion. Such architecture changes are not proposed as the default.

## Required bounded experiments

1. Compare A, B optical baseline and any separately approved perspective experiment with identical1080p routes/locales/input/video/camera conditions. Record exact laptop CPU/DPI/power profile, frame-time distributions, repaint/overdraw and allocations. Provisional target remains1920×1080/60fps, unmeasured.
2. Draw all six **wrapper** bounds and compare pointer/MediaPipe selection through extreme decorative poses. Test gaps, captions, rapid changes, locale/reflow, overlay suppression and semantic loss/reacquisition. Decorative bounds must never replace wrapper bounds.
3. Check a selected artwork's crop at all proposed extremes, including GAIA's right anchor. Test near/far apertures, mask clipping, edge aliasing and alpha handling before adding richer light.
4. Exercise rapid A↔B/theme changes, confirmation during a response, overlay opening during the bridge, route cancellation, focus restoration and detached callbacks. One command commits once.
5. Compare reduced motion and opaque mode with the same semantic event sequence; confirm no timeline length or color animation substitutes for gesture stability/video completion.
6. Validate six palettes over bright/dark media, both locales, primary/focus/unavailable text and narrow panel rims. Keep reading surfaces neutral; measure actual composites and physical readability.

No experiment was run in Unity. No runtime test, camera access, scene/UXML/USS/C#/package/media/MediaPipe modification, import or architecture migration occurred. The fourteen pure-model checks validate isolated logic/bounds only; they do not measure Unity compatibility, browser smoothness or performance. All prior consent, video preparation/completion/cancel and configured missing-video contracts remain unchanged.
