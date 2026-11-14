# Verified GitHub recovery checkpoint — Stage 4.2C

| Item | Verified result |
| --- | --- |
| Remote | [Kiraakatsuki/Awakening_Celebrations_Ver.Ka](https://github.com/Kiraakatsuki/Awakening_Celebrations_Ver.Ka) |
| Development branch | `main`, confirmed against remote HEAD and local tracking |
| Checkpoint | [9415962d10db3f18266fd53d83e05e6854f31713](https://github.com/Kiraakatsuki/Awakening_Celebrations_Ver.Ka/commit/9415962d10db3f18266fd53d83e05e6854f31713) |
| Annotated tag | `awakening-stage4.2c-stt-checkpoint`; remote tag object `786355b6dc87244182fe01bba686ff1a59cf9d68`; peeled commit equals checkpoint |
| Push | Successful atomic branch+tag push. Independent `ls-remote` and fetch confirmed the remote branch, tag and expected scene, capture/worker source, refinement report and recovery inventory. |

Pre-checkpoint local/remote HEAD: `e57d4fa0c72e087c4a8a86f6e5c4657ec51e4488`. No staged changes or merge/rebase state existed at baseline. Staged 57 reviewed voice files and documentation only. No large binaries, credentials or personal/voice materials were found in the staged set. No Git LFS-managed files or LFS attributes were in use. The nine unrelated screenshot deletions and five local Stage 4.3A documents were not included. No runtime behavior changed before remote verification.

Excluded: ignored Whisper installation, model weights, native inference binaries, local microphone selection, Python environments, Unity caches/builds, backend credentials, approved reference WAV, Qwen conditioning/assets, audition files and external runtime. Nothing was uploaded from the external audition tree. Existing pre-checkpoint generated folder metadata contains Unity's conventional trailing blank values; the checkpoint retains that serialized file intact.

Recover in a separate checkout. For example:

```shell
git fetch origin tag awakening-stage4.2c-stt-checkpoint
git worktree add --detach ../Awakening-STT-Recovery awakening-stage4.2c-stt-checkpoint
```

Then apply [tracked versus external dependency inventory](Stage4_2C_RecoveryDependencies.md) and the [Whisper installer](../Stage4_2/Install-LocalSpeech.ps1). Restore the pre-existing MediaPipe tarball prerequisite and existing external assistant configuration if needed. Do not copy private credentials or ignored installations into the Git checkout.

Only this checkpoint was pushed. Stage 4.3B is local work.
