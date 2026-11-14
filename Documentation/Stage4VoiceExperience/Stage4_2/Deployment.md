# Windows local speech deployment

Stage 4.2 installs a Windows x64 CUDA worker outside Unity's asset import tree. The development copy is installed at the project root under ignored `VoiceLocal/`. For a future Windows Player, place that folder beside the executable and its `_Data` folder. The adapter resolves it as a sibling of `Application.dataPath` in both configurations.

```text
VoiceLocal/
  bin/       whisper-cli.exe and the pinned x64 dependencies
  models/    ggml-large-v3-turbo-q5_0.bin, ggml-silero-v6.2.0.bin
  script/    STCharacters.txt, STPhrases.txt
  licenses/  supplier notices
  microphone.txt   optional exact Unity input-device name in UTF-8
```

Measured installation size is **1,792,187,902 bytes (1.67 GiB)** across 26 files. The exact deployment subset is listed in [RuntimeInventory.json](RuntimeInventory.json). The source archives and benchmark corpus are outside the repo in `D:/Coding/Unity/Archives/AwakeningCelebrations/WhisperStage42-b5454`. The installation and preparing folders are covered by `.gitignore`. Install downloads are the only network operation specific to this feature. Recognition uses local files and private process input.

## Install and recover

From the project directory, run the provided installer with a new destination and a cache location on a drive with several GB free.

```powershell
pwsh -NoProfile -File Documentation/Stage4VoiceExperience/Stage4_2/Install-LocalSpeech.ps1 -Destination ./VoiceLocal -DownloadCache D:/LocalWhisperDownloads
```

The installer checks SHA-256 for the runtime and NVIDIA archives, both models and both dictionaries before copying files. It builds a staging directory and promotes it after retrieving the notices. It preserves any existing deployment. After an interrupted install, inspect the reported staging path, then move it out or remove that exact staging directory before retrying. A missing installed file leaves the Stage 4.1 availability marker at the next conversation bind. A process that starts but cannot use CUDA yields a localized voice error and leaves text entry available.

An operator can use `microphone.txt` to pin the exact name of the directional microphone. If it is absent, the first device reported by Unity is used. Verify input ordering on the exhibition machine and disable loopback devices or pin a physical microphone. The application blocks recording when its own environmental audio is active and cancels an in-progress capture if it starts. This is an application overlap check. Directional positioning and an authorized room test are still needed to check ambient speaker bleed.

To disable speech locally, move the entire `VoiceLocal` folder outside the app directory while no recognition is active. Reopen the conversation. To roll back code, restore the pre-4.2 versions of the Stage 4.1 stylesheet and integration file from a reviewed diff. Remove the new Speech scripts with their metadata and the two added ignore rules if the phase is reverted. The Stage 4.1 work was already local and uncommitted at phase start, so a blanket Git reset would lose it.

## Pinning and native dependencies

