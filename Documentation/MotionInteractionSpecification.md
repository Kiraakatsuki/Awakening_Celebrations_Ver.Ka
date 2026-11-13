# Motion and interaction specification

Stage 1 proposal · 13 November 2026 · AC-03/07/08/10/11. No thresholds, coroutines, USS or runtime behavior changed.

## Existing behavior and constraints

`AwakeningApplicationController` uses coroutine opacity fades, 0.65s serialized performer transitions, 3s Activation including fades, 2s source-default Color Release presentation, and a 10s configured missing-video preview. `FadeVisualElement` interpolates linearly; Activation artwork uses SmoothStep. Flags `transitionInProgress`, `performerSequenceActive` and `performerFlowTransitionLock` arbitrate input; next-frame lock release and several coroutine handles coordinate progression. Other navigation replaces UI trees without a shared presentation transition.

Gesture timing is a separate input contract: pose dwell .14s, release dwell .15s, open-palm Back hold 2s, event cooldown .8s, tracking loss reset .35s; point target stability .12s and target release .75s in app defaults. Preserve the actual scene values and semantic recognizer rules. Do not tune these to make a cosmetic animation look faster. Video completion is `loopPointReached`, not a motion duration. Missing-video timing retains its own configured behavior.

## Motion language and tokens

Use immediate control acknowledgement, short spatial transitions for navigation, and deliberately paced performer content. Motion explains state; it never delays command dispatch. Initial tokens are proposals to tune after authorized runtime review.

| Token | Duration / curve | Intended role |
| --- | --- | --- |
| instant | 0ms | Input dispatch, stop/cancel, permission removal, semantic state commit |
| feedback | 80–120ms / ease-out | Hover/press surface, border or opacity feedback |
| settle | 160ms / ease-out | Target confirmation and small content changes |
| enter | 220ms / ease-out-cubic in C#; compatible USS easing where supported | Panels and normal page entrance |
| exit | 160–180ms / ease-in | Dismissal; input disabled at start |
| page | 280ms / ease-in-out | Stable page movement/crossfade, limited 12–24px translation |
| cinematic | Preserve existing 650ms fade / review curve independently | Focus/Activation/Portrait/Release transitions |
| stagger | 24ms, total <=120ms | Small groups appearing; no stagger of required action availability |
| status | 120–160ms | Status text/camera/locale updates without layout jumping |

Curve names describe intent. Implement an explicit shared curve in C# where USS doesn't provide the exact easing; do not assume arbitrary browser cubic-bezier syntax. USS transitions support property timing and events, but presentation lifecycle still needs a completion/cancellation contract. [Unity 6.4 USS transitions](https://docs.unity3d.com/6000.4/Documentation/Manual/UIE-Transitions.html).

## Motion contracts by interaction

