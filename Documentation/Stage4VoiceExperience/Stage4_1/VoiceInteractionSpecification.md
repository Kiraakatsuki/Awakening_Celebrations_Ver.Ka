# Living Glass voice interaction specification

## Shared composer

One composer serves Reflect & Converse, the assistant page and the global assistant window. The left rail holds a 52 × 52 microphone silhouette and a localized availability label. The native multiline question field fills the center. Ask stays in the bottom-right of the frame, with 12 px horizontal and 10 px vertical padding inside its 1 px edge. The 142 × 50 Ask target shares the question editor’s lower edge. The 48 px Cancel request target uses reserved space above Ask and appears while the existing request owner enables it. The transcript remains a separate native scroll view. Long drafts scroll within the editor, and the composer uses 144 px of height, or 128 px below 900 px viewport height.

Keep the existing visible question label, native selection and IME behavior, 3,600-character limit and explicit Ask submission. Tab reaches the transcript, question and Ask, then Cancel request when enabled. The microphone marker is informational and has no tab stop. Future actionable voice controls need localized accessible names and a visible neutral focus ring. A physical microphone exception or failed request must leave the typed draft editable.

| Performer | Accent token |
| --- | --- |
| GAIA | Cyan / turquoise `#5BD8ED` |
| PANDEMIC | Sand / bronze `#C0AA91` |
| RE-BIRTH | Rose / magenta `#E0B0C8` |
| WHITE-GUARDIAN | Ice / silver `#C5E9FA` |
| CONSCIENCE | Ember orange `#FF956A` |
| SANCTUARY | Amber / gold `#F1C57A` |

Use the accent on the top edge or a small listening indicator. Keep text warm white, supporting copy neutral and focus visible across palettes. The surface uses a dark reading tint with the existing opaque alternative. The white microphone stroke uses the same Painter2D drawing approach as the gesture illustrations.

## Microphone state and copy

The first row is the visitor-facing Stage 4.1 state. The remaining rows are future presentation contracts. The isolated review shows selected states and labels the entire specimen as an Editor design preview.

| State | English | Traditional Chinese | Presentation and recovery |
| --- | --- | --- | --- |
| Voice unavailable | Voice unavailable | 語音尚未提供 | Quiet silhouette and visible caption alongside the question editor. |
| Permission not granted | Allow microphone access to dictate a question. | 若要口述問題，請允許使用麥克風。 | Show an explicit Allow microphone action after the visitor chooses dictation. Explain processing first. Offer Type instead. |
| Permission denied | Microphone access is off. You can still type. | 麥克風存取已關閉，您仍可輸入文字。 | Keep the editor in place. Offer a separate settings link only where the platform supports it. |
| Preparing to listen | Preparing microphone… | 正在準備麥克風…… | Warm the edge slowly. Keep Cancel visible. Wait for genuine capture readiness. |
| Listening | Listening | 聆聽中 | Use a small accent edge and visible Stop / Cancel affordances. |
| Speech detected | Speech detected | 已偵測到語音 | Raise the meter within its reserved region using approved audio-level or voice-activity data. Keep the listening text visible. |
| Recognition processing | Processing speech… | 正在處理語音…… | Hold the accent without a live meter. Cancel remains available during recognition. |
| Editable transcription ready | Check your draft, then choose Ask. | 請確認草稿，再選擇「詢問」。 | Restore focus to the question editor. Insert the transcription as an editable draft. Preserve existing text and composition. |
| No speech detected | No speech detected. Try again or type your question. | 未偵測到語音。請再試一次，或輸入問題。 | Return the surface to its resting pose. Keep any existing draft. |
| Recognition failure | Speech could not be recognized. Try again or type. | 無法辨識語音。請再試一次，或輸入文字。 | Explain the failure beside the microphone and keep Ask available for a valid typed draft. |
| Network unavailable | Voice service is offline. You can still type. | 語音服務目前離線，您仍可輸入文字。 | Use only for an applicable provider with a confirmed connection failure. Preserve text controls. |
| Recording cancelled | Listening cancelled | 已取消聆聽 | Return the surface to rest. Keep focus near the initiating control and retain the prior draft. |

Stop listening commits the captured segment to recognition. Cancel discards the segment and invalidates any pending result. Permission, capture and provider events drive the status. A completed visual transition cannot trigger those events or submit a question.

## Listening motion

