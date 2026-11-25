# Stage 5.6.2 — Six performer video replacements

November 25, 2026. Local replacement, focused Edit Mode validation and explicitly authorized video/audio-only Play Mode recovery. Camera and microphone capture were not enabled. Branch `development/stage-5.2-jfc-ai`.

## Mapping and references

All existing paths are retained. Sources remain in the user's Downloads folder. Mapping follows the exact supplied filenames and the active scene's serialized performer array. The application order remains GAIA, PANDEMIC, RE-BIRTH, WHITE-GUARDIAN, CONSCIENCE, SANCTUARY.

| Performer | Existing asset path, now replaced | Supplied source | Retained GUID |
| --- | --- | --- | --- |
| GAIA | [Gaia.mp4](<D:/Coding/Unity/Projects/Awakening Celebrations ver.RM/Assets/Resources/Videos/LivingPortrait/Gaia.mp4>) | [Gaia.-new.mp4](<C:/Users/Nakul J S/Downloads/Gaia.-new.mp4>) | `8b6f6f1e3f264810aba6d340fe734e09` |
| PANDEMIC | [Pandemic.mp4](<D:/Coding/Unity/Projects/Awakening Celebrations ver.RM/Assets/Resources/Videos/LivingPortrait/Pandemic.mp4>) | [Pandemic-new.mp4](<C:/Users/Nakul J S/Downloads/Pandemic-new.mp4>) | `c00ec61dfec4470bb0453aa212307594` |
| RE-BIRTH | [Re-Birth.mp4](<D:/Coding/Unity/Projects/Awakening Celebrations ver.RM/Assets/Resources/Videos/LivingPortrait/Re-Birth.mp4>) | [Re-Birth-new.mp4](<C:/Users/Nakul J S/Downloads/Re-Birth-new.mp4>) | `0d0041709efa403fab018e2ee14f0c26` |
| WHITE-GUARDIAN | [White_Guardian.mp4](<D:/Coding/Unity/Projects/Awakening Celebrations ver.RM/Assets/Resources/Videos/LivingPortrait/White_Guardian.mp4>) | [White_Guardian-new.mp4](<C:/Users/Nakul J S/Downloads/White_Guardian-new.mp4>) | `cee0636ab52a4b9d8d5026944c69fc25` |
| CONSCIENCE | [Conscience.mp4](<D:/Coding/Unity/Projects/Awakening Celebrations ver.RM/Assets/Resources/Videos/LivingPortrait/Conscience.mp4>) | [Conscience-new.mp4](<C:/Users/Nakul J S/Downloads/Conscience-new.mp4>) | `3647aa80071445388d8227a6da31ef08` |
| SANCTUARY | [Sanctuary.mp4](<D:/Coding/Unity/Projects/Awakening Celebrations ver.RM/Assets/Resources/Videos/LivingPortrait/Sanctuary.mp4>) | [Sanctuary-new.mp4](<C:/Users/Nakul J S/Downloads/Sanctuary-new.mp4>) | `af0527d9d7044ba28bc9c274f8d91c97` |

Targeted GUID/reference search found these six serialized clip references in `Assets/Scenes/AwakeningLivingGlass.unity`, lines 499–504. `StartLivingPortrait` reads `performerPortraitVideos[performer]` directly. No alternate URL, StreamingAssets video, Resources.Load video path, prefab clip reference or separate video-selection configuration was found by the targeted search. The six resource-folder asset paths remain available to existing consumers.

The optional scene VideoPlayer reference is empty. The controller creates/reuses one VideoPlayer. It uses a clip-sized ARGB32 RenderTexture, ScaleToFit UI image, non-looping playback, first-frame preparation, existing error fallback and native completion callbacks. These configurations, event subscriptions, cancellation, transitions and souvenir completion callbacks were preserved.

## Before and after metadata

The current files supersede the first Stage 5.6.2 replacements. The newest supplied copies use H.264 Main Level 5.0; the earlier 30-second replacements used High Level 5.0. The original pre-Stage-5.6.2 videos were approximately 20.042 seconds, at 1280 × 720 (four clips) or 1284 × 724 (CONSCIENCE and SANCTUARY).

