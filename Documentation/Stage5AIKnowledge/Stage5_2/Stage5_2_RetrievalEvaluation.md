# Stage 5.2 — Retrieval and evaluation

The startup loader reads the Stage 5.1 canonical files once per app instance, validates source references, scene-independent registry relations, official inventory, locale pairs and passage bounds, then builds 124 indexed passages. Missing or invalid required data disables contextual chat with HTTP 503 while leaving legacy and health available. No network source fetch or vector service is used. The broader Stage 5.1 validator separately verifies Unity image files and GUIDs.

Retrieval combines exact IDs, known aliases, all explicit request years, English words, Traditional Chinese substrings and character bigrams. Explicit defiles and dated editions outrank selected context. A short session window supports same-scope follow-ups, including a relative year. Each result has a record ID, section, source IDs, locale, provenance and text. The default cap is six passages. Set `JFC_RETRIEVAL_BUDGET` to an integer from 1 through 10. Passage bodies are bounded to 1,024 UTF-16 units each.

Official themes, reported occurrences, curatorial readings, scheduled events, exhibition bindings and unresolved credits travel with distinct provenance. The first three editions retain unconfirmed umbrella titles. Image bindings expose a public scope and explicit uncertainty. Draft visual captions and asset paths are absent from the model prompt. The prompt uses separate trusted instructions and a JSON input object containing the question, locale, context, recent questions and selected facts. It offers no tools and asks for plain text with exact evidence keys. The server rejects foreign citation keys and URL/path-shaped reply content, and returns citations exclusively from selected public registry entries. It uses localized deterministic responses for unsupported image credits, early umbrella-title queries, ambiguous references and missing evidence. Provider-side response storage is disabled for the contextual request.

All **21 bilingual pairs, 42 localized cases**, passed the local suite against the existing expectations. For answerable cases, tests check that required records were independently retrieved and referenced, then a fake model returns the fixture answer. For invalid and unknown-image cases the HTTP behavior is tested without generation. The fake suite validates evidence coverage, request isolation and citation integrity. It cannot establish the factual fidelity, naturalness, or prompt-injection resistance of live model prose. Supporting evidence keys constrain citation metadata; they do not mathematically verify every generated sentence.

Additional tests cover all eight English/Traditional Chinese defile names, six image IDs, Roots from Gaia, cross-scope and locale continuity, session eviction/expiry, reset during generation, provider failure, missing startup data, legacy compatibility and shared throttling. Run:

```sh
node --test awakening-ai-backend/test/context-chat.test.mjs
node awakening-ai-backend/scripts/validate-knowledge.mjs
```

## Local measurements

| Metric | Observed |
| --- | ---: |
| Load and index | 4.78 ms for 124 passages |
| Retrieval p50 / p95 | 0.070 / 0.114 ms, 1,000 calls after 100 warm-up calls |
| Retrieved passages | Maximum 6 over the mixed fixture workload |
| Largest serialized provider request | 3,142 bytes across 18 fake requests |
| Warm local HTTP p95 | 2.47 ms across 16 samples after discarding the first two |
| Process RSS | Approximately 82.4 MiB in the combined benchmark process |
| Observed heap increase | Approximately 0.78 MiB when locally filling 256 sessions with maximum question excerpts |

These are Node.js 24.22.1 measurements on this Windows workstation. The loopback HTTP measurements use a fake provider. Heap delta includes normal V8 allocation noise. Neither result includes real provider latency or concurrent Unity, Whisper, Qwen or MediaPipe workloads. [Raw benchmark](Evidence/Measurements.json)