Reserve the microphone region and draw illumination inside it. Keep the question, Ask and transcript fixed. Use the existing monotone quartic easing in `LivingGlassMotion` for future state transitions. Prepare the accent over 180 ms. When listening begins, let the inner visual settle to a maximum 1.025× scale over 240 ms. Keep the apparent footprint fixed. A small 15 px waveform region stays within the rail. Use a limited opacity range of 0.86–1.00 over a proposed 2.8-second breathing cycle while the genuine session is listening. The waveform follows real level or voice-activity data with clamped rise and decay. On processing, hide level feedback over 180 ms and retain a steady status. Return to idle over 280 ms. The current Editor fixture depicts a static five-bar silhouette.

Use one presentation track per voice surface. Stop, Cancel, denial, navigation, overlay close and locale replacement cancel the old visual track and invalidate its owner. Latest state takes priority, and transitions resume from the current pose. Reduced motion uses steady borders, text and a fixed level glyph with immediate state changes. Use the opaque surface under reduced effects. At every state the visible status explains the change without relying on motion or accent color.

## Spoken responses

Attach a compact speaker action to an eligible assistant reply within the transcript. Keep the action close to the reply. A future playback coordinator accepts a stable reply and session identity. It owns one audible spoken item and releases it on navigation, a new question, Stop, session reset or view replacement. A stale prepare or playback callback carries the old session identity and is discarded.

| State | English | Traditional Chinese | Visual behavior |
| --- | --- | --- | --- |
| Spoken response available | Read this reply aloud | 朗讀這則回覆 | Small speaker action under the reply. |
| Loading | Preparing audio… | 正在準備音訊…… | Text status in the same row and a Cancel action. |
| Speech playback active | Pause / Stop | 暫停／停止 | Use genuine playback state to show the pause and stop controls. |
| Paused | Resume / Stop | 繼續播放／停止 | Retain the current reply reference while paused. |
| Stopped or completed | Play again | 再次播放 | Settle the reply action to idle. |
| Playback unavailable | Audio unavailable | 音訊尚未提供 | Quiet text at the reply. |
| Error | Could not play audio. Try again. | 無法播放音訊，請再試一次。 | Keep the text response readable and offer retry. |
| Narration unavailable | Voice guide unavailable | 語音導覽尚未提供 | Keep authored text instructions visible. |

Stage 4.1 shows a reply and speaker-action placement specimen in the Editor. No response text, audio state or new playback control is injected into visitor conversations.

## Contextual voice guide

Use brief authored lines in the current Help or instruction surface. Offer Replay and Mute next to those instructions once guide playback exists. Route entry can offer guidance once per eligible visit. Repeat is explicit, and route replacement cancels the prior line. Keep subtitles or equivalent on-screen instructions throughout.

| Visitor stage | Placement and delivery |
| --- | --- |
| Home | One warm, confident welcome by the existing invitation. An upbeat delivery matches the first arrival. |
| Privacy | One calm statement alongside consent and text alternatives. Keep camera and microphone explanations independent. |
| Tutorial | Short purposeful instructions at each existing illustrated step. Replay the current step. |
| Attract | A brief uplifting awakening cue beside the existing next action. Yield to the environmental sound where they overlap. |
| Gallery | Explain targeting and confirmation once, quietly. Keep guide clips out of rapid performer target changes. |
| Performer experience | A brief Focus cue can precede Activation. Leave Activation and Living Portrait free of guide panels and guide audio. |
| Reflection | One composed optional prompt in contextual Help. Yield to dictation and spoken replies. |
| Closing | One warm farewell by the closing message. Preserve the existing progression. |

## Future ownership and integration

Stage 4.2 should define microphone permission separately from `GestureCameraConsentGate`. A single recording owner starts after an explicit visitor command and verified microphone permission, and releases the device and buffers on completion, cancellation or loss of ownership. The recognizer consumes the approved local stream or an explicitly disclosed remote stream. Select the provider, locale support, disclosure, data retention and transport policy before collecting audio. A transcription coordinator delivers draft text only to the matching current session and field. Defer delivering the draft while an IME composition is active. Confirm all cancellation and error paths, including permission denial, device removal, unavailable service and late results. The existing text request remains inside the current assistant session and uses the existing Ask action.

Stage 4.3 should assign audio output and narrator coordination to one speech playback owner with session and route scope. Decide and disclose local synthesis versus remote processing of reply text. Agree how voice guide and spoken replies arbitrate with environmental audio, clap playback and protected video. A visitor read-aloud request should interrupt guidance, and dictation should take priority over clips that could feed back into capture. Verify speech state against actual provider readiness, playback and time-based progress. Locale copy, subtitles, pronunciation, Traditional Chinese voice selection, replay and mute need product review. Existing environmental audio stays with the application controller, and video stays with its current VideoPlayer lifecycle.
