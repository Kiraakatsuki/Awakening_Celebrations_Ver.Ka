# Stage 4.3A — Performance benchmark

Measured on Windows 11 Home Single Language x64 build 26300, Intel Core i7-13650HX (20 logical processors), 16 GB RAM, NVIDIA RTX 4060 Laptop GPU (8,188 MiB reported, driver 621.01, SM 8.9). CUDA 12.8 DLLs were local. The project Editor was not used to run content; other applications and the desktop contribute to the device readings. No simultaneous Unity exhibition, Whisper, camera, or live AI load was measured.

| Measurement | Actual result |
| --- | --- |
| Downloads | Two GGUFs total 1,283,766,112 bytes (1.284 GB / 1.196 GiB). |
| Portable runtime, models, voice package | About 2.19 GB installed (2.04 GiB) with models, local CUDA, portable Python + NumPy + native DLLs, notices and conditioning. The full experiment tree is about 3.01 GB including downloads, dev environment, source, samples and evidence; excludes the separate portability test folder. |
| Native executable / DLLs | No `qwen-tts.exe` distributed in the selected wheel. Portable `python.exe`: 105,832 bytes. `qwen.dll`: 309,760 bytes. GGML DLLs: 53,088,256 bytes. CUDA DLLs: 788,957,696 bytes. Portable Python directory including native libraries and NumPy: 116,494,608 bytes, excluding Python bytecode caches. |
| First engine initialization observed | 13.13 s from engine call to return in the first CUDA probe; 1.24–1.44 s across six later process starts in the audition/bench harness. OS caches were not flushed. |
| Spawn-to-ready, standalone worker | 1.47 s with dev venv, 1.91 s with embedded CPython in the primary folder, 5.23 s in the separate relocation smoke check. Readiness means loaded model/cache; it does not promise that first synthesis kernels are warm. |
| Raw reference conditioning | 16.49 s once on the 27.47 s WAV (native extraction 16.48 s; speaker plus audio codec encoding). Model initialization and disk save separate. |
| Cached conditioning load | 1.53–3.25 ms in later benchmark invocations, independent of synthesis. |
| Short warm generation | 0.40–0.54 s for 2.0–2.8 s of audio (n=3 after first). |
| Medium warm generation | 1.41–1.82 s for 7.76–9.92 s of audio (n=4); median 1.62 s. |
| Warm first PCM | 35–43 ms, median 37 ms across n=7 warm runs after first. First callback chunk with RMS over −45 dBFS: 122–135 ms. Threshold crossing is a signal proxy, not measured first audible sound in speakers. |
| Warm real-time factor | Median 0.185 across the same n=7; roughly 5.4× real time. PCM16 WAV writing was below 3 ms in this run, separate from inference. |
| First-synthesis outlier | English Welcome x-vector took **23.43 s to first PCM / 26.98 s total** in the initial audition; an identical-output later run took **29.7 ms / 1.85 s**. Other first runs also showed initialization costs (first ICL warm-set clip: 0.85 s to PCM / 1.27 s complete). Logs point to a large early CUDA cost; its exact source and cold repeatability were not isolated. Do not promise a ≤2 s first reply after startup. |
| GPU memory, cached inference | 3,747 MiB sampled whole-device peak in portable profile vs 1,265 MiB baseline (≈2,482 MiB extra); 3,762 MiB maximum during initial audition. VRAM returned to 1,266 MiB after portable shutdown. WDDM counters were not per-process. |
| GPU memory, extraction | 4,497 MiB device peak vs 1,271 MiB baseline; 1,272 MiB after exit. Pre-stage offline; avoid extraction in a visitor session. |
| RAM and CPU | On the portable child, peak working set **1,867 MiB**, peak private commit **4,124 MiB**. Process CPU mean **92% of one logical core**, peak **140%** (≈4.6%/7.0% normalized across 20 logical processors). Earlier venv CPU/RAM samples were from its launcher stub, so those counters are excluded. |
| GPU utilization | 67% mean and 97% peak whole-device samples during portable short/medium process lifetime including startup and shutdown; interval = 200 ms. GPU load is material despite short synthesis. |
| Shutdown | Timed engine close 0.17–0.19 s; pipe shutdown ack to exit about 0.27–0.32 s measured from sending the shutdown command in smoke tests. No benchmark process remained. No OOM in the bounded runs; cumulative growth over a long session was not evaluated. |

Method: one native engine, maximum batch 1, flash attention on, 24 s codec chunk limit; Windows CUDA wheel v0.2.0, pinned Q8 talker and codec, 24 kHz mono. Seed 42 for the primary pair set and documented alternate seed 43. Generation wall time is measured from the streaming call through the last iterator result, excludes engine/load/encoding and includes callback/queue handoff. WAV time is separate. NVIDIA device readings sampled every 200 ms, process memory and CPU every ~100 ms. Disk sizes are actual uncompressed file sizes. No DAC or Unity playback latency was measured. PCM startup results describe native streaming, while the prototype pipe returns audio only once the whole WAV is ready.

[Raw timing and per-run statistics](<D:/AI-Voice-Auditions/Qwen3-TTS-Q8/Benchmarks/summary.json>) · [Raw synthesis manifest](<D:/AI-Voice-Auditions/Qwen3-TTS-Q8/Benchmarks/manifest.jsonl>) · [Initialization/extraction records](<D:/AI-Voice-Auditions/Qwen3-TTS-Q8/Benchmarks/initialization.jsonl>) · [Portable run GPU CSV](<D:/AI-Voice-Auditions/Qwen3-TTS-Q8/Benchmarks/portable-profile-gpu.csv>) · [Portable process CPU and RAM CSV](<D:/AI-Voice-Auditions/Qwen3-TTS-Q8/Benchmarks/portable-profile-cpu.csv>).
