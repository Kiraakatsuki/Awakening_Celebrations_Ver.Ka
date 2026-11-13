---
name: unity-architecture
description: "Plan or make authorized C# architecture, state, lifecycle, event, dependency, or maintainability changes in Awakening Celebrations. Use for ownership and system boundaries rather than visual-only changes; major restructuring requires approval."
---

# Unity architecture

Follow repository AGENTS.md. Read Documentation/RuntimeArchitecture.md and UIDocumentArchitecture.md as historical context, then verify current scripts and scene wiring. Do not assume older statements that gesture, video or assistant services are absent remain true.

Map ownership before changes: AwakeningApplicationController owns application/session flow, navigation, performer transitions and assistant behavior; ScreenDocumentManager owns page-family documents and the overlay document; RuntimeScreenCatalog maps route/locale assets; HomeLanguageController manages Home interactions. These live in Assets/Scripts/. Confirm actual responsibilities rather than forcing a new pattern on them.

Trace consent through GestureCameraConsentGate in Assets/Samples/MediaPipe Unity Plugin/0.16.3/Official Solutions/Common/Scripts/, its registered HandLandmarker runner, and Assets/Scripts/Interaction/MediaPipeGestureBridge.cs. Modified sample scripts are application integration code, not safe cleanup targets. Keep one capture owner and preserve preview/consent separation.

For an authorized refactor, record affected state, serialized fields/GUIDs, events, callbacks, subscriptions, coroutines, cancellation, disposal and scene/prefab references. Protect assistant session/request cancellation and late responses; retain credentials on the backend in awakening-ai-backend/. Protect video preparation, RenderTexture ownership, completion callbacks and missing-video behavior. Preserve locale, consent and performer state independently.

Prefer the smallest justified boundary change with an explicit before/after contract. Avoid speculative service frameworks or packages. Present major changes, migration steps, affected assets, regression plan and rollback approach for approval before implementation. Do not delete apparently unused assets without checking serialized references and obtaining destructive-cleanup approval.

Use static checks and appropriately authorized tests; installed Unity implementation and validation skills can provide general mechanics. Report preserved contracts and verification limits without launching Play Mode.
