# Stage 5.6 — Startup branding and opening music

Date — November 24, 2026. Local changes and Windows test build. Development branch `development/stage-5.2-jfc-ai`.

## Assets and supported splash

The supplied Downloads image `Awakening Celebrations_ Jember Fashion Carnival.png` was copied without modification to `D:/Awakening-Branding/Opening/a_hyper_detailed_cinematic_highly_polished_promo.png` and `Assets/LivingGlass/Branding/Opening/AwakeningCelebrationsStartup.png`. It decodes as a 1672 × 941 landscape PNG. SHA-256 `38430f68d81d9edd5ee3a3d9905cd3c1ae0b6d5ef0a2323626e98da34a62a698` matches all copies.

Unity's native splash presents this complete image as an aspect-preserving logo on black for four seconds, using static animation and the full sprite rectangle. `show` is enabled and `showUnityLogo` is disabled. No scene or Home design was added. The Windows Player splash was directly observed showing all six performers, the title and JFC branding with black framing and no Unity logo.

The Editor reports a non-Pro license. Unity 6 permits optional splash branding on Personal. Supported PlayerSettings accepted this configuration and the Windows build succeeded. [Unity plan policy](https://unity.com/products/pricing-updates), [Unity 6 release notes](https://unity.com/releases/editor/whats-new/6000.0.0), accessed November 24, 2026.

The supplied `Awakening_Living_Gallery_Cinematic_Loop_72s.ogg` was copied unchanged to the requested external Opening folder and `Assets/Resources/Audio/Opening/`. SHA-256 `1aed5fc7a46dcb2a7c1231d4c6aa16021f9ece768fa3dd9f0573864ff5cbced4` matches source and imported file. No artwork or audio was regenerated or edited.

## Opening music lifecycle

The existing `Home` route is the first page. The application controller owns one `LivingGlassOpeningMusic` instance. It asynchronously loads the supplied clip and owns a dedicated child GameObject and 2D AudioSource. The child prevents the existing clap owner's controller-level `GetComponent<AudioSource>()` lookup from selecting opening music.

- Stereo, 72.000 seconds, 48 kHz, background loading, Streaming, Vorbis quality 1, preserved sample rate and `AudioSource.loop` enabled.
- Source volume 0.16, fade-in 1.8 seconds, fade-out 1.25 seconds, unscaled delta time. Existing global Unity listener mute/volume applies. Settings has no active music-specific volume control.
- Home with no overlay and application focus permits playback. Other routes, overlays and focus loss fade it out and stop it. After a completed fade-out, returning to Home restarts the clip from the beginning. A rapid reversal continues the same source and reverses the fade.
- Existing speech playback and microphone capture have immediate mute priority on this source, followed by its fade-out. Speech ownership, synthesis, recording and submission contracts are preserved. The voice coordinator exposes one read-only playback-status property.
- Controller disable stops and destroys only the owned music child and discards pending-load authority. A late resource load cannot start a disposed owner.

Original-file boundary measurements found peak 0.235748, first/last 100 ms RMS 0.067035/0.064483, maximum per-channel endpoint difference 0.000671, and no leading/trailing region below −60 dB. No crossfade or audio editing was applied. These measurements and the output check support ordinary looping. Sample-perfect seamless playback has not been established.

## Focused evidence

| Check | Result |
| --- | --- |
| Editor compilation and current production scene | Compiled, idle Edit Mode, no compilation failure, scene clean before build. Final Windows rebuild succeeded with zero errors and three existing compiler warnings. Play Mode was not entered. |
| Windows native splash | Observed directly during normal File Explorer launch, complete image with preserved aspect ratio, no Unity logo. |
| Fullscreen Home | Observed in the built application at the laptop's native display size. |
| Automatic music and loop | Process-specific Windows render meter recorded continuous output across the first 72-second loop boundary. First-run maximum peak 0.038849. No microphone or loopback audio recordings were made. |
| Overlay fade and restoration | Settings reduced output to zero. Closing Settings restored output. Repeating Settings and confirming the existing Restart returned to Home and restored music. Meter segments are in the external validation directory. |
| Human listening | USER VERIFIED — the background level is restrained, fade-out smooth and return smooth. The final ownership correction retains these assets, gain and fade settings. |
| Gallery exclusion | Source review confirms `Show(currentScreen == HomeScreen)` is the sole page eligibility condition. A full Gallery journey was not rerun. Home's session-camera consent remained off during automated navigation. |
| Speech preservation | Qwen and Whisper runtime files are copied byte-for-byte from the accepted VoiceFix build. Existing speech/souvenir types and QR dependency are verified in the package. Live GPT, STT and Qwen regression suites were not repeated. |

External build and measurement evidence is under `D:/Awakening-Exhibition-Builds/Stage5_6_Opening.validation/`. Final rebuild and package verification are recorded there. Human listening remains distinct from automated output measurement.

After the ownership correction, the final executable was launched from normal Windows File Explorer. The custom splash and fullscreen Home were observed again. Its process-specific meter confirmed audio before Settings, zero output while Settings remained open, and restored output after closing Settings. `final-render.json` records those intervals at 0–19.89 s, 19.99–36.26 s and 36.36–37.97 s respectively. The final maximum output peak was 0.038849. The final normal-desktop Player log contained no startup exception; existing invalid-font-reference warnings were present. The application is left at Home.

The three final compiler warnings concern an existing unused souvenir exception variable and two existing obsolete background-scale API uses. No startup/audio compilation warning remains. The first build also compiled existing Sentis shader variants, producing additional warnings. The final build is the release candidate. An unrelated Unity AI Editor subscription/transport error was observed in the Editor console and is outside this change.

## Files changed in this task

- `Assets/LivingGlass/Integration/LivingGlassOpeningMusic.cs` and its `.meta`.
- `Assets/LivingGlass/Integration/LivingGlassFlowIntegration.cs` — owner creation and Home eligibility.
- `Assets/Scripts/AwakeningApplicationController.cs` — tick and disposal.
- `Assets/LivingGlass/Speech/LivingGlassVoiceCoordinator.cs` — read-only `IsPlaying` property.
- `Assets/LivingGlass/Branding/Opening/AwakeningCelebrationsStartup.png`, texture `.meta` and Opening folder `.meta`.
- `Assets/Resources/Audio/Opening/Awakening_Living_Gallery_Cinematic_Loop_72s.ogg`, audio `.meta` and Audio/Opening folder metadata.
- `ProjectSettings/ProjectSettings.asset` — supported native splash settings, preserving existing local configuration changes.
- This report.

## Windows package and boundaries

Executable `D:/Awakening-Exhibition-Builds/Stage5_6_Opening/Awakening Celebrations.exe` uses the accepted Windows x64 Mono configuration, production scene, product name, icon and fullscreen/window switching configuration. Both voice sidecars remain beside the executable, including 26 VoiceLocal files and 1005 QwenLocal files. Sidecar checks verify every file hash against VoiceFix. Private enrollment files and cloud authentication data are excluded. Build/debug evidence is kept outside the delivery folder and repository.

Earlier Stage5_4, ListenFix and VoiceFix builds remain available. Existing unrelated working-tree changes are preserved. No commit, push, cloud deployment, credential operation or issuance activation was performed. Public souvenir issuance is recorded according to its previously accepted paused state, without a new hosted check.

For a listening recheck, launch this executable normally, watch the custom splash, leave Home playing for more than 72 seconds, open Settings, then close it. The remaining rehearsal check is actual exhibition entry into the Gallery and return through the accepted visitor flow. Camera consent and that interaction remain under the user's control.

## Stage 5.6.1 — Fullscreen splash configuration correction

November 25, 2026. Configuration-only correction, awaiting the next authorized Windows build. The preceding Stage 5.6 runtime evidence describes the existing executable.

The original image occupied a centered foreground-logo slot, leaving black margins. This update changes only three splash settings in `ProjectSettings/ProjectSettings.asset` and adds this report section.

- Assign the existing complete `AwakeningCelebrationsStartup` sprite as Background Image.
- Disable Blur Background Image.
- Replace the foreground image with an empty four-second logo entry, retaining the original timing without drawing another image.

Static animation, zero overlay opacity, disabled Unity branding, fullscreen/window switching, startup resolution, icon and existing preloaded input asset remain exactly as recorded before this task. No image, texture metadata, script, opening-music asset or audio import setting changed.

Unity documents uniform cover scaling for splash backgrounds and empty logo entries for timing. At 1920 × 1080, 1280 × 720 and 2560 × 1440, the 1672 × 941 source is expected to cover the full display, with approximately 0.25 source pixels cropped from the top and bottom. The source is almost exactly 16:9. At 16:10, approximately 83.2 source pixels per side are cropped. At 4:3, approximately 208.67 per side are cropped and outer performers may be obscured. Extreme landscape and portrait ratios also require content review. The native cover setting preserves proportions but cannot guarantee every performer and all branding remain visible at every ratio. No custom adaptive renderer was introduced. [Unity background and logo settings](https://docs.unity.com/en-us/engine/6000.5/manual/unity-editor/editor-settings-reference/comp-manager-group/class-player-settings-splash-screen), accessed November 25, 2026.

Editor API inspection confirmed the correct background sprite, disabled blur, empty four-second foreground entry, idle Edit Mode and no script compilation failure. Persisted settings were compared against the task baseline and differ only in the three splash fields. Artwork decoding and SHA-256 match the original. Opening-music code, its dependencies and audio, texture metadata, unrelated modified files and the existing executable retain their baseline fingerprints.

Validation is limited to Editor configuration, asset integrity and cover-scaling calculations. No native splash preview or Windows Player verification was performed for this correction. Actual edge coverage, composition at the intended display ratios and timing require the next authorized build. No Play Mode, build, deployment, commit or push was performed. Existing build directories were preserved.
