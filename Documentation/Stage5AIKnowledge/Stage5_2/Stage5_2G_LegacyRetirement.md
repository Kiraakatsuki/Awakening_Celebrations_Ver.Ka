# Stage 5.2G — Legacy AI retirement

The scene supplied the Render HTTPS origin and inherited the controller's Legacy mode initializer. That sent visitor questions to ungrounded `POST /chat`.

Unity now sends all questions, including presets and Retry, to `POST /chat/v2`. It creates and resets sessions through `POST /chat/v2/session` and `POST /chat/v2/reset`. It retains the configured HTTPS Render origin, normal certificate validation and redirect rejection. Developers can explicitly set `AWAKENING_CONTEXT_CHAT_URL` to an HTTPS loopback origin in the Editor for fixture testing. There is no protocol fallback.

Removed the Unity mode enum, Inspector field, performer-scoped text wrapper and Legacy serializer. Removed the Express `POST /chat` handler and its legacy validation and response handling. The contextual routes still share the existing limiter. The scene and its origin were preserved. Existing performer and portrait payload mappings, locale, cancellation and reply rendering paths were retained.

| Check | Result |
| --- | --- |
| Backend syntax and targeted local tests | PASS. Three tests covered the contextual-only route contract, retirement HTTP 404, session lifecycle and the shared per-IP limit. |
| Unity 6000.4.7f1 Edit Mode | PASS. The project compiled and the contract verified 24 bilingual dedicated and portrait payloads, general context, token parsing, HTTPS routing and override restrictions. The serialized scene has no Legacy mode dependency. |
| Play Mode and Windows Player | NOT RUN. Visitor acceptance remains with the user. |

Implementation commit `52190268100f1599080ef8e1eb013b7141bd0a5c` was verified on GitHub's `development/stage-5.2-jfc-ai` branch. Render auto-deployment ID `dep-dbsmkp68bjmc73encq80`.

Render marked the intended implementation live at 08:18:34 UTC on November 15, 2026. A bounded production smoke test returned HTTP 200 for `GET /health`, 404 for `POST /chat`, 201 for session creation, 200 for both contextual replies, 204 for reset and 410 when reusing the reset token. The English reply named founder Dynand Fariz. The Traditional Chinese reply compared Gaia with Pandemic, returned both supporting defile records and echoed the expected Gaia portrait context. Each reply took about 2.43 seconds end to end.

Older Unity copies that still call `POST /chat` now receive HTTP 404. For recovery use the preserved `awakening-stage5.2-ai-accepted` checkpoint and coordinate the application and Render revisions. The documentation-only follow-up carries `[skip render]` so the verified deployment stays live.
