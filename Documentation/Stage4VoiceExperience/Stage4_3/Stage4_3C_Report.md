# Stage 4.3C — Living Voice Experience

This phase adds a session-only **Voice assistance / 語音導覽** preference in Home and Settings, cached guide playback infrastructure, and better completion handling for dynamic speech. The approved ICL voice, existing Stage 4.3B private worker, Whisper dictation and GPT transport are preserved. No Git push was made. The Stage 4.2C remote recovery point remains `9415962`.

## Abrupt endings

Two paths could produce a short or abrupt ending. The previous adapter accepted any valid PCM WAV, even if Qwen exhausted its 450-codec-frame budget instead of reaching EOS. Offline, forcing an 8-frame cap reproduced incomplete speech with a nonzero end; the same phrase reached natural EOS at 56 frames with the full words. The exact visitor reply was not captured or replayed, so this is a proven truncation mechanism rather than a claim that every reported ending had this cause. The old audio owner also used one observation of `AudioSource.isPlaying` to infer completion, with no scheduled start or output drain; this was a code-level risk, not directly reproduced on speakers.

A result that reaches the pinned worker's frame cap is retried in shorter text chunks without exposing partial audio; if it still cannot complete within the bounds, the full rendered reply remains and the row shows a localized audio error. Normal PCM is preserved verbatim through its final source sample. An 8 ms bridge *after* that sample reaches zero, followed by 120 ms between chunks and 240 ms at reply end. Unity schedules the clip and retains ownership through its sample-count duration and an output-buffer margin. An explicit Stop cancels generation immediately; when already audible it requests a short ~35 ms volume ramp. Navigation, disable, dictation and protected/exhibition audio stop immediately to avoid audible overlap. No claim of sample-accurate fade under every Unity frame rate.

## Visitor policy and audio ownership

Home starts Off. Ordinary navigation and locale changes preserve the preference within a visit; returning to the initial Home boundary and disable clear it, with no PlayerPrefs persistence. The preference never changes camera consent, Begin eligibility or mic availability. On enables cached guide only where approved audio exists. Off interrupts current speech and never starts anything automatically. With the existing operator Qwen opt-in, a later explicit **Listen / 朗讀** remains available even with guidance Off. Text and microphone operation are independent.

One coordinator owns one runtime playback source for guide clips and dynamic replies. Existing environmental sound and VideoPlayer remain separate, with priority; no ducking or volume changes to those systems. Opening a modal, switching performer/locale, navigating, closing the chat, submitting a new question, beginning dictation and resetting the visit prevent stale playback. Guide loads are tied to the active context and are not queued to replay later. See the [narration manifest](Stage4_3C_NarrationManifest.md) and [runtime plan](Stage4_3C_RuntimePlan.md).

Eight bilingual cues were generated with the approved ICL identity. The 16 unaltered PCM masters total 5,273,024 bytes; padded PCM review files total 5,463,488 bytes; the 16 verified Vorbis candidates total 819,238 bytes (~0.78 MiB). Runtime Unity import/build footprint remains unmeasured. The words and performances still need listening approval. No draft audio or approved catalog was installed in the Unity application, so turning guidance On today leaves guide audio silent until approved assets are added.

## Files changed in Stage 4.3C

| File | Change |
| --- | --- |
| `Assets/LivingGlass/Speech/LivingGlassVoiceCoordinator.cs` and meta | One playback owner, route/locale cue policy, approval-gated cache and session preference. |
| `Assets/LivingGlass/Speech/LivingGlassReplySpeech.cs` | Shared audio playback with DSP completion, cancel fade and guide handoff. |
| `Assets/LivingGlass/Speech/LocalQwenReplySpeech.cs`, `ReplySpeech.cs` | Capped-segment retry, natural PCM tail, localized incomplete-audio result. |
| `Assets/LivingGlass/Integration/LivingGlassFlowIntegration.cs` | Preference and cancellation hooked into existing route, modal and chat boundaries. |
| `Assets/Scripts/AwakeningApplicationController.cs` | Tick/dispose existing voice coordinator; preempt speech for environmental sound and newly submitted chat. |
| `Assets/LivingGlass/Presentation/LivingGlassPresentation.cs`, `Assets/LivingGlass/UI/Styles/LivingGlass.uss` | Bilingual Home/Settings preference and compact layout styling. |
| `Assets/LivingGlass/Speech/VoiceExperienceChecks.cs` and meta | Edit Mode layout and voice policy/PCM fixtures. |
| `Documentation/Stage4VoiceExperience/Stage4_3/Stage4_3C_*` | Reports, preview scripts/manifest data and validation evidence. |
| External Qwen audition tree | Standalone generation, compression, transcription and diagnostic utilities plus drafts and evidence. Existing renderer, pinned model/voice and production worker scripts were not replaced. |

[Validation and remaining tests](Stage4_3C_AudioValidation.md). Audible Unity playback, combined workload and release readiness are not verified.