All six newest MP4s contain 1920 × 1080, 24 FPS, 721-frame H.264 video, lasting 30.041667 seconds, with BT.709 limited-range metadata. Every file contains one AAC-LC stereo 44,100 Hz track, lasting 30.069841 seconds. All timing, encoder padding and audio bytes remain unchanged. They are exactly 16:9.

| Performer | Superseded 30-second file bytes | Latest file bytes | Latest video bitrate, Mbit/s |
| --- | --- | --- | --- |
| GAIA | 88,971,672 | 80,608,034 | 21.328 |
| PANDEMIC | 61,825,818 | 57,325,710 | 15.128 |
| RE-BIRTH | 64,687,081 | 58,781,766 | 15.516 |
| WHITE-GUARDIAN | 63,560,624 | 57,671,763 | 15.220 |
| CONSCIENCE | 49,298,757 | 45,459,912 | 11.968 |
| SANCTUARY | 78,492,096 | 73,643,922 | 19.474 |

## Quality, integrity and sound

Every replacement was copied byte-for-byte to its original project asset path. Complete file SHA-256 equality verifies preservation of resolution, frame rate, bitrate, codec, color metadata, embedded audio, packet timing, synchronization data and full duration. No conversion, filtering, trimming, resizing or artwork change was performed.

Each existing .meta had `enableTranscoding: 1`. Only that flag was changed to `0` in each of the six files. GUIDs and all other importer fields remain identical, including `importAudio: 1`. Unity now imports the supplied originals without the prior transcoding step.

The existing controller explicitly selected `VideoAudioOutputMode.None`, so replacing media alone would leave it silent. The media-integration audio block selects `Direct`, controls/enables the clip's embedded track, unmutes it and uses direct volume 1.0. The subsequent recovery additionally disables `skipOnDrop`; no other playback architecture was changed. Every new clip has exactly one track. There is no additional AudioSource or independent soundtrack. Existing Stop, reset, cleanup, preparation and completion logic is unchanged. The VideoPlayer continues owning audio/video timing.

