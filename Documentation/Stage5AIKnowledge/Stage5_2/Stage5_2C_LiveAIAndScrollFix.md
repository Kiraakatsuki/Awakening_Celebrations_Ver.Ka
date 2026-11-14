# Stage 5.2C — Live contextual AI and transcript scrolling

## Baseline and causes

The local and remote development branch began at `2b320b21347045fa22c5859c955b554208fb99c4`. Runtime source matched that commit. Nine local screenshot deletions and both local recovery-report receipts were outside this patch. The Render service was checked through the HTTPS origin serialized in `AwakeningLivingGlass.unity`.

1. **Exhibition requests went to legacy chat.** The saved scene had the Render origin, but contextual routing depended on the Editor-only `AWAKENING_CONTEXT_CHAT_URL` numeric-loopback override. Normal requests used `/chat` and sent a display performer name plus an instruction to use the facts supplied in the request. That request supplies no festival evidence. The deployed contextual endpoint did answer the sampled selected presets once the structured IDs were sent.
2. **Comparison retrieval dropped the selected operand.** On the deployed endpoint, “How is this different from Pandemic?” with Gaia selected returned only Pandemic's record and asked for more detail. The named-entity branch discarded the selected implicit subject. The Traditional Chinese presence preset could also miss the selection semantics after a recent question about another performer, and unselected presets lacked consistent HEAL evidence anchors. The local retriever now recognizes explicit references to the current presence, resolves the missing operand for comparative questions, preserves ordinary named-entity precedence and keeps recent conversational subjects for genuine follow-ups. The provider prompt contains localized resolved subjects and grounds an answer in the returned passages. Registered portraits still carry the existing authorship and identity uncertainty.
3. **Automation fought an intentional scroll position.** In Play Mode, the wheel itself moved in both directions. The old status code reparented its label into the scroll content and called `ScrollTo` unconditionally. Reply completion scheduled another unconditional `ScrollTo` after appending text and the optional speech row. In a controlled reproduction on the same scene, those calls pulled the transcript from mid-history to the tail. The patch keeps status separate from the transcript and follows once after UI layout when the visitor sends a question or is already at the tail when a reply arrives. An intervening scroll or a detached session suppresses that follow operation.

## Operator routing

| Configuration | Route |
| --- | --- |
| Saved scene and project default, **Legacy** | Existing HTTPS `/chat` |
| **Contextual** on `AwakeningApplicationController` in the Inspector | Existing HTTPS origin with `/chat/v2/session`, `/chat/v2` and `/chat/v2/reset`. Applies in the Editor and through the same serialized field in a future Player. |
| Editor process with a valid `AWAKENING_CONTEXT_CHAT_URL` | Overrides the Inspector protocol with contextual routing to numeric HTTPS loopback for tests. Unset it to exercise the configured Render origin. |

The public base URL must be an HTTPS origin with no path, query, fragment or embedded userinfo. Contextual requests disallow redirects and use Unity's normal certificate validation. The former optional automatic fallback is removed. A contextual failure surfaces as an error in that conversation. An operator can explicitly select Legacy. The saved scene remains on Legacy because the corrected backend still needs promotion. Reply parsing, session reset, camera consent, Whisper and Qwen interfaces remain the same.

## Actual Unity presets and evidence

These are the three existing buttons in `BuildChat`. They submit the displayed string using the same request flow as Ask. No preset copy was changed. The backend test reads the preset strings from the Unity source and exercises all three in both locales across six dedicated contexts, six registered chooser portraits and the unselected common assistant, for **78 HTTP fixture cases**.

| English | Traditional Chinese | With a selection | General assistant |
| --- | --- | --- | --- |
| What can I notice? | 我可以留意什麼？ | Selected `DEF-JFC26-*` official theme, plus presentation or Gaia relationship where relevant. Distinguish festival imagery from the depicted costume. | `JFC-HEAL-2026/narrative` and `JFC-EXHIBITION-SCOPE/summary` support an observation about the exhibition. |
| Help me reflect. | 引導我回顧感受。 | Selected defile theme and relationship passages support a reflective question or observation. | HEAL narrative and exhibition scope support a reflection without assuming a selected image. |
| What is known about this presence? | 關於這種存在，有哪些已知資訊？ | Selected defile passages, even after discussing another named defile. | Offer exhibition context and ask which presence when needed. |

The unchanged mapping is `GAIA → GAIA`, `PANDEMIC → PANDEMIC`, `RE-BIRTH → REBIRTH`, `WHITE-GUARDIAN → WGUARDIAN`, `CONSCIENCE → CONSCIENCE`, and `SANCTUARY → SANCTUARY` under the `JFC26-` prefix. The dedicated request sends its canonical performer ID with `source=performer-page`. The chooser sends that performer ID and the matching `-IMG01` with `source=common-ai`. General AI sends the source without IDs. Raw images and inferred costume credits are absent. See the [full context map](Stage5_2_ContextMapping.md).

