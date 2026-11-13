# Validation and performance plan

Stage 1 · 13 November 2026 · filesystem-only results plus pending checks. Baseline commit `b9f0972b38008b31e4bd2117ee88cb34ef9ce6fe`.

## Checks actually performed

| Check | Result | Limits |
| --- | --- | --- |
| Repository guidance and nine skills | Read before source audit; applied in coordinated blueprint | No application permission inferred from skills |
| Catalog keys/locales | 104 entries, 52 route IDs, 52 en + 52 zh; no duplicate route/locale key or unpaired route | Does not establish translation quality or reachability |
| Catalog assets | All 104 GUIDs resolve to existing UXML | Filesystem check, not AssetDatabase import/compilation |
| UXML parse | All catalog XML parses | USS/UXML compiler and rendered layout not run |
| Local UXML `Assets/` references | No missing target after URL decoding | Does not validate package import IDs, image decoding or all references inside every asset type |
| Manifest vs snapshot | 104 rows each; root/asset/node ID/path/tag/classes agree | Raw JSON differs because manifest carries extra design metadata; that difference is not a defect |
| Snapshot→actual XML path/tag | 251 mismatches after controller remaps across 15 localized assets; detail in `StructuralDrift.csv` | Includes decorations and intentional edits; **not 251 broken controls**; matching tag alone cannot establish semantic identity |
| Scene document state | 16 documents: two enabled Home order 0, thirteen disabled main order 10, disabled overlay order 20 | No scene opened or lifecycle executed |
| Six video slots | All resolve in correct performer order | Actual importer output/codec, preparation, decode, video completion not executed |
| Font glyph source check | 363 Han-containing labels in 51 UXML files lack characters in explicitly assigned Inter font cmap | Actual fallback chain/rendered glyph failure unverified; `FontCoverage.csv` records labels |
| Static type/style census | 3,001 XML elements, 2,585 inline absolute positioning, 847 labels explicitly <18px; 91 of 97 USS empty | Counts include XML wrappers/styles, do not themselves prove layout defects |
| Basic token contrast arithmetic | Current `#787671` / `#171717` ≈3.95:1 | Opaque token calculation only, not composited runtime validation |
| First-party tests under Assets | No `*Test*.cs` outside imported samples found | Not an assertion about all vendor/package tests or external CI |
| Source lifecycles / tools | Inspected current app, gate, bridge, runner/config, player/request cleanup; validator and screenshot helper read only | Existing validator does not traverse actual UXML; screenshot helper requires runtime and writes files, not run |

`static_audit.py` is a documentation-side evidence utility. Run from repository root with Python to regenerate `StaticEvidence.json`, route/text/drift CSVs. It reads project files and writes only in this directory; it invokes `git rev-parse` but no Unity, camera, network, imports or application code. Font cmap scan uses already-available fontTools and local package-cache Inter, without installing anything. CSVs carry UTF-8 text. These artifacts are not a Unity test suite or proof of feature functionality.

## Checks not run

Unity compilation/Console/import validation, Editor mapping menu/prebuild hook, Edit Mode/Play Mode tests, Windows build/Player, GPU/CPU/memory profiling, subjective visual inspection, screenshot capture, actual camera/gesture recognition, assistant live requests, accessibility/screen-reader or physical viewing tests: **NOT RUN by Stage 1 restriction**. No current-runtime broken behavior is asserted as reproduced. AC-03 identifies a source branch inconsistency; AC-09 identifies static validator coverage, not a tested runtime failure.

No Unity MCP calls were needed for the allowed filesystem evidence. Do not use MCP, test runner, batch build, screenshot helper or standalone Player to bypass the runtime prohibition. Inspect every future Edit Mode validator/test for hardware, networking, runtime initialization and asset-writing side effects before invoking it.

## Required regression matrix after explicit authorization

| Domain / cases | Acceptance / evidence | Scenario precautions |
| --- | --- | --- |
| Startup/Home (both locales) | One visible locale, consent false, no capture; repeated language changes and preference persistence | Restart must not persist consent; don't auto-check consent |
| Privacy | Review, agree after checkbox, decline, Back, revoke/suspend and return; notice matches actual processing | Approve consent scenario explicitly; denial tests use no camera |
| Tutorial | All seven sequential steps plus return/dismiss, start/skip, existing Back constraints, EN/ZH | Distinguish intended restrictions from accidental missed binding |
| Attract/first awakening | Wave + existing click/Enter alternatives, Color Release presentation, selection readiness, no duplicate first awakening | Preserve thresholds and first-session flags |
| Selection | Six cards, pointer, 1–6, point→fist, lost tracking, target stability/release | Point alone never confirms; no duplicate selection on held pose |
| All six×five stages×two locales | **60 stage/locale cases**, correct artwork/name/code/clip, target focus, stage transitions | Do not confuse 60 stage presentations with 30 locale combinations |
| Video | Preparation delay, first-frame reveal, actual end→Release once; cancel during prep/play/exit; repeated same/different performer entry | Use owned player only; mocks cannot weaken production gate |
| Video missing/error | Each performer slot missing in isolated approved fixture, configured 10s auto fallback, auto off/timeout<=0 hold, decoder error | Do not overwrite production clip slots to make a test; preserve approved configuration |
| Back/session | Pre/post awakening, overlay topmost, video cancel, allowed history targets, Settings Home, Closing/Souvenir Done/re-entry | Record current contract vs intentionally approved new reset behavior |
| Shell policy (future) | Every route/phase×Settings/AI/Help/language/privacy; available/deferred/unavailable reasons | Deferred intents never surprise-open; protected stages don't lose completion |
| Overlays | Open/close/Escape/palm Back, repeated/reentrant requests, focus entry/trap/opener restore | No page click/Enter/fist leakage; one authority |
| Locale | Every stable family + overlay, history language, long text, CJK/Latin mix, IME input | Active video/request not silently recreated; review language change policy |
| AI | All six themes both locales: empty/3,600-char input, suggestions, PostVideo prompts, success, timeout, connection/HTTP429/5xx, invalid JSON/missing reply, Retry | Stub transport first; live request only if authorized; never log messages/credentials |
| Async lifecycle | Close/navigate/locale/disable/destroy/reset during request or media work; late response/re-entry | No stale UI/media completion; explicit cancellation and released ownership |
| Audio | Chime fallback, future approved clip mode, repeat clap and stop on exit; video remains muted | Deferred Sound settings tested only after D1 authorization |
| Camera | denied/granted/revoked, route suspension, preview hidden/visible independence, startup error/disconnect/retry, duplicate-owner rejection | One approved capture owner; no duplicate feed, consent bypass or actual hardware in isolated tests |
| Accessibility | Tab/Shift+Tab, Enter/Space, Escape/Backspace editing, scroll, no duplicate focus, labels/focus shape, no color-only state | Actual assistive-technology support must be assessed; module presence is insufficient |
| Visual/layout | Approved matched viewport/state references, all palettes/locales, bright/dark/moving video, readable opaque fallback, long assistant replies | This is future authorized validation, not performed in audit |
| Retained deferred features | Sound/accessibility/tracking/reset/help/Souvenir state/service matrices after D1 | No fake “applied,” calibration or QR success; final inclusion decisions first |

