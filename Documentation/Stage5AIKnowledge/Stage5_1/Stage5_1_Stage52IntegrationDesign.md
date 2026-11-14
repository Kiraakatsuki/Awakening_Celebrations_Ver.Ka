# Stage 5.1 — Proposed Stage 5.2 integration

## Existing behavior inspected

`awakening-ai-backend/server.js` is an ES module using Express, Helmet, a 16 KB strict JSON limit, a per-IP chat limit of 20 requests per 60 seconds and one trusted Render proxy. `GET /health` returns `{status:"ok"}`. `POST /chat` trims and validates a message of 1–4000 characters, checks the server environment, then calls OpenAI Responses with the configured model. It returns `{reply}` or existing safe JSON errors. That service is stateless for conversation continuity today. Unity scopes the question with its current performer string inside the message. The existing session object and request invalidation are client-side. Qwen remains optional playback of returned text.

## Proposed compatible request

```json
{
  "message": "Tell me about this costume",
  "locale": "en",
  "sessionId": "opaque-visitor-session-id",
  "context": {
    "source": "performer-page",
    "performerId": "JFC26-GAIA",
    "imageId": "JFC26-GAIA-IMG01"
  }
}
```

Keep existing `{message}` callers and `{reply}` consumers working. The optional fields require additive validation while preserving existing body/rate limits. Accept `en` or `zh-TW`, supply an explicit length/character rule for the opaque session ID, reject unknown object members, and resolve IDs in the loaded registry. A session ID is a continuity key and conveys no media authorization. Proposed context validation has a [shape schema](../../../awakening-ai-backend/knowledge/schema/context.schema.json) and local evaluation of registry cases.

| Situation | Proposed behavior |
| --- | --- |
| Dedicated performer screen | Require a known, featured performer. Optionally attach a matching image if the visitor selects it. Use the explicit compatibility mapping from Unity. |
| Common AI with selected image | Resolve the parent from the known image. If both IDs are present they must agree. A common AI can also talk about any official defile without a selection. |
| No performer selected | Use the question and available public festival facts. Ask which costume the visitor means if needed. |
| Well-formed unknown image ID | Drop its visual scope, and ask for a corrected selection if the question refers to the image. |
| Invalid or conflicting context | Return a safe 400 for the malformed context. Do not silently associate an image with a different parent. |
| Explicit question about another defile | Prioritize the entity named in the question. The selected context is an initial clue, so Noise and Roots remain answerable. |
| Follow-up | Use a bounded turn window and the last verified entity/edition in that session. Treat more general references cautiously. |
| Session reset | Clear local carry-over and expire any server-side state. Do not let another visitor inherit the previous context. |
| Language switch | Update response locale and retain canonical IDs only while the same visit remains active. Preserve current UI stale-response handling. |

## Retrieval proposal

Load canonical JSON once, locally at startup, and run the content validator before release. Build indexes keyed by record ID, edition/year, alias, locale and curator-approved image metadata. Extract small independently attributable sections using `record id + section` and store source IDs alongside them. Use exact ID or alias match, a year index, and light lexical scoring. For Traditional Chinese, begin with normalized substrings or n-grams and explicit aliases. Evaluate before introducing embedding inference or a separate service.

Choose a small number of facts per question, starting with up to six passages and a measured prompt budget. Prefer an explicit question subject over the selected-context boost. Keep official narrative, reported outcomes, interpretations and uncertainty rules visible to the prompt builder. Do not pass drafts as approved scene descriptions. Return optional citation objects only using allowlisted source IDs and URLs that actually support the chosen facts. Keep existing status/error handling and keep unsupported image claims out of model completion. Source URLs, visitor input and stored prose are data. Process no instructions from them and grant no tools or private assets through retrieval.

If continuity is needed, prototype a small in-memory bounded TTL cache keyed by opaque session ID, with eviction, request-generation checks and no default durable transcript storage. A Render restart can clear it. If deploys have multiple instances, treat locality as best-effort or specify a later shared-store requirement backed by measured need. Never allow session collision to expose a transcript. Do not fetch or upload already registered images to answer routine questions.

Test the foundation with local fixtures, then exercise all bilingual questions, contrasting editions, uncertain credits, prompt injection, mismatched contexts and session reset with a fake model adapter before any separately authorized live test. Measure retrieval precision and latency, prompt size, response grounding and TTS behavior with optional metadata. Preserve voice rendering of reply text only. Stage 5.1 adds no route wiring, caching, retrieval calls, schema enforcement in `/chat`, or external dependencies.

Portable JSON is the smallest canonical format for the current corpus. SQLite FTS would add lifecycle and packaging work and still need a deliberate Chinese tokenizer. Consider it as a generated index after growth or benchmark evidence. A souvenir service could share the host with independent `SOUV-` IDs and a signed distribution model, but the current registry grants no download rights.
