# Unity feasibility, risks and validation experiments

For current Arc spatial effects, see [the addendum's feasibility assessment](SpatialAddendum/UnityFeasibility.md), which distinguishes2D transform/optical baseline from unvalidated CSS perspective and optional deformation. No additional camera, world-space conversion or video pipeline is required by the baseline.

Static baseline reconfirmed: Unity 6000.4.7f1; URP 17.4.0; all six Quality overrides reference UniversalRP, which references Renderer2D; no Graphics default pipeline; renderer feature list empty; opaque/depth textures off; render scale1 and MSAA1. PanelSettings has targetTexture null, render mode0, constant pixel scale1, reference1200×800 and no textSettings. Built-in blur shader slots are not evidence of supported backdrop composition. Runtime graphics API/quality are unmeasured.

Provisional exhibition target: **1920×1080 at steady 60fps**, ROG Strix G16, RTX4060 laptop8GB VRAM, RAM16GB. Exact CPU, DPI/display scaling, power/thermal profile and viewing comfort remain to record. No measured performance or Unity compatibility claim is made.

## Translation path

Use shared UI Toolkit structures: shell root, overlay host, stable presence wrapper with non-picking art child, identification, chapter surface, reading window, setting row, status and confirmation. Performer and locale are data, not copied roots. Keep catalog route IDs and provenance mapping while moving away from exported child-index coupling only in an approved migration. No speculative dependency or new general router is required by the visual language.

Version-specific check: Unity's published update places native `backdrop-filter` and `drop-shadow()` in6.6, and grid layout/`gap` in6.7. Do not infer those later APIs on this6000.4 project; translate the browser grid into flex rows/columns with explicit spacing. Existing element-subtree filters are a different mechanism from backdrop processing. That distinction supports the tint/opaque baseline and a separately approved enhanced experiment, rather than an engine upgrade assumption. [Unity UI Toolkit version update](https://discussions.unity.com/t/state-of-ui-toolkit-in-unity-6-7/1736756), [Unity built-in filter documentation](https://docs.unity.com/en-us/engine/6000.3/manual/uitoolkits/uielements/uie-uss/uss-filter/built-in-filters). The attempted exact6000.4 manual URL was unavailable to the research tool; local serialized assets and later version notes are not a substitute for runtime validation.

AwakeningApplicationController currently owns flow/session/assistant/video; ScreenDocumentManager activates families/overlay; RuntimeScreenCatalog maps routes/locales. A scoped presentation adapter may later translate committed state to visual motion. This is a proposed boundary, not authorized architecture code. One transition scope per page/overlay plane must cancel schedules/handlers on detach. Do not add a second controller that can advance states independently.

Stable wrapper rectangles map to panel space; decorative image transforms never enter gesture picking. Verify actual framework `worldBound`/panel scaling and current source mapping before implementation. Browser `inert`, WAAPI, CSS `color-mix`, filters and shadows are not portable APIs; implement supported behavior through validated Unity facilities. Figma depth components are design representations, not prefabs.

Baseline glass is tint/opaque reading fields with bounded authored rims/shadows if needed. Enhanced blur is separately authorized: source-content composition→shared downsample/blur→bounded mask→sharp UI. Camera color does not imply inclusion of screen-overlay UI. Underlying-UI blur may require offscreen panel/render ordering changes and major approval. No second webcam owner, decoder or automatic consent is permitted.

At FHD, one RGBA8 full-size buffer is ~7.91MiB; a pair at quarter dimensions is ~0.99MiB. These are arithmetic allocations only. HDR formats, source/mask textures, camera pools, media decoder buffers and driver overhead add cost. Avoid one blur source per panel/performer. Dynamic updates should be content-driven where possible; turn optional effects off when covered or cinematic. Six large PNGs in a browser do not establish Unity texture residency or video decode cost.

## Required experiments, after explicit runtime authorization

| ID | Experiment | Evidence and acceptance intent |
| --- | --- | --- |
| U01 | Baseline routes with all optional effects disabled | Identical Windows build/route/locale/performer/power conditions; p50/p95/p99/max CPU/GPU frame times, missed frames and memory; target16.67ms frame budget |
| U02 | Arc input mapping | Visible region diagnostics at1080p, Windows DPI variants and letterboxing; all six, gaps, pointer/gesture positions, no drift-driven target changes |
| U03 | Portrait reveal/idle cost | Compare static vs six transform/opacity children; layout/repaint/overdraw, allocations; stop hidden schedules; no ongoing layout animation |
| U04 | Tint/opaque contrast | Bright/dark/moving artwork/video, all accents, both locales, focus/disabled/error; measure actual composites |
| U05 | Shared dynamic backdrop spike | Baseline vs480×270 shared pair, bounded5–9-tap-per-axis study; capture render order/alpha and incremental GPU time; no capture feedback |
| U06 | Overlay interruption/focus | Open/close/reopen/minimize, Privacy nesting, locale switch and detached opener; one scope, no stale close or double navigation |
| U07 | Media lifecycle preservation | Prepare/first frame/real completion/cancel/error, configured missing clip10s/hold, late callbacks, repeated entry; same single VideoPlayer/owned RT path |
| U08 | Camera/privacy ownership | Consent denied/granted/revoked and Settings suspension with approved hardware scenario or safe mocks; no duplicate owner; immediate stop independent of fades |
| U09 | Font/localization/accessibility | TC glyphs/punctuation/fallback/atlas, long names/replies, CJK IME, keyboard/screen reader, physical reach/readability |
| U10 | Resource endurance | Repeated six-performer/locale/overlay/video cycles; stable retained trees, textures, requests and allocations after warmup |
| U11 | Reduced modes and thermal run | Same logical progress without drift/scale/stagger; opaque fallback; sustained laptop power/thermal/frame pacing comparison |

Initial *hypotheses* from Stage1 may use <=12ms p95 active CPU/GPU work each and <=1.5ms incremental enhanced-glass GPU cost as headroom goals. These require approval and baseline measurement; CPU/GPU overlap, so do not add those values as serial frame time. If the effect fails budget or contrast, ship the strong baseline and revise the enhancement. Do not raise resolution before measuring the combined camera/inference/video/UI workload.

No Play Mode, Edit Mode runner, Player build, camera, media decode, assistant request or profiling was run. Do not use another test route to bypass that restriction. No production scripts/assets/packages/settings changed. Current gesture values and video callback/fallback contracts remain preserved; renderer and architecture changes require separate approval and regression scope.
