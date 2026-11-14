# Stage 4.3A — Standalone Windows deployment and protocol

The audition lives at `D:/AI-Voice-Auditions/Qwen3-TTS-Q8/`. It is external to Unity, including the embedded runtime, binary libraries, models, voice and samples. The original Whisper deployment has not been reconfigured. In this experiment Qwen and Whisper were run sequentially.

| Unit | Responsibility and lifetime |
| --- | --- |
| Identity | `Conditioning/awakening-guide.spk` holds 1,024 raw float32 speaker values (4,096 bytes). `awakening-guide.rvq` holds 16 reference codebooks × 343 time frames (7,546 bytes in the wrapper’s packed format). For ICL also carry `Reference/transcript.en.txt`; strictly match it to the source clip. These are conditioning artifacts and require both pinned models. |
| Inference | `Runtime/PortablePython/python.exe`, `Scripts/worker.py` and `Scripts/engine.py`; bundled native x64 CUDA DLLs via ctypes, including `qwen.dll`, `ggml.dll`, `ggml-base.dll`, `ggml-cpu.dll`, `ggml-cuda.dll`, and app-local MSVC libs. Runtime loads two local GGUFs, one engine, at most one request. |
| CUDA | `Runtime/Cuda/`: `cudart64_12.dll` from 12.8.90; `cublas64_12.dll` and `cublasLt64_12.dll` from 12.8.4.1. NVIDIA driver exposes `nvcuda.dll`; no toolkit installed and no permanent PATH edits. The process resolves the DLL directory relative to its own files. |
| Audio output | Completed binary WAV, 24 kHz mono PCM16, carried as Base64 on private stdout. No audio device opened by the worker. Native streaming is available, but the protocol currently returns only completed audio. |

The worker ran in a separate folder assembled by the [pinned portable setup script](<D:/AI-Voice-Auditions/Qwen3-TTS-Q8/Scripts/setup_portable.py>) using the verified archives and relocated voice files, without the original recording, venv, or system Python at worker runtime. This was a same-machine relocation test, not clean-machine qualification. Current standalone payload approximately 2.19 GB including 1.284 GB weights, 789 MB CUDA DLLs and 116 MB portable Python/NumPy/native wheel. Release packaging should re-pin a maintained Python build and review the included libraries. No login, model download or network listener is needed during synthesis.

The extracted artifact SHA-256s are `474cddf90736262b3027e1eda846cfe93157c4cd8270ecac7b76def6e0e9a61d` (speaker) and `614afb3b0418055642cf104c6269f241536a9e896911750077a87c1b12b56494` (reference codes). The format and shape come from the downstream wrapper’s documented `.spk`/`.rvq` methods. Re-extract and re-audition after a model/encoder/format change. Bundle known signed-off voices only; no visitor uploads or voice registration endpoints.

## Local protocol

Launch from the standalone folder; supervise the child process and provide both pipes. Execution log is diagnostics on stderr; stdout carries one JSON line per event or result. UTF-8 request input. `requestId` is an opaque local ASCII ID, up to 64 characters; client must allocate unique IDs. Wait for `{"event":"ready","voiceId":"awakening-guide"}`. Supported operations:

```json
{"op":"health","requestId":"health-1"}
{"op":"synthesize","requestId":"reply-1","language":"en","voiceId":"awakening-guide","conditioning":"x-vector","timeoutMs":10000,"text":"Welcome to Awakening Celebrations."}
{"op":"cancel","requestId":"cancel-1","targetId":"reply-1"}
{"op":"shutdown","requestId":"quit-1"}
```

`language`: `en`, `zh`, or `mixed` (last two use Chinese hint), with original UTF-8 text. Input is limited to 360 characters and one active request, with a 450 codec token cap. No external storage of visitor WAV, no public HTTP, no independent playback. Status is `ok` or `error` and stable code; successful synthesis includes `mime` and `audioBase64`. Duplicate work while busy is rejected, and cancellation/deadline use the native cooperative callback. If a native call wedges or the process crashes, a future controller must enforce a separate hard timeout/kill and discard stale responses. The prototype makes no guarantee about unload after forceful parent termination outside its tested normal exit, EOF, timeout or cooperative cancellation.

Smoke tests passed health, invalid locale, timeout, busy, cancel, new result after cancellation, Mandarin WAV framing and clean shutdown. [Main portable smoke](<D:/AI-Voice-Auditions/Qwen3-TTS-Q8/Benchmarks/worker-smoke.json>) · [Fresh relocated install smoke](<D:/AI-Voice-Auditions/Qwen3-TTS-Q8/Benchmarks/portable-relocation-check.json>).

Prefer a small standalone warm-up only after route/capability policy permits, or pre-generate fixed guide cues after approval. Keep voice quality, latency consistency and resource headroom separate gates. A future existing-flow adapter would own process supervision/cancellation with session/route IDs and keep text replies immediate. A separate playback owner should arbitrate guide and reply audio, dictation, environmental sound and protected video. No Stage 4.3B work is included.

## Reproduction, notices and rollback

The [artifact lock](<D:/AI-Voice-Auditions/Qwen3-TTS-Q8/Scripts/ArtifactLock.json>) pins exact downloader URLs and SHA-256 for GGUFs, CUDA archives, embedded Python 3.13.3, NumPy 2.4.6 and the native wheel. Existing local Python can run `Scripts/setup_portable.py` in a fresh folder with at least 4 GB free; production runtime uses only embedded Python. With that layout and validated conditioning, run `Runtime/PortablePython/python.exe Scripts/worker_smoke.py`. To reproduce auditions, copy the approved WAV as `Reference/audio_en.original.wav` plus the recovered transcript, then use `Scripts/audition.py extract` followed by `audition`, `warm`, `alternates` or `portable-profile` as appropriate. Use a fresh output folder for overlapping names; the audition script refuses to overwrite clips. Renderer/UI playback remains out of scope.

Distribute license notices if packaging: [native MIT](https://github.com/KoljaB/qwentts.cpp/blob/b91bca43f9adc5df839161ce4c88b0f6743b27ff/LICENSE), [GGML MIT](https://github.com/ServeurpersoCom/ggml/blob/c044c6f03892f9d5e98213b05f8afea1f8b0d3c9/LICENSE), [wrapper MIT](https://github.com/KoljaB/realtimetts-qwen-native/blob/v0.2.0/LICENSE), [Qwen Apache-2.0](https://github.com/QwenLM/Qwen3-TTS/blob/main/LICENSE), [NVIDIA CUDA 12.8.1 EULA and redistributables](https://docs.nvidia.com/cuda/archive/12.8.1/eula/index.html), and portable CPython and NumPy bundled license trees, alongside relevant vendor and MSVC notices. The signed embedded Python executable was verified by Windows, and CUDA archive integrity was checked against NVIDIA’s published SHA-256. Archive integrity is not a claim about clean-machine support or redistribution rights for private reference/derived voice materials. Preserve those as separate controlled assets pending user decision. No package was published.

To roll back, exit the worker and archive or remove the audition folder and separate portability staging folder, preserving the source WAV. Remove only the added Stage 4.3A documents if no longer needed. Unity runtime and the existing `VoiceLocal` install do not require rollback.
