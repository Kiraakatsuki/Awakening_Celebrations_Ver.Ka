# Stage 4.2C recovery dependencies

This checkpoint tracks the Unity source and serialized scene, the Stage 4.1–4.2C implementation and tests, and the scripts and checksums needed to restore local dictation. It does not archive an installation, recordings, credentials, or the Stage 4.3A voice audition.

| Boundary | How to restore |
| --- | --- |
| Unity application | Use Unity 6000.4.7f1 and open `Assets/Scenes/AwakeningLivingGlass.unity`. Retain the package manifest and lock. The pre-existing MediaPipe 0.16.3 manifest entry uses a local absolute tarball path; a fresh machine must supply the same package and adjust the local path if necessary. The Stage 3.4 recovery archive has the original tarball; do not copy credentials or other local settings from that machine. |
| Whisper Windows x64 runtime | Restore ignored `VoiceLocal/bin/` with whisper.cpp `b5454` (v1.9.5, commit `d1be6fde11ac6e0407606b4e42fe72d34add8037`), its GGML DLLs, CUDA 11.8 runtime, and cuBLAS 11.11.3.6. Use the installed NVIDIA driver and Microsoft x64 VC++ runtime. No hardware testing is performed by the installer. |
| Whisper models and text | Restore `VoiceLocal/models/ggml-large-v3-turbo-q5_0.bin` and `ggml-silero-v6.2.0.bin`, and the pinned OpenCC 1.1.9 dictionaries under `VoiceLocal/script/`. An optional operator-created `VoiceLocal/microphone.txt` is machine-specific. |
| Existing assistant | The repo contains the backend source and an example environment configuration. A deployed service, credentials and service account are external prerequisites. The speech recognition pipeline runs locally; existing Ask is the point that submits assistant text. |

Reproduce the local Windows speech install from the project root:

```powershell
pwsh -NoProfile -File Documentation/Stage4VoiceExperience/Stage4_2/Install-LocalSpeech.ps1 -Destination ./VoiceLocal -DownloadCache D:/LocalWhisperDownloads
```

Use a new destination on a recovery checkout. Do not overwrite an existing working installation. [Deployment, hashes, and runtime DLL requirements](../Stage4_2/Deployment.md) · [Exact runtime inventory](../Stage4_2/RuntimeInventory.json) · [Stage 4.2C behavior and isolated evidence](../Stage4_2/SingleControlRefinement.md).
