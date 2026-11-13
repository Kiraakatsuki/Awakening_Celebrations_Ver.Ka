# Components, layout, typography and accessibility

These are the Living Glass v1 design specifications for production review. Values live in `DesignTokens.json`; they are not imported into active Unity assets. Compose a small number of shared families rather than copying hundreds of absolutely positioned elements.

## Component boundaries

| Pattern | Variants | State/content contract |
| --- | --- | --- |
| Application shell | normal floating / reduced cinematic | available, deferred, unavailable, current; camera status and locale independent |
| Presence | Far / Mid / Near; Salon pair | performer ID/art/name; idle, focus, targeted, confirming, tracking loss; one stable hit wrapper |
| Identification | no target / target / confirming | index, name, concise line, explicit action; target is not committed selection |
| Action | primary / secondary / quiet | default, hover, pressed, focus, unavailable, deferred, busy; reason text outside disabled action where necessary |
| Reading window | Settings / AI / Help / Privacy | title bar, scroll body, close, optional minimize; modal ownership/opener restoration |
| Setting row | value / unavailable / applying / error | label, explanation, action/value; no fake deferred toggle |
| Assistant | chooser / empty / composing / waiting / response / failure | theme independent of performer; cancel and late-response guard; IME-aware editor |
| Chapter surface | Focus / Activation / preparing / playing / Release | artwork/media slot, localized status, permitted action region, protected shell |
| Status | information / warning / error / success | icon optional; text and shape communicate meaning independently of color |
| Confirmation | consent / safe exit / end/reset | explicit consequence, Cancel, scope; privacy denial immediate |
| Optional takeaway | unavailable / preparing / ready / expired / offline | real content/destination/expiry only; no pseudo QR |

The native Figma file reuses Stage 2A action components, bilingual text styles and isolated Living Glass colors. Three depth components solve actual caption sizing. It does not claim every state is a finished native component variant: busy/long-response/recovery pixel refinements remain acceptance work. The component/state contracts are complete enough to scope implementation and tests without inventing services.

## Typography and tokens

Use Source Serif 4 for English display, Source Sans 3 for body/controls; propose Noto Serif TC and Noto Sans TC for Traditional Chinese. Native font availability was verified. The local prototype uses existing Source font files and system Traditional Chinese fallback, including Microsoft JhengHei; this is not identical TC glyph rendering. Shipping fonts, license/bundle size, glyph atlas strategy and TC editorial approval remain required.

At FHD begin with display 64–82px, page title 44–48px, panel title 34px, body 24–26px, action 22px, performer label 22–24px and status 18–20px. English display line height 112%; Chinese display 130%; body 150%, increasing to 160–165% for dense Chinese. Names remain exact Latin identities. Never truncate WHITE-GUARDIAN or replace it with an unapproved translation. Use 8/12/16/24/32/48/64/96px spacing, 14px action radius, 24px identification, 28px dock and 30px window.

## Responsive composition

Design at 1920×1080 first. Gallery gutters 96px, six stable tracks, 32px gaps, art windows within fixed interaction columns. Reading content prefers 45–65 Latin characters per line and more generous CJK vertical rhythm. Reserve shell space; scroll a window's body before shrinking text or letting its Close control disappear. The title bar remains reachable.

Prototype breakpoints at 1500px and 900px are review studies. At constrained widths use a stable 3×2/2×3 composition, stop decoration during reflow, then recalculate input rectangles. The current serialized PanelSettings uses constant pixel scale and no target texture; changing its scale/reference behavior needs explicit validation. A reference resolution alone does not make the UI responsive. Do not use one arbitrary scale transform for both all text and gesture coordinates.

## Accessibility acceptance

- Prefer 60–64px controls at FHD; minimum initial target 48px with >=8px separation. Physical display/DPI/viewing distance trials decide final comfort.
- Persistent neutral 3px focus ring with dark separation. Target/selected/error use text/shape as well as accent. Opacity alone never explains unavailability.
- All six performers discoverable without hover or scrolling at the approved kiosk target. One focus stop per composite; logical left-to-right, then row order.
- Tab/Shift+Tab, Enter/Space, Escape and supported 1–6 remain meaningful. Overlay input cannot trigger the underlying route. Full gesture equivalence for global controls is not assumed.
- Minimize/Close are named; background inert while modal. Restore actual opener or selected gallery identity. Reflow/localization may not strand focus on a removed node.
- All visible navigation, privacy, status, recovery and deferred reasons have Traditional Chinese variants. Preserve route/session/consent independently of language.
- Respect OS reduced-motion in the prototype. Production preference controls remain deferred; their existence is not assumed. Opaque/reduced-motion baselines retain progress and confirmation meaning.
- Assistive-technology exposure, screen-reader reading order, Windows DPI/zoom, CJK IME and actual Unity font fallback remain unverified. UI Toolkit/package presence is not proof of accessibility support.

See `BilingualCopy.csv` for Living Glass-specific strings, `CoreStateCopy.csv` for shared loading/error/recovery wording, and `DeferredFeatureStates.csv` for inherited paired state copy. `StateContracts.json` separates semantic events from presentation. Final copy must be reviewed by a Traditional Chinese editor and checked with realistic long replies, not only short labels.
