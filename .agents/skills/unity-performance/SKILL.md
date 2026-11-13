---
name: unity-performance
description: "Investigate Awakening Celebrations CPU/GPU time, memory, garbage collection, frame pacing, MediaPipe, video, RenderTextures, or UI animation cost. Use for evidence-based performance work rather than speculative cleanup."
---

# Unity performance

Follow repository AGENTS.md. Separate static cost hypotheses from measurements. Inspect Unity/package versions, quality pipeline overrides, display resolution, graphics API, target Windows CPU/GPU and desired frame rate before setting budgets. Do not invent measured timings or hardware requirements.

Inspect the actual camera/MediaPipe runner and consent gate, Assets/Scripts/Interaction/, video and assistant lifecycle in AwakeningApplicationController.cs, UI documents, PanelSettings and rendering resources. Preserve one camera pipeline; a profiler experiment must not create another or auto-grant consent.

For an authorized runtime session, use Unity Profiler CPU/GPU and Memory views and Frame Debugger as appropriate. Record platform/build, scene/route, locale, performer, quality, resolution, warm-up and capture interval. Distinguish Editor overhead from Player behavior. Capture a comparable baseline before optimizing; runtime/Player profiling is unavailable when runtime testing is prohibited.

Prioritize evidence around:

- MediaPipe inference scheduling, capture resolution, CPU/GPU transfers, synchronization, frame-copy/readback costs and shutdown after consent withdrawal.
- Living Portrait preparation/decode stalls, audio/video synchronization, reusable VideoPlayer state, texture dimensions/formats and RenderTexture allocation/release across repeated entry/exit.
- UI layout/repaint/overdraw, full-screen glass passes, scheduled animation work on hidden documents, callback churn, per-frame allocations and garbage collection spikes.
- Assistant network/resource cleanup and retained visual trees or textures during navigation.

Change one measured cause at a time; favor lifecycle fixes and bounded shared resources over premature pooling or added packages. Compare frame-time distributions and spikes, not only average FPS. Check memory after repeated route/locale/performer cycles. Report data, hypothesis, proposed change, quality tradeoffs and regression coverage. When runtime is unauthorized, provide static evidence and the pending capture plan without running it.
