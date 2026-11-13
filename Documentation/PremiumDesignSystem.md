# Premium design system and glass strategy

Stage 1 specification · 13 November 2026 · proposed values for review, not application edits.

## Identity: living light

An exhibition space with calm controls and expressive performer art: deep mineral backgrounds, warm readable text, restrained luminous edges and six distinct performer accents. Use layered materials to clarify action and depth. Let movement/artwork carry emotion; UI becomes quieter during playback. These are original principles informed by premium desktop/spatial interfaces, not a reproduction of a proprietary system.

Current tokens already establish a useful warm neutral/six-accent palette. Current screens mostly bypass tokens through inline values: 91 of 97 generated USS files are empty, and a search found no `var(...)` consumption in the exported UXML/USS. Make semantic tokens the source of truth during approved redesign, without reimporting exports or deleting provenance. Different component states should not require a new entire screen export.

## Proposed foundation tokens

Values use logical panel units at the initial 1920×1080 review viewport. Confirm physical viewing distance with comfortable interaction trials, rather than selecting a meter value from assumption.

| Role | Proposed value | Use / constraints |
| --- | --- | --- |
| Stage deepest / base / raised | `#0E1014` / `#171A20` / `#232831` | Calm field; distinguish page from controls without heavy outlines |
| Text primary / secondary / muted | `#EFEAE0` / `#C1BDB5` / `#A39F98` | Muted is still reading text, not decorative low-contrast metadata; validate actual composite |
| Accent GAIA / PANDEMIC | `#31D7F2` / `#B29A82` | Identity, selection and small highlights |
| Accent RE-BIRTH / WHITE-GUARDIAN | `#D8A9C1` / `#BFE9FF` | Preserve intended six-performer distinction |
| Accent CONSCIENCE / SANCTUARY | `#FF6A2A` / `#F3B95F` | Avoid large saturated bright fields |
| Success / warning / error / information | `#79D9B0` / `#F2CA7D` / `#FF9A9A` / `#94CDF5` | Always paired with icon/text; never replace performer identity with status color |
| Focus | Neutral high-luminance ring + dark separation, 3px prototype | Visible against every performer palette; not only hover |
| Space scale | 4, 8, 12, 16, 24, 32, 48, 64, 96 | 8px primary rhythm, 4px optical adjustment; clear group separation |
| Radius | 8 compact, 12 control, 20 card, 28 panel, 36 cinematic surface, pill only for compact actions | Avoid rounding every nested rectangle equally |
| Stroke | 1px quiet, 2px selected, 3px focus | Stroke/shape/text distinguish state even without color |
| Targets | 48px minimum initial pointer target, 56–64px preferred exhibition actions | 8px separation; visible hit surface; test at actual display/DPI and reach |

These contrast values are design requirements, not a claim of standards certification: aim for at least 4.5:1 normal text, 3:1 large text and meaningful non-text boundaries, using 7:1 where practical for primary reading on a kiosk. Measure composited colors rather than foreground token alone. Current `#787671` over `#171717` is approximately 3.95:1 before opacity, so it is unsuitable for ordinary small text; assistant placeholder uses a similarly low-contrast value. Alpha and moving backgrounds can reduce it further. Use an opaque/tinted reading region when contrast cannot be maintained.

## Typography and bilingual rules

| Role | EN initial size / line height | Traditional Chinese initial size / line height | Behavior |
| --- | --- | --- | --- |
| Hero / performer | 56–72 / 1.1–1.2 | 52–64 / 1.2–1.3 | Brief titles only; Source Serif or approved display face sparingly |
| Page title | 36–44 / 1.2 | 36–42 / 1.3 | Wrap, no clipping; one meaningful title per context |
| Section | 26–30 / 1.25 | 26–30 / 1.4 | Align to shared content rhythm |
| Body / assistant | 22–24 / 1.45 | 24–26 / 1.5–1.65 | Provisional exhibition reading size; scroll long replies |
| Control label | 20–22 / 1.3 | 22–24 / 1.4 | Height grows with wrapping, no duplicate label focus target |
| Caption / status | 18–20 / 1.4 | 20–22 / 1.45 | Avoid critical instructions at caption size |

