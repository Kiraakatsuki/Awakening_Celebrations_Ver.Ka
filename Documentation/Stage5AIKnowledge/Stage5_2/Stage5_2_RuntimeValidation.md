# Stage 5.2 — Local validation

| Check | Result |
| --- | --- |
| Unity 6000.4.7f1 Edit Mode compilation | **PASS**. Runtime and Editor assemblies rebuilt and existing checks executed. The Editor reported a duplicate Unsafe assembly selection involving existing package paths; no application compilation errors remained. |
| Context serializer | **PASS**. 24 combinations across six dedicated pages, six chooser portraits and two locales. General AI emits no specific IDs. Legacy fallback shape, status allowlist and HTTPS loopback validation passed. [Contract](Evidence/Context.txt) |
| Mock transport | **PASS**. The compiled UnityWebRequest factory posted all 24 request combinations over HTTPS loopback with a fixture-only certificate pin. The fake service resolved both locale variants, tested general chat with no IDs, rejected a revoked session and returned legacy JSON. No Play Mode or live provider. [Transport](Evidence/Transport.txt) |
| First mock transport attempt | Hit HTTP 429 during the second locale because setup and both languages exceeded 20 requests in one minute. The completed retest used two mock instances with independent counters. The limiter remained enabled and the Node suite separately verifies shared throttling. |
| Established reply layout suite | **PASS**, 72 layouts with isolated reply audio-policy checks. [Reply](Evidence/Reply.txt) |
| Established conversation suite | **PASS**, 164 cases. [Conversation](Evidence/Conversation.txt) |
| Established microphone suite | **PASS**, 432 layouts and isolated state checks. [Microphone](Evidence/Mic.txt) |
| Voice preference and serialized references | **PASS** in existing checks. [Voice](Evidence/Voice.txt) · [References](Evidence/References.txt) |
| Backend and knowledge | **PASS**, 9 Node test groups and all 42 localized cases. The Stage 5.1 foundation validator passed. [Retrieval report](Stage5_2_RetrievalEvaluation.md) |
| Reset and stale responses | **PASS** for server-side reset during a held mock request and rejection of the superseded token. Unity client generation checks were inspected. Actual rapid scene navigation during an in-flight request was **NOT RUN** because this phase establishes Edit Mode coverage. |
| Mock TLS and chooser in the running scene | **NOT RUN**. The fixture used a narrowly scoped pin against the temporary loopback certificate. A deployed or running Editor configuration still needs to exercise its normal trusted-certificate path and real overlay interaction. |
| Live GPT, Render, camera, microphone, Qwen synthesis, Play Mode and Windows Player | **NOT RUN**. No live inference or deployment was attempted. |

The next runtime gate is a separately authorized Play Mode pass with the HTTPS mock. Exercise focused Gallery/chooser and dedicated reflection routes, rapid navigation while a mock reply is in flight, the general option, language switching, visitor reset, and cancellation with camera/microphone denied. A separately authorized live-model exercise should evaluate bilingual naturalness and unsupported claims. Test Render and the eventual Player independently before any production-readiness decision.
