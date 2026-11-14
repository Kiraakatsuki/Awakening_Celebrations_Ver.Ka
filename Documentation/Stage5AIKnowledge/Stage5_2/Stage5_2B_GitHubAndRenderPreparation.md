# Stage 5.2B — GitHub checkpoint and Render preparation

## Checkpoint scope

The development branch is `development/stage-5.2-jfc-ai` in [Awakening Celebrations Ver.Ka](https://github.com/Kiraakatsuki/Awakening_Celebrations_Ver.Ka). Its parent is Stage 4.3E commit `cf2f5013911304ce18c7ff2b40c13d62648d238e`, verified against the remote milestone branch and the peeled tag `awakening-stage4.3e-voice-complete`. Before the checkpoint, GitHub `main` pointed to `9415962d10db3f18266fd53d83e05e6854f31713`. There were no staged changes or unfinished Git operations. The nine local screenshot and metadata deletions and the previous checkpoint report's local verification receipt are outside the checkpoint scope.

The exact commit ID of this report's own checkpoint is the [GitHub development branch head](https://github.com/Kiraakatsuki/Awakening_Celebrations_Ver.Ka/commits/development/stage-5.2-jfc-ai/). A local post-push receipt records the independently verified SHA without changing that commit. Inspect the current remote ref and the complete checkpoint file list with:

```sh
git ls-remote origin refs/heads/development/stage-5.2-jfc-ai
git fetch origin development/stage-5.2-jfc-ai
git diff --name-status cf2f5013911304ce18c7ff2b40c13d62648d238e origin/development/stage-5.2-jfc-ai
```

| Area | Included in the checkpoint |
| --- | --- |
| Knowledge | `awakening-ai-backend/knowledge/` contains all 10 JSON collections, the contextual schema, loader/index and dataset README. The verified inventory contains 24 numbered editions, 21 sourced umbrella themes, three unresolved earliest umbrella titles, eight HEAL defiles, six featured performers, 15 contributors, 18 Unity image bindings with unresolved depicted identities, 39 sources and 21 bilingual evaluation pairs. |
| Server and tests | `awakening-ai-backend/server.js`, `context-chat.mjs`, launcher documentation, the Stage 5.1 validator, Node tests and the credential-free loopback fixture. The existing package manifest and lockfile are inherited. |
| Unity | `AwakeningApplicationController.cs`, `LivingGlassFlowIntegration.cs`, `LivingGlassPresentation.cs`, the existing speech capture presenter’s neutral accent adjustment, and `LivingGlassChatProtocol.cs` and `ContextChatEditorChecks.cs` with their metadata. |
| Documentation | Stage 5.1 research and integration design, Stage 5.2 API/context/retrieval reports and their evidence, and this runbook. |
| Preserved from Stage 4.3E | Living Glass startup scene, settings and approved narration directory with all 26 Ogg clips and their 26 Unity metadata files. The Whisper and Qwen runtimes remain external. |

Existing ignore rules cover `.env`, local override files, `node_modules`, Whisper and Qwen packages, private reference WAVs and audition material in their local directories, Unity caches, builds, generated logs and user settings. The deployment narration and knowledge JSON remain eligible for tracking. Scoped candidate and tracked-file checks looked for credential signatures and prohibited paths. Local credentials, private recordings, local runtime and model dependencies and temporary test files are excluded. The source checkpoint relies on the separately backed-up voice dependencies covered by the [Stage 4.3E recovery guide](../../Stage4VoiceExperience/Stage4_3/Stage4_3E_GitHubCheckpoint.md).

## Local validation

| Check | Result |
| --- | --- |
| JavaScript syntax | **PASS** for all six backend JS/MJS modules. |
| Stage 5.1 validation | **PASS** with `node awakening-ai-backend/scripts/validate-knowledge.mjs`. Knowledge/source/locale/Unity asset contracts and the counts above passed. |
| Backend and grounding | **PASS** for nine Node tests and all 42 localized fixture evaluations with `node --test awakening-ai-backend/test/context-chat.test.mjs`. Covers legacy and contextual APIs, index load, cross-defile retrieval, source citation allowlist, validation, session issuance/reset/expiry/races, provider failures and shared throttling. The model is faked. |
| Render-root portability | **PASS** on Node 24.22.1. Copied the runtime and ten data files under an isolated backend root, checked every installed dependency version against the lockfile, loaded from a different working directory and used loopback HTTP with the provider disabled. `GET /health` returned 200, legacy `POST /chat` returned the expected 503 without credentials, and v2 session issue, question and reset returned 201, 503 and 204. The isolated root had no Unity assets, mock service files or `.env`. A fresh Render dependency install and deployed Node version remain unverified. |
| Unity Edit Mode | **PASS** on 6000.4.7f1. The temporary Editor launcher compiled and ran 24 bilingual dedicated/portrait payload cases, general AI with no IDs, 72 reply layouts, 164 conversation checks, 432 microphone layouts, bilingual Home and Settings voice-preference checks, audio policy checks and serialized route/video validation for all six performers. These were isolated checks in Edit Mode. |
| Git hygiene | **PASS** for ignore probes, candidate-file review and credential-signature scan. Unity metadata uniqueness and committed asset preservation were checked. |
| Live validation | **NOT RUN** for Render, live OpenAI, Unity Play Mode, camera, microphone, local synth and Windows Player. The earlier mocked HTTPS transport run is recorded in [Stage 5.2 validation](Stage5_2_RuntimeValidation.md). |

## Expected settings for the existing service

These settings come from the repository's [backend README](../../../awakening-ai-backend/README.md), manifest and server. The Render dashboard configuration was outside this pass. Treat any branch change on the existing service as a **live rollout**. Render's dashboard source update can start a deployment as soon as you confirm it. [Render source changes](https://render.com/changelog/change-your-services-backing-repo-or-image-in-the-render-dashboard)

| Setting | Expected value |
| --- | --- |
| Repository | `Kiraakatsuki/Awakening_Celebrations_Ver.Ka` |
| Source branch after approval | `development/stage-5.2-jfc-ai` |
| Root Directory | `awakening-ai-backend` |
| Runtime | Node.js 20 or newer per `engines`. Tested on 24.22.1. Check the resolved version in the Render build log. `NODE_VERSION` takes precedence where it is already configured. The open-ended engine range can resolve to a newer major version. [Render Node version precedence](https://render.com/docs/node-version) |
| Build Command | `npm install` as documented for the existing service. The repo includes `package-lock.json`. |
| Start Command | `npm start` |
| Health Check Path | `/health` |
| Required environment | `OPENAI_API_KEY` stays in Render's server-side secret configuration. `PORT` is supplied by Render. |
| Optional environment | `OPENAI_MODEL` must support structured output for contextual replies if overridden. `JFC_RETRIEVAL_BUDGET` defaults to 6 and accepts integers from 1 to 10. |

Runtime loads its canonical JSON files relative to the knowledge module at startup. It has no dependency on a local Windows path, Unity asset, test fixture or local voice package. `scripts/validate-knowledge.mjs` also checks Unity paths, so run it from a full development checkout. Keep that test outside Render's start command.

## Later rollout and smoke checks

1. In the existing Render web service, note the currently successful `main` deploy's SHA and auto-deploy setting. Confirm the repository, root, commands and health path in **Settings → Build & Deploy**. Verify that the required secret variable is configured without displaying its value. Review any build filters. The root already limits automatic deploys to backend changes. If there is an include list, make sure its repository-relative patterns admit `awakening-ai-backend/server.js`, `awakening-ai-backend/context-chat.mjs`, `awakening-ai-backend/knowledge/**` and package manifest changes. Excludes must leave those paths eligible. A filter can suppress subsequent auto-deploys. Manual deploys and configuration-triggered deploys bypass the filters. [Render monorepo settings](https://render.com/docs/monorepo-support)
2. Once authorized to affect the live service, edit **Branch** in **Settings → Build & Deploy** and select `development/stage-5.2-jfc-ai`. If the UI groups it under **Build → Source → Edit**, keep the same GitHub repository and Node runtime, select the development branch, then confirm the source update. On **Deploys**, confirm the expected checkpoint SHA. If no deployment starts, use **Manual Deploy → Deploy latest commit** once. [Render source dialog](https://render.com/docs/native-runtimes) · [Render deployment controls](https://render.com/docs/deploys)
3. Inspect the build and startup logs for package installation, port binding, health failures or a contextual knowledge-load warning. `GET /health` must return HTTP 200 with `{"status":"ok"}`. Health alone cannot confirm knowledge or provider availability. From a trusted operator client, `POST /chat/v2/session` with `{}` must return 201. Reset the issued token with `POST /chat/v2/reset` and verify 204. Avoid publishing the token. [Render health checks](https://render.com/docs/health-checks)
4. Under a separate live-inference authorization, send a small legacy `POST /chat` request containing only `message` and check for a nonempty `reply`. Send one valid contextual request in each locale and inspect the `reply`, registered IDs and allowlisted citations where applicable. Follow the [v2 API contract](Stage5_2_APIContract.md). Existing Unity Player builds still use `/chat`. Hosted v2 availability does not enable Player routing. The current v2 Unity setting is Editor-only and requires HTTPS loopback.
5. Session history is short-lived and held in one Node process. Deploys, rollbacks and restarts can invalidate issued tokens. Multi-instance session routing and authorized live bilingual answer quality remain separate validation items.

## Rollback to the live legacy branch

1. In the existing service, switch the linked Branch back to `main`. Check the Deploys page for the known-good legacy SHA and use **Manual Deploy → Deploy latest commit** if the branch update doesn't start a deploy. Use the previously recorded SHA if `main` has advanced beyond the known-good revision. [Render deploy controls](https://render.com/docs/deploys)
2. If the last known-good deploy artifact is retained and fast recovery is needed, use **Deploys → Rollback** on that saved deploy. Dashboard rollback disables auto-deploys and preserves the service's current linked-branch configuration. Restore `main` as the source and re-enable its prior auto-deploy setting only after confirming recovery. [Render rollback behavior](https://render.com/docs/rollbacks)
3. Confirm a successful rollout in Deploys and check `GET /health` for 200. With authorization to exercise the provider, verify the original `POST /chat` shape and a nonempty reply. Contextual routes are absent on the earlier `main` backend. Discard contextual tokens after rollback and restart any contextual development conversation as needed. A failed build, a port-bind problem, an unhealthy deploy, or unexpected legacy route errors are signals to stop and inspect deployment logs.
