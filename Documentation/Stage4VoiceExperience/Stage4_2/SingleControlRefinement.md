# Stage 4.2C — Single-control microphone refinement

The shared Living Glass composer now starts and stops local dictation from the same microphone button. This applies to Reflect & Converse, assistant pages and the AI window, for all six performers and in English and Traditional Chinese. The Stage 4.2 worker, model and microphone device configuration are unchanged. The user’s successful real-microphone test applies to the earlier Stage 4.2 workflow; this refinement was checked in Edit Mode.

## Interaction and lifecycle

| State | What the visitor sees | What the control does |
| --- | --- | --- |
| Idle | Microphone and “Dictate” / “語音輸入” | Explicit activation attempts to start the one existing Unity microphone capture. The tooltip explains that recording and transcription happen on this computer. No separate menu opens. |
| Starting | Busy dots and “Starting…” / “啟動中……” | Waits for actual sample progress; repeated activation is ignored. A missing device or failed start returns to idle with a localized explanation. |
| Recording | Square Stop, performer accent edge, and “Listening…” / “聆聽中……” | Activating the same control ends capture and starts local transcription. A 250 ms startup guard ignores double activations while capture settles; the existing 20-second cap applies. |
| Processing | Busy dots and “Transcribing…” / “轉錄中……” | No new capture can start from this binding. The device has been released. |
| Transcript ready | Microphone returns to idle and the resulting text is editable | An empty composer receives the text. A nonempty draft or active IME composition requires the existing Keep/Replace decision. After resolution the composer regains focus. No message is sent. |
| Unavailable or error | Microphone with a localized recovery status, or the existing inert availability marker when the worker is not installed at binding time | Preserves typed text. A recoverable failure allows a new attempt. Unity device enumeration does not prove Windows granted access. |

The only voice actions removed are the secondary **Record**, **Stop**, **Cancel** and post-insertion **Discard** presentation controls. Stop now lives on the microphone; inserted text can be edited or cleared normally. Keep/Replace is temporary and appears only for a draft conflict. The separate assistant-request Cancel and the rest of the text assistant behavior stay as before.

Closing or rebinding the assistant, navigating, resetting the session, changing performers or locale, losing application focus, or explicitly submitting while voice work is pending uses existing cancellation and context guards. Teardown stops owned capture and clears the binding’s pending result. Escape still follows the existing overlay/back priority rather than introducing a global voice shortcut. One capture owner and the existing worker isolation remain in place.

## Layout and motion

The fixed 52 × 52 logical-pixel control retains one focus target through microphone, Stop and busy states. Recognizable vector glyphs crossfade within it over 160 ms using the existing Living Glass motion coordinator, retargeting from the current opacity after an interruption. Only the upper edge approaches the full performer accent during recording. Reduced motion applies the same state immediately; opaque material options are unchanged. No waveform or fake audio amplitude is shown. The lower-right Ask button and composer dimensions are unchanged. The compact caption uses a measured fixed slot so wrapped bilingual status text stays visible.

## Files changed

| File | Change |
| --- | --- |
| `Assets/LivingGlass/Speech/LivingGlassSpeechCapture.cs` | Single state-aware button, keyboard repeat gating and visual refresh. Retains worker, actual-capture readiness, draft confirmation and cancellation ownership. |
| `Assets/LivingGlass/UI/Components/LivingGlassMicrophoneIcon.cs` | Square and busy vector glyphs in the same footprint as the microphone. |
| `Assets/LivingGlass/UI/Styles/LivingGlass.uss` | Fixed button and status geometry, focus edge and draft action styling; removed submenu styling. |
| `Assets/LivingGlass/Presentation/LivingGlassPresentation.cs` | Exposes the existing motion owner internally and routes voice activation keys to the focused microphone button. |
| `Assets/LivingGlass/Integration/LivingGlassFlowIntegration.cs` | Supplies the participating view’s motion owner to the shared binding. |
| `Assets/LivingGlass/Editor/LivingGlassVoiceReview.cs` | Isolated listening and processing specimens use the new glyphs. No waveform, audio or network in the preview. |
| `Assets/LivingGlass/Editor/LivingGlassSingleControlChecks.cs` and `.meta` | Repeatable service-free Editor geometry and interaction checks. |
| `Documentation/Stage4VoiceExperience/Stage4_2/SingleControl*` | This report and the evidence listed below. |

The scene, build entry, worker and model, assistant transport, video and gesture/camera code were not changed by this pass. The prior Stage 4.1/4.2 local work and existing user screenshot deletions were retained.

## Validation and review

- **PASS — Unity 6000.4.7f1 compilation.** Runtime and Editor scripts compiled. Two known obsolete UI Toolkit API warnings remain in `AwakeningApplicationController.cs`. The project’s installed Unity Editor package/licensing messages are separate from the Living Glass assistant; the checks did not invoke the product’s assistant transport.
- **PASS — [Single-control checks](SingleControlEditorChecks.txt).** 432 real UI Toolkit layouts from the Edit Mode preview: six performers, two locales, three surfaces, Full HD and compact fallback, six voice states. Checks include Ask and editor position, caption fit, one visible microphone control, glyphs, UI Toolkit keyboard/focus, repeated activation, readiness, worker failure, draft protection, teardown and rejection of a stale result. Permission/startup failure was injected without accessing Windows devices.
- **PASS — [Offline worker checks](SingleControlWorkerChecks.txt).** The installed CUDA worker ran on the existing English and Mandarin WAV files. The suite checked Traditional Chinese output, silence, missing model installation, cancellation, timeout and successful reuse, plus draft/IME policy and PCM conversion. Existing source `EditorChecks.cs.txt` was reused in a temporary runner with the removed Discard-only assertion omitted.
- **PASS — [Existing conversation regression](SingleControlConversationChecks.txt) and [layout data](SingleControlConversationLayouts.csv).** 144 theme/locale/viewport layouts across three surfaces and 20 Editor-only conversation state specimens. Ask stayed in place with a long draft, hidden suggestions and a pending request.
- **PASS — [Static checks](SingleControlStaticChecks.json).** No protected tracked-file differences, one capture start site, no voice network or submission path added, and a unique Unity test metadata GUID.

Open **Awakening Celebrations → Living Glass → Voice composer design review** to inspect the labeled Editor specimens. Use **Run isolated single-control voice checks** in the same menu to rerun the 432-case suite. Both use a service-free Edit Mode window.

**NOT RUN:** Play Mode, real microphone activation, Windows privacy denial, device disconnect, native IME, physical input, live AI or Windows Player build. In a separately authorized retest, check an explicit Start/Stop with mouse and keyboard, unavailable/denied and unplugged devices, fast repeats, automatic cutoff, focus loss and navigation during recording/processing. Confirm Keep/Replace with a native Chinese IME, the 160 ms crossfade and reduced-motion change in the actual player UI. Leave camera capture and live requests off; verify the recognized text is editable and does not send on its own.
