# Stage 4.2 local Whisper speech recognition

**Gate A: GO for Windows x64 CUDA. Gate B: integrated and validated offline in Edit Mode.** The RTX 4060 Laptop GPU processed 22 short English and Mandarin trials with a 903 ms median and 1,013 ms p95, including a fresh process per request. CPU trials took about 14–15 seconds. The local deployment uses the verified Large-v3 Turbo Q5_0 and Silero VAD weights, and includes just the CUDA CLI and its required dependencies. [Benchmark, corpus and measured limits](Benchmark.md) · [Installation and licenses](Deployment.md)

## Visitor interaction

On the installed Windows configuration, the existing composer microphone offers **Record** with a localized “Local only” disclosure. Opening a conversation or clicking the microphone does not start capture. Recording begins only after **Record**, requires an active conversation, and calls Unity's microphone API once through the single capture owner. The microphone status moves through Starting, Listening and Processing based on actual capture and worker state. Stop releases the device before recognition, and Cancel discards capture or invalidates an outstanding recognition. The visitor can continue typing if the device or worker is unavailable.

After successful recognition, an empty editor gets an editable draft. An existing draft or IME composition requires confirmation. The visitor can Keep their existing text or Replace it when composition ends. Discard restores the preceding draft while the inserted transcription is still unedited. Manual edits remove that discard action. The existing Ask button is always the explicit submission step. A typed Ask or suggestion cancels an outstanding voice operation before using the existing submission binding.

Voice controls use the left rail in the Stage 4.1 composer. The selected performer accent appears on the listening edge. A visible status and steady tint distinguish listening. The same static state changes work under reduced motion. The compact controls retain 48 × 44 logical targets for Record, Stop, Cancel, Keep and Replace, with localized tooltips and labels. Full desktop accessibility and native IME behavior are pending hardware/runtime verification.

## Ownership, privacy and release

The current chat binding owns a separate `LivingGlassSpeechCapture`. Its authority checks view attachment, current assistant session, performer, input eligibility and app focus. Existing binding teardown also disposes it on route or locale replacement, close, controller disable and destruction. Opening a substitute overlay cancels current capture before changing context. Late or cancelled tasks have no retained path to the next composer. The reader polls the captured position, stops at 20 seconds and treats stalled capture or an interrupted device as an error. Device names are checked only during the explicit recording action.

`LocalSpeechWorker` serializes inference requests, converts bounded PCM off the main thread and owns a fresh Windows process inside a kill-on-close job. The worker receives WAV bytes through stdin and returns structured JSON. The pinned Windows CLI routes its `-of -` path through `CON`; the hidden process test revealed that no usable result reached stdout on that path. The adapter therefore uses a private temporary JSON file, deletes it with its lease, and reaps abandoned result folders when the next request starts. Audio stays in memory, and neither visitor audio nor text is sent to a speech service or written to logs. On successful Mandarin capture, the adapter runs the pinned Traditional Chinese dictionary pass on the new draft. It leaves Latin, punctuation, whitespace and manually entered text alone. A phonetic recognition error is still visible in the editable draft.

The Windows Unity API exposes observable device and capture behavior. It cannot reliably identify every privacy denial separately from a device startup failure. Missing devices show “No mic found” and failed starts show “Check mic access”. The UI does not infer OS permission from device enumeration. Test coverage for actual Windows consent, no-device behavior and disconnect is pending. [Unity microphone API](https://docs.unity3d.com/ja/6000.0/ScriptReference/Microphone.html) · [Windows desktop capability policies](https://learn.microsoft.com/en-us/windows/client-management/manage-desktop-app-privacy-access)

## Changed files

| Path | Purpose |
| --- | --- |
| `Assets/LivingGlass/Speech/LocalSpeechWorker.cs` | PCM/WAV conversion, bounded CUDA worker, cancellation, timeout, result parsing and cleanup |
| `Assets/LivingGlass/Speech/LivingGlassSpeechCapture.cs` | Single capture owner, UI state and draft lifecycle |
| `Assets/LivingGlass/Speech/TraditionalSpeechText.cs` | Han-only longest-match display conversion |
| `Assets/LivingGlass/Speech.meta` and three script metadata files | Unity asset identity |
| `Assets/LivingGlass/Integration/LivingGlassFlowIntegration.cs` | Speech binding and teardown at the existing chat session boundary |
| `Assets/LivingGlass/UI/Styles/LivingGlass.uss` | Speech rail and control layout |
| `.gitignore` | Local runtime, weights and staging paths |
| `Documentation/Stage4VoiceExperience/Stage4_2/` | Report, benchmark, deployment, installer, corpus measurements and isolated checks |

The Stage 4.1 composer and review changes and nine screenshot deletions predated this phase. This phase keeps them in the working tree. The existing controller, camera/gesture stack, assistant request implementation, stage media, scene, route catalog, theme tokens and package settings are outside the voice changes. No Player build was generated.

## Validation and remaining acceptance

| Check | Result |
| --- | --- |
| Standalone CUDA and CPU evaluation, artifact hashes and first-pass deployment | PASS. 34 process trials and verified 26-file installation. |
| Installed C# adapter in Unity Edit Mode with corpus WAV | PASS. English and Mandarin, Traditional conversion, silence, PCM stereo conversion, missing install, cancellation, timeout and successful subsequent request. [Worker results](EditorWorkerChecks.txt) |
| Draft and IME guard policy | PASS in isolated checks. Tested replace protection and preservation of edited text. Native IME interaction is pending. |
| Voice control geometry | PASS. 288 static specimen layouts covering six themes, two locales, two surfaces, two viewports and six voice states. [Layout results](EditorVoiceLayout.txt) |
| Existing conversation regressions | PASS. 164 isolated layout/state cases. The existing Stage 4.1 checker refreshed its output, with a copy included here. [Regression results](ConversationRegression.txt) |
| Binding lifecycle | PASS. Explicit disposal is idempotent, restores the inert marker, and detaching the view disposes its binding. No worker process or temporary result directory remained after the offline checks. [Teardown evidence](EditorBindingLifecycle.txt) |
| Localized text and asset scope | PASS. 64 CJK glyphs in the speech source are covered by the bundled font. Four new metadata GUIDs are unique. [Static checks](StaticChecks.json) |
| Compilation, scene links and pure route/motion contracts | PASS. [Serialized validation](SerializedValidation.txt) · [Pure contracts](PureContractChecks.txt) |
| Real microphone, Windows denial, default device/disconnect, Play Mode, physical input, forced worker crash and live AI | NOT RUN. |
| Exhibition concurrency, room acoustics, Taiwan Mandarin accuracy and Windows Player packaging | NOT RUN. |

For the next separately authorized operational test, use the primary scene and an isolated assistant transport. Start with camera denied. Confirm that entering all three chat surfaces creates no capture. Use Record with the pinned physical microphone, test denial/disconnect, manual Stop, 20-second cutoff, cancel during recognition, focus loss, overlay close, locale and performer changes, Reuse after cancel, and an IME draft followed by Keep or Replace. Include silence, natural Mandarin and bilingual speaker samples, and capture with the live room's background audio. Sample Unity frame time and GPU use while relevant exhibition workloads run. Confirm no process, audio device or temporary result remains after the session and that Ask remains the only explicit submission path.