Use existing Source Sans 3 for Latin if editorially approved; choose a Traditional Chinese family and fallback covering the actual corpus. Current Noto **SC** fonts are not proof of appropriate Taiwan/Traditional Chinese glyph forms, even when a codepoint exists. No new font dependency is installed in this stage. `FontCoverage.csv` records assigned-font cmap deficits, not final fallback rendering. Panel text settings currently have no explicit asset; specify and verify a deliberate fallback stack, punctuation, ellipsis, emoji policy and numeric metrics.

Keep internal `en`/`zh` and `AppLanguage.Chinese` identifiers. Visitor labels should say English / 繁體中文. Use bilingual string keys for new components; do not find elements by text or clone positional indices between languages. Allow longer English labels and multi-line Chinese; apply locale-specific line height and appropriate punctuation. Avoid forced Western uppercase/tracking for Chinese. Test mixed performer names, full-width punctuation, unavailable reasons and 3,600-character input. Define actual multiline/IME submission policy before coding.

## Responsive composition

Start with 1920×1080 at 60fps on the user's ROG Strix G16, RTX 4060 8GB, 16GB RAM. Maintain this display target unless measured evidence justifies a higher resolution. Exact CPU, power profile and monitor DPI remain to be recorded during validation. Rendering 4K has four times the pixels of FHD; it is not a default quality improvement for a mostly fixed-coordinate UI.

Use content-driven flex containers, maximum reading widths and aspect-preserving art regions. Initial safe margins 48–72px at 1080p; body column 720–960px; Settings/AI panel width approximately 640–920px with constrained max-height and independent scroll. Assistant conversation should fit its panel, not retain the current 1,200px absolute width. Controls remain reachable without hovering across moving art.

Proposed layout variants: >=1600 logical width = full exhibition layout; 1280–1599 = compressed gutters/stacked Settings details; below 1280 = usable scrolling/fallback layout pending supported-window decision. At 1440p/4K, scale logical composition to maintain physical readability rather than shrinking text through constant-pixel defaults. Panel scaling and hit-test coordinate transforms must be reviewed together. Do not distort video aspect or move a pointer target away from its artwork. User trials determine final viewing distance.

## Component/state system

| Component | Required states | Presentation / contract |
| --- | --- | --- |
| Primary / secondary / quiet button | default, hover, pressed, focus, disabled-with-reason, busy | One semantic action, same hit area for all inputs; activate immediately; busy state prevents duplicate command |
| Performer card | idle, hovered, focused, acquiring gesture, targeted, confirmed, unavailable | Accent edge + shape/name feedback; targeting doesn't confirm; preserve six mappings |
| Shell action | available, deferred, unavailable, selected overlay | Persistent label/status, subtle busy indicator; no unexplained dead icon |
| Glass panel / modal | opening, open, closing, reduced effects | Solid reading zone; focus scope; backdrop input blocker owned by overlay authority |
| Camera indicator | consent off, suspended, warming, ready, no hand, lost, error | Distinguish device vs tracking; retry/alternative described; preview is separate |
| Assistant | empty/start, editing, sending, success, network error, invalid response, retry, cancelled | Stable input, visible request status; no fabricated reply/typing stream; clear close |
| Video surface | placeholder, preparing, first frame, playing, completion, failure, cancelled | Actual player state drives; unobtrusive loading/error; preserve configured fallback |
| Settings row | supported value, unavailable, unsaved/applying, applied, error | No fake functional toggles; C features remain deferred despite complete states |
| Souvenir (C) | no service, preparing, ready, expired, offline/error, cancelled | QR paired with readable destination/expiry; no mock QR represented as usable |

Icons use a consistent 24/32px optical grid, restrained 1.5–2px stroke and recognizable silhouettes; pair unfamiliar icons with text. Use owned vector/code-native assets or approved existing assets. Color accents cannot be the sole focus/selected/error cue. Shadows and glow are supporting depth cues with bounded area/opacity, never text glow for readability.

## Elevation and glass variants

| Plane / material | Fill proposal | Edge / depth | Backdrop policy |
| --- | --- | --- | --- |
| Base/artwork | Opaque stage or existing art/video | No decorative blur | Direct source |
| Quiet chrome | Dark neutral .72–.88 alpha | 1px white .10–.16, restrained top highlight | Tint-only baseline; opaque fallback |
| Reading card | Dark neutral .92–1.0 alpha | Subtle border, spacing provides depth | No blur dependency for legibility |
| Floating Settings/AI | Dark neutral .86–.94 plus opaque text subregions | 1px edge, soft bounded shadow | Preblurred static source or shared dynamic backdrop after validation |
| Priority dialog | .96–1.0 reading fill; .35–.55 scrim | Strongest separation, no decorative glow | Prioritize readable choice over effects |

