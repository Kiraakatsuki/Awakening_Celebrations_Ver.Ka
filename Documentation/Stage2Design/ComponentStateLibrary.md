# Component and interaction-state library

Design patterns shared across all families; do not create a component for every locale/performance-stage combination. The Figma foundations page contains token bindings, bilingual text styles and 18 reusable button state components. The prototype’s **Components & states** view presents the visual state language; no preference/service outcome is simulated.

| Pattern | Variants / states | Shared contract |
| --- | --- | --- |
| Action button | primary, secondary, quiet; default, hover, pressed, focus, disabled, deferred, busy | One semantic focus target; 60–64px initial height; action dispatch immediate; state not communicated by opacity/color alone |
| Performer selector | portrait row (A), spatial pairs (B), index + targeted art (C) | Six identities/codes remain exact; idle, focus, point target, confirm, loss; targeting never selects |
| Shell action | normal/reduced; available/deferred/unavailable/current | Text label and reason; one capability authority; no generic global gesture assumption |
| Reading panel | Settings, AI, Privacy, Help; side/center/left concept placement | Stable title, constrained line length, scroll region, close, focus scope/opener restoration |
| Language selector | English / 繁體中文; selected/focused/busy | Session facts independent; actual Traditional Chinese family/fallback rather than Inter-only glyph assumption |
| Setting row | value + description + action; unavailable/applying/applied/error | Complete designed states; deferred controls disabled until D1; no fake toggle success |
| Assistant prompt/input | empty, composing, sending, reply, retry, failed, cancelled | Context distinct from active performer; user content not interpreted as rich HTML; no fabricated network activity |
| Camera/gesture status | off, suspended, starting, ready, no hand, lost, fault | Text/icon/shape; preview not permission; pointer/keyboard alternative |
| Portrait surface | artwork placeholder, preparing, first-frame reveal, playing, complete, failed, cancelled | Media event owns completion; layer transitions never replace callbacks |
| Feedback/status | information, warning, error, success; inline/banner | Human-readable recovery; no developer paths in visitor UI; no needless modal for every status |
| Confirmation | safe end, leave, consent choice | Clear consequence, Cancel, explicit affirmative action; safe defaults; privacy stop cannot wait for a decorative fade |
| Takeaway (conditional) | unavailable, content preview, preparing, ready, expired, offline/error | Real destination/expiry only when implemented; no pseudo QR motif representing a generated code |

## Shared anatomy and typography

Use a heading, short supporting sentence, grouped content and one primary action. Avoid card-within-card structure. Labels remain a part of one hit surface, not another Tab stop. Reading panels use 24px body at 1080p, 22px control labels, 18–20px metadata, with extra CJK line height. Width and height grow with content; mixed performer names and translated labels wrap deliberately.

Icons are optional while exploring; text-only controls keep meaning clear. Future icons must come from approved owned/library vectors with consistent optical size and not replace unfamiliar action names. Focus uses a neutral 3px ring with dark separation. Selection adds border/name/state text, so performer accent is not the only cue.

## States requiring final refinement after direction selection

The representative frames demonstrate major anatomy; full per-state pixel refinement is not falsely claimed. All listed states have a UX contract and bilingual copy inventory in `DeferredFeatureStates.csv`, while the motion study provides visual events. Next design work should refine actual button busy/error examples, assistant long-response layout, CJK IME interaction and contrast under the chosen art/video conditions. Native Figma variables/styles are design tokens; they are not installed Unity components or bindings.

Reduced-effects appearance keeps panel placement and hierarchy using opaque fills and stable borders. Reduced motion keeps focus and completion semantics with <=80ms opacity or immediate changes. The review toolbar switches illustrations outside the exhibition canvas; it does not implement the deferred Accessibility settings feature.
