# Stage 4.3D.1 — Tutorial narration coverage

**Seven tutorial steps are mapped. Steps 1 and 4 play in English and Mandarin. Steps 2, 3, 5, 6 and 7 have new bilingual audio awaiting listening approval and stay silent in the visitor experience.**

[Open the local bilingual audition](<D:/AI-Voice-Auditions/Qwen3-TTS-Q8/Stage4_3D1_TutorialDraft/Audition.html>) · [Clip inventory and checksums](<D:/AI-Voice-Auditions/Qwen3-TTS-Q8/Stage4_3D1_TutorialDraft/ClipInventory.json>) · [Unity playback trace](Stage4_3D1_Evidence/Runtime.txt)

## Confirmed cause and correction

The seven-step sequence in `LivingGlassRoutes.Tutorials` was already visible in both locales. `GuideCues.For` returned `overview` and `gestures` for steps 1 and 4. Every other tutorial route reached the default branch and returned null. The previously approved catalog also had no matching clips for the other five steps. The coordinator therefore exited before attempting an asset load, irrespective of voice preference, locale, audio priority or replay state. Play Mode reproduced the silence on those five routes in both languages.

The route resolver now maps every step to a distinct cue. The existing approved catalog continues to be the admission point for prerecorded playback. The five draft pairs are held outside Unity with their masters and deployment candidates. When a visitor explicitly enables Voice Assistance in tutorial Settings, the current eligible cue can start after the window has finished closing. An update or ordinary dismissal cannot replay an already-consumed cue.

## Coverage matrix

The IDs below match the English and Mandarin clip stems, with `-zh` used for Mandarin in the Traditional Chinese UI. The shared replay policy is one automatic attempt per cue and locale per visit. Entering a different step cancels old playback and invalidates any pending load. Rerenders, same-locale revisits and ordinary Settings dismissals do not repeat a consumed cue. Voice Assistance Off remains silent, and the visible instructions are always available.

| Step / stable route | Displayed English instruction | 顯示的繁體中文指示 | English clip ID | Mandarin clip ID | Availability and runtime result |
| --- | --- | --- | --- | --- | --- |
| 1 · `TUTORIAL---ENTRY---DIM` | Seven short steps introduce the experience. Mouse and keyboard alternatives remain available. | 透過七個簡短步驟認識體驗，也可使用滑鼠與鍵盤。 | `overview-en` | `overview-zh` | Approved. Both played and completed. |
| 2 · `TUTORIAL---INTRODUCTION` | Keep your hands visible. Hold one open palm for two seconds to go Back where the experience permits. | 讓雙手保持在鏡頭範圍內。在允許返回的階段，張開一隻手掌並保持兩秒即可返回。 | `tutorial-move-en` | `tutorial-move-zh` | Pending. Both remained silent as required. |
| 3 · `TUTORIAL---LOOK---MOVE` | Wave side to side to release colour. Clap to play the existing environmental sound. | 左右揮手釋放色彩；拍手播放既有環境音效。 | `tutorial-colour-en` | `tutorial-colour-zh` | Pending. Both remained silent as required. |
| 4 · `TUTORIAL---POINT` | Point at one of six presences. Pointing identifies a target; it never confirms your choice. | 指向六種存在之一。指向僅辨識目標，不會確認選擇。 | `gestures-en` | `gestures-zh` | Approved. Both played and completed. |
| 5 · `TUTORIAL---HOLD---SELECT` | After targeting a presence, make a fist to confirm. You can also click a portrait or press 1–6. | 鎖定目標後握拳確認。也可點擊肖像或按 1–6 選擇。 | `tutorial-confirm-en` | `tutorial-confirm-zh` | Pending. Both remained silent as required. |
| 6 · `TUTORIAL---EXPERIENCE` | In Focus, show both open palms toward the camera to start. A Play video button provides the same action. | 在專注畫面中，將張開的雙掌朝向相機以開始；也可按「播放影片」。 | `tutorial-portrait-en` | `tutorial-portrait-zh` | Pending. Both remained silent as required. |
| 7 · `TUTORIAL---READY` | The gallery awaits. Continue, then wave—or use the on-screen awakening action. | 藝廊正等待著您。繼續後揮手，或使用畫面上的喚醒按鈕。 | `tutorial-ready-en` | `tutorial-ready-zh` | Pending. Both remained silent as required. |

