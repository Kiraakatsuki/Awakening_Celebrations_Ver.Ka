---
name: motion-interaction
description: "Design or tune Awakening Celebrations animation timing, easing, microinteractions, gesture feedback, and interruptible navigation transitions. Use for temporal interaction behavior rather than static layout or rendering passes."
---

# Motion and interaction

Follow repository AGENTS.md. Inspect transition durations, coroutines, UI schedules and cancellation in Assets/Scripts/AwakeningApplicationController.cs; inspect Assets/Scripts/Interaction/GestureCursorController.cs, GestureRecognitionLayer.cs and MediaPipeGestureBridge.cs for semantic input timing. Read affected UXML/USS before introducing another animation owner.

Describe each motion's trigger, purpose, properties, duration/easing, completion event and interruption behavior. Preserve navigation state ownership. Repeated input, Back, language changes, overlays and screen replacement must cancel or retarget motion without stale callbacks, invisible hit targets or double navigation. Release scheduled work and handlers when elements detach. Prefer transform/opacity where compatible and check layout/repaint costs rather than assuming animations are free.

Treat gesture dwell, stability, release and cursor smoothing as input semantics. Do not retune recognition thresholds merely to match animation. Keep gesture feedback consistent with pointer and supported keyboard activation; preserve desktop fallback controls. Living Portrait advancement must retain its playback-completion contract; visual fades must not substitute for the video event or skip configured placeholder behavior.

Specify reduced-motion behavior that preserves completion/state changes, avoids unnecessary zoom/parallax/flashing and communicates progress without movement alone. Inspect existing Settings accessibility handling before wiring a setting; do not claim a disabled or deferred option works. Plan an accessible static alternative if functional implementation is outside scope.

Check rapid repeat activation, interrupted fades, focus visibility, inactive documents and both languages. Runtime smoothness, frame pacing and cancellation checks require explicit authorization; report static findings separately. Use unity-performance for measured costs and accessibility-localization for inclusive alternatives.
