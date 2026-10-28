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

The service reads `PORT` (defaults to `3000`), `OPENAI_API_KEY` (required for `/chat`), and `OPENAI_MODEL` (defaults to `gpt-4.1-mini`). The default model is listed in OpenAI's [models documentation](https://platform.openai.com/docs/models) and works with the Responses API. Set `OPENAI_MODEL` to another model enabled for your API project when needed.

`GET /health` returns `{"status":"ok"}`. `POST /chat` accepts `{"message":"Hello"}` and returns `{"reply":"..."}`. Messages must contain 1–4000 non-whitespace characters. The service limits JSON bodies to 16 KB and chat requests to 20 per IP per minute.

Example request:

```sh
curl -X POST http://localhost:3000/chat \
  -H 'Content-Type: application/json' \
  -d '{"message":"Hello"}'
```

When the API key is absent, the server still starts and `/health` remains available; `/chat` returns HTTP 503 with a safe error. This allows Render health checks to pass while configuration is incomplete.

## Render Web Service

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

The Unity runtime uses the language-specific UI Toolkit assets registered in `RuntimeScreenCatalog`. `AwakeningApplicationController` now adds a live text input, send/retry action, suggestion handlers, and conversation view to the imported assistant panel. It sends JSON `{ "message": "..." }` to the deployed HTTPS `/chat` endpoint and displays the `reply` field. Configure the service base URL in the controller's **Assistant Backend Base URL** Inspector field. The OpenAI API key remains exclusively in Render.
