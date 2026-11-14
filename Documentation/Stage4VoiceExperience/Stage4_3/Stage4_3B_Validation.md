# Stage 4.3B — Validation

Scope: Unity 6000.4.7f1 Edit Mode with service-free UI fixtures, static project checks, and bounded file-free local Qwen subprocess tests with fixed text. No scene was entered in Play Mode. No microphone, camera, audio device playback, live GPT, deployed backend change, Player build, or combined hardware profiling was performed.

| Check | Result |
| --- | --- |
| Remote recovery gate | PASS. [Checkpoint](GitHubCheckpoint.md) independently verified before any Unity source changed. |
| Unity compilation | PASS. Runtime and Editor assemblies compiled. Existing CS0618 warnings in the existing controller at lines 946 and 1171; no new C# errors in final compile. Unity Editor package/licensing messages are separate from the application’s GPT path. |
| Existing scene/build/config/locale mapping | PASS. [Configuration check](Stage4_3B_Configuration.txt): scene script GUIDs, sole development entry, all-six media/art, panel and 52 bilingual routes. |
| Bilingual glyph coverage | PASS. All 30 unique Traditional Chinese characters in the new labels/tooltips are covered by the bundled project CJK font. |
| New output layout and lifecycle | PASS. [Editor checks](Stage4_3B_EditorChecks.txt): 72 actual UI Toolkit layouts including all six themes, both locales, three surfaces and two viewports; Ask stayed anchored. No autoplay, keyboard activation, Stop, completion, timeout/failure status, stale result and detachment were checked with fixture audio. |
| Existing conversation suite | PASS. [164 unchanged cases](Stage4_3B_ConversationChecks.txt), with [geometry CSV](Stage4_3B_ConversationLayouts.csv). |
| Existing microphone Start/Stop suite | PASS. [Unchanged 432-case suite](Stage4_3B_SingleControlChecks.txt) plus existing focused state and keyboard assertions; hardware not invoked. |
| Real C# → native worker → PCM | PASS. [Worker evidence](Stage4_3B_WorkerChecks.json): missing install fallback, no Qwen queue behind an occupied inference lease, cancellation/teardown, two controlled synthesized ICL fixtures. English 5.605 s end to end with 2.64 s of audio, Mandarin 2.967 s end to end with 2.80 s of audio. Includes process launch, ready/health, synthesis, decode and shutdown; no audible playback or combined frame-time measurement. |
| Failure paths | Local missing install, busy rejection, cancellation and strict WAV rejection were exercised. Timeout uses the isolated reply fixture and the worker’s existing Stage 4.3A private-pipe smoke evidence. Force-kill, malformed live worker response, and GPU resource exhaustion with the app running: NOT RUN. |
| Protected scope | PASS. Scene, packages, ProjectSettings, backend source and the existing Whisper installation were not edited. Neither the Qwen pack nor its reference assets were staged or pushed. No matching test Qwen worker remained after the isolated invocation. |
| Actual runtime audio, combined workload, concurrent hardware | NOT RUN. See the [bounded runtime plan](Stage4_3B_RuntimePlan.md). |

The approved voice is configured as ICL. The local package’s first request may still exceed the desired latency; it does not delay the existing text path. Production readiness remains pending authorized runtime validation.
