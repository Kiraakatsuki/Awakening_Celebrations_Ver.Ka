# Stage 5.7 — Final integration acceptance

November 25, 2026. Completed local integration; the user has confirmed that the Windows application was launched and runs correctly. Git finalization and retention completed as recorded below. No new build or runtime test is part of this finalization.

## Navigation correction

`BuildDock` previously sampled `LivingCanBack` while `Navigate` held its transition lock. The post-selection Focus Back button was consequently created disabled and did not refresh after the lock cleared.

Focus is now explicitly eligible for Back. Both the visible button and the existing `OpenPalmHeld` event enter the same `Back()` callback. Its Focus-specific path stops the existing performer sequence, including a pending Focus-to-Activation coroutine, settles presentation, clears transient selection/qualification state, releases the transition lock and returns to the existing Gallery. It does not reset the visitor, sound preference, journal or assignments. Activation and active-video policies remain unchanged. No gesture detection, threshold, cooldown, debounce or camera change was made.

USER VERIFIED — the user explicitly confirmed the Stage 5.7 Back-button and open-palm navigation fix works correctly. This confirmation is distinct from automated tests and does not establish an individually documented six-performer hardware test matrix.

## Preserved integration

| Area | Included status and evidence |
| --- | --- |
| Startup branding | Approved JFC startup image and Unity metadata; supported splash background, blur disabled, empty four-second foreground entry, preserved fullscreen configuration. The Stage 5.6.1 cover correction is included in the Stage 5.7 build. No new appearance test was performed during finalization. |
| Opening music | Original approved OGG, volume 0.16, 1.8-second fade-in and 1.25-second fade-out retained. Prior user listening acceptance is recorded in the Stage 5.6 report. |
| Gallery music / Settings | Supplied MP3, one Gallery source continuous through Focus/Activation, fade to silence before prepared video starts, bilingual background-only Sound toggle, session ON default and New Visitor restoration. Focused Stage 5.6.3 Editor checks and user listening confirmation are recorded separately. |
| Six videos | Latest GAIA, PANDEMIC, RE-BIRTH, SANCTUARY, WHITE-GUARDIAN and CONSCIENCE MP4s replaced in place with original GUIDs. Supplied 1920 × 1080, 24 FPS video and embedded stereo AAC remain unchanged. Transcoding and problematic frame skipping remain disabled. Direct video audio remains enabled. Stage 5.6.2 records hashes, decoding and focused Editor playback evidence, including the accepted SANCTUARY ending. |
| Voice / AI / kiosk | Existing assistant, Whisper, Qwen, localization, Settings/reset, souvenir and TPM implementations preserved. Only the existing speech-playing accessor required for music priority is included; backend and authentication code are unchanged. |

## Verification evidence and limitations

- PASS — required branch `development/stage-5.2-jfc-ai`, repository root and origin verified. Before finalization, local/fetched remote tips matched `7d566d4053ab3fbb47f8dfd6cfb0562ab8eb46e6`; no merge/rebase state or unresolved conflicts existed.
- PASS — reviewed source diffs, six video GUIDs, required Unity metadata pairs and ordinary Git tracking. No LFS migration or quality change. Each video is below GitHub's 100 MiB limit; the largest is GAIA at 80,608,034 bytes.
- PASS — source tracing confirms one shared Focus cancellation callback and the unchanged recognizer's emitted-pose latch/release hysteresis.
- EDITOR LIMITATION — the attempted independent Stage 5.7 navigation runtime pass was blocked by a minimized/unresponsive Editor at that time; it was not subsequently executed. Do not treat earlier Stage 5.6 checks as a completed Stage 5.7 six-performer or physical-hand test matrix. Later Unity build compilation succeeded.
- USER VERIFIED — the user reported the Back-button/open-palm fix works, then personally launched the Stage 5.7 Windows application and confirmed it runs correctly. This is general runtime acceptance, not a claim that each music transition, video and gesture was individually tested.
- NOT REPEATED — runtime/Play Mode checks, AI regressions, physical camera/microphone tests or a new build during Git finalization.

## Existing Windows package

`D:/Awakening-Exhibition-Builds/Stage5_7_FinalIntegration/Awakening Celebrations.exe`

Previously completed in Unity 6000.4.7f1 using the existing Windows x64 Mono release settings and `Assets/Scenes/AwakeningLivingGlass.unity`, native-resolution Fullscreen Window, approved product name/icon and Alt+Enter behavior.

