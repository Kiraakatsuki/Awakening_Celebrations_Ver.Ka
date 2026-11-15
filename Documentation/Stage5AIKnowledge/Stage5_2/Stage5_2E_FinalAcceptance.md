# Stage 5.2E — Final AI alias and Qwen playback verification

| Acceptance item | Result | Evidence |
| --- | --- | --- |
| Gaia alias promotion | PASS | GitHub branch `development/stage-5.2-jfc-ai` and Render both resolved to `6345d7296ede7cd997b150e7bf47322452422681`. Render deployment `dep-dbsm0ue7bikc73f7m7t0` was live at 15:36:30 Taipei on November 15, 2026. `GET /health` returned HTTP 200. The deployment required no rollback. |
| Live Mandarin alias | PASS | In selected Gaia context, “蓋亞和 Pandemic 有什麼不同？” returned HTTP 200 in 4.70 s with `DEF-JFC26-GAIA` and `DEF-JFC26-PANDEMIC`. The reply contrasted Gaia as the Earth and life theme with Pandemic as a public-health crisis exposing social vulnerability. It requested no clarification. The test session reset returned HTTP 204. |
| Qwen startup and lifecycle | PASS | With the isolated Editor closed, the original worker reported ready in 1.83 s. Health succeeded, English and Mandarin produced 24 kHz WAV output, shutdown was acknowledged, and the process exited with code 0. |
| Live GPT → Unity → Qwen, English | PASS | Contextual text appeared in 3.72 s. Manual Listen produced a 7.53 s clip and playback advanced in 7.77 s. The last observed sample frame was 180,224 of 180,672, followed by clip release. On replay, Stop cleared active audio within the 43 ms observation interval. |
| Live GPT → Unity → Qwen, Mandarin | PASS | Contextual text appeared in 1.15 s. Manual 朗讀 produced a 6.09 s clip and playback advanced in 4.98 s. The last observed sample frame was 145,920 of 146,112, followed by clip release. Closing the conversation on replay stopped and released audio by the first check. |
| Startup-failure fallback | PASS | An isolated Unity Edit Mode fixture returned a synthetic worker startup error in both locales. The transcript stayed visible, Listen was restored, and no audio started. |
| English voice listening | USER APPROVED | The user personally listened and found the audible quality acceptable. |
| Mandarin voice listening | USER APPROVED | The user personally listened and found the audible quality acceptable. |
| Audible natural endings | USER APPROVED | The user confirmed the English and Mandarin speech endings were acceptable. |
| Qwen technical playback | PASS | The Stage 5.2E Unity run verified playback and clip release in both locales. |
| Live GPT and backend validation | PASS | See the Stage 5.2E results above. |
| Full Windows Player and combined hardware-load testing | NOT YET VERIFIED | Outside the acceptance checks recorded here. |

Stage 5.2F records the user's listening judgment separately from the Stage 5.2E technical evidence. This checkpoint did not rerun the tests.

## Cause and correction

The isolated Editor discovered the Stage 4.3D Qwen pack through its directory junction. Runtime and model paths resolved, and the speaker embedding, reference codec and reference transcript matched the pinned hashes. With both Editors open, direct launches from the project and isolation paths returned `startup_failure`. A direct native launch reached model loading and then failed at `GGML_ASSERT(ctx->mem_buffer != NULL)`. This puts the observed failure at native worker buffer allocation, before synthesis, decoding or playback. Closing only the task-owned isolated Editor restored operation with the same pack in the primary Editor. The exact constrained resource and allocation size were not established. No Qwen code, model, reference, scene or production configuration was edited.

## Focused checks and files

- Commit changed `awakening-ai-backend/knowledge/data/defiles.json` and `awakening-ai-backend/test/context-chat.test.mjs`. The regression verifies unambiguous Gaia resolution for `Gaia`, `GAIA`, `蓋婭`, `蓋亞` and `Dewi Bumi`, plus the exact Gaia–Pandemic subject pair from a different selected performer. Unrelated local changes stayed outside the commit.
- JavaScript syntax checks and four targeted contextual tests passed. The Unity 6000.4.7f1 Editor compiled the temporary playback fixture and ran it against the production HTTPS endpoint. Both requests carried the expected locale, Gaia performer ID and registered portrait ID. Text rendered before Listen and remained visible during normal speech playback.
- The temporary fixture selected Contextual mode without saving it, disabled the camera rig for the test and restored the saved scene on exiting Play Mode. The saved mode remains Legacy. Camera and microphone devices and the Windows Player build were outside the test. The prior full regression suites were used as historical evidence only.