## Live deployed service and response review

The measurement covers small synthetic exhibition questions generated for this diagnostic. No visitor conversation, secret or session token was captured in report evidence. The Render service ran the pre-fix server during these tests.

| Deployed check | HTTP | Time | Result |
| --- | ---: | ---: | --- |
| `GET /health` | 200 | 273 ms | Health returned `ok`. |
| Legacy greeting | 200 | 1,732 ms | Ordinary `reply` returned. |
| `POST /chat/v2/session` | 201 | 94 ms | Issued token of expected shape. Session issuance and returned citations confirm knowledge availability. |
| Gaia, English “What can I notice?” | 200 | 1,518 ms | Direct answer about Gaia and the festival presentation. The Gaia passages support the Earth motif and Raja Ampat reference. |
| Pandemic portrait, Traditional Chinese reflection preset | 200 | 2,712 ms | Recognized Pandemic and discussed social vulnerability and recovery. Natural script and understandable wording. A reflective question would have matched the request more closely than a thematic summary alone. |
| Gaia, English presence preset | 200 | 1,479 ms | Gaia and the central earth narrative were identified directly, with a distinction between the reported marine motif and costume attribution. |
| Pandemic portrait, Traditional Chinese presence preset | 200 | 2,336 ms | Correct subject and evidence. Traditional Chinese response was understandable but the phrase “名為Pandemic的表現” was awkward. |
| Gaia, English “What does this represent?” | 200 | 1,654 ms | Correct selected subject, concise grounded response, no clarification. |
| Gaia, English comparison with Pandemic | 200 | 1,760 ms | **FAIL on deployed baseline.** Pandemic evidence alone, unnecessary clarification. Local fix retrieves both operands and passes the fake-provider contract. Actual GPT with the patched backend awaits promotion. |
| Pandemic portrait, Traditional Chinese costume designer | 200 | 108 ms | Explicitly unverified. Deterministic policy response supported by image binding and defile records. This case did not invoke GPT. |
| Invalid locale, reset, then reuse of reset token | 400, 204, 410 | 75, 69, 104 ms | Safe validation error, successful reset, stale session rejected with `session_expired`. |
| Natural 15-minute token expiry | 410 | 929 s after issuance | **PASS**. An issued token left unused was rejected with `session_expired`. No provider generation was required. The final HTTPS check included network time. |

Live model question response times for the six contextual cases that reached the provider were 1.48–2.71 s, median 1.71 s. Includes the failed comparison. Two additional Unity Play Mode requests through the configured Render HTTPS origin rendered real English and Traditional Chinese replies in 4.27 s and 3.95 s from the fixture’s wait start, including Unity transport and visible reply handling. The first Unity attempt exceeded the fixture’s 12 s wait budget and was canceled. The completed diagnostic allowed Unity’s normal transport timeout. These samples establish deployed routing and representative behavior. They do not establish model quality for all questions.

## Unity Play Mode and scroll evidence

Tests used the actual Living Glass scene in Unity 6000.4.7f1. Unsaved fixture overrides kept the gesture rig inactive and consent denied. A temporary local HTTPS fake provider used a certificate pin restricted to its exact loopback address and was removed after testing. The Render test used the ordinary Unity TLS path. The fixture invoked the actual chooser and Ask/suggestion handlers, used direct route entry for reflection, and dispatched Unity UI Toolkit wheel events to the transcript. No physical camera or microphone was activated. The Editor exited Play Mode and the scene and temporary routing overrides were restored.

| Scenario | Observed result |
| --- | --- |
| General and portrait chooser, six dedicated reflection pages | **PASS**, rendered replies from the fake provider and verified received scope, performer and image IDs. Both locales exercised across the runtime cases. All 24 locale/surface payload variants run separately in Edit Mode. The first combined run later reached the existing shared 20/minute limiter. The lifecycle tests were completed in a separate pass. |
| Follow-up and context change | **PASS**, the fake provider saw the same-scope follow-up with one previous turn and no stale history when the route's selected context changed. |
| Close, reopen and visitor reset | **PASS**, closing while a fake-provider reply was pending aborted it at the server, reopening created an empty transcript, and Home reset cleared the local token. Local backend tests independently verify revocation and overlapping-request behavior. |
| Long answers and wheel | **PASS**, real UI Toolkit scroll moved in both directions in English and Chinese. With the same pending status transition, offsets 1378.2 and 1577.9 stayed fixed. The reproduction of the old calls moved them to 2671.2 and 3055.0. With the fixed after-layout follow, a new question reached the tail at 1367.0/1367.0, and a delayed 1,247-character reply left the manually selected reading position at 528.8 while appending the reply and speech row. |
| Manual Qwen Listen | **PASS** for a returned local-fixture reply. The UI reached `Preparing audio…`, then `Playing`, and the fixture invoked Stop. Audible sound quality was outside this check. |
| Further runtime cases | **PASS**. Incoming 496-character Traditional Chinese reply preserved manual offset 529.7. The normal locale switch retained the Pandemic portrait, reused the conversation context and sent `en`. Navigating from a pending Pandemic reflection to White Guardian aborted the server task and left the destination transcript empty. |
| Physical touchpad, physical dictation/camera, Windows Player | **NOT RUN**. No hardware activation or Player build. |