Build succeeded in approximately 64 seconds: **0 errors, 488 warnings** (3 C# warnings: unused exception variable and two obsolete background-scale API uses; 485 Sentis shader warnings). Full details remain in the external `Stage5_7_FinalIntegration.validation/build-warnings.txt` and `build-report.txt`.

Executable/x64 PE header, UnityPlayer, Mono runtime, Data/Managed, application identity and required assemblies were checked without launching the Player. VoiceFix/Whisper `VoiceLocal` contains 26 files; `QwenLocal` contains 1,005 files including `config.json` and `RuntimePack`. All copied files were previously hash-verified against the accepted Opening build. Finalization rechecks required files/counts without replaying runtime acceptance. No credentials or enrollment data are packaged.

## Git checkpoint

The reviewed checkpoint contains 33 paths: five source scripts (including the music helper), helper metadata, approved startup artwork/metadata, opening/Gallery audio/metadata, six MP4s/metas, Player settings and four Stage 5.6–5.7 acceptance reports. Full current files are preserved without reconstructing earlier content.

Excluded local changes: rendering/volume/panel cache assets, Unity Connect settings, two historical report edits and an untracked older Stage 5.2D report. Protected final souvenir artwork, private credentials, local D1/Wrangler data and Windows binaries are not staged. No ignore rules or tracking policy changed. No deleted screenshots were restored.

Commit message: `Stage 5.7: finalize exhibition media, audio and performer navigation [skip render]`.

The established `[skip render]` directive is included to avoid triggering the existing Render auto-deployment. No Render/backend/configuration or Cloudflare file is included. Implementation commit: `b7abf220f47ab4adef35e611d38e38f148d5ebaf`. Ordinary push to `origin/development/stage-5.2-jfc-ai` succeeded; a fresh `git ls-remote` returned the same SHA. GitHub accepted all assets, with recommended-size warnings for five MP4s over 50 MiB; no hard-limit violation or storage-policy change occurred. A separate documentation-only receipt records these final results without amending the implementation commit.

Scoped staged checks found no obvious private-key/token signatures, protected souvenir artwork, build output or backend files. Source/text whitespace checks passed; Unity-generated trailing spaces on empty new `.meta` fields were preserved to avoid changing imported files. No `.gitignore` changes were necessary.

## Build retention

Build-success records and executable timestamps identify the two builds to retain:

- `D:/Awakening-Exhibition-Builds/Stage5_7_FinalIntegration/` — success record November 25, 2026, 04:12 Taipei.
- `D:/Awakening-Exhibition-Builds/Stage5_6_Opening/` — final success record November 24, 2026, 20:50 Taipei.

Both have complete essential Player files and the 26-file VoiceLocal / 1,005-file QwenLocal packages. The following positively identified older completed Player folders were removed from the managed root using the Windows Recycle Bin:

- `D:/Awakening-Exhibition-Builds/Stage5_4/`
- `D:/Awakening-Exhibition-Builds/Stage5_4_ListenFix/`
- `D:/Awakening-Exhibition-Builds/Stage5_4_VoiceFix/`

Before removal, all targets resolved to direct children of the managed root, contained no junctions/symlinks, had successful build records and essential Player files, and had no running executable under their paths. Automatic safety review blocked the first permanent recursive-delete command; no deletion occurred from that command. Recoverable recycling was used instead. No Recycle Bin purge was performed.

PASS — exactly the two retained completed-build folders remain. Their executable, UnityPlayer, globalgamemanagers, Assembly-CSharp and Qwen configuration hashes match the pre-cleanup fingerprints; both still contain their 26-file VoiceLocal and 1,005-file QwenLocal packages. Separate `.validation` folders containing diagnostics/tools were preserved, including those associated with older builds. No uncertain directory was removed and no deletion target was outside the managed root.

No new build, Play Mode entry or Player launch occurred during finalization.

No cloud deployment, D1 write, TPM operation, production-service change or issuance activation occurred. The last accepted public souvenir state remains paused; no new hosted verification was performed.

## Preserved unrelated local changes

The following existing tracked modifications remain unstaged and unchanged:

- `Assets/DefaultVolumeProfile.asset`
- `Assets/LivingGlass/UI/LivingGlassPanel.asset`
- `Assets/Settings/UniversalRP.asset`
- `Assets/UniversalRenderPipelineGlobalSettings.asset`
- `Documentation/Stage4VoiceExperience/Stage4_3/Stage4_3E_GitHubCheckpoint.md`
- `Documentation/Stage5AIKnowledge/Stage5_2/Stage5_2B_GitHubAndRenderPreparation.md`
- `ProjectSettings/UnityConnectSettings.asset`

The untracked `Documentation/Stage5AIKnowledge/Stage5_2/Stage5_2D_LiveAcceptanceReport.md` also remains excluded. No existing source modification was reverted or untracked.