- **Entry triggers** Step 1 follows Privacy Continue in the visitor flow. Steps 2–7 follow each preceding tutorial Continue. The final button attempts to enter Attract and respects camera consent. Switching locale may play the current step once in the new locale.
- **Return alias** `TUTORIAL---RETURN---DISMISS` has the same title, instruction and cue as step 2. The normal numbered flow does not enter the alias. Its existing Continue goes to step 2, which shares the same replay identity. Alias coverage was checked in Edit Mode.
- **Reused audio** The eight approved pairs were reviewed against their contexts. The two tutorial pairs stay on their original routes. The Point clip briefly previews fist and click, but step 5 also needs the 1–6 keyboard alternative. The Attract clip assumes its awakening button is already on screen and does not cover the clap cue. Its timing and content are unsuitable for steps 3 or 7.
- **Existing visual detail** Step 7 still displays its existing Clap illustration alongside instructions for using the awakening action after Continue. Narration follows the written instruction. The UI artwork and text were outside this correction.

## Draft generation and review

**All ten draft clips are PENDING USER LISTENING APPROVAL.** The local audition includes both scripts and a labeled player per language, along with links to the PCM masters and Ogg deployment candidates. The saved approved English WAV and established ICL conditioning were used with Qwen3-TTS 0.6B Base Q8, native release v0.2.0, runtime revision `b91bca43f9adc5df839161ce4c88b0f6743b27ff`, GGUF revision `b7ee2e8c7459c3bea99da23e3d178125a7d1713c`. Both model files and the reference/conditioning inputs matched the recorded hashes. Generation retained the prior seed and sampler settings, recorded in the linked inventory.

The standalone generation produced 10 untrimmed PCM masters, totaling 2,895,800 bytes, and 10 24 kHz mono Ogg review copies, totaling 458,728 bytes. The original encoded durations are 5.368–7.368 seconds. All clips decoded, contained nonzero signal below digital clipping, and stayed under the generation cap. The review copies retain the existing short end bridge and quiet tail. No inference is required to play these files after approval. Pronunciation, phrasing and voice fidelity require listening approval.

Offline Whisper recovered the intended instructions for the five English and three Mandarin primary drafts, allowing for transcription punctuation, spelling and numerals. The Mandarin step 5 and step 7 clips produced character substitutions around the confirmation instruction, keyboard terms and the gallery name. Two alternate Mandarin takes using the same model, reference and generation parameters are available. Their automated transcripts still contain substitutions, so human review should focus on those terms and decide whether either take is acceptable. All twelve audio files remain outside the visitor catalog. [Primary screen](<D:/AI-Voice-Auditions/Qwen3-TTS-Q8/Stage4_3D1_TutorialDraft/OfflineContentScreen.json>) · [Mandarin alternate audition](<D:/AI-Voice-Auditions/Qwen3-TTS-Q8/Stage4_3D1_RevisionReview/Audition.html>) · [Alternate screening](<D:/AI-Voice-Auditions/Qwen3-TTS-Q8/Stage4_3D1_RevisionReview/OfflineContentScreen.json>)

## Validation

