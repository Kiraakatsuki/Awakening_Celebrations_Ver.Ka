# Deferred and conditional feature UX

All features below were deliberately deferred according to the user’s Stage 1 clarification. Stage 2A authorizes their **design**, not their business logic. Primary overhaul → stabilization/cleanup → separately authorized functionality remains the required order. `DeferredFeatureStates.csv` supplies paired EN/Traditional Chinese state copy. The prototype exposes an explicit design overview and optional calibration/Souvenir layouts; it does not show fake applied preferences or success results.

| Feature | Proposed entry/layout | State and cancellation design | Recommendation / decision |
| --- | --- | --- | --- |
| Sound | Settings → Sound; grouped master/environment scope, value, mute and output availability | Default/current value; output unavailable with reason; applying; applied only on actual service confirmation; failed restores prior value; cancel retains old setting | Include. Approve control scope, default, persistence, soundtrack and video audio policy. No working slider/toggle in this package |
| Accessibility | Settings → Accessibility; motion/effects/text controls with explanatory specimen | Readable baseline; preference available/unavailable; applying/applied/error; static/reduced-effects comparison; never hide essential progress | Include. Exact choices and persistence approval pending. Review overrides are outside product UI and do not persist preferences |
| Tracking/camera | Settings → Camera & Privacy; permission, capture, readiness, hand tracking, preview separately labeled | Off/declined, suspended, starting, ready/no hand, lost, device missing/error; retry through existing owner only; preview not consent; explicit revoke stays immediate | Include status and understandable control design. Approve capture suspension/resume and consent-free participation |
| End/Restart | Stable shell/Settings → Experience; clear consequence dialog with Cancel and explicit End | Explain reset field matrix; confirm, cancelling, cleanup pending, failure/recovery; no completion claims until all owners acknowledge | Include safe end design. Restart destination, consent/awakening/conversation reset and operator/visitor scope require approval |
| Contextual Help | Global Help; current gesture cue + pointer/keyboard alternative; optional tutorial replay | Ready, no-camera alternative, unavailable replay reason, return to opener; replay cannot discard session silently | Combine Guide with Help. Keep tutorial route coverage and confirm replay semantics |
| Calibration (conditional) | Help → Alignment assistance, only if measured need exists; one target with pointer alternative and clear Cancel | Explain → check device/consent → acquire → evaluate → actual result/retry; unavailable reason; cancel retains prior transform. Never call a camera restart “calibration” | Design optional, keep out of primary navigation until need/algorithm are approved. No success animation in prototype |
| Souvenir (conditional) | Closing → optional takeaway preview; approved content, delivery region, readable destination/expiry and Done independent of service | No service, preparing, actual ready, expired, offline/error, retry/cancel; QR region omitted until a real code exists; Done remains possible | Keep optional design, decide usefulness/content/destination/retention first. Prototype says no delivery and has no QR graphic |

## Optional calibration layout

Quiet reading panel over a stable paused interaction context; title “Alignment, only if needed / 僅在需要時，調整對位。” Body explains the task and privacy state. The active region must show one target with text progress, not moving/flickering decoration. “Cancel / 取消” always restores the previous experience and calibration. Pointer/keyboard alternatives support the same logical calibration contract if an algorithm is eventually approved. The current prototype only presents the explanatory/unavailable layout; no acquisition or success is synthesized.

## Optional Souvenir layout

Artwork preview on one side; purpose and delivery region on the other. Initial state: “Delivery unavailable / 留存服務未提供”, with no scan instruction and no fake QR. A future ready state contains a real generated code, readable approved destination and expiration. Loading never exposes a stale previous visitor’s code. Offline/error state preserves Done and offers Retry only if service recovery is real. Expired content removes the code before re-generation. Copy and retention rules require product approval; no provider or link destination is invented here.

## Integration and accessibility

All future actions use the shared shell, one modal priority authority, focus entry/opener restoration, English/Traditional Chinese labels and input alternatives. Protected video stages defer nonessential Settings/AI. Calibration/Souvenir are not new top-level global actions. All asynchronous outcomes require cancellation and generation guards. Reduced motion communicates progress with text; reduced effects uses opaque reading surfaces. No placeholder service interfaces or packages are installed in Unity.

## Approval boundary

This package supplies screen/layout capacity, entry/return rules, state lists, bilingual copy and motion language. It does not approve exact persistence, data retention, reset semantics, new gestures, calibration algorithm, QR provider or functional activation. Use the [approval checklist](DesignApprovalChecklist.md) to make those decisions explicitly.
