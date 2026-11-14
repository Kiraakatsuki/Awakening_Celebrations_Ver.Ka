# Stage 4.3C — Audio validation

Scope: Unity Edit Mode, non-audible fixtures and bounded standalone production-model inference using authored text. No visitor replies were captured. Original 4.2C and 4.3B reports and regression sources were preserved.

| Check | Result and evidence |
| --- | --- |
| Unity compiler | PASS, Unity 6000.4.7f1. Historic obsolete background-scale warnings remain, unrelated to speech. |
| Existing reply layouts and lifecycle | PASS: [72 cases](Stage4_3C_ReplyChecks.txt), plus fake-audio cancellation, late-response rejection, failure, replay to idle and focus/keyboard interaction. |
| Existing conversation | PASS: [164 cases](Stage4_3C_ConversationChecks.txt), [layout geometry](Stage4_3C_ConversationLayouts.csv). |
| Existing microphone | PASS: [432 layout cases](Stage4_3C_SingleControlChecks.txt) and state/keyboard fixtures. The checked microphone test does not open a device. |
| Home and Settings | PASS: [new Edit Mode rows](Stage4_3C_EditorChecks.txt), two languages, two viewports and normal vs reduced-motion/opaque profiles. Begin does not move when preference changes. The compact layout was corrected after a dock clearance check. |
| Completion / intentionally clipped audio | PASS: known-duration sample clock and drain calculation, untouched last source sample and post-speech de-click bridge, 240 ms natural tail, distinct ~35 ms cancellation gain. Native bounded experiment compared eight forced frames (0.64 s, last 10 ms peak ~0.048 FS, truncated) to natural EOS at 56 frames (4.48 s, last peak ~0.000084 FS, complete words in offline transcription). [Raw diagnostic](<D:/AI-Voice-Auditions/Qwen3-TTS-Q8/Logs/stage43c-endings.log>) · [Offline text screen](<D:/AI-Voice-Auditions/Qwen3-TTS-Q8/Stage4_3C_Draft/ASR/Screening.json>). The C# cap retry is exercised by the cap decision fixture; no long real reply reaching 450 codec frames was needed or captured. |
| Guide navigation/replay/disable/reset/missing | PASS: [pure policy fixtures](Stage4_3C_EditorChecks.txt) for missing and draft catalog, one cue/locale per visitor, rapid invalidation, disable, reset, protected route exclusion and all-six performer mapping. Actual AudioSource events were not invoked. |
| Bilingual draft media | PASS for 16 files in each of master, PCM review and Vorbis review sets, matching manifest, hashes, verified mono 24 kHz and frame counts. Compressed tails decode to digital silence in final 100 ms. No release approval. Mandarin ASR warned about several phrases; listening remains pending. |
| Existing build configuration and protected files | PASS: [configuration](Stage4_3C_Configuration.txt). Scenes, Unity packages, build settings, deployed backend source and project media assets unchanged. No draft Resources catalog found. |
| Actual local Qwen adapter | PASS: [C# adapter → local ICL worker → padded PCM](Stage4_3C_WorkerChecks.json). English 4.960 s end-to-end (2.888 s of audio), Mandarin 3.629 s end-to-end (3.048 s of audio); missing installation, existing inference-slot contention and cancellation passed. Includes process/health, inference and shutdown, not audible playback. |
| CJK font and asset identities | PASS: all 352 distinct CJK characters in the inspected presentation and speech labels are present in the configured Traditional Chinese font; both new Unity script GUIDs are unique. |
| Audible Unity playback, microphone, camera, live GPT, Player build, combined GPU/frame time | NOT RUN. [Runtime plan](Stage4_3C_RuntimePlan.md). |

Attribution limit: the standalone forced-cap trial reproduces a plausible synthesis cause. No capture of the visitor's original failing GPT reply exists. The Unity startup/end-buffer handling was improved based on lifecycle inspection and bounded fixtures, and still needs actual device testing.