Keep six accents at edges/selection rather than tinting all body text. Alpha values are starting hypotheses; bright art/video must never make text unreadable. Thin subtle refraction may be evaluated only at panel edges, never applied to text, icons or hit geometry. No live camera feed belongs in a reusable captured backdrop by default.

## Unity implementation feasibility and version boundary

USS/UI Toolkit covers flex layout, dimensions, padding, text, border/radius, background tint/images, opacity, transforms and transitions. Existing imported glass-looking surfaces are tinted rectangles/art; transparency is not backdrop blur. Shadow/glow can use a bounded preauthored texture/vector or verified custom material; do not assume browser `box-shadow` support.

UI Toolkit has subtree filters and custom UI shaders in this Unity generation, but subtree blur processes the UI subtree, including its text. The Unity UI team's documented distinction is that these filters do not provide background blur by themselves. [Unity UI team filter limitations](https://discussions.unity.com/t/feedback-request-custom-shaders-and-post-processing-filters/1680973). Native `backdrop-filter` is described as a later 6.6 feature; it is not a justified assumption for this project's 6000.4.7f1. [Unity UI team 6.7 feature/version summary](https://discussions.unity.com/t/state-of-ui-toolkit-in-unity-6-7/1736756). These sources inform a compatibility hypothesis; prototype the actual 6.4 path only in a separately authorized implementation/validation stage. No upgrade is proposed as a prerequisite.

## Recommended bounded Windows rendering strategy

1. **Baseline:** tinted translucent chrome + opaque reading surfaces; preblurred copies of approved static artwork where needed. This has no dynamic capture pass and remains the reduced-effects fallback.
2. **Dynamic enhancement, only if needed:** a shared backdrop provider consumes the already-owned artwork/video texture when that is the actual content behind glass. It downsamples and blurs once per required update, shared by all compatible surfaces. Do not open a camera or duplicate VideoPlayer to obtain a source.
3. **If underlying UI must also appear blurred:** explicitly compose the lower content layer to an offscreen target, then blur that lower layer and render the sharp shell/overlay above it. This changes panel/compositing architecture and needs approval, input-coordinate validation and rollback. A URP camera capture cannot be assumed to include screen-overlay UIDocuments. Prevent the glass output from feeding back into its own input.
4. **Effects ceiling:** no per-card full-screen captures, no multiple independent blur stacks, no full-screen refractive text. Prefer selective floating panels, not glass on every object. Use no runtime blur during uninterrupted Living Portrait unless an approved actionable surface is actually visible.

Initial experiment: quarter width/height (480×270 at FHD), separable 5–9 tap blur per axis, one shared source and ping-pong pair, refresh only while an actionable panel needs moving content. Radius and refresh cadence require visual/performance validation; a stale 15/30Hz backdrop is not automatically acceptable. Static content refreshes on change. Hide/release or safely retain bounded resources according to measured re-entry cost. Keep straight/premultiplied alpha and Linear/sRGB conversions explicit.

Memory arithmetic, **not measurements**: an FHD RGBA8 texture is ~7.91MiB; two 480×270 RGBA8 targets total ~0.99MiB. A full source copy plus that pair is ~8.90MiB before masks, UI intermediate targets, decoder surfaces, camera buffers, HDR formats or driver overhead. RGBA16F doubles those color-buffer figures; 4K quadruples pixel counts. Renderer2D currently has no custom feature, and opaque/depth textures are disabled. Any pass/material is a small authorized spike before production adoption, with no new dependency assumed.

Provisional acceptance on the supplied laptop: sustained 1080p60 under combined tracking + video + overlays, no recurring frame spikes due to capture/blur, sharp text, correct ordering/alpha, no residual resources after cycles, and readable opaque fallback. Concrete CPU/GPU/VRAM budgets and capture conditions are in [ValidationAndPerformancePlan](TransformationAudit/ValidationAndPerformancePlan.md). If enhancement fails, ship the baseline design; quality must not depend on blur being enabled.

## Design review gates

Approve tokens and bilingual typography on one representative core screen and one performer/AI panel before scaling to all 104 records. Approve actual service scope for C features separately. Then review the full state matrix, all six palettes, focus, bright/dark video and reduced-effects/static alternatives. No static audit can certify pixel fidelity or physical exhibition readability.
