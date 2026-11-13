# Global shell and overlay design

The normal shell is one centered floating glass dock, separate from portrait artwork and the identification plate. It contains Settings / 設定, Ask AI / 詢問 AI, Help / 說明 and Privacy / 隱私. Language is in the stable header and Settings. Camera status is explicit; at constrained review widths it moves into contextual status rather than silently disappearing from the product specification.

## Overlay lifecycle

Settings and AI expand into a right-aligned, approximately 760px-wide window with a maximum height bounded by the reserved header/dock regions. The window keeps a consistent anchor and corner/rim treatment. The title bar has clearly labelled Minimize / 縮小 and Close / 關閉. Help, Privacy and confirmation windows reuse the same anatomy but omit minimize when it would obscure a required decision.

| State | Presentation and input |
| --- | --- |
| Closed | Dock available; no invisible panel intercepts input |
| Opening | Focus/input scope acquired immediately; visual panel settles 360ms; Close already works |
| Open | Background page and dock inert to ordinary interaction; Tab stays in the top dialog |
| Closing | Reverse from the current visual pose, not the original endpoint; background remains inert until removal |
| Minimized | Panel yields its modal scope; one labelled Resume chip appears outside art/identification; focus moves to it |
| Restoring | Reopen the same panel context; camera consent and selected performer remain unchanged |
| Replaced/cancelled | Cancel old animation and invalidate callbacks; detached panels cannot restore focus or modify routes |

At most one ordinary modal exists. Privacy/confirmation has priority. Settings→Privacy returns to Settings's privacy control on dismissal; it does not create an independent stack of generic routers. A remembered minimized window does not reopen automatically on route changes. Dismiss it when entering protected media or resetting the session. Exact transcript retention and reset behavior remain product decisions.

The prototype uses cancellable Web Animations, current computed transforms and generation guards. Its external “Overlay reversal study” illustrates opening→closing→reopening. This code is not an approved Unity overlay implementation and browser execution remains unverified.

## Protected cinematic states

Activation, portrait preparation, playback and protected exit retain a reduced, opaque edge strip outside the principal artwork. No floating Settings/AI window or attention-seeking badge overlays uninterrupted content. Nonessential entry is deferred; the next stable boundary offers a fresh explicit Open, never a delayed surprise popup. The prototype communicates availability and keeps Privacy and Exit options visible as design proposals.

Preserve the existing Living Portrait Back/cancellation path. Immediate safe exit during Activation remains an unapproved policy difference: the design dialog explicitly marks it as a preview, and production must retain current behavior until that decision is approved. Revoking consent must stop capture immediately through the existing gate, independently of decorative fades.

## Focus and input

Capture the actual opener, route identity and relevant focus key. Escape dismisses the top overlay before any underlying Back command. Restore focus to the connected opener; for a replaced tree use a stable component key and then an intentional route fallback. Returning to the gallery restores the selected performer, rather than always jumping to GAIA. Keyboard listeners respect text editors and IME composition; production Enter does not submit while a CJK composition is active.

Input priority is immediate privacy stop, top modal, editor/focused action, shell, experience. Overlay events must not reach the full-screen progression handler. One semantic action equals one command even if press, key and animation completion arrive together. No generic gesture-to-shell click is assumed; preserve gesture-supported experience commands and provide pointer/keyboard access globally.

## Availability and privacy policy

Available, temporarily deferred, unavailable and deliberately deferred functionality are separate states. Give a reason and a real recovery action where one exists. Session-only capture consent, capture suspension, tracking readiness and preview visibility remain separate. Current Settings routing suspends capture; an overlay implementation must preserve that expectation unless explicitly revised. Assistant overlay capture policy remains a decision because current contexts differ. No new camera pipeline, auto-consent or fabricated service status is permitted.

Locale changes retain route, selected performer, awakening state, consent and assistant context independently. Protected layout changes wait for a safe boundary. Existing conversation content is not silently translated or re-sent. Bilingual navigation and reason strings are part of the shared shell, not copied into 104 unrelated roots.
