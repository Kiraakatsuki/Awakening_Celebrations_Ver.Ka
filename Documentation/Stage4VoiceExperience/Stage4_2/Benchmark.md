# Stage 4.2A standalone Whisper benchmark

**GO for the measured CUDA path.** The RTX 4060 Laptop GPU completes a short question in about a second with a fresh process per request. That process lifetime fits the planned resource policy. The measured Turbo Q5_0 CPU path took about 14–15 seconds for comparable short input, so the installed voice feature requires CUDA. Accuracy on this small read-speech corpus is provisional.

## Reproducible setup

- Runtime: whisper.cpp 1.9.5, official x64 build `b5454`, commit `d1be6fde11ac6e0407606b4e42fe72d34add8037`. CUDA 11.8 archive supplemented with NVIDIA cuBLAS 11.11.3.6. CPU comparison used the official x64 CPU ZIP from the same build. [Pinned distribution](https://github.com/ggml-org/whisper.cpp/releases/tag/b5454)
- Model: multilingual Large-v3 Turbo Q5_0, 574,041,195 bytes. Measured SHA-256 `394221709cd5ad1f40c46e6031ca61bce88931e6e088c188294c6d5a55ffa7e2`, matching the publisher. [Pinned weight](https://huggingface.co/ggerganov/whisper.cpp/blob/5359861c739e955e79d9a303bcbc70fb988958b1/ggml-large-v3-turbo-q5_0.bin)
- Silence filter: Silero 6.2.0, 885,098 bytes, verified SHA-256 `2aa269b785eeb53a82983a20501ddf7c1d9c48e33ab63a41391ac6c9f7fb6987`. VAD threshold 0.5, minimum speech 250 ms. [Pinned detector](https://huggingface.co/ggml-org/whisper-vad/blob/9ffd54a1e1ee413ddf265af9913beaf518d1639b/ggml-silero-v6.2.0.bin)
- Host: Windows 11 Home Single Language build 26300, i7-13650HX 20 logical processors, 16 GB RAM, RTX 4060 Laptop 8,188 MiB reported, driver 621.01, WDDM, 1920 × 1080 NVIDIA output. Unity Editor and other desktop apps were open. The exhibition scene was idle.
- Inference: mono PCM16 at 16 kHz, forced `en` or `zh`, transcription, 4 threads, greedy beam/best-of 1, context 0, temperature 0 with fallback disabled, non-speech token suppression and flash attention. Structured JSON output. [Benchmark source](benchmark.py) · [Raw trials](Results.json)

The release ZIP alone loaded only CPU. PE import inspection found the missing cuBLAS dependency. After the verified NVIDIA DLLs were installed beside the CLI, the log showed CUDA device 0, compute capability 8.9 and 573.45 MB of CUDA model tensors. Every scored CUDA trial required the CUDA backend log marker. Vulkan was not run. CUDA met the target using a pinned upstream binary. A Vulkan comparison would need a separate source build or vetted binary and is unnecessary for this host. Source builds use CMake with MSVC x64 and the chosen SDK, `-DGGML_CUDA=ON` or `-DGGML_VULKAN=ON`. [Upstream build instructions](https://github.com/ggml-org/whisper.cpp/tree/d1be6fde11ac6e0407606b4e42fe72d34add8037#nvidia-gpu-support)

## Measured latency and resource use

| Measurement | Result |
| --- | --- |
| First successful CUDA process, 3.5-second English probe, upstream default 5 beams | 552 ms model load, 259 ms encode, 912 ms reported engine total |
| Fresh process per short human utterance, warmed filesystem, 22 runs | 903 ms median, 1,013 ms nearest-rank p95, 1,027 ms maximum |
| Model initialization across timed CUDA processes | 474–528 ms, median 489 ms |
| One loaded CUDA context, repeated short clips | Mean engine time after subtracting model load, 142 ms English across 10 utterances and 164 ms Mandarin across 12 |
| Longer utterances | 14.2-second English in 1,127 ms and 18.4-second Mandarin in 1,057 ms, including fresh process startup |
| CPU fresh process comparison | English 14,831 ms and Mandarin 14,380 ms, four threads |
| CUDA process memory peaks | 350 MiB working set and 1,722 MiB peak commit, sampled on Windows |
| CPU process memory peaks | 745 MiB working set and 1,318 MiB peak commit |
| CUDA total GPU memory, sampled every 100 ms | Around 1,803 MiB before, 2,706 MiB maximum, 1,807 MiB after. Roughly 903 MiB over the starting desktop baseline. These are adapter totals under WDDM, including other applications. [Samples](GpuSamples.csv) |
| Process/resource behavior | 32 CUDA processes and 2 CPU processes exited. No CUDA allocation failure or persistent adapter memory growth was observed in this bounded run. This does not measure sustained Player load. |

Cold means a fresh process and model context. The first successful probe followed model hashing and CPU tests, so OS disk caches may have been warm. True first-boot disk-cold timings were not measured. Repeated in-process timings are aggregate engine averages, separate from the per-request process latency distribution. Readback, audio capture and Unity frame pacing require a runtime measurement.

## Recognition and corpus

| Case | Result |
| --- | --- |
| English human read speech | 5 edits / 84 words, 6.0% WER across 6 clips |
| Mandarin human read speech | 18 edits / 162 normalized characters, 11.1% CER across 7 clips |
| Repeatability | All 11 short clip reruns matched their first transcription. Both in-process batches produced the same two sequences of outputs. |
| Performer names | One local synthetic reading preserved Gaia, Pandemic, Rebirth, White Guardian, Conscience and Sanctuary as recognizable words. Exhibition capitalization and hyphens were not imposed on the model. |
| Reflective question and additive noise | The same local synthetic English question was recognized with and without seeded white noise. |
| Silence and noise alone | Digital silence and a four-second seeded white-noise clip each returned an empty transcript with Silero. |
| Mixed wording | A spliced Mandarin/English test retained both languages and `WiFi`. This is a synthetic splice, with no claim about natural code-switching. |
| Display script | Mandarin output was Simplified Chinese. The Unity adapter applies a pinned Han-only phrase/character conversion to the machine-generated draft and leaves typed drafts, Latin spelling and punctuation alone. |

The English source is LibriSpeech `test.clean`, CC BY 4.0, carried by the OpenSLR dataset mirror on Hugging Face, whose selected rows are 0, 1, 6, 12, 21 and 31. See individual IDs and file SHA-256 in the results. [LibriSpeech attribution](https://www.openslr.org/12/) · [Dataset mirror](https://huggingface.co/datasets/openslr/librispeech_asr)

The Mandarin source is Google FLEURS `cmn_hans_cn/validation/0000.parquet`, CC BY 4.0, downloaded from its converted Parquet branch. SHA-256 `18698ffdd46c36c54af641821684d3f0313a7b64e0a49615597ec61a98f2b57e`. Selection was the first six 2–6-second rows and first 12–24-second row. IEEE float samples were down-converted to PCM16. [FLEURS attribution](https://huggingface.co/datasets/google/fleurs)

WER uses lowercase words with punctuation removed. CER removes punctuation and whitespace and normalizes both scripts to Simplified with OpenCC for scoring, independently of visitor output conversion. WER and CER use summed Levenshtein edits divided by total reference units. Microsoft Zira Desktop generated two local test files without playback. The synthetic, mixed and negative controls do not contribute to WER/CER. The model weights, native binaries and audio files stay local. Taiwan Mandarin and exhibition-room speech quality remain provisional.

The benchmark cache is outside the Git project. It contains the downloaded archives, `en.json`, the Mandarin Parquet, test-only audio and a Python dependency directory. The included script requires Python 3.13, pyarrow 26.0.1, psutil 7.2.2 and opencc-python-reimplemented 0.1.7. To repeat locally, give `benchmark.py` the cache directory and a fresh results path. The runtime probe never enables the CLI's microphone or server examples.
