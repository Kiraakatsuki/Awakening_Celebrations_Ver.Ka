# Stage 4.3D.2 — Bilingual tutorial narration finalization

The active catalog revision is `stage43d2-approved-tutorial`. All seven numbered tutorial routes have English and Mandarin audio. Mandarin uses the `zh` resource suffix in the Traditional Chinese interface. The ten step 2, 3, 5, 6 and 7 files were copied byte-for-byte from the user-approved auditions. The Mandarin files at steps 5 and 7 came from the revision-review audition.

## Active narration manifest

Each ID below has a stable Unity Resources address under `LivingGlassGuide/Approved/`. The activated source paths, English and Mandarin scripts, SHA-256 checksums, sample counts, durations, and Unity GUIDs are in the [activation inventory](Stage4_3D2_ApprovedSources.json). Step 1 remains on its existing approved pair.

| Step | Route | English ID | Mandarin ID | Active selection |
| --- | --- | --- | --- | --- |
| 1 | `TUTORIAL---ENTRY---DIM` | `overview-en` | `overview-zh` | Existing approved |
| 2 | `TUTORIAL---INTRODUCTION` | `tutorial-move-en` | `tutorial-move-zh` | User-approved primary takes |
| 3 | `TUTORIAL---LOOK---MOVE` | `tutorial-colour-en` | `tutorial-colour-zh` | User-approved primary takes |
| 4 | `TUTORIAL---POINT` | `gestures-en` | `gestures-zh` | Revised pair passed the conditional quality gate |
| 5 | `TUTORIAL---HOLD---SELECT` | `tutorial-confirm-en` | `tutorial-confirm-zh` | User-approved primary English and **alternate Mandarin** |
| 6 | `TUTORIAL---EXPERIENCE` | `tutorial-portrait-en` | `tutorial-portrait-zh` | User-approved primary takes |
| 7 | `TUTORIAL---READY` | `tutorial-ready-en` | `tutorial-ready-zh` | User-approved primary English and **alternate Mandarin** |

The alias `TUTORIAL---RETURN---DISMISS` shares step 2 and its replay identity. The six non-tutorial pairs, step 1, and the prior approval decisions remain as described in the [Stage 4.3D inventory](Stage4_3D_AudioAssetInventory.md). The Mandarin primary files for steps 5 and 7 differ in SHA-256 from the copies installed here.

## Step 4 script and quality gate

| Language | Previous | Active replacement |
| --- | --- | --- |
| English | Point toward the presence that catches your attention. Pointing highlights your choice. Make a fist to confirm, or click a portrait. | Point toward the presence that catches your attention. Pointing highlights the target without selecting it. You can also click a portrait to choose. |
| Mandarin | 指向吸引你注意的那位存在。指向只會標示目標；握拳即可確認，也可以點擊肖像。 | 請用手指，指向吸引你的那一位。畫面會亮起提示，選擇還沒有完成。也可以用滑鼠點選肖像。 |

Step 4 describes targeting, the distinction between highlighting and selection, and the existing mouse alternative. Step 5 owns the dedicated fist-confirmation instruction on `TUTORIAL---HOLD---SELECT`, including mouse and numeric-key alternatives. The two steps retain distinct route and replay identities. They use the same playback source, and advancing cancels the old cue. No gesture, consent or UI wording was changed.

The initial Mandarin step 4 take produced substitutions around the cue’s core instruction, including `只想追随` for `指向最吸引` and `夸以` for `確認`. The initial pair stayed outside Unity. The active candidate was generated using Qwen3-TTS 0.6B Base Q8, release v0.2.0, the established ICL reference and seed 42. The generator verified the original English WAV, reference transcript, speaker embedding, reference codes and both GGUF hashes. Both active takes reached native EOS below the generation cap.

| Check | English | Mandarin |
| --- | --- | --- |
| Whisper CPU transcription, beam 5 and beam 1 | Exact words on both passes, ignoring punctuation | All intended words on both passes, allowing for Simplified Chinese output and punctuation |
| Final phrase recovered on both passes | “You can also click a portrait to choose.” | 「也可以用滑鼠點選肖像」 |
| Decoded deployment clip duration | 8.088 s, 24 kHz mono | 9.048 s, 24 kHz mono |
| Deployment peak / clipped master samples | −4.04 dBFS / 0 | −2.63 dBFS / 0 |
| Onset / longest internal quiet interval | 0.28 s / 0.65 s | 0.48 s / 0.61 s |
| Last 10 ms of unpadded master / quiet following speech | −90.31 dBFS / 0.328 s in Ogg | −84.29 dBFS / 0.328 s in Ogg |
| Speaker embedding cosine versus old approved same-language take | 0.9955 | 0.9941 |

The exact transcripts, decoded sample integrity, quiet tails and speaker comparison gave no unresolved quality warning for the active takes. The approval condition was satisfied for the selected English and Mandarin recordings. Automated measurements substantiate content and signal integrity. They do not constitute a subjective listening assessment of accent or acoustics. [Active step 4 audio](<D:/AI-Voice-Auditions/Qwen3-TTS-Q8/Stage4_3D2_PointFinalReview/Audition.html>) · [First take transcription](<D:/AI-Voice-Auditions/Qwen3-TTS-Q8/Stage4_3D2_PointReview/OfflineContentScreen.json>) · [Final beam 5 transcript](<D:/AI-Voice-Auditions/Qwen3-TTS-Q8/Stage4_3D2_PointFinalReview/OfflineContentScreen.json>) · [Final beam 1 transcript](<D:/AI-Voice-Auditions/Qwen3-TTS-Q8/Stage4_3D2_ConfirmationScreen/OfflineContentScreen.json>) · [Signal and speaker comparison](<D:/AI-Voice-Auditions/Qwen3-TTS-Q8/Stage4_3D2_PointFinalReview/QualitySignals.json>)