| Item | Source and identity |
| --- | --- |
| whisper.cpp Win64 CUDA 11.8 | `b5454`, v1.9.5, commit `d1be6fde11ac6e0407606b4e42fe72d34add8037`. ZIP SHA-256 `e81c86c542be6e3d7b7245392d4e1c52ab6e5c61dbe38721754bc0ab59d4d027`. [Download](https://github.com/ggml-org/whisper.cpp/releases/tag/b5454) |
| CUDA cuBLAS Win64 | NVIDIA 11.11.3.6 from CUDA 11.8 redistribution manifest. ZIP SHA-256 `67b0934a6359e4ee26fff823c356021589d392c4fd49ca12624f570edc08e2b9`. [Manifest](https://developer.download.nvidia.com/compute/cuda/redist/redistrib_11.8.0.json) |
| Whisper weights | GGML Large-v3 Turbo Q5_0 revision `5359861c739e955e79d9a303bcbc70fb988958b1`, SHA-256 `394221709cd5ad1f40c46e6031ca61bce88931e6e088c188294c6d5a55ffa7e2`. [Publisher](https://huggingface.co/ggerganov/whisper.cpp/blob/5359861c739e955e79d9a303bcbc70fb988958b1/ggml-large-v3-turbo-q5_0.bin) |
| VAD weights | GGML Silero v6.2.0 revision `9ffd54a1e1ee413ddf265af9913beaf518d1639b`, SHA-256 `2aa269b785eeb53a82983a20501ddf7c1d9c48e33ab63a41391ac6c9f7fb6987`. [Publisher](https://huggingface.co/ggml-org/whisper-vad/blob/9ffd54a1e1ee413ddf265af9913beaf518d1639b/ggml-silero-v6.2.0.bin) |
| Script dictionaries | OpenCC 1.1.9 commit `556ed22496d650bd0b13b6c163be9814637970ae`, STCharacters and STPhrases, SHA-256 pinned by installer. [Upstream](https://github.com/BYVoid/OpenCC/tree/556ed22496d650bd0b13b6c163be9814637970ae/data/dictionary) |

`whisper-cli.exe` loads `whisper.dll`, `ggml.dll`, `ggml-base.dll`, CPU variants and `ggml-cuda.dll`. CUDA uses `cudart64_110.dll`, `cublas64_11.dll`, `cublasLt64_11.dll` and the installed NVIDIA driver's `nvcuda.dll`. The x64 Visual C++ runtime supplies MSVCP140, VCRUNTIME140, VCRUNTIME140_1 and VCOMP140, along with Windows UCRT. The file-only Editor tests used the installed subset above. The SDL microphone/stream utilities, HTTP server, quantizer and other executable examples are outside the deployment. CUDA initialization must succeed. No automatic CPU fallback is accepted.

## Licenses

Whisper.cpp and OpenAI Whisper code/weights are MIT. Retain their copyright and license notices, plus third-party notices for included components under the pinned source tree. Silero and its converted VAD weights are MIT. OpenCC dictionaries are Apache-2.0. Preserve the Apache license, existing attributions and note that this adapter uses a separate Han-only longest-match pass rather than the OpenCC runtime. Microsoft's Visual C++ redistributable must be installed through Microsoft's approved redistributable, not copied from another application.

NVIDIA's CUDA runtime and cuBLAS are covered by the CUDA Toolkit 11.8 agreement, with redistributable components listed in Attachment A and additional distribution terms. The installer gets them from their publisher and includes the cuBLAS archive notice. A future offline installer that bundles them must carry the applicable CUDA terms and notices. The NVIDIA device driver is a system prerequisite. [CUDA license and redistribution terms](https://docs.nvidia.com/cuda/archive/11.8.0/eula/index.html) · [Whisper.cpp license](https://github.com/ggml-org/whisper.cpp/blob/d1be6fde11ac6e0407606b4e42fe72d34add8037/LICENSE) · [OpenAI weights license declaration](https://github.com/openai/whisper#license) · [OpenCC license](https://github.com/BYVoid/OpenCC/blob/556ed22496d650bd0b13b6c163be9814637970ae/LICENSE) · [Silero license](https://github.com/snakers4/silero-vad/blob/be95df9152c0d7618fa1edfeb296fc3dae32376f/LICENSE)

## Data lifecycle

The app allocates a bounded clip only after the visitor explicitly chooses **Record**. End the device before copying the captured portion into owned memory. The worker thread resamples and encodes that buffer, writes it to an anonymous pipe and wipes the managed PCM and WAV arrays. The Windows CLI writes structured JSON to a random directory under the current user's temporary folder, `AwakeningVoice`. Each request removes its result and lease on completion, cancellation or error. An interrupted application exit may leave JSON until the next recognition, which removes abandoned request folders with expired creation times and an unlocked lease. No visitor audio file is generated. The transcript and CLI diagnostics are not logged.

The Windows job object terminates the worker on cancellation, timeout or process exit. Process creation and inference run off the Unity UI thread. The managed binding accepts completion only while its chat view, session identity and performer scope are current. Windows permission feedback is based on observable device/start behavior. An unsuccessful start asks the visitor to check microphone access because Unity's Windows microphone API cannot distinguish each denial from other startup failures.
