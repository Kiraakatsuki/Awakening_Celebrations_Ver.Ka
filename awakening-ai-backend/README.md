# Awakening AI Backend

Small Express service that keeps the OpenAI API key on the server and exposes a Unity-friendly JSON API.

## Local development

Requires Node.js 20 or newer. From `awakening-ai-backend/`:

```sh
npm install
cp .env.example .env
# Set OPENAI_API_KEY in .env using your local secret manager/editor.
npm start
```

PowerShell users can copy the example with `Copy-Item .env.example .env`. Never commit `.env`.

The service reads `PORT` (defaults to `3000`), `OPENAI_API_KEY` (required for generated contextual replies), and `OPENAI_MODEL` (defaults to `gpt-4.1-mini`). The default model is listed in OpenAI's [models documentation](https://platform.openai.com/docs/models) and works with the Responses API. Set `OPENAI_MODEL` to another model enabled for your API project when needed.

`GET /health` returns `{"status":"ok"}`. Unity obtains a session from `POST /chat/v2/session`, sends questions to `POST /chat/v2`, and ends sessions through `POST /chat/v2/reset`. The contextual routes share the 20 requests per IP per minute limit. JSON request bodies are limited to 16 KB.

A stateless local request:

```sh
curl -X POST http://localhost:3000/chat/v2 \
  -H 'Content-Type: application/json' \
  -d '{"message":"Who founded JFC?","locale":"en"}'
```

Without a configured API key, the server still starts and `/health` remains available. Requests that need the provider receive a safe HTTP 503 response.

## Render Web Service

For Stage 5.2 local contextual development and its credential-free HTTPS mock, see the [API contract and setup](../Documentation/Stage5AIKnowledge/Stage5_2/Stage5_2_APIContract.md).

Create a Web Service from the Git repository with:

| Setting | Value |
| --- | --- |
| Runtime | Node |
| Root Directory | `awakening-ai-backend` |
| Build Command | `npm install` |
| Start Command | `npm start` |
| Health Check Path | `/health` |

Add these environment variables under the Render service's Environment settings:

| Name | Value |
| --- | --- |
| `OPENAI_API_KEY` | Your OpenAI API key, entered directly in Render's secret environment settings |
| `OPENAI_MODEL` | `gpt-4.1-mini` or another model enabled for your API project |

Render provides `PORT` automatically. Do not add the key to Unity, the repository, or a client build. Commit `package-lock.json` alongside `package.json`; Render installs only this backend's Node dependencies.

## Unity integration

Unity uses the configured HTTPS **Assistant Backend Base URL** and sends structured locale, session and selection context to `/chat/v2`. The reply text is read from the top-level `reply` field. The OpenAI API key stays in Render. An explicitly configured HTTPS loopback override is available in the Unity Editor for local tests.
