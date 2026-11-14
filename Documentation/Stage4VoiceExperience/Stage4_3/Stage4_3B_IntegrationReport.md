# Stage 4.3B — Optional Qwen reply audio

Stage 4.2C was pushed and [independently verified](GitHubCheckpoint.md) before integration began. Stage 4.3B preserves text-first conversation: the visible assistant answer is added immediately; a small reply-level **Listen / 朗讀** action is offered when an opt-in local pack is present. The same action changes to **Stop / 停止** during preparation and playback. Media, localization and GPT transport remain owned by their existing systems. The integration is local and has not been pushed.

| Changed path | Responsibility |
| --- | --- |
| `Assets/LivingGlass/Speech/ReplySpeech.cs` and meta | Replaceable synthesis contract, localized input hints, bounded multilingual segmentation, PCM validation and shared local-inference lease. |
| `Assets/LivingGlass/Speech/LocalQwenReplySpeech.cs` and meta | Relative opt-in config, approved voice/ICL identity, private process transport, health and revision checks, startup and request limits, offline PCM and bounded teardown. |
| `Assets/LivingGlass/Speech/LivingGlassReplySpeech.cs` and meta | Single conversation output owner, transcript-level Listen/Stop, bilingual state labels and isolated audio source lifecycle. |
| `Assets/LivingGlass/Speech/ReplySpeechChecks.cs` and meta | Isolated UI/layout, WAV/segmentation and Windows worker checks; Editor only. |
| `Assets/LivingGlass/Speech/LivingGlassSpeechCapture.cs`, `LocalSpeechWorker.cs` | Narrow arbitration hook and shared engine lease. Capture, worker protocol, draft/IME and Ask semantics are unchanged. |
| `Assets/LivingGlass/Integration/LivingGlassFlowIntegration.cs` | Binds optional audio to the current chat and disposes it with the existing bindings. |
| `Assets/Scripts/AwakeningApplicationController.cs` | Appends the reply action after text; stops audio on new question or existing cancellation. Assistant HTTP request and parser unchanged. |
| `Assets/LivingGlass/UI/Styles/LivingGlass.uss` | Compact accessible inline row, consistent target, performer accent while active, no added animation. |
| `.gitignore`, Stage 4.3 documentation/evidence and example config | Protects `QwenLocal` and records the deployment, checkpoint and verification. |

Voice identity is the approved English reference’s existing speaker embedding + RVQ audio context + reference transcript; no new extraction, synthetic replacement or default x-vector fallback. UI language controls labels; text-script detection selects the native English, Mandarin or mixed hint. Neither derived audio nor visitor requests are persisted. Responses up to 2,400 UTF-16 units chunk at normal dialogue boundaries where possible; requests above the cap or time/audio limits leave the full text displayed. Long reply prosody and actual cross-language listening in Unity are pending.

Playback runs after awaited inference and off-thread PCM decoding. Unity’s AudioClip creation and bounded PCM upload occur on the Unity thread, then AudioSource plays asynchronously. There is one active reply; Stop clears generation/playback and ignores late completion. Detach, new question, active-capture priority, and invalid context stop it immediately. Startup/model/worker/transport/timeout failures display a short localized status without altering the answer. No autoplay, no capture permission changes, no changes to protected routes, no narrator library, no new Unity packages. See the [runtime plan](Stage4_3B_RuntimePlan.md).

The [validation report](Stage4_3B_Validation.md) separates actual local worker evidence from UI fixtures. The Stage 4.3A standalone timings and memory numbers are only baselines; live playback, combined hardware/load, native input and production readiness were not established.