| Check | Result | Evidence |
| --- | --- | --- |
| Unity 6000.4.7f1 compilation | PASS | Editor completed script reload and ran both Play Mode passes and the isolated check suites. |
| Approved catalog, tutorial route mapping, alias and audio policy | PASS | All existing approved assets loaded as 24 kHz mono. The checks covered all seven step IDs, draft exclusion, invalidation, locale, replay, the six reflection routes, and protected-stage silence. [Voice checks](Stage4_3D1_Evidence/Voice.txt) |
| Reply / conversation / microphone | PASS, 72 / 164 / 432 | [Reply](Stage4_3D1_Evidence/Reply.txt) · [Conversation](Stage4_3D1_Evidence/Conversation.txt) · [Microphone](Stage4_3D1_Evidence/Mic.txt) |
| Scene and media references | PASS | Living Glass development entry, 52 bilingual route pairs and six performer media references verified. [References](Stage4_3D1_Evidence/References.txt) |
| All seven steps in both locales | PASS for active clips and approval gating | Began at Home, enabled voice using the real control, and entered step 1 through a safe route fixture because Privacy Continue requires consent. Actual UI Toolkit keyboard events drove the consecutive tutorial Continue buttons. No camera or microphone was authorized. Both approved clips played per locale, and all pending steps were silent. |
| Revisit, locale switch, rapid navigation and Voice Assistance | PASS | Existing Back remained disabled during onboarding. Safe same-visit route revisits did not replay consumed clips. The initial synthetic rapid-click probe advanced multiple generations in one editor update and failed its expectation. A separate targeted pass issued each activation on a new update and confirmed immediate interruption, no stale Point playback, correct locale and prompt stopping. Settings opt-in waited until its exit finished. Off and Home reset stayed silent. [Runtime steps](Stage4_3D1_Evidence/Runtime.txt) |
| AudioSource completion and output integrity | PASS for technical observations | Four approved clips advanced to within 64 samples of the imported end and cleared the clip reference naturally. Windows Realtek render endpoint loopback aligned across the full duration of each approved source, captured its final audio and then quiet. This is digital evidence, with no subjective room or phoneme-quality assessment. [Signal analysis](Stage4_3D1_Evidence/Signal.json) |
| Console, isolation and exit | PASS | Two expected Attract consent warnings, zero captured application errors or exceptions. Both passes exited to Edit Mode with zero voice sources. The saved scene was clean and the camera gate denied. The assistant endpoint was disabled in the unsaved test instance. No live GPT, physical camera or microphone, Windows Player, or full performer journey rerun was performed. [Console capture](Stage4_3D1_Evidence/Console.txt) |
| Runtime playback of ten newly generated clips | BLOCKED pending approval | No draft asset was installed or played in the visitor app. |

The render-endpoint WAVs are available for [step 1 English](<D:/AI-Voice-Auditions/Qwen3-TTS-Q8/Stage4_3D1_TutorialDraft/RuntimeEvidence/overview-en-endpoint.wav>), [step 1 Mandarin](<D:/AI-Voice-Auditions/Qwen3-TTS-Q8/Stage4_3D1_TutorialDraft/RuntimeEvidence/overview-zh-endpoint.wav>), [step 4 English](<D:/AI-Voice-Auditions/Qwen3-TTS-Q8/Stage4_3D1_TutorialDraft/RuntimeEvidence/gestures-en-endpoint.wav>) and [step 4 Mandarin](<D:/AI-Voice-Auditions/Qwen3-TTS-Q8/Stage4_3D1_TutorialDraft/RuntimeEvidence/gestures-zh-endpoint.wav>).

## Files and handoff

- `Assets/LivingGlass/Speech/LivingGlassVoiceCoordinator.cs` maps all seven steps and handles current-step opt-in after Settings dismissal.
- `Assets/LivingGlass/Integration/LivingGlassFlowIntegration.cs` provides the coordinator with the existing presentation’s overlay visibility.
- `Assets/LivingGlass/Speech/VoiceExperienceChecks.cs` adds coverage for numbered routes, the alias, localization and draft exclusion.
- This report and `Stage4_3D1_Evidence/` contain the new validation records. Local audition files, generation scripts and versions are under `D:/AI-Voice-Auditions/Qwen3-TTS-Q8/Stage4_3D1_TutorialDraft` and the adjacent `Scripts` directory. Temporary Editor instrumentation is removed at handoff.
- The approved catalog, scene and project settings were preserved. All 16 approved audio hashes matched the Stage 4.3D inventory. The local Git baseline remains `9415962d10db3f18266fd53d83e05e6854f31713`. No Git push was made.

After listening approval, add only the accepted bilingual pairs to the established approved catalog and asset path, retaining the verified hashes and manifest mapping. Then run the full two-language tutorial again, observe all seven actual starts and completions, and verify transition cancellation for the newly active clips.
