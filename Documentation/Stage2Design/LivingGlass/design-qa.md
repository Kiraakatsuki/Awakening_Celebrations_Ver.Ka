# Living Glass design QA

**final result: blocked**

The current Arc-only A/B/theming addendum is also unverified in the browser. See [its validation record](SpatialAddendum/ValidationReport.md). Native34-board geometry and pure-model14-check results do not satisfy this visual/input gate. The previous source references below remain historical; current sources are indexed in the addendum.

This is the Stage 2B prototype QA gate. Native board review/static checks are recorded separately and do not count as browser proof.

**Source visual truth:** editable Living Glass page `18:48`, Arc `18:66`, Settings `18:401`/`18:969`, AI `18:485`/`18:1053`; corresponding native PNGs in `review-evidence/` and `VisualIndex.md`. Native source dimensions are 1920×1080, Arc PNG1600×900 and most other exports960×540. Contact sheets scale these sources for comparison. The source intentionally develops Stage 2A rather than pixel-cloning the old demo.

**Implementation:** `prototype/index.html`, `living-glass.css`, `living-glass.js`; intended FHD1920×1080 canvas with additional review-width studies. Implementation screenshot path, rendered CSS viewport, density normalization and source/implementation side-by-side comparisons are **unavailable** because the browser permission gate denied the origin. No substitute command-line browser or different address was used. There is no valid matched-state browser comparison or focused browser-region evidence.

## Findings and history

| Priority | Finding / evidence | Correction or remaining work |
| --- | --- | --- |
| P1 | Browser visual/interaction evidence unavailable after permission retries | Resolve saved/managed origin permission, then capture matched gallery/Settings/AI and both locales; remains blocking |
| P2, corrected in native design | Outer presence captions clipped in the first native Arc render | Reusable Far/Mid/Near masters replaced failed instance child resizing; post-fix native Arc render and dimension readback show labels visible |
| P2, corrected in native design | Native chooser did not visibly expose all six assistant contexts | Added six artwork/name contexts. Prototype already has data-driven six-context anatomy; browser result unverified |
| P2, corrected in native design | Salon auto-layout rows clipped artwork to shallow strips | Set both locale boards' rows/cells to210px height, moved gallery start to350px, refreshed native exports and comparison sheet |
| P2, corrected in native design | GAIA's centered narrow crop cut into the face | Right-anchored the original image in native narrow slots and specified the matching browser focal position; inspected post-fix native Arc |

The corrected native findings are not assertions that the browser prototype matches. Native source and browser proportions differ intentionally in responsive studies; a matched FHD pass must decide which implementation differences need reconciliation rather than claim pixel fidelity.

## Required fidelity surfaces

| Surface | Native evidence | Browser/Unity gap |
| --- | --- | --- |
| Fonts/typography | Source Sans/Serif and Noto TC families read back; native TC Settings inspected | Actual browser TC fallback, long labels/replies, Unity atlas and IME pending |
| Spacing/layout rhythm | Fixed six-region wrappers and corrected caption sizing; comparison sheets exported | Need matched FHD and constrained-width captures, header/art/status/dock separation and modal scrolling checks |
| Colors/tokens | Isolated Living Glass variables; dark tinted material baseline | Browser material effects and actual art/video contrast not measured |
| Image fidelity | All native hashes reference six approved original artwork images | Browser asset loading/crops and actual Unity import/residency pending |
| Copy/content | Paired Living Glass and deferred-state inventory, exact performer names | Native-speaker copy approval, source tutorial substep binding and long-content acceptance pending |

## Implementation checklist

1. Unblock browser permission without bypassing the gate.
2. Open the prototype; capture source/implementation at the same viewport, locale, target and overlay state. Build full-view and focused comparisons.
3. Test all three gallery refinements, all six confirmations, tracking loss/reacquisition, Settings/AI reversal/minimize/restore, modal focus, return focus and locale switches.
4. Test a complete event sequence and reduced/opaque alternatives. Inspect console errors and screenshot every family/locale. No camera, real video or assistant request is needed.
5. Correct any P0/P1/P2 differences, recapture and compare. Only then change this gate to `passed`. Unity acceptance remains a separate authorized task.

No primary browser interaction or console-error check is claimed as executed. No production implementation should be approved on the basis of a syntax pass alone.