Isolated Edit Mode candidates after inspection: catalog/locale/performer mapping contracts, capability policy, route transaction rollback, topmost-dismissal precedence, session-field independence, completion generation rejection and non-hardware request cancellation. Keep pure data/state tests hardware-free. Runtime media/gesture proof requires separately permitted scenarios; a string-search test is insufficient.

## Hardware target and provisional performance criteria

User-supplied: **ASUS ROG Strix G16, NVIDIA RTX 4060 laptop GPU with 8GB VRAM, 16GB RAM, current Full HD display**. CPU model, GPU power limit, display refresh/DPI and chosen laptop operating mode are not supplied. Viewing distance should be established by comfortable reading and gesture usability rather than assumed physical dimensions.

Recommended starting target: **1920×1080, steady 60fps**, keeping native device capability/refresh confirmation as part of setup. Prefer improved composition/typography/frame pacing before increasing resolution. Higher resolution is optional only after a comparable workload passes. All numeric limits below are proposed engineering targets, **not measured results or hardware guarantees**.

| Measure | Initial proposed acceptance |
| --- | --- |
| Frame pacing | Target 16.67ms frame interval; inspect p50/p95/p99/max. Provisional p95 <=16.67ms, p99 <=25ms after warmup, no recurrent visible stalls; tighten on baseline review |
| CPU/GPU work | Initial p95 active CPU and GPU each <=12ms to leave headroom; they overlap and must not be added as serial budgets |
| Glass increment | Initial dynamic enhancement target <=1.5ms GPU p95 versus comparable baseline; fallback if combined workload misses target |
| Pointer/keyboard acknowledgement | <=100ms provisional, independent of backend/dwell; preserve existing gesture temporal semantics |
| Allocation / memory | No recurring avoidable allocation in steady UI loops; report actual GC frequency and stalls. No progressive growth across at least 20 repeated cycles after caches stabilize |
| Resource headroom | Aim to retain ~20–25% VRAM headroom and enough system RAM to avoid paging under venue workload; define process ceilings from measured baseline, not the GPU model alone |
| Loading / requests | Report prepare-to-first-frame, cold/warm AI response and cancellation times; immediate truthful status. Do not promise network completion latency; current client timeout 45s is a contract to review |

## Capture protocol (pending permission)

1. Record commit, Windows/driver/Unity/build versions, CPU/GPU names, plugged-in/power mode, resolution/refresh/DPI, actual renderer/quality/API and profiler attachment. Compare Editor and Player separately; Player captures need explicit permission too.
2. Start from original effects baseline. Warm application/decoder/model, record warmup separately, then sample comparable intervals (proposed 30–60s captures). Add a longer thermal session (proposed >=20min) only when runtime work is authorized. Log metadata, not camera imagery or assistant personal content.
3. Workloads: Home idle; tracking off; consented tracking + preview; Color Release targeting; each performer preparing/playing; AI overlay on stable eligible context; combined video/gesture/UI at approved state; baseline tint vs dynamic enhancement; repeated entry/exit and fault recovery.
4. Use CPU/GPU Profiler, Memory views and Frame Debugger as applicable. Record layout/repaint, render batches/overdraw/fullscreen passes, readback waits/pool skips, inference/recognition timing, arrays/GC, decoder/RT allocation and network cleanup. Bridge diagnostic counters aid investigation but do not constitute a measured baseline until captured.
5. Attribute one cause before optimization. Candidate costs: Windows CPU two-hand inference and CPUAsync readback; result copies; per-frame status strings; cursor left/top layout; preview repaint; per-decoded-frame video repaint; retained visual trees; high-resolution images/RTs and optional blur.
6. Compare distributions and spikes, first-frame latency, correctness and memory after identical cycles. Retain intentional latest-frame dropping if it improves latency; do not call it a bug solely because a counter rises. Do not pool/cap recognition data in ways that change semantic behavior without validation.

## Evidence and release reporting

Every future result records check ID, baseline/introduced distinction, route, performer, locale, conditions, expected/actual outcome, pass/fail/not-run status and evidence path. A release pass requires actual compilation/build and permitted behavioral verification; this static audit does not meet that gate. Keep stable finding IDs, close only with evidence, and retain known limitations.