| Trigger / purpose | Properties / token | Completion authority | Interruption / reduced motion |
| --- | --- | --- | --- |
| Stable page navigation | Content opacity + <=24px translation; page token | Flow owner commits route; presenter reports visual completion | Newer valid command retargets from current value; reduced = immediate or <=80ms opacity |
| Hover / press / focus | Surface tint/border, optional <=1% scale; feedback | Control state only; action dispatch on activation, no delayed click | Pointer leave/cancel resets; focus never fades away; reduced = tint/ring only |
| Settings/AI open | Scrim opacity + panel opacity/<=16px translation; enter | Overlay owner establishes focus/input scope before animation | Close during open cancels/retargets; reduced = instant visible panel |
| Settings/AI close | Panel/scrim opacity; exit | Overlay owner cancels request and restores focus after detach | Closing disables hit targets immediately; privacy stop preempts without waiting |
| Glass material change | Crossfade two bounded surface appearances; settle | Effects owner applies tier; preserve sharp content | No animated blur radius continuously generating large targets; reduced effects = opaque surface |
| Gesture acquisition / target | Cursor/edge/progress indicator; continuous semantic progress | Recognizer/target state, never tween completion | Tracking lost cancels acquiring state; keep text/shape alternative; existing dwell preserved |
| Fist confirm performer | Accent/outline settle, then approved page motion | Existing semantic ConfirmPerformer | Duplicate confirms ignored until route commit; reduced = selected label + immediate transition |
| Focus start | Existing fade, same selected performer | Performer flow owner | Repeated start ignored; safe explicit stop cancels; reduced fades shortened but no content event skipped |
| Activation content | Existing 3s sequence / artwork awakening; cinematic | Performer flow duration contract, not arbitrary decoration | Keep content duration until approved change; reduced removes zoom/pulse, can shorten fades only |
| Portrait preparing / reveal | Static art → first decoded frame, fade within approved cinematic interval | Prepared + visible + first-frame callbacks, then actual playback | Back cancels; no fake progress percent; reduced reveal immediate, playback unchanged |
| Portrait complete → Release | Existing fade out/in | Video completion event or configured placeholder policy | Late callback rejected by generation; reduced = immediate visual transition after real completion |
| Release / continue | Cinematic entrance, normal explicit action feedback | Flow owner | Don't react to residual held gesture automatically; preserve release semantics |
| Assistant send/loading/reply | Immediate user message, static “Thinking…”; short reply opacity | Request/session ID and actual response | Close cancels; reduced/static never uses looping shimmer; no artificial typewriter delay |
| Locale change | Replace localized content without large page movement; status token | Locale owner; safe text-layout commit | Preserve focused semantic action, media/session state; defer protected-page rebuild |
| Camera waiting/ready/lost/error | Small icon/state crossfade, status token | Existing gate/runner/bridge signals | Revocation hides preview immediately; feedback animation cannot prolong capture |
| Deferred C feature flows | Same form/status/overlay tokens in complete design | Future approved service, never the animation | Cancel/error/unavailable designs now; no simulated success implementation |

## Ownership, interruption and cancellation

One **TransitionCoordinator** owns page/overlay presentation scopes; controls may own local feedback only. It does not navigate, grant consent, advance playback or submit requests. Existing controller remains the flow authority during migration. Do not run coroutines, USS transitions and schedules against the same property concurrently.

Each operation has owner scope, generation, final visual state and completion result (`Completed`, `Cancelled`, `Superseded`). Starting a new transition cancels the old operation explicitly; callbacks are guarded by generation and attached-tree identity. Detach/disable/route exit disposes schedules and event registrations. Cancellation must resolve input/focus/opacity to a stable state rather than leave an invisible clickable element or a permanent transition lock.

Navigation is a transaction: validate destination and capability, prepare view, acquire scoped input ownership, commit business state, animate/present, then release ownership exactly once. Failure before commit restores the old stable state; failure after commit uses the new route's visible recovery state. Never push history for a rejected transition. Use at most one presentation transition per plane. Settings/AI requests during protected phases follow [global policy](GlobalNavigationRedesign.md); there is no second queue inside motion code.

Specific stress cases: close during modal opening; repeat Enter; click captured by page while shell clicked; language change during request; Back during video preparation or completion fade; loss/reacquisition while pointing; rapid theme switching; re-enter same performer after cancellation; disable while a coroutine/request is pending. Exactly one state transition and no stale completion is acceptance, not simply a visually pleasing fade.

## Reduced motion and accessibility

Design a stable alternative for every motion: no parallax, zoom, repeated bounce, flashing or indefinite shimmer; immediate or brief opacity changes; textual progress/readiness; same semantic completion events. Do not shorten the 2s Back hold or skip video because reduced motion is requested. Distinguish animation reduction from exhibition content duration.

The user confirmed Accessibility preferences were deferred. Specify the complete reduced-motion Settings UI now, but keep its functional preference logic in D1 after primary overhaul and cleanup, unless separately re-scoped explicitly. The primary design must still be understandable without motion and may use a reviewed static configuration for validation. A disabled preference is not a working feature.

## Performance and validation

Prefer transform/opacity over layout animation, but measure UI rebuild, repaint, draw calls and overdraw; opacity is not free. Current cursor updates left/top and preview marks repaint every frame; video marks repaint on decoded frames. Measure before optimizing these or introducing pooling. No background schedules should animate hidden documents. See the [capture plan](TransformationAudit/ValidationAndPerformancePlan.md).

Acceptance: immediate acknowledged input; no stuck locks; deterministic cancellation; clear focus at every boundary; no double navigation; first-frame/loopPoint/missing-clip contracts unchanged across all six performers and both locales. Runtime smoothness and these behavioral checks remain not run under Stage 1 restrictions.
