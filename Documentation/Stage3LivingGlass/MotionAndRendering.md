# Implemented presentation contract

The runtime baseline uses UI Toolkit 2D composition. It has no added camera, renderer feature, custom shader, fullscreen capture, blur chain or duplicate video decoder. Tinted surfaces and narrow edges are glass styling, not native backdrop filtering or refraction.

| Event | Visual work | Timing | Authority / cancellation |
| --- | --- | --- | --- |
| Stable navigation | Opacity and 8px presentation offset | 300ms | Route commits first; replacing content clears tracks |
| Gallery entrance | Coordinated art reveals in fixed hit wrappers | 520ms, 20ms offsets capped at100ms | Targeting can interrupt individual artwork tracks |
| Stable target | Aperture lift/scale and localized light | 240ms / theme280ms | Existing stable-target event or exact pointer/focus target |
| Target change | Interpolate from current theme and pose | 240ms /280ms | Never stacks six fullscreen theme planes |
| Rest | Bounded artwork return | 340ms | Semantic target clear; no new tracking-loss timer |
| Gallery→Focus | Inert original gallery + selected art proxy + Focus reveal | 650ms | Flow already commits Focus; proxy cleanup never navigates |
| Settings/AI entry | Origin-related translation, .94→1 scale, opacity | 360ms | Scope acquired immediately; Close already works |
| Window close | Reverse from current sampled pose | 220ms | Background stays inert until removal; request canceled immediately |
| Reopen while closing | Retarget current window pose | Remaining opening interval | Existing window reused; old track canceled |
| Activation and portrait stages | Existing cinematic ownership / protected fades | Existing3s Activation and650ms fades | Media semantics preserved; no input skipping |
| Reduced motion | Immediate pose/color outcomes; short protected fade | No spatial work | Same flow/consent/media commands |

`LivingGlassMotion` owns visual tracks; the existing controller still owns protected stage coroutines. The view does not synthesize video completion, fist recognition, successful calibration or AI responses. Idle artwork is intentionally still, matching the latest spatial addendum rather than the earlier exploratory continuous drift.

Controlled Spatial B is implemented with a supported **optical fallback**: aperture scale≤1.020, x≤4 logicalpx, y−7…−5px around a6px lift, artwork parallax≤3/2px and static crop overscan. Wrappers, labels, focus ring and identification geometry stay fixed. The baseline has no perspective pitch/yaw: those are an explicit fidelity compromise, not a claim that overlay UI Toolkit reproduces CSS rotateX/rotateY. A future mesh/shader trial needs separate measured justification; no additional camera is proposed for a1° effect.

One low-opacity environment plane uses the approved six atmosphere colors at≤10%, falling to2.5% behind a window. Text and reading materials stay neutral. Reduced effects removes that plane. The selected portrait rim and index carry the accent; no six unrelated UI themes are created.

The coordinator uses a monotone quartic deceleration as the Unity baseline. It is intentionally bounded and non-bouncing; it is not described as a numerically identical implementation of the browser cubic Bézier. Final easing/optical fidelity, combined workloads and60fps require authorized runtime review and profiling.

The existing gesture release grace is preserved. Where the exploratory addendum proposed consuming confirmation immediately on an observation gap, the established source's release/confirmation behavior remains the compatibility authority. No visual animation changes those thresholds. Gesture input never confirms a window control.
