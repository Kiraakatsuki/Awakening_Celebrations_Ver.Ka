# Stage 5.2 — API contract

Legacy `POST /chat` still accepts `{"message":"Hello"}` and responds with `{"reply":"..."}`. `GET /health` remains available. Helmet, the strict 16 KB JSON body limit, the trusted Render reverse proxy and the 20 requests per IP per minute limit remain in place. Legacy and contextual routes share the limit, including session requests.

## Contextual routes

| Route | Input | Success |
| --- | --- | --- |
| `POST /chat/v2/session` | Empty JSON object `{}` | HTTP 201 with a server-issued `sessionId` |
| `POST /chat/v2` | Contextual request described below | HTTP 200 with plain `reply` plus optional-use metadata |
| `POST /chat/v2/reset` | `{"sessionId":"<issued token>"}` | HTTP 204. Idempotent, including expired and unknown well-formed tokens. |

```json
{
  "message": "Tell me about this costume",
  "locale": "zh-TW",
  "sessionId": "<server-issued 43-character token>",
  "context": {
    "source": "common-ai",
    "performerId": "JFC26-GAIA",
    "imageId": "JFC26-GAIA-IMG01"
  }
}
```

The example token is a placeholder. Send an actual issued token or omit the field for stateless requests. The message is a nonempty trimmed string, with at most 4,000 UTF-16 units before trim. Locale must be `en` or `zh-TW`. Only the keys shown are accepted. The sole nested object is optional `context`; extra fields, arrays, nested values and client-supplied URLs or instructions are rejected. Context source is `performer-page` or `common-ai`. Optional IDs must be a string or null. The performer suffix permits 1–24 uppercase letters. An image adds `-IMG` and 2–5 digits. Well-formed unknown image IDs drop visual scope and may produce a clarification. Unknown performers, malformed IDs, nonfeatured performers on dedicated pages, and mismatching known IDs produce HTTP 400.

The success envelope contains `reply`, `locale`, resolved `context` IDs, `recordIds` and up to eight citations. Each citation has `id`, `title` and a public registry `url`. A repository source can contribute to a record ID without exposing its local locator. Unity reads the existing top-level `reply` only. Text stays suitable for manual Qwen playback. The new route returns safe errors for invalid requests (400), in-flight overlap (409), expired or unknown issued token (410), oversized JSON (413), quota (429), provider or output failure (502), missing knowledge/provider or full session store (503), and provider timeout (504). JSON parser errors retain existing behavior.

## Development and future deployment

From `awakening-ai-backend/`, run `node --test test/context-chat.test.mjs`. To run a manual local fixture, supply a certificate valid for the numeric loopback IP plus its private key, both kept outside tracked directories.

```text
node test/mock-context-server.mjs <certificate.pem> <private-key.pem> 8743
```

The mock listens on HTTPS loopback and always uses a fake provider. Set `AWAKENING_CONTEXT_CHAT_URL=https://127.0.0.1:8743` in the environment of a new Unity Editor process. Use a certificate trusted by that process. The runtime preserves normal TLS validation and rejects redirects on the contextual requests. A temporary Edit Mode transport fixture pinned its own one-day test certificate, without changing system trust. No certificate or key is part of this change.

Leaving the variable unset keeps the production HTTPS `/chat` connection. Windows Players always retain legacy routing in this stage. Optional Editor fallback requires `AWAKENING_CONTEXT_CHAT_LEGACY_FALLBACK=1` and occurs only on connection failure or HTTP 502, 503, or 504. It sends the visitor's question as the sole legacy field with a localized general-reply status. A valid production legacy URL is required. Keep fallback unset during mock tests to avoid contacting Render. HTTP 410 causes the client to discard the token and allows manual Retry to issue a new one. Validation failures, malformed responses and rate limits stay on the contextual path.

A later authorized deployment needs the canonical knowledge directory, server-side OpenAI configuration, a model supporting structured output, and a deployment readiness check that exercises the contextual routes in addition to `/health`. Never copy credentials into Unity. A multi-instance deployment would need a decision about sticky routing or a separately approved ephemeral-state solution. Roll back by disabling the Editor variable. The server code can be rolled back independently to the existing checkpoint without changing Unity assets or resetting unrelated changes.
