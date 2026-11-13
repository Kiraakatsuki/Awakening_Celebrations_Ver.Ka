# Unity feasibility and performance risk

Target remains **Unity 6000.4.7f1, URP 17.4 2D renderer, UI Toolkit**, at a provisional **1920×1080 steady 60fps** on the user’s ROG Strix G16 / RTX4060 laptop 8GB VRAM / 16GB RAM. No runtime measurements, renderer experiments or package changes occurred. Stage 1’s serialized configuration remains the baseline.

| Design technique | Feasible path / boundary | Risk / approval |
| --- | --- | --- |
| Layout, color, borders, radii, type | UI Toolkit/USS, shared tokens and family presenters | Exact font/focus behavior and responsive hit mapping need validation |
| Local feedback / transforms / opacity | USS transitions or a single scoped C# presentation owner | Browser easing/scheduling is reference only; do not add a second navigation owner |
| Art/video surface | Existing artwork and single VideoPlayer→RT/Image path | Preserve VP8 intent, first frame, real loopPointReached, cancel and configured missing-video behavior |
| Tinted/opaque glass | Solid/translucent fills and authored art; recommended baseline | Maintain contrast under all palettes; no dynamic capture cost assumed |
| Static diffusion | Preblurred approved background artwork | Asset variant memory/crop review; never blur a complete UI screenshot into production |
| Dynamic backdrop | Shared bounded downsample/blur source, only where actual moving content needs it | Needs explicit compositing source/pass architecture approval; no native backdrop-filter assumption |
| Underlying UI blur | Compose a lower UI layer offscreen, then sharp overlay above | Current camera-only capture omits screen-overlay UIDocuments; panel/input/order changes are major scope |
| Shadow / rim / edge refraction | Bounded authored texture or verified custom material | No browser box-shadow/refraction parity assumed; never refract text or hit geometry |
| Reduced effects | Opaque reading surfaces, no dynamic blur; retain placement/selection | Visitor preference is still deferred; static review override is not functional setting |

A has the simplest baseline. B’s floating surfaces demand the strongest guard against overdraw, too many independent captures and contrast loss. C mostly changes composition but full-bleed art crops and target geometry need careful review. No concept requires a new camera source or duplicate video decoder. Installed Shader Graph/URP does not prove a particular effect is ready.

**Bounded later experiment:** start with quarter-dimension 480×270 targets at FHD, one shared pair, 5–9 blur taps per axis, update static content only when changed. This is an experiment proposal, not code or a performance result. An FHD RGBA8 buffer is ~7.91MiB; two quarter-dimension targets ~0.99MiB. Composited source, masks, decoder surfaces, camera pool, HDR formats and driver overhead add cost; “8GB GPU” alone does not settle a budget.

Use Stage 1’s capture protocol after explicit runtime permission: identical route/locale/performer/quality/power conditions; p50/p95/p99/max frame times; CPU readback/inference/GC; UI repaint/overdraw; video preparation and repeated-cycle resources. Proposed headroom includes <=12ms p95 active CPU/GPU each, <=1.5ms incremental dynamic-glass GPU target and stable memory; these are tentative acceptance targets, not guarantees. CPU and GPU work overlap rather than adding serially. Thermal/power profile and exact CPU remain to record.

Browser `backdrop-filter`, CSS shadows, DOM focus/inert and key events demonstrate appearance and temporal intent only. Figma effects similarly describe a reference. They do not prove Unity rendering, glyph fallback, accessibility APIs, interruption behavior or 60fps. No upgrade is a prerequisite; compare enhancement with the tint/opaque baseline before deciding to implement it.

Retain one capture owner and session-only consent. Do not start live camera tests, decoder tests, assistant requests, Player builds or profiling from this package. Production changes await design/architecture approval; deferred functional work still waits until after primary overhaul and cleanup.
