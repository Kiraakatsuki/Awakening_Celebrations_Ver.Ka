# Stage 5.4 — Cursor and video acceptance

## Cursor

- The Unity window uses a custom arrow with a 4×4 texture hotspot. Enabled buttons get a warm outline variant. The physical mouse continues to use normal UI Toolkit pointer events.
- The pointing hand uses the exact smoothed panel point already passed to gesture hit-testing. The fingertip is the hotspot. Existing gallery target detection drives its hover ring, and the existing fist confirmation drives selection. Successful gallery selection can show one brief ring for either input source, with a still variant when Reduced Motion is on.
- Tracked pointing must be stable for 140 ms before the hand is shown. A mouse press or four panel units of accumulated displacement gives the arrow priority for 400 ms. Tracking loss returns to the arrow. This affects presentation alone. The existing recognition, selection timing, camera consent, touch and keyboard handlers remain in place.
- The overlay and its children ignore picking. It reattaches after a route rebuild and restores the default Unity cursor on focus loss or teardown. Missing arrow artwork uses the platform default. Missing hand artwork uses a ring at the same mapped point.
- The three PNGs are original project artwork. The Figma Community page was the requested visual reference, but its asset access and redistribution terms could not be verified. No Figma or Apple cursor artwork was copied. [Figma reference](https://www.figma.com/community/file/1517119747499682321/apple-macos-26-cursors) · [Unity cursor import requirements](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/Cursor.SetCursor.html)

## Video replacements

| Performer | Supplied file | Replaced project asset | Existing GUID | Matching SHA-256 |
| --- | --- | --- | --- | --- |
| SANCTUARY | `sanctuary_new.mp4` | `Assets/Resources/Videos/LivingPortrait/Sanctuary.mp4` | `af0527d9d7044ba28bc9c274f8d91c97` | `066653e0ef4e7325cf5f9ffb56912b6e9f80a588919134ab43a14375d8c167a7` |
| CONSCIENCE | `conscience_new.mp4` | `Assets/Resources/Videos/LivingPortrait/Conscience.mp4` | `3647aa80071445388d8227a6da31ef08` | `50244e8fd3965ab903bbb08167014da455f2c3561b4c090bcd77a3de8eb413f9` |

Both source files were copied byte for byte. Their H.264 video tracks are 1284×724 at 24 FPS and contain 481 frames. Unity reports 20.042 seconds for each imported clip, with one stereo audio track. The existing Unity importer settings and scene references were preserved. Unity's importer reported H.264 timestamp correction for both clips. In-app playback still uses a clip-sized RenderTexture, Scale To Fit, silent video output, and the existing non-looping completion callback. Source audio is present but the existing application routing does not play it. Release, Reflection, Closing and souvenir qualification code retain their existing playback and completion paths.

## Verification

| Check | Result |
| --- | --- |
| Unity 6000.4.7f1 Roslyn compilation | Passed with Editor and Windows Player defines using Bee compilation options. Existing obsolete-style and unused-variable warnings remain. |
| Isolated Unity 6000.4.7f1 Editor checks | Passed texture loading, transparency and hotspots, non-picking overlay, single reattachment after a root clear, detach cleanup, gallery Button Clickable availability, and import properties for both replacement clips. |
| File scope | Source and destination hashes match for both replacements. Both video metas, the scene references, and the other four performer videos are unchanged. |
| Live Unity Editor and Windows x64 Player | Pending. No Play Mode or new Player run was performed. Native cursor appearance, hand tracking and arbitration, real mouse and touch, scaling, natural video completion and downstream progression remain to be exercised on the exhibition hardware. |

## Changed files

- `Assets/Scripts/Interaction/GestureCursorController.cs`
- `Assets/Scripts/AwakeningApplicationController.cs`
- `Assets/LivingGlass/Integration/LivingGlassFlowIntegration.cs`
- `Assets/LivingGlass/Presentation/LivingGlassPresentation.cs`
- `Assets/Resources/InteractionCursors.meta` and `Arrow.png`, `ArrowHover.png`, `Point.png` with their metas under `Assets/Resources/InteractionCursors/`
- `Assets/Resources/Videos/LivingPortrait/Sanctuary.mp4` and `Conscience.mp4`
- `Documentation/Stage5Media/Stage5_4_CursorAndVideoAcceptance.md`

## Settings cleanup and Restart

- Removed the unavailable Accessibility preferences and Calibration rows, including their disabled controls. Existing accessibility and calibration functionality was preserved. Sound preferences retain their existing presentation.
- Restart / End Experience opens a bilingual confirmation in the existing Settings overlay. Cancel is the initial focus. Cancel and Escape preserve the visitor route and return to Settings with the Restart control focused and visible. The dialog language switch retains the open confirmation.
- Settings Restart and Closing’s New Visitor both call the same visitor-ending method, which delegates to the existing souvenir reset and Home flow. The existing busy guard, QR cleanup, journal and pending-reset protections remain in force.
- Unity 6000.4.7f1 compilation and focused Play Mode checks passed. Mouse input opened and cancelled the confirmation. Keyboard checks covered opening, Tab focus, locale switching, Cancel, Escape and Restart. A temporary journal and mock souvenir client verified a single reset, Home return, recovery after a reset failure, no replacement remote visit while blocked, and Closing’s existing New Visitor action. Camera consent stayed off, and the tests used no live souvenir calls. Physical gesture input was not exercised.
- Changed for this Settings task: `Assets/LivingGlass/Presentation/LivingGlassPresentation.cs`, `Assets/LivingGlass/Integration/LivingGlassFlowIntegration.cs`, and this report.

## Windows exhibition checkpoint

- The user accepted the final pointer and gesture behavior. The normal and hover arrows retain a 19×21-pixel visible footprint, a 32×32 texture canvas and a (4,4) hotspot.
- Unity's Standalone product name is Awakening Celebrations. Player Settings reference a square crop of the approved emblem for the Windows executable and Player icon. The complete supplied emblem is retained alongside that crop. Existing splash settings were kept.
- The Windows x64 Mono build uses the existing AwakeningLivingGlass scene and Fullscreen Window mode at the display's native resolution. The window is resizable in windowed mode, with Unity's fullscreen switching enabled. Use Alt+Enter or launch with `-screen-fullscreen 0` for troubleshooting.
- The company and Standalone application identifier remain unchanged. Unity's product rename changes the Windows PlayerPrefs registry namespace. The legacy application-language preference was not present on this laptop. The souvenir enrollment and journal use their existing separate per-user path.
