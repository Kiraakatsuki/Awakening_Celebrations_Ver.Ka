# Stage 5.6.3 — Gallery music and Sound preferences

November 25, 2026. Local implementation and explicitly authorized, camera/microphone-free Editor validation. No Windows build, commit, push or deployment.

## Asset

- Source: `C:/Users/Nakul J S/Downloads/Spring Will Come When the Snow Melts Away (From Fruits Basket) - Homura Records.mp3`.
- Imported unchanged to `Assets/Resources/Audio/Gallery/Spring Will Come When the Snow Melts Away (From Fruits Basket) - Homura Records.mp3`.
- MP3, 320 kb/s, stereo, 44,100 Hz; Unity duration 225.071 seconds; 9,002,840 source bytes.
- Source/imported SHA-256: `583cbaca02f90f6cab2688c183a7881ee69b24ac81e07643c824952270253cd4`.
- Full audio decode to a null output succeeded. No external conversion, trimming, normalization or composition change.
- Unity importer: Streaming, PCM, Preserve Sample Rate, stereo, background loading, no preload. The supplied MP3 stays intact; Unity's decoded PCM artifact avoids another lossy encode at the cost of a larger packaged audio artifact (roughly 40 MB). No new runtime dependency.
- Import decisions follow Unity's [Audio Clip Import Settings reference](https://docs.unity.com/en-us/engine/6000.0/manual/audio/reference/class-audio-clip), accessed November 25, 2026.

## Behavior and ownership

The existing `LivingGlassOpeningMusic` helper, owned/disposed by the application controller, now owns two 2D looping AudioSources. No global manager or mixer was added. One session-only boolean on the existing flow controller controls only these background tracks; no PlayerPrefs mute value is read or written.

| Context | Music |
| --- | --- |
| Home | Existing opening track, unchanged volume 0.16, 1.8 s fade-in / 1.25 s fade-out; existing overlay suspension preserved |
| Ready Gallery, Focus and Activation | Supplied Gallery track, volume 0.14, 1.8 s fade-in / 1.25 s fade-out; same source and playback position across the pre-video screens |
| Settings over Gallery | Same Gallery track continues; Sound control can mute/unmute it |
| Living Portrait | Gallery fades to zero; actual video start waits for both existing preparation/visibility readiness and Gallery silence |
| Other routes | Background music fades out and stops |

Outgoing opening/Gallery tracks reach zero before the other starts. Sound OFF immediately mutes both; the relevant clip may continue silently, retaining position. ON fades its level back in without restarting the clip. Speech/capture retain immediate audio priority. Existing video Direct output, volume, embedded tracks and synchronization are unchanged.

The Settings row uses existing components, callbacks and `T` localization: `Sound: ON/OFF`, `聲音：開啟／關閉`, with localized explanation/tooltips. Locale rerender reads the same session state. Protected-flow restrictions remain in place.

The existing `EndVisitor` callback restores ON and resets/stops the two sources before the established Home lifecycle. Ordinary navigation does not reset the preference. There are no competing fade coroutines; the existing unscaled Update owns both fades. Disposal destroys both owned sources.

## Focused validation

- PASS — Unity compilation and Assets-related Console error check.
- PASS — fresh session Home starts with Sound ON, opening source playing at 0.16, Gallery stopped; camera consent false.
- PASS — actual mouse activation of the Settings button sets OFF and both sources mute.
- PASS — OFF survives Gallery navigation; opening stops; ON restores Gallery to its background level.
- PASS — English/Traditional Chinese Settings state matches the session boolean; 20 rapid toggles end consistently OFF.
- PASS — Gallery source/time remains continuous through Settings, Focus and Activation.
- PASS — actual prepared native video starts only after Gallery volume is zero and its source stops; embedded Direct audio remains unmuted at volume 1.0. One video reaches natural completion, then Gallery music returns.
- PASS — existing New Visitor callback restores ON, Home opening playback and normal level, with Gallery stopped. For this test only, an empty temporary souvenir journal was injected; the actual enrolled journal/client were restored and no Cloudflare call was made.
- PASS — exactly two owned background music sources after repeated navigation/toggling.
- PASS — protected-file fingerprints preserve pending splash settings/artwork, opening audio/importer, all six MP4s/metas, production scene and existing VoiceFix coordinator.
- PASS — a focused seek near the supplied track's end wraps to the beginning, stays playing, and retains exactly one Gallery source. This checks native looping, not a full 225-second listening pass or sample-perfect seam.
- PASS — ending a Play Mode session with OFF, then entering a fresh Play Mode session restores ON; after initialization/loading, Home opening music plays unmuted at 0.16 and Gallery is stopped. This verifies Editor-session initialization, not a newly built Windows process.
- PASS — opening source immediately mutes on OFF and ON starts its fade from zero with the same approved clip.
- USER VERIFIED — the user confirmed the Gallery volume and OFF/ON toggle sound right, including smooth return after Settings.

Subjective listening/level acceptance above is separate from source-state and frame evidence. This task does not claim sample-perfect looping or Windows Player compatibility testing. Supplied musical lead-in/tail content is retained; no seam editing was performed. AI speech/microphone/clap and full exhibition regressions were not rerun. Windows playback remains for a separately authorized combined build.

## Files changed

- `Assets/LivingGlass/Integration/LivingGlassOpeningMusic.cs`: extend existing owned helper with Gallery playback and background-only sound control.
- `Assets/LivingGlass/Integration/LivingGlassFlowIntegration.cs`: session sound state, route classification, existing reset hook.
- `Assets/LivingGlass/Presentation/LivingGlassPresentation.cs`: activate the existing bilingual Sound row.
- `Assets/Scripts/AwakeningApplicationController.cs`: existing Update tick and prepared-video start gate.
- New Gallery MP3 and its Unity `.meta`, plus `Assets/Resources/Audio/Gallery.meta`.
- This report.

Previous local changes remain intact. No backend, TPM, protected artwork, video asset, opening soundtrack, splash configuration or runtime package was changed. Public souvenir issuance was not modified; the last accepted hosted state remains paused, without a new hosted check.
