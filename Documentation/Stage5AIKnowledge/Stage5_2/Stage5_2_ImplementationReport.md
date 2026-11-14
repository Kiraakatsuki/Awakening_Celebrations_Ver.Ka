# Stage 5.2 — Context-aware JFC intelligence

The local backend now serves versioned contextual chat alongside the existing legacy and health routes. It indexes the Stage 5.1 corpus, resolves performer and image IDs, searches all eight HEAL defiles and historical editions, and returns a plain reply with separate citation metadata. The Unity Editor can select the new service through a loopback development setting. The serialized Render URL and production route selection remain as before.

| Changed path | Contribution |
| --- | --- |
| `awakening-ai-backend/server.js` | App factory, one-time knowledge load, injectable provider, versioned route mount and preserved legacy error contract |
| `awakening-ai-backend/context-chat.mjs` | Strict API validation, scoped ephemeral sessions, source-restricted output envelope, deterministic uncertainty responses |
| `awakening-ai-backend/knowledge/index.mjs` | Startup integrity, locale indexing, lexical retrieval and subject continuity over the existing data |
| `awakening-ai-backend/test/context-chat.test.mjs`, `mock-context-server.mjs` | Local mock suite and credential-free HTTPS fixture |
| `awakening-ai-backend/README.md` | Development contract pointer |
| `Assets/LivingGlass/Integration/LivingGlassChatProtocol.cs` and metadata | Explicit compatibility IDs, request encoding, loopback validation and allowed fallback policy |
| `Assets/LivingGlass/Integration/LivingGlassFlowIntegration.cs`, `Assets/Scripts/AwakeningApplicationController.cs` | Existing assistant owner attaches context, manages session tokens and preserves request cancellation and rendering |
| `Assets/LivingGlass/Presentation/LivingGlassPresentation.cs` | Development-only general conversation entry and neutral existing composer |
| `Assets/LivingGlass/Speech/LivingGlassSpeechCapture.cs` | Neutral visual accent for the unscoped conversation. No capture or recognition behavior changed. |
| `Assets/LivingGlass/Speech/ContextChatEditorChecks.cs` and metadata | Isolated request-shape and fallback coverage |
| `Documentation/Stage5AIKnowledge/Stage5_2/` | Contract, mapping, evidence, validation and rollback guidance |

No dependency packages, canonical Stage 5.1 records, scene, build settings, voice models, narration assets or deployed configuration were changed. Existing user screenshot deletions and checkpoint report edits were preserved. The Stage 4.3E commit and annotated recovery tag were not modified.

All 42 localized fixtures passed across nine backend test groups. Measured retrieval p95 was 0.114 ms. The 24 Unity contextual payloads passed Edit Mode serialization and mocked HTTPS transport. Existing 72 reply, 164 conversation, 432 microphone and voice-policy checks passed. No actual GPT calls, Render deployment, Play Mode, camera/microphone use, Player build, or GitHub push occurred. Live model accuracy, real scene cancellation against the contextual endpoint and full-system performance remain open.

[API and rollback](Stage5_2_APIContract.md) · [Context mapping](Stage5_2_ContextMapping.md) · [Retrieval and evaluation](Stage5_2_RetrievalEvaluation.md) · [Runtime validation](Stage5_2_RuntimeValidation.md)
