# Proposed unified tokens and typography

These are Stage 2A proposals derived from Stage 1, not replacements for active `tokens.uss`. Figma uses scoped semantic variables aliased to primitives, bilingual text styles and reusable button states. `DesignTokens.json` is the reviewable machine-readable proposal; it is not imported into Unity.

| Role | Proposed value | Rationale |
| --- | --- | --- |
| A base / raised | #101419 / #212831 | Slight mineral-blue separation; calmer than pure gray |
| B base / raised | #111C28 / #29394A | Supports distinct floating spatial surfaces |
| C base | #0A0D10 | Artwork-first cinema field |
| Primary / secondary / muted text | #F3EEE4 / #C3C8CF / #A8B0BB | Warm title with legible neutral reading hierarchy |
| GAIA / PANDEMIC / RE-BIRTH | #31D7F2 / #B29A82 / #D8A9C1 | Preserve six identities; color plus name/border/state |
| WHITE-GUARDIAN / CONSCIENCE / SANCTUARY | #BFE9FF / #FF6A2A / #F3B95F | Identity without tinting all text |
| Success / warning / error | #92D9BC / #F2CA7D / #FFACAC | Add words/shape; never sole meaning |
| Spacing | 8, 12, 16, 24, 32, 48, 64, 96px | Shared grouping rhythm; 4px optical adjustment permitted |
| Radius | 12 control-quiet, 20 card/action, 28 panel, 40 expressive surface | Distinct material hierarchy; B dock may use a pill |
| Target/focus | 60–64px preferred action; 48px minimum initial target; 3px focus + dark separation | Exhibition readability/reach subject to actual viewing trials |

## Typography

| Role | English | Traditional Chinese | Initial size / line height |
| --- | --- | --- | --- |
| Display | Source Serif 4 Regular | Noto Serif TC Regular | 72–88 / 112% EN, 130% ZH; cinematic title may be 72 |
| Title | Source Sans 3 | Noto Sans TC | 44–48 / 130–150% |
| Section | Source Sans 3 | Noto Sans TC | 28–30 / 140–150% |
| Body | Source Sans 3 | Noto Sans TC | 24 / 140% EN, 150–165% ZH |
| Control | Source Sans 3 | Noto Sans TC | 22 / 140–150% |
| Caption/status | Source Sans 3 | Noto Sans TC | 18–20 / 140–150%; no critical instruction at decorative size |

Figma font availability/styles were queried and loaded explicitly. Local prototype copies existing Source Sans/Serif assets and uses system Traditional Chinese fallbacks (`Noto Sans TC`, `Microsoft JhengHei`); it does not bundle proprietary Windows fonts or install fonts. Native TC typography and local fallback may differ. Shipping TC font files, license/size, fallback coverage, punctuation and glyph forms remain approval/validation work. Do not reuse SC font naming as proof of Traditional Chinese correctness.

## Composition and materials

At 1920×1080 begin with 64–96px side gutters, a reserved 80–96px shell, 640–790px reading panel in A/C and a wider but constrained two-column surface in B. Reading content should stay around 45–65 characters per line. Preserve image aspect ratio and editorial focal points. Alternate viewports reflow rather than scale tiny constant-pixel type; the prototype has 1400px/900px responsive studies, not a commitment to support every kiosk dimension.

Material tiers: opaque artwork/base; quiet tinted chrome; opaque reading region; floating panel; priority confirmation. Highlights use small white edges rather than large glow. Shadows are bounded and may be authored textures in Unity. B may use a single shared lower-layer backdrop source only after feasibility review; no refraction of text/hit geometry. Reduced effects replaces diffusion with opaque fills and borders.

Motion proposals: 110ms feedback, 160ms exit, 220ms panel entry, 280ms stable navigation, 24ms stagger capped at 120ms. Existing cinematic 650ms fades/3s Activation remain separate preservation contracts. Reduced motion is immediate or <=80ms opacity without semantic skips.

Text contrast goals remain >=4.5:1 normal text, >=3:1 large text/meaningful boundaries, ideally >=7:1 primary kiosk reading. These are design targets, not certification. Verify actual composites over bright/dark/moving images. Reserve explicit dark reading zones rather than hoping blur provides contrast. TC/EN layout and six palettes require actual Unity verification later.
