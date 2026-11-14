# Stage 4.3E — GitHub recovery checkpoint

## Recovery point

| Item | Reference |
| --- | --- |
| Repository | [Kiraakatsuki/Awakening_Celebrations_Ver.Ka](https://github.com/Kiraakatsuki/Awakening_Celebrations_Ver.Ka) |
| Remote | `origin` |
| Milestone branch | `milestone/stage-4.3e-voice-complete` |
| Annotated recovery tag | `awakening-stage4.3e-voice-complete` |
| Commit SHA | Resolve the annotated tag to its commit with `git rev-parse "awakening-stage4.3e-voice-complete^{commit}"`. The branch head should match. |
| Previous checkpoint | `9415962d10db3f18266fd53d83e05e6854f31713`, tag `awakening-stage4.2c-stt-checkpoint`, annotated tag object `786355b6dc87244182fe01bba686ff1a59cf9d68` |

Local `main`, upstream `main`, and GitHub HEAD were aligned at the previous checkpoint. The starting index was empty, with no merge, rebase or cherry-pick in progress. Nine unrelated screenshot deletions were left in the user's worktree. The local Stage 4.3A voice audition notes and index were kept out because they contain the source transcript or index private experimental material.

## Included in Git

- The validated Living Glass Unity application through Stage 4.3D.2, including the working Whisper and GPT source, Qwen reply-audio adapter and lifecycle, Home and Settings voice assistance, all six performer routes, and bilingual guide playback.
- Active guide catalog `stage43d2-approved-tutorial` and all 26 production Ogg clips and Unity metadata, across 13 cue pairs. The payload is 1,301,114 bytes. It includes all seven tutorial steps in both languages, the final step 4 pair and the selected Mandarin alternates for steps 5 and 7. [Selected recordings and hashes](Stage4_3D2_ApprovedSources.json)
- Stage 4.3 implementation, deployment, activation and validation documentation, safe configuration examples and test evidence. The Stage 4.3D.2 runtime reports are historical evidence.

## Git hygiene

The existing Unity cache, build, logs, per-user, environment and Whisper exclusions were retained. The milestone carries the existing local `/QwenLocal/` rule and adds exclusions for `/QwenLocal-preparing-*/`, an in-project copy of `/AI-Voice-Auditions/`, and the two private Stage 4.3A audition documents. Git confirmed that the 26 Oggs, their metadata and the backend environment example remain trackable. The backend ignore file was inspected and retained.

The candidate contained no known credential signatures or embedded audio payloads. The local backend environment file, both local inference installations, original reference WAV, matching private transcript, conditioning data, native runtime payloads, model weights, provisional/rejected takes and runtime recordings were excluded. The largest added or modified file was the 75,064-byte `reflection-zh.ogg`. The largest inherited tracked blob was the 44,368,228-byte GAIA video. Git LFS is configured locally, with no LFS-managed paths in this checkpoint.

## Restore into a separate checkout

```shell
git fetch origin refs/heads/milestone/stage-4.3e-voice-complete refs/tags/awakening-stage4.3e-voice-complete:refs/tags/awakening-stage4.3e-voice-complete
git rev-parse "awakening-stage4.3e-voice-complete^{commit}"
git worktree add --detach ../Awakening-Voice-Recovery awakening-stage4.3e-voice-complete
```

This checkpoint contains application source, assets and setup documentation. A fully runnable offline deployment requires the additional files below. Copy them from a private backup with their original versions and directory structure.

| Dependency | Restoration |
| --- | --- |
| Unity and MediaPipe | Use Unity 6000.4.7f1 and `Assets/Scenes/AwakeningLivingGlass.unity`. Supply the MediaPipe 0.16.3 tarball separately and update the machine-specific local package path in the recovery checkout as needed. The manifest still points at the existing developer machine’s tarball. |
| Whisper | Restore `VoiceLocal/` beside `Assets/` in the Editor project, or beside the future Player executable. It needs whisper.cpp Windows x64 CUDA `b5454`, GGML and CUDA 11.8/cuBLAS 11.11.3.6 DLLs, the NVIDIA driver and the Microsoft x64 VC++ runtime. Retain Large-v3 Turbo Q5_0, Silero v6.2.0 VAD and the OpenCC 1.1.9 dictionaries. Restore optional local microphone selection privately. [Installer and hashes](../Stage4_2/Deployment.md) |
| Qwen reply audio | Restore `QwenLocal/RuntimePack/` with the trusted worker and engine scripts, portable Python/native v0.2.0 pack and CUDA 12.8 DLLs, compatible NVIDIA driver, and both Qwen3-TTS 0.6B Base Q8 GGUFs. Native runtime revision `b91bca43f9adc5df839161ce4c88b0f6743b27ff` and GGUF revision `b7ee2e8c7459c3bea99da23e3d178125a7d1713c` are pinned. Copy the example to the ignored `QwenLocal/config.json` and use the relative `RuntimePack` path and ICL voice. Cached guide narration plays independently of this pack. [Qwen setup](Stage4_3B_RuntimePlan.md) · [Example config](QwenLocal.config.example.json) |
| Approved voice identity | Privately restore the matching `Conditioning/awakening-guide.spk`, `Conditioning/awakening-guide.rvq` and `Reference/transcript.en.txt` to the Qwen pack. Retain the original reference WAV and artifact lock outside Git for future conditioning recovery. The original WAV is unnecessary for normal runtime when the verified ICL cache and matching transcript are available. [Deployment assessment](DeploymentAssessment.md) |
| Existing assistant | Keep the deployed Render service and its account/environment configuration. The existing backend source and `.env.example` are in Git. Supply the OpenAI key and authorized model selection through the backend environment for a replacement deployment. The Unity app still targets its configured backend. GPT replies require access to that service. |

Back up `VoiceLocal/`, `QwenLocal/`, the external `D:/AI-Voice-Auditions/Qwen3-TTS-Q8/` workspace, and the MediaPipe tarball separately. Store backend secrets in the authorized secret store rather than a project archive.

## Pre-commit validation

| Check | Result |
| --- | --- |
| Unity 6000.4.7f1 Edit Mode compilation | Passed. The Editor compiled the isolated check launcher and executed the existing suites against the current project assemblies. |
| Existing layout and state suites | Passed 72 reply layouts, 164 conversation cases, and 432 single-control microphone layouts with isolated state fixtures. [Reply](Stage4_3E_Evidence/Reply.txt) · [Conversation](Stage4_3E_Evidence/Conversation.txt) · [Microphone](Stage4_3E_Evidence/Mic.txt) |
| Guide and scene validation | Passed catalog import, bilingual routes, clip references, voice policy, camera-independent validation fixtures and six performer media references. All 26 Ogg hashes matched their approved sources. Metadata GUIDs were unique across the scanned `Assets/` Git inventory. [Voice checks](Stage4_3E_Evidence/Voice.txt) · [Scene references](Stage4_3E_Evidence/References.txt) |
| Excluded from this run | Play Mode, camera and microphone activation, live GPT, Qwen synthesis, Player builds, and full-system hardware acceptance. [Stage 4.3D.2 runtime evidence](Stage4_3D2_TutorialNarrationFinalization.md) |

To verify GitHub independently, compare the branch SHA and peeled tag SHA from `git ls-remote` with the checked-out commit. Confirm the annotated object is distinct from the commit SHA, then fetch both refs and inspect the fetched tree for the scene, voice source, approved catalog, and all 26 Ogg and metadata pairs.