Unity documents direct output and track control in the existing VideoPlayer API. [VideoPlayer audio modes](https://docs.unity3d.com/cn/6000.0/ScriptReference/VideoAudioOutputMode.html), [controlled audio tracks](https://docs.unity3d.com/cn/current/ScriptReference/Video.VideoPlayer-controlledAudioTrackCount.html), accessed November 25, 2026.

| Performer | Superseded 30-second SHA-256 | Latest supplied and installed SHA-256 |
| --- | --- | --- |
| GAIA | `39de2c375686446d6367f04cabca03d538d5f98b0d1bc7bf03bf7cbcbc3c4176` | `0b170a283608d202ee80a2b1091195123bc0277af09e69fb70d39bce1f6faf6a` |
| PANDEMIC | `792bd4bae3ff71ee87ab0559c8a547236e9d01f530a52805daa4e1396b76e93a` | `82f719397af82a42b44dbc310ff1fd3f546395f8fa5f3198b6382871eb7e8002` |
| RE-BIRTH | `72e8e7aa583f80cca110920c69ab769d999b53783cd0d715e8c65fb690f14ed2` | `25dd5c2e23102ec95375bc2417442e5069cef573327480b7e403d21e6c6b08f7` |
| WHITE-GUARDIAN | `9ddaaad0e6c1134be2274b75685d43bacec82e758b7c835242a9190d50275c15` | `a7b39eccec7777b4e2cd68aa6630d7aeebc3f6fcc7e4a5dbf5666a934a4f5d6f` |
| CONSCIENCE | `dcec08827d41aa5430bb3038adfe9338376cc13ce2722a8a88f3bba56236e66f` | `9b7863537731ad17c8991d96b03a886861cb23929c62859a7fe6f9e680b675c2` |
| SANCTUARY | `4c475a81ec57edbc0bc2c2cfa916d3c4b44b5db444091e32fe119b92717ffdbc` | `aba19e7904c05c1f18741967bd53df2d573bc18c8e865aa513cd77e4129c6735` |

## Playback recovery and focused validation

The reported native `AudioSampleProvider buffer overflow` warnings were reproduced with video stuck at frame zero while its clock advanced. An isolated native VideoPlayer also reproduced the stall. Changing audio routing alone did not correct it. Disabling frame skipping (`skipOnDrop = false`) restored frame progression and sound using the existing Direct output. This is an observed compatibility problem in this Editor/native playback configuration; the underlying Unity decoder defect is not independently established. No conversion, software decoder, DSP-clock override, new AudioSource or new playback system was retained.

- PASS — newest source/install SHA-256 equality for all six files; 4,326 video frames fully decoded; all embedded stereo audio tracks retained. Earlier installed versions were replaced in place, without duplicate clips. User Downloads, existing rollback builds and Git history were not deleted.
- PASS — existing six GUIDs, scene references, clip-sized RenderTextures and audio track settings remain valid. Importer transcoding is disabled; each existing meta differs only in that flag from the original baseline.
- PASS — focused compilation; no compilation failure. Play Mode ended with camera consent false and the production scene clean.
- PASS — uninterrupted application-path playback: GAIA, PANDEMIC, RE-BIRTH, WHITE-GUARDIAN and CONSCIENCE each emitted 721 frame callbacks, reached frame 720 and invoked natural completion. Test receipt durations, including preparation and a one-second post-completion observation, were 31.657, 31.658, 31.655, 31.656 and 33.414 seconds respectively.
- INCONCLUSIVE — the sequential harness did not start SANCTUARY in its sixth slot (zero frames, timeout). This is not recorded as a playback pass. Early diagnostic attempts also ran before UI initialization and failed; these were excluded from acceptance evidence.
- PASS — a separate SANCTUARY application-path check after visible Home initialization reached natural completion in 30.211 seconds and the existing Release screen. Quarter-second sampling reached frame 716 at 29.958 seconds before native completion reset the player. This sampling does not claim receipt of all 721 frame callbacks.
- PASS — native Editor.log contained zero new AudioSampleProvider overflow warnings across the final five-video sequence and the successful separate SANCTUARY check. Native log inspection was used because managed log callbacks did not reliably capture those warnings.
- PASS — Windows render-output metering detected actual Unity audio output during the focused tests. This measures output signal, not subjective sound quality or microphone input.
- USER VERIFIED — after reporting a slight hiccup near SANCTUARY's ending on an earlier pass, the user confirmed “The ending is smooth now.” This is separate from automated frame/log evidence. An earlier inspected pass had a roughly 1.4-second Editor-update gap; later checks avoided mid-playback MCP inspections. That association does not prove the cause of every perceived hiccup.
- PASS — all preserved unrelated baseline fingerprints match, including scene, pending splash correction, opening music, existing local fixes and the accepted Windows executable.
- PENDING — Windows x64 playback/audio acceptance in the next separately authorized build; subjective audio quality and A/V synchronization across every clip; full pause/restart and Reflection/Closing journey acceptance. No Windows build or full exhibition regression was authorized here.

Unity continues to warn that the non-baseline H.264 timestamps are adjusted during native import/playback. The latest clips played through in the focused Editor checks, but this does not establish universal decoder compatibility or perfectly stall-free playback on every machine. The supplied files remain unmodified.

## Changed files and boundaries

- Six existing MP4 files under `Assets/Resources/Videos/LivingPortrait/`, mapped above.
- Their six existing .mp4.meta files, retaining GUIDs and disabling transcoding (from the initial replacement task).
- `Assets/Scripts/AwakeningApplicationController.cs`: existing embedded-video Direct audio block plus the recovery's one-line `skipOnDrop = false` change. Earlier unrelated opening-music work in the same file was preserved.
- This report, `Documentation/Stage5Media/Stage5_6_2_PerformerVideoReplacement.md`.

All changes remain local and uncommitted. Focused Play Mode was explicitly authorized; physical camera/microphone capture remained off. No Windows build, standalone launch, Git commit, push or deployment was performed. Existing build directories were not modified. No Cloudflare, D1, TPM, AI, voice runtime, clap, navigation, locale, UI, splash or opening-music behavior was changed by this recovery.
