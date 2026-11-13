# Performer-reactive themes and state authority

The neutral gallery retains #0C1117, common neutral glass and warm primary type. Stable targeting introduces one performer's atmosphere around the artworks and small context accents. It does not recolor all text, Settings values, assistant content or the entire screen.

| Performer | Main accent | Local atmosphere | Secondary reflection tone |
| --- | --- | --- | --- |
| GAIA | #5BD8ED | #137F91 | #3BAFAE |
| PANDEMIC | #C0AA91 | #756453 | #9A8163 |
| RE-BIRTH | #E0B0C8 | #8B557C | #B77EA5 |
| WHITE-GUARDIAN | #C5E9FA | #6A839A | #A5B6C4 |
| CONSCIENCE | #FF956A | #9B4B2B | #C87546 |
| SANCTUARY | #F1C57A | #98713B | #BB9456 |

The atmosphere plane is capped at10% opacity through a soft localized mask. The identification upper rim uses42% accent opacity over a1px strip, the selected artwork rim is restrained, and a small reflection remains subordinate. During overlays, atmosphere falls to2.5% and only a2px contextual panel rim at22% may retain performer color. Actual window fill, primary/secondary text, focus ring and input materials remain neutral. Opaque mode removes the light/reflection layers without removing state information.

## Visual-state contract

`SelectionStateMatrix.csv` provides bilingual state copy, triggers, ownership, motion/confirmation policy and recovery for every selection state. `spatial-model.js` is an isolated **presentation/arbitration specimen**, not a replacement application router or detector.

- **Neutral:** no target/theme; confirmation unavailable; all six remain discoverable.
- **Acquiring:** candidate/local rim only. If another theme was previously stable, hold that atmosphere until a new stable event; never flash through neutral.
- **Targeted:** authoritative stable target exists; blend atmosphere over280ms, pose over240ms. Gesture point alone never selects.
- **Changed:** new source candidate/stability replaces the old target directly. CSS color/transform transitions retarget from their current interpolated values; no intermediate neutral reset or stacked theme animations.
- **Observation gap:** immediately consume gesture acquisition and suspend fist confirmation; hold the last atmosphere while the existing source decides target loss. No new timer decides when it is lost.
- **Semantic target lost:** the source's cleared-target state triggers rest/neutral settling. Pointer/keyboard-owned targets are not erased by an unrelated gesture tracking gap.
- **Recovered:** readiness is not selection. Require a fresh point and stable target before fist; a retained atmosphere is context only.
- **Confirmed:** fist on the fresh gesture target, explicit portrait activation or supported1–6 commits once. Latch the chosen theme through the650ms bridge into Focus. Stale confirmations cannot select another performer or navigate twice.
- **Overlay open/closing:** modal scope wins. Retain only quiet contextual color, invalidate gesture acquisition, suppress competing movement and preserve common reading glass. After dismissal, restore focus/context and require a fresh gesture target.

## Input priority and handoff

Priority is modal/privacy input scope → already committed transition → explicit activation → keyboard-owned gallery focus → pointer-owned hover → ambient gesture target. A deliberate keyboard target cannot be recolored by a resting pointer or incidental gesture update. A pointer hover owns the visual theme until it leaves; keyboard navigation can take over. Explicit pointer/keyboard confirmation can choose its activated identity directly, as the existing alternatives do. No generic gesture click on global controls is added.

Releasing an input owner preserves only visual context; it does not leave a confirmable gesture behind. Overlay opening, reflow, locale replacement and tracking gaps consume gesture acquisition. Focus restoration can identify the selected performer without becoming a fresh MediaPipe event. The authoring controls can explicitly hand off ownership for review; those controls are outside the visitor interface.

Pointer hover and keyboard focus already identify an exact control and may become visually stable immediately. There is no new hover delay or simulated detector dwell. Gesture acquisition/stability/loss are supplied as explicit semantic review events; future integration must consume the current recognizer/bridge state rather than re-time it. Current source values such as .14s pose dwell, .8s cooldown and .35s tracking-loss reset remain untouched; their animation timing is independent.

## Localization and accessibility

State text is English/Traditional Chinese, exact performer names stay unchanged, and transitions never depend on translated text width. The six fixed regions and reading surfaces remain valid in both locales. Focus is a persistent neutral3px ring, not a performer-colored edge. Selection, acquisition and loss have text as well as color/light. Reduced motion and opaque fallback retain the same command/state behavior. CJK IME, assistive technology, physical readability, actual DOM geometry and live gesture mapping still require authorized verification.
