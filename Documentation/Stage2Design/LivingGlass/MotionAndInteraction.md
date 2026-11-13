# Complete motion and interaction specification

For current gallery targeting, idle, direct theme changes, loss and confirmation choreography, use the approved-Arc [spatial addendum](SpatialAddendum/SpatialMovement.md). It supersedes the original selection idle/target treatment below: no continuous drift,240ms target response,340ms return and280ms theme blend. Existing650ms cinematic and3s Activation contracts remain unchanged.

Motion communicates focus, ownership and continuity. The command takes effect independently of the decorative tween; a fade never impersonates detector readiness, a network reply or video completion. The isolated prototype implements visual motion with CSS and Web Animations. Browser execution is presently blocked, so the following is a specification plus authored implementation, not an observed smoothness result. [The visual index](VisualIndex.md) also links three native editable Figma timelines: 6s gallery/Settings/AI, its reduced-motion comparison, and a 14s complete GAIA chapter event storyboard. Their tracks/durations were read back; no motion-video render or Unity validation is claimed.

## Timing and easing

Shared feedback 120ms; target light 260ms; identity text 180ms; stable navigation 300ms; panel open/restore 360ms; panel close/minimize 220ms. Gallery art entrance 520ms with 20ms offsets, capped at 100ms total stagger. Interaction is not held behind the stagger. Existing cinematic intent remains 650ms transitions and 3000ms Activation. No MediaPipe dwell/stability/cooldown value changes.

Use `standard = cubic-bezier(.22,.76,.24,1)`, `settle = cubic-bezier(.18,.82,.22,1)`, `exit = cubic-bezier(.4,0,.8,.25)`. Settle produces a spring-like deceleration with no decorative bounce or overshoot. It is a curve approximation, not a measured physical spring. If a real spring is later evaluated, require critical/near-critical damping and no perceptible overshoot of controls; preserve these durations as initial comparison targets.

| Motion | Trigger / purpose | Properties and duration | Semantic completion / interruption |
| --- | --- | --- | --- |
| Gallery entrance | Stable route commits; reveal the curated grouping | Art child opacity .4→1 and y18→0, 520ms, 20ms stagger; captions/hit wrappers stable | Input regions exist immediately; confirm cancels pending decorative reveal |
| Idle atmosphere | Gallery remains active and unoccluded | Artwork-only y0↔−3px, 18s alternating, scale1.025 to prevent exposed edges; muted diffuse source | Stop when hidden/occluded, on route exit, or reduced motion; no target motion |
| Target acquisition | Hover/focus or semantic point identifies a performer | Image luminance/saturation and bounded light, 260ms; identification opacity/y3px, 180ms | Does not select; new target retargets from current values |
| Press / hover / focus | Explicit action feedback | Immediate activation; 120ms fill/edge/brightness; persistent 3px neutral focus ring | Focus is never delayed or replaced by performer color; press cancellation resets |
| Confirmation | Fist on fresh target, explicit pointer/keyboard activation | Target → chapter artwork continuity, 650ms; neutral UI settles separately | Selection commits once; duplicate input ignored by current state authority; cancelled visual callback cannot commit again |
| Identification | Target changes | One fixed glass plate; text cross-settle 180ms | Name/index/state update together; geometry does not chase target |
| Settings / AI opening | Available stable-shell action | Current pose → y0/scale1/opacity1, 360ms settle; initial y65/scale.94 | Scope/focus acquired immediately; close works during entrance |
| Closing / minimizing | Close/Escape or Minimize | Current pose → y65/scale.94/opacity0, 220ms exit; Resume chip at stable shell edge | Keep background inert until panel removal; minimize moves focus to chip; close returns actual opener |
| Reversal / replacement | Reopen while closing or open a higher-priority panel | Read current pose, cancel old tween, retarget; generation guard | Exactly one panel and focus scope; old close cannot delete the new panel |
| Stable navigation | New route commits | Opacity .45→1 and <=8px translation, 300ms | Router owns route; animation does not push history; rapid Back/language/overlay cancels detached work |
| Focus→Activation | Existing both-palms or manual play action | Preserve 650ms transition intent, then 3s bounded luminance passage | Existing state/timing authority governs progression. Prototype uses explicit review beats instead of pretending to run media |
| Portrait preparation | Actual prepare request | Artwork retained, quiet text; dimmed art if needed; no invented progress percentage | Actual prepare/first-frame outcome controls reveal; prepare failure retains recovery |
| First-frame reveal | First decoded frame is ready | Reveal the existing video surface; retain configured transition timing | No timer stands in for first frame; prototype shows a clearly labelled still-art specimen |
| Playing→Release | Actual completion or distinct cancellation | Existing 650ms exit intent; readable Release actions | Real loopPointReached/completion path remains authoritative; cancel is not completion |
| Return to gallery | Explicit return/Back at permitted boundary | 300ms calm route reveal; selected focus restored | No automatic replay or camera consent change |
| Tracking loss / recovery | Existing semantic status reports loss/ready | Clear acquisition immediately; textual warning with bounded neutral/amber edge, <=160ms fade | Recovery never confirms a stale target; fresh point→fist required |

## State transitions

Gallery idle → identified target → explicit confirmation → Focus → Activation → portrait preparing → first-frame playing → Release → PostVideo/return → Closing/optional takeaway. Playback, cancellation, unavailable media and configured missing-video timer/hold remain separate branches. The prototype's **Completion event** requires the first-frame specimen; it never fires automatically from animation length.

Review-only readiness/point/fist/loss/recovery buttons illustrate semantic transitions without detector input. A fist with no fresh target is ignored. Pointer/number-key alternatives remain supported by their current meanings. Mixing pointer and gesture targets must not confirm a different performer than the visible target.

Overlay state is a separate plane: closed → opening → open → closing → closed, with minimize/resume and interruption edges. Modal priority and immediate privacy stop outrank page motion. On detach, cancel animations, schedules, request callbacks and proxy artwork. Retain one application state owner; do not introduce a second router to animate these views.

## Reduced alternatives

Reduced motion disables idle drift, parallax, proxy expansion, stagger, scale and translation. Use an immediate update or <=80ms opacity only; keep semantic preparation/completion and current Activation behavior unless separately approved. Reduced effects uses opaque glass regions and removes backdrop blur/diffuse atmosphere; it is independent of reduced motion. Preference functionality remains deferred; external review overrides and OS reduced-motion handling are prototype facilities only.

## Required interruption acceptance

Confirm during entrance; rapidly change targets; close during panel opening; reopen during closing; minimize and restore; open Privacy from Settings then restore its opener; change locale in a panel; Back while transitioning; lose tracking before fist; enter protected media with a minimized panel; cancel preparing/playing; receive a late video/request callback after exit. Every result must leave one route, one permitted overlay, valid focus and no stale target. These checks are pending browser/Unity execution; no runtime pass is claimed.
