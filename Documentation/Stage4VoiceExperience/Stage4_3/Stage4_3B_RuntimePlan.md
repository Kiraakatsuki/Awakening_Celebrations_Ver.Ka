# Stage 4.3B — Runtime and release plan

The Stage 4.2C checkpoint is [verified on GitHub](GitHubCheckpoint.md). The Stage 4.3B candidate is local. Operator configuration is optional; the experience remains text-only when it is missing, disabled or incomplete. ICL-conditioned Awakening Guide is the selected voice for all six performers and both locales. Native Stage 4.3A timings are historical, not promises about the exhibition.

## Installation and protocol

Keep the Stage 4.3A audition and `VoiceLocal` untouched. Use a sibling `QwenLocal` directory beside the Unity project’s `Assets` directory, or beside the future Windows Player executable. From the audited Stage 4.3A pack, preserve this layout under `QwenLocal/RuntimePack`:

- `Runtime/PortablePython/` (embedded Python and pinned native/NumPy packages), `Runtime/Cuda/` (CUDA redistributable DLLs).
- `Scripts/worker.py` and `Scripts/engine.py` (unchanged, trusted Stage 4.3A scripts).
- `Models/qwen-talker-0.6b-base-Q8_0.gguf` and `Models/qwen-tokenizer-12hz-Q8_0.gguf`.
- `Conditioning/awakening-guide.spk`, `Conditioning/awakening-guide.rvq` and `Reference/transcript.en.txt` from the approved reference extraction. Preserve original bytes for the three pinned hashes. No reference WAV is needed when synthesizing from this cache.
- Preserve the associated notices separately or in the distributed runtime pack. Verify downloaded model/runtime hashes with the Stage 4.3A artifact lock; the Unity startup path does not rehash gigabytes of model data.

Copy [the example](QwenLocal.config.example.json) to `QwenLocal/config.json`. Enable it only for a trusted operator installation. Unity resolves the configured relative directory below `QwenLocal`. The developer pack on this laptop used same-volume hard links for the immutable runtime, weights and cache, plus separate copies of worker scripts and the transcript. No binaries, private voice files or config are in Git. Copy files rather than editing a hard-linked runtime in place when experimenting. Set `enabled` to `false` and reopen the assistant (or relaunch) to disable. Keep the deployment directory writable only by trusted operators: it contains executable code.

The adapter sends newline-delimited JSON to a hidden child process using a fixed executable and command. No shell, HTTP listener or network inference. It verifies the ready event and a health response with voice/native revision, then issues fixed `synthesize` requests (ICL) and a graceful `shutdown`. Cancellation ends the one-shot request/worker; the worker shutdown command signals its active native job. A Windows kill-on-close job provides the hard stop on slow exit or forced parent termination. The adapter reads bounded, validated 24 kHz mono PCM16 WAV responses; child output is encoded and decoded only in memory and no visitor text or generated clips are saved. Subsequent explicit Listen starts a fresh child after crash/error.

## Warm-up and contention

The initial policy uses one worker per explicit Listen and reuses it across chunks of the same reply, then exits. It never preloads on navigation, while GPT prepares, or during protected video. This trades a per-request start for negligible idle retention and avoids guessing available GPU headroom. Timed standalone Stage 4.3A peaks were about 2.42 GiB additional VRAM, with one 27 s first-generation outlier. The Stage 4.3B actual isolated tests measure the whole adapter path only; concurrent Unity/Whisper/GPU contention is unverified.

One external inference lease coordinates existing Whisper and Qwen; optional Qwen returns busy without waiting. The mic cancels reply audio before capture; Whisper may briefly wait for Qwen teardown on its existing async wait. Qwen cannot begin while mic capture or dictation processing owns the conversation, while the existing environmental source is audible, during a new assistant request, or in protected states. Layout and transitions are not blocked. No jobs prewarm by default. There is no automatic CPU fallback policy from this integration; the tested pack runs CUDA, and a failed local synthesis falls back to text.

Compare these policies in a separately authorized pass: current on-demand; prewarm explicitly on a stable conversation entry only; and a bounded residency window with unload before mic ownership/route exit. Capture cold process startup, first full-WAV handoff vs. warm synthesis, idle VRAM/RAM, shutdown, uncontended and combined frame-time percentiles. Include the occasional slow first synthesis. Decide whether to expose native streaming only after measuring whether completed WAVs and optional prewarming meet the experience target. The current private protocol does not stream audio for playback.

## Audio priorities and future fixed cues

Protected video and existing environmental playback take priority; capture takes priority over reply audio. Dynamic reply audio exists only on explicit Listen. Stop, navigation, overlay closure, locale/performer/session change, new question and app disable all invalidate it. Sound design and videos retain their owners. Future guide clips should be pregenerated, indexed by locale, stable route cue ID and voice revision (for example `Guide/en/home-welcome.wav`, `Guide/zh/home-welcome.wav`), with authored captions, cache/provenance information and a separate static provider. If later enabled, replay and mute should be explicit; visitor-requested speech and capture take priority over guide queues. No fixed narration library is implemented here.

## Next authorized pass

1. In an offline Play Mode fixture with camera denied and assistant transport unavailable, inject known reply fixtures. Verify text is visible first, Listen/Stop via mouse and keyboard, stable Ask/composer, fast repeat, old/new reply switching, read-aloud completion, locale/performer switch, navigation/overlay close/reset, loss of focus and disable. Confirm no autoplay.
2. Exercise missing files, forced worker death, timeout, GPU pressure and recovery. Measure main-thread PCM handoff/clip upload, frame pacing, peak and idle VRAM/RAM. Verify protected video and environmental audio retain priority. Inspect process/job cleanup and play/stop in the actual Windows audio system.
3. Separately authorize any microphone and camera tests. Test dictation while TTS is generating or playing, plus co-residency with MediaPipe and video. Run a live GPT text-to-audio pass only with separate authorization. Only then choose a release readiness and warm-up policy.