[Scroll reproduction](../../../Temp/Stage52C/oldscroll.txt) · [Fixed bilingual offsets](../../../Temp/Stage52C/fixed.txt) · [Local delayed reply and Listen](../../../Temp/Stage52C/mockscroll.txt) · [Chinese delay, locale and route cancellation](../../../Temp/Stage52C/mocknav.txt) · [Unity to Render](../../../Temp/Stage52C/live.txt). These are local, ignored scratch logs.

## Regression and measurements

- Node.js **PASS**, 11 test groups, all 42 established bilingual evaluations and the new 78 actual preset/scope cases. The separate knowledge validator passed with 24 editions, 21 sourced umbrella themes, eight defiles, 18 image mappings, 39 sources and three unresolved early titles. The dataset and schemas were preserved.
- Unity Edit Mode **PASS** on Unity 6000.4.7f1 for 24 contextual payloads and route policy, 72 reply layouts, 164 conversation checks including new up/down wheel assertions, 432 microphone layouts, existing voice preference/audio policy and serialized route/media references.
- Local deterministic retrieval on Node 24.22.1 loaded 124 indexed passages in 11.83 ms. Across 2,000 warm calls after 200 warmups, p50 was 0.068 ms and p95 was 0.111 ms. Responses receive at most six passages by default. These are local retrieval measurements, separate from network or provider latency.
- The initial temporary Unity fixture had its own compilation and layout setup errors before validation. The Editor also logged unrelated Unity Assistant package entitlement errors. The application tests use the configured Render endpoint or the fake loopback service. The fixture and pin source were removed.

## Files changed

- `Assets/Scripts/AwakeningApplicationController.cs` and `Assets/LivingGlass/Integration/LivingGlassFlowIntegration.cs` for operator mode selection, request routing and transcript scroll ownership.
- `Assets/LivingGlass/Integration/LivingGlassChatProtocol.cs` for HTTPS origin policy and route choice.
- `Assets/LivingGlass/Presentation/LivingGlassPresentation.cs` for status placement.
- `awakening-ai-backend/knowledge/index.mjs` and `awakening-ai-backend/context-chat.mjs` for selected-subject resolution, preset evidence and model grounding.
- `awakening-ai-backend/test/context-chat.test.mjs`, `Assets/LivingGlass/Speech/ContextChatEditorChecks.cs` and `Assets/LivingGlass/Editor/LivingGlassConversationChecks.cs` for regression checks.
- This report. The recovery tag, scene YAML, audio assets, existing voice runtimes and GitHub/Render deployment configuration are untouched.

## Promote only after review

1. Review the isolated local diff and run `node --test awakening-ai-backend/test/context-chat.test.mjs`, `node awakening-ai-backend/scripts/validate-knowledge.mjs` and the isolated Unity checks. Keep credentials and temporary fixtures out of the change set.
2. A later authorized push to `development/stage-5.2-jfc-ai` can update the live Render service. Record the current successful deploy SHA, inspect Build Filters and Auto-Deploy, and follow the existing [rollout and rollback guide](Stage5_2B_GitHubAndRenderPreparation.md). A backend code change does not require a Render environment or model setting change.
3. After the authorized rollout, verify health, legacy chat, session issuance and reset. Re-run a bounded live bilingual check including Gaia versus Pandemic, a named unrelated defile, a preset after a subject switch, and an unverified costume credit. Select **Contextual** for a build candidate only after checking the deployed fix. Existing clients keep the legacy route until their operator configuration is changed.
4. For immediate Unity recovery select **Legacy**. If the server rollout must be reverted, restore the previous successful Render deploy or source commit using the recorded SHA. Redeployments invalidate in-memory visitor sessions. Leave the Stage 4.3E recovery refs as they are.

No commit, push, Render settings change or deployment was performed in this phase.
