# Three proposed directions

Stage 2A exploration, not approval. See [all frame links](VisualMockupIndex.md). Every concept uses the same six source performer identities, bilingual purpose, modal requirements and FHD target so the comparison is meaningful.

## A — Living Light

**Identity:** a calm mineral-dark gallery with warm editorial titles and luminous performer identity. Home separates reading from artwork rather than making text compete with the image. Selection offers six equal vertical portraits. Global actions sit on a consistent lower edge; Settings/AI occupy a right reading panel that preserves visible context.

**Materials/type:** dark stage `#101419`, raised `#212831`, warm white `#F3EEE4`; Source Serif 4 display and Source Sans 3 controls/body. Traditional Chinese uses Noto Serif TC display and Noto Sans TC controls in Figma. Use contrast-stable reading surfaces, a thin luminous rim, modest radii and deliberate whitespace.

**Interaction:** immediate fill/border response, visible neutral focus ring and a restrained selected-performer outline. Panel entrances 220ms and exits 160ms; stable page transition 280ms. Cinematic timing stays on the existing contract. No ambient motion is necessary to make the composition feel finished.

**Strengths:** clarity, bilingual capacity, discoverable six-way choice, straightforward shared components and a convincing opaque fallback. **Weaknesses:** less dramatic glass; six portraits need adequate width or a responsive two-row pattern. **Unity complexity:** low–medium for structure; optional dynamic glass is a separately evaluated enhancement.

## B — Fluid Glass

**Identity:** a layered spatial composition, deliberately different from A: artwork spreads behind an offset floating text surface; navigation is a floating dock; selection uses a 3×2 collection with image/text pairs. Settings/AI become broad central surfaces with generous two-column potential.

**Materials/type:** blue mineral field `#111C28`, raised blue-gray `#29394A`, larger 28–40px corners, soft edge illumination, bounded background diffusion. A tighter serif display and neutral sans maintain continuity, while material/structure carry the distinction. Refraction is an optional edge study only; no distorted text or controls.

**Interaction:** short depth-aware translations, intentional stacking and crossfaded surface intensity. Hover brightens the local edge; press acknowledges immediately. Dock movement must not become a target-tracking problem. **Strengths:** the richest material identity and strongest sense of floating layers. **Weaknesses:** most sensitive to bright backgrounds, wide overlay reading lengths and GPU bandwidth; crowded on smaller viewports. **Unity complexity:** medium–high. Native Figma blur and browser `backdrop-filter` are references, not an available Unity implementation.

**Fallback:** opaque blue-gray panels retain layout, edge hierarchy and spacing. If this loses the intended quality without blur, the concept fails the feasibility gate.

## C — Cinematic Spatial

**Identity:** the exhibition itself becomes the interface. Home uses a broad artwork field with a dedicated dark reading zone. Selection uses a vertical index and a large targeted artwork region rather than six equal cards. Settings/AI become a quiet left-side reading surface while the art remains spatially present.

**Materials/type:** near-black `#0A0D10`, warm editorial type, almost no ornamental card chrome. Performer accent appears in a name/selection rule; the art supplies color and texture. Large surfaces stay opaque where needed for reading. **Interaction:** gentle reveals, very little spatial travel, no moving targets. Persistent shell collapses to the edge during a protected portrait; no Settings/AI float above uninterrupted content.

**Strengths:** most immersive, most respectful of performer work, simple material baseline. **Weaknesses:** full-bleed crops need editorial review; vertical selection requires explicit numeric/pointing guidance and equal-access checks; language/status text must never sit over unbounded bright art. **Unity complexity:** medium, mostly composition/input mapping rather than a new renderer.

## Comparison and recommendation

| Criterion | A | B | C |
| --- | --- | --- | --- |
| Reading stability | Strong separate zones | Needs controlled opaque text regions | Strong if reading field enforced |
| Six-performer discoverability | Immediate equal portraits | Clear two-row grouping | Focused index; needs guidance |
| Global capability clarity | Conventional edge + side panel | Distinct dock + central panel | Quiet edge + left panel |
| Material richness | Restrained | Highest | Subtle atmosphere |
| Opaque/reduced-effects resilience | Strong | Must be demonstrated carefully | Strong |
| Relative implementation complexity | Low–medium | Medium–high | Medium |

Recommend **A as the base**, retaining C’s cinematic reduction and a modest B-like highlight for floating panels. This keeps premium craftsmanship dependent on typography, composition, feedback and continuity rather than expensive blur. Do not treat this hybrid as selected automatically: approve which A/B/C composition, shell, overlay placement and material ceiling should be refined. No measured performance ranking is asserted.
