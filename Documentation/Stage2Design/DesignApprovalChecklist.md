# Design approval and unresolved decisions

**Status: all decisions below are proposals pending user review.** Stage 1 completion and Stage 2A authorization approve exploration, not production migration, a renderer, new service or destructive cleanup. The only external transfer specifically approved during this task was uploading the six existing performer artwork images to the new Figma file.

| ID | Decision | Recommendation / options | Required before |
| --- | --- | --- | --- |
| D01 | Primary visual direction | A base + C cinematic restraint, bounded B highlights; alternatively pure B or C | Selected-direction detailed design |
| D02 | Shell / overlay placement | A lower band + right panel; one ordinary modal; actual opener restoration | Navigation implementation |
| D03 | Privacy and entry | Final camera vs AI data notice; decide whether full pointer participation can bypass current consent-gated entry | Core experience implementation |
| D04 | Protected cinematic access | Defer Settings/AI, offer fresh explicit Open later; preserve existing video cancel | Global policy implementation |
| D05 | Safe exit during Activation | Explicit immediate privacy stop and stable return proposed; current Activation Back restrictions differ | Any behavior change |
| D06 | Capture while overlays open | Settings/Privacy suspend via existing gate; assistant policy to choose; never auto-grant consent | Shell/camera integration |
| D07 | Session end/reset | Field-by-field reset: consent, awakening flags, selection, conversation, history, locale; next visitor route | Deferred End/Restart functionality |
| D08 | Typography | Source Sans/Serif + Traditional Chinese Noto TC proposal; approve family/glyph forms, licensing/bundle size | Font/system implementation |
| D09 | Material ceiling | Tint/preblur baseline; dynamic blur only if validated; no Unity upgrade assumed | Renderer spike/integration |
| D10 | Gesture reachability | Keep existing meanings/thresholds; pointer/keyboard global access; no new generic gesture click without approval | Gesture behavior change |
| D11 | Deferred scope | Include Sound/Accessibility/tracking status; merge Guide→Help; conditional calibration/takeaway | Complete UX scope freeze; functions remain D1 |
| D12 | Calibration | Optional only for demonstrated alignment need, with real algorithm and cancel behavior | D1 implementation |
| D13 | Souvenir | Approve useful content, real destination, expiry/retention/privacy; otherwise omit delivery from release | D1 implementation |
| D14 | Assistant scope/content | Approve factual context, transcript policy, backend behavior and copy; no invented performer facts | Assistant refinement/new knowledge services |
| D15 | Audio/media | Preserve muted video audio, completion callback, VP8 intent and missing-video configuration; approve soundtrack/timing changes separately | Performer changes |
| D16 | Hardware/quality | FHD60 initial target; physical viewing trials and exact CPU/DPI/power metadata; higher resolution after profiling | Production visual/performance acceptance |

## Review acceptance checklist

- [ ] Compare all three concepts across the same five views and both locales.
- [ ] Select a primary direction and explicitly identify any borrowed elements.
- [ ] Accept normal/reduced shell and modal priority/dismissal policy.
- [ ] Resolve entry consent, safe exit and visitor reset semantics.
- [ ] Review six-performer identity treatment, artwork crops and portrait UI restraint.
- [ ] Accept typography/contrast/reduced-effects direction; rendered Unity verification remains pending.
- [ ] Decide conditional calibration/Souvenir scope and Help/Guide merge.
- [ ] Review loading/error/unavailable recovery states, keyboard and CJK IME requirements.
- [ ] Accept that browser/Figma effects are references and no Unity performance guarantee is made.
- [ ] Authorize the next detailed design or implementation task explicitly; deferred business logic remains separate.

## What approval does not automatically authorize

Choosing a visual direction does not approve destructive cleanup, changing MediaPipe thresholds, altering consent ownership, changing video completion/fallbacks, installing packages, new backend/QR services, Unity upgrades or entering Play Mode. These remain separately scoped decisions and validation permissions. No production implementation should proceed until the applicable direction and architecture policies are accepted.
