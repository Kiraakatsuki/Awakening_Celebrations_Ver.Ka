# Stage 4.3D — Narration activation

The 16 user-approved English and Mandarin narration files are installed in the Living Glass Resources catalog. The approved compressed review files were copied byte for byte; the masters and Qwen voice/conditioning remain untouched. [Audio inventory](Stage4_3D_AudioAssetInventory.md) · [Runtime verification](Stage4_3D_RuntimeValidation.md).

| Path changed | Scope |
| --- | --- |
| `Assets/LivingGlass/Resources.meta`, `Assets/LivingGlass/Resources/LivingGlassGuide.meta`, `Assets/LivingGlass/Resources/LivingGlassGuide/Approved.meta` | Unity directory identities for the new Resources hierarchy. |
| `Assets/LivingGlass/Resources/LivingGlassGuide/ApprovedCatalog.json` and `.meta` | Active, approved eight-cue catalog; binds both locales. |
| `Assets/LivingGlass/Resources/LivingGlassGuide/Approved/{welcome,privacy,overview,gestures,awakening,discovery,reflection,closing}-{en,zh}.ogg` and each `.meta` | 16 exact approved inputs and import settings. |
| `Assets/LivingGlass/Speech/VoiceExperienceChecks.cs` | Stage 4.3C assertion now supports an explicitly approved catalog; draft rejection is still tested. |
| `Documentation/Stage4VoiceExperience/Stage4_3/Stage4_3D_*` | Approved-source and Unity-import evidence, test records and reports. |

There are no new routes, narration scripts, changes to gameplay policy, playback owner changes, or inference models in Assets. The existing cue mapping activates on the actual route, locale and readiness changes. Home starts Off; On starts appropriate cached guidance; Settings reflects the same visit choice and Off silences playback immediately. Done clears the voice preference for the next visit. The existing replay guard consumes each cue at most once per locale per visit. Manual Listen/Stop remains independent of that preference. Protected video and existing environmental sound retain priority over guide/reply audio, with no ducking or edits to the media pipeline.

Unity imported the 819,238-byte payload as mono 24 kHz Vorbis, compressed in memory with background loading and preload. No Qwen process is required for a cached clip. Dynamic replies still use the existing separately installed ICL worker and do not affect when text is displayed.

Rollback: set `approved` to `false` in the new catalog to silence the guide while keeping the candidate files, or remove precisely the new guide assets and their matching metadata. Keep the versioned Stage 4.2C checkpoint and the local Stage 4.3B–4.3C modifications intact. No Git commit or push was made, and no unrelated local changes were reset. Neither the scene nor the build settings were saved with test overrides.
