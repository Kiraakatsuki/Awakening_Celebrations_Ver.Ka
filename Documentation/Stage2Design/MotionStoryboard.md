# Motion storyboard and demonstration

Open `prototype/index.html`, then use the event controls **below** the exhibition canvas. They are authoring/review controls, not visitor UI or new gesture bindings. Source thresholds, camera lifecycle and video completion remain unchanged. Prototype JS does not access camera/network/media APIs; CSP also blocks connections and media sources.

| Beat | Visual treatment / time proposal | State authority / interruption |
| --- | --- | --- |
| 01 Home entrance | Title/art opacity + <=16px rise, 280ms; no delayed CTA activation | View entry only; replay retargets, reduced motion immediate |
| 02 Hover / focus / press | 110ms local fill/edge response; 3px persistent focus; <=1.5% press scale | Native pointer/keyboard; cancellation restores base; no delayed command |
| 03 Settings open | 220ms panel opacity +16px, stable scrim | Overlay acquires input/focus immediately; close can interrupt |
| 04 Settings close | 160ms fade, input removed immediately | Top modal dismisses; restore actual connected opener or fallback |
| 05 AI open/close | Same temporal grammar as Settings; static empty state | No fabricated Thinking/reply; actual request lifetime belongs to later client |
| 06 Stable navigation | 280ms opacity/limited translation | View change separate from animation; protected routes retain policy |
| 07 Ready / pointing | Text + cursor/target edge; feedback follows actual semantic progress later | Review event illustrates readiness; no detector execution or dwell retuning |
| 08 Target / confirm / loss | Target outline; confirm label, then view change; lost status cancels acquisition | Point never selects. Review “fist confirm” is an explicit simulation, not new gesture logic |
| 09 Focus→Activation | Existing 650ms fade intent, 3s Activation content; bounded luminance change | No click skip of automatic states; broader safe exit still needs approval |
| 10 Portrait preparing | Existing artwork + quiet status; no percentage without data | Wait for real preparation and visible state; first-frame event reveals the image |
| 11 First frame / completion | First-frame reveal and 650ms exit intent; sharp art/text | **Completion event must be explicit.** The demo advances only on the separate review event; no video-length timer substitutes |
| 12 Release / return | Calm appearance, 650ms approved cinematic baseline; explicit return action | Cancel and completion are distinct events; configured missing-video timer/hold remains its own contract |

## Review procedure

1. Replay Home entrance; Tab to an action and compare hover/focus/press.
2. Open Settings/AI, close with Escape, observe restored focus. Switch locale within Settings and verify it remains understandable.
3. Choose selection; illustrate ready → point target → fist confirm, then loss. Numeric 1–6 and pointer selection are actual prototype navigation alternatives, not recognizer tests.
4. Run Focus→Activation. Use **Illustrate preparing**, **Illustrate first frame**, **Illustrate completion event** individually. Completion never arrives from a media timer. Use cancel at any review state and inspect a stable Release view.
5. Repeat with Reduced motion preview and Opaque surface preview in the review toolbar. These switches are not implemented deferred preferences.

## Cancellation and temporal policy

Production proposal remains one transition scope per page/overlay plane, generation tokens and explicit completed/cancelled/superseded results. A fade owns presentation only. Privacy denial/revocation cannot wait for a fade. Requests and media callbacks cannot act on a detached tree. Repeating input must not push history or advance twice. Deferred shell requests do not auto-open minutes later; offer a fresh explicit Open at a stable boundary.

Source timing preserved on paper: pose dwell .14s, release .15s, open-palm Back 2s, cooldown .8s, tracking-loss reset .35s, target stability .12s/release .75s and current two-palms rules. None are implemented or altered in the standalone review.

Browser animation is a motion reference, not evidence of Unity frame pacing, GPU cost or callback correctness. Native Figma storyboard frames and this table specify temporal intent. Actual video decoding, missing-clip 10s fallback/hold configuration, MediaPipe tracking and runtime cancellation require separately authorized Unity validation.