## Runtime verification

| Check | Result | Observed evidence |
| --- | --- | --- |
| All seven steps × both locales | **PASS** | All 14 authorized clips resolved to the expected imported AudioClip, started on one speech source, advanced to the end, and cleared their clip reference. English and Mandarin ran as separate visits. Settings and tutorial Continue were activated through actual UI Toolkit keyboard events. |
| Output and endings | **PASS** | The 48 kHz Windows Realtek render loopback captured all 14 clips. Each capture aligned across the entire imported clip and ended in quiet. Last-quarter waveform correlations ranged from 0.799 to 0.902. These digital captures verify the route and tail delivery. The loopback driver reported two capture discontinuity warnings during the session; all 14 captures still aligned over the full clip length. |
| Navigation and cancellation | **PASS** | Advancing from active step 1 stopped the old source. Rapid advancement skipped stale cues and started the distinct step 5 Mandarin fist clip. No duplicate speech source appeared. Reopening Settings stopped the active clip. |
| Back and replay | **PASS** | The existing onboarding Back control was unavailable and Escape held the current step. Safe same-visit route revisits of consumed steps 4 and 1 did not replay. Ordinary route updates did not restart completed clips. |
| Locale and voice preference | **PASS** | Home defaulted to Off for each fresh visit. The English-to-Mandarin switch stopped the previous locale and started the current step in Mandarin. Enabling in tutorial Settings started only after the window closed. Disabling kept the next tutorial step silent, and visitor reset restored Off. |
| Safety, Console and shutdown | **PASS** | Scene was restored clean in Edit Mode with zero remaining voice sources. The camera rig stayed inactive, consent stayed denied, no microphone owner appeared, and the assistant endpoint and request state were isolated. The only captured Console events were the two expected Attract consent warnings. |

Unity 6000.4.7f1 spent about 116 seconds in the measured Play Mode sequence. With camera consent denied, a test fixture entered the real first tutorial route from Home. Normal tutorial buttons then drove steps 2–7. The final transition correctly remained gated, so the consented Privacy-to-Tutorial and Tutorial-to-Attract paths were outside this test. The alias was verified in the isolated route check. Live hand recognition, microphone dictation, live GPT, Windows Player validation and a fresh run of the six full performer journeys were **NOT RUN**. [Unity trace](Stage4_3D2_Evidence/Runtime.txt) · [Console trace](Stage4_3D2_Evidence/Console.txt) · [Output signal results](Stage4_3D2_Evidence/Signal.json) · [All 14 render-endpoint WAVs](<D:/AI-Voice-Auditions/Qwen3-TTS-Q8/Stage4_3D2_RuntimeEvidence/>)

## Regression verification

| Suite | Result |
| --- | --- |
| Unity compilation | **PASS**. Unity 6000.4.7f1 rebuilt the runtime and Editor assemblies and ran the isolated checks. |
| Assistant reply layouts | **PASS**, 72 layout cases plus output policy and cancellation checks. [Reply](Stage4_3D2_Evidence/Reply.txt) |
| Conversation | **PASS**, 164 cases across the existing three surfaces and isolated state fixtures. [Conversation](Stage4_3D2_Evidence/Conversation.txt) |
| Single-control microphone | **PASS**, 432 layouts plus isolated capture, draft, cancellation and keyboard state checks. [Microphone](Stage4_3D2_Evidence/Mic.txt) |
| Guide catalog, locale, replay and voice preference | **PASS**. All 26 source clips resolved in Unity, all seven tutorial cues had two valid assets, and guide priority and stale load policy checks passed. [Voice checks](Stage4_3D2_Evidence/Voice.txt) |
| Serialized scene and asset integrity | **PASS**. The active scene and build entry matched baseline hashes. The 12 introduced or replaced Ogg files matched their selected sources, and the remaining 14 matched the Stage 4.3D inventory. The 52 route pairs and all six performer media references passed the existing check. [References](Stage4_3D2_Evidence/References.txt) · [Asset hashes](Stage4_3D2_ApprovedSources.json) |

## Change scope and recovery

- `Assets/LivingGlass/Resources/LivingGlassGuide/ApprovedCatalog.json` adds the five newly approved cue pairs. The ten new Ogg files and metadata use the existing 24 kHz mono source, Vorbis quality 0.75, compressed-in-memory, preload and background import settings. The replacement step 4 content keeps the existing addresses and metadata GUIDs.
- `Assets/LivingGlass/Speech/VoiceExperienceChecks.cs` now checks that the release catalog exists and loads all 13 bilingual cue pairs. It retains explicit draft catalog rejection, route/locale parity, replay and cancellation coverage.
- This report, the activation inventory and the verification evidence describe the local change. The original approved step 4 Ogg files, associated metadata and pre-stage catalog were copied to [the recovery folder](<D:/AI-Voice-Auditions/Qwen3-TTS-Q8/Stage4_3D2_Recovery/>) and hash-verified before replacement. To revert step 4 alone, restore the two backed-up Ogg files at their original names and refresh Unity. To revert the complete activation, also restore the backed-up catalog, remove the ten new assets and their matching metadata using the activation inventory, and restore the prior expectations in the Editor check. Do not reset the working tree to the older STT checkpoint.
- The scene, build entry, voice services and existing local work remain intact. The local Git checkpoint stays at `9415962d10db3f18266fd53d83e05e6854f31713`. No Git push, camera activation, microphone recording, live GPT, or Windows Player build was performed.
