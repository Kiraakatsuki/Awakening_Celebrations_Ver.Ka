# Stage 3 — bounded Play Mode validation and acceptance

**13 November 2026 · Unity 6000.4.7f1 · candidate scene only.** This report supersedes the earlier static-only acceptance matrix. The authorized offline, non-camera Editor pass is complete and Play Mode is stopped. The candidate is **not production-ready or accepted for cutover**.

Open the [actual runtime visual review](BoundedValidation/Review.html), [overview](BoundedValidation/RuntimeOverview.jpg), [six themes](BoundedValidation/RuntimeSixThemes.jpg), [motion samples](BoundedValidation/MotionFrames.jpg), and [Controlled Spatial B comparison](BoundedValidation/RuntimeVsFigma.jpg). Source PNGs are actual 1920×1080 Game view captures, not browser renders or static Editor previews.

## Scope, isolation and preservation

The session read AGENTS.md, the project regression-testing, UI Toolkit, motion and accessibility procedures, and the installed Unity build-validation workflow before runtime work. Lifecycle and test utilities were inspected before use. No test runner, Player build, live assistant operation or profiler was started.

The application normally shares its established service lifecycles. For this bounded pass, the explicitly armed Editor-only [validation helper](../../Assets/LivingGlass/Editor/LivingGlassBoundedValidation.cs) applied **unsaved** scene overrides before every entry: deactivate the MediaPipe runner GameObject and blank the assistant backend URL. Consent began false and stayed false. An Editor update guard checked the consent gate, session consent, inactive/not-started runner, empty endpoint and absent assistant request; it disabled consent controls, Send and assistant suggestions. A violation would revoke/stop the pass. Each session had a ten-minute deadline. There was no capture, backend call or permission grant.

Four Play Mode entries covered initial discovery and targeted retests. The first keyboard and backdrop failures were reproduced before the corrections below. The final entry checked consent-label readability, actual window-motion frames and additional native keyboard operations. Video tests in the third entry used real local clips at their configured speed, without synthesized completion events.

Gallery readiness, tutorial route access and camera-off Attract presentation were explicitly set up through an isolated **presentation fixture** because the normal visitor journey requires camera consent. Fixture access is not a passing result for consent, awakening or gesture recognition. Semantic calls to the same UI commands exercised all six themes and local video sequences. Log phrases such as “Both hands raised” during those calls describe the existing command's log text; no hand recognition occurred. Physical mouse and keyboard inputs were separately exercised through the native Unity window.

On exit, the helper restored the pre-pass language preference and Game view configuration, discarded its unsaved isolation overrides and reloaded the candidate scene from disk. The restored rig being active in **Edit Mode** is its original serialized configuration, not capture. Final state: playing=false, dirty=false, consent=false, configured endpoint restored, one active rig in Edit Mode. See [final state](BoundedValidation/FinalEditorState.json).

| Preservation / safety check | Status | Actual evidence |
| --- | --- | --- |
| Record pre-pass Git state | PASS | [GitBefore.txt](BoundedValidation/GitBefore.txt); three shared-script changes and Stage 1–3/untracked work already existed |
| Original scene and production build entry | PASS | SampleScene remains the sole enabled build scene; byte hash unchanged |
| Candidate scene preserved on disk | PASS | Candidate scene hash unchanged; isolation overrides were never saved |
| Packages and six video importer metadata files | PASS | All 11 protected hashes match before/after, including both scenes and build settings; [summary](BoundedValidation/EvidenceSummary.json) |
| No automatic camera initialization | PASS | Runner deactivated before Play; gate/session consent false; guard never tripped |
| No application assistant request | PASS | Empty endpoint plus disabled sending; request reference absent in every recorded snapshot |
| Snapshot safety audit | PASS | 135 snapshots, zero unsafe samples; update guard additionally ran between samples |
| Existing validators / new helper side effects inspected | PASS | No asset saves, importer writes or hardware calls; reports/screenshots written only under Stage 3 documentation; original language preference restored |
| New C# compilation / repository preservation checks | PASS | Corrected code compiled; read-only static verifier rerun: 24 checks, zero failures, 104 locale records |

The existing Editor Unity AI plugin continued to emit its pre-existing NoSubscription diagnostic. The application was isolated from its assistant backend; this was not an air-gapped or packet-audited Windows session, and no claim is made about unrelated Editor plugin traffic.

## Actual test results

PASS means the stated bounded check passed, not that its entire feature family is certified. FAIL marks an observed unresolved issue. BLOCKED identifies tests incompatible with the authorization. NOT RUN identifies additional cases that were not exercised.

| Test | Status | Actual result / evidence |
| --- | --- | --- |
| Startup, initial UI and settled Home | PASS | Real Play Mode Home rendered in English; repeated entries rendered the same shell. First entrance frame had content opacity zero as expected, followed by settled content |
| Home entrance motion | PASS | Entrance observed in the native Game view; no stuck hidden content. Initial frame is retained as an entrance sample, not a settled screen |
| English and Traditional Chinese Home | PASS | Native window inspection and captures 25/28 show readable content; no missing glyphs seen; TC consent label retested after fix. Some English PNG readbacks omit chrome, tracked separately below |
| Gallery Arc composition, proportions and artwork alignment | PASS | Six visible presences, Far/Mid/Near/Near/Mid/Far apertures; names readable, no hit-region overlap in sampled 1920×1080 layouts |
| Gallery wrappers remain fixed during depth motion | PASS | 23 live Gallery samples, six wrappers identical across neutral/targeted/locales/reduced mode; row x=96, y=340, w=1728, h=464 |
| Controlled Spatial B optical movement baseline | PASS | Actual targeted scale/translation and theme response observed; sampled aperture max scale 1.019998, translation 6 logical px; wrappers stayed fixed |
| Full perspective pitch/yaw parity with browser Spatial B | NOT RUN | Candidate uses 2D optical transforms and crop parallax, not CSS perspective or a 3D renderer; no such renderer was introduced |
| All six performer-reactive environments | PASS | GAIA cyan, PANDEMIC sand/bronze, RE-BIRTH rose, WHITE-GUARDIAN ice, CONSCIENCE ember, SANCTUARY amber; actual themed images and identification text via semantic target commands |
| Direct transitions between performer themes | PASS | Commands retargeted in succession without leaving Gallery; no neutral flash observed in the native view. This is visual target-command coverage, not gesture arbitration proof |
| Physical pointer hover / target | PASS | Native movement into RE-BIRTH targeted it without navigation or video; screenshot 13 and Pointer-hover-Rebirth snapshot |
| Physical pointer click confirmation | PASS | Native SANCTUARY click committed performer 5 and opened its Focus in Chinese; screenshot 20 |
| Physical Tab / Shift+Tab in Gallery | PASS | Initially failed; corrected and retested: presence-0 → presence-1, then back to presence-0; identification/theme followed PANDEMIC/GAIA |
| Physical Enter selection | PASS | Focused GAIA committed to actual Focus after correction; Enter-retest snapshot and screenshot 14 |
| Physical numbered shortcut | PASS | Native key 3 committed RE-BIRTH; Numeric-key-3 snapshot and screenshot 32 |
| Physical Escape / return focus | PASS | RE-BIRTH Focus → Gallery, focus=presence-2; Escape-return-gallery snapshot |
| Gallery → Focus route, selected identity and final pose | PASS | Real keyboard commit preserved GAIA and settled at Focus; same route/identity behavior exercised for all six by semantic commands |
| Gallery → Focus premium visual continuity | FAIL | Actual 120–280 ms samples show the moving proxy and destination portrait simultaneously, with overlapping old/new text; settles correctly but is not an acceptable seamless final treatment |
| Global shell stable-state visibility/layering | PASS | Settings, AI, Help, Privacy and Back rendered in Home/Gallery/Focus; behind modal plane when an overlay is open |
| Protected cinematic shell | PASS | Actual media captures show reduced strip rather than floating window; video remains dominant. Settings/AI unavailable during protected moment; no bypass was attempted |
| Settings open / close actual motion | PASS | Final samples at approximately 20/120/280/480/750 ms show progressive opacity/scale/translation; close removes plane by 280 ms |
| AI chooser / conversation rendering offline | PASS | Chooser and GAIA conversation rendered without a request; disabled Send/suggestions were deliberate validation interlocks |
| AI opening motion | PASS | AI-open-final samples show real panel expansion and settle; no service call |
| AI closing / reopening | PASS | Dismissal and alternating reopen cycles completed; keyboard Space closed the final chooser |
| Settings interrupt / close / reopen | PASS | Reversal sequence opening→close→reopen settled, no stuck plane; screenshot 17 |
| Settings minimize / resume | PASS | Screenshots 18/19 show actual minimize/resume; route preserved |
| AI conversation minimize / resume | NOT RUN | Shared mechanism exists; a separate real conversation-minimize trial was not performed |
| Eight alternating Settings/AI open/close cycles | PASS | Final overlay plane count zero, overlay=false, focus=shell-settings; no duplicate visible window or stuck disabled content observed; not an exhaustive callback-leak test |
| Modal input blocking | PASS | Native click on underlying Focus action did not start video or leave route while AI modal remained open |
| Backdrop then native Escape | PASS | Initially failed with focus cleared. Retest after fix focused overlay-plane, then Escape closed and restored shell-settings |
| Modal keyboard traversal and Space | PASS | Native Tab: window-close→assistant-0; Shift+Tab returned to window-close; Space dismissed and restored shell-settings |
| Complete modal tab wrap / all control paths | NOT RUN | Forward/reverse steps and native activation passed; exhaustive wrap across every overlay control not exercised |
| Seven English tutorial layouts | PASS | All seven route snapshots and Game view PNGs have the expected route; point/palm/wave/fist/raised/clap artwork displayed as instruction, not recognized input |
| Traditional Chinese Tutorial Point | PASS | Actual layout and illustration readable; no missing glyphs observed |
| Every tutorial step in Traditional Chinese | NOT RUN | Only representative Point plus Home/Gallery/Privacy/assistant presentation inspected operationally |
| All 52 routes / 104 localized records operationally | NOT RUN | Static inventory still passes; this bounded pass sampled shared families, not every localized route. All six media sequences ran in English; complete TC chapters remain untested |
| Camera-off status | PASS | Common shell reads camera off/permission required; denied-consent Home Begin remains disabled |
| Attract camera-off presentation | PASS | Screenshot 24; fixture only, normal consent-gated entry was not bypassed |
| Tracking-unavailable / loss / recovery branch | BLOCKED | Requires an eligible capture/tracking state. No false consent or fake recognition was used to force it |
| Reduced-motion and opaque fallback | PASS | In-memory cloned configuration applied to actual Chinese Gallery/WHITE-GUARDIAN target; layout stayed stable and neutral aperture pose retained. No saved preference functionality implied |
| Reduced-mode complete journey / overlay interruption | NOT RUN | Representative Gallery only; no complete reduced-motion journey trial |
| All-six real Activation → prepare → first decoded frame | PASS | Real VideoPlayer frame >2/isPlaying observed for each, local clips only; actual decoded PNGs and CSV |
| All-six real media completion → Release | PASS | Each real loopPointReached observed, no decoder error, actual Release route; [MediaResults.csv](BoundedValidation/MediaResults.csv) |
| All-six Release → PostVideo | PASS | Existing Continue command reached the expected PostVideo in each real sequence |
| Closing / truthful unavailable Souvenir | PASS | Actual Closing then unavailable takeaway view captured; no QR or external service generated |
| GAIA replay and Back cancellation | PASS | Actual decoded replay cancelled via existing Back; stopped player, released owned RenderTexture, unregistered handlers; [result](BoundedValidation/MediaCancellation.txt) |
| Other-five cancellation / mid-prepare cancellation | NOT RUN | Completion checked all six; cancellation checked on decoded GAIA only |
| Missing clip / decoder error / configured timeout fallback | NOT RUN | No fault injection or importer/media alteration in this pass |
| Gesture thresholds / point / fist / palm / wave / raised / clap | BLOCKED | Camera and MediaPipe capture forbidden; semantic commands are explicitly not evidence of recognition |
| Consent accept/revoke/regrant / actual preview | BLOCKED | Permission remained denied; hardware lifecycle not exercised |
| Live AI success, loading, retry, cancel, late replies | BLOCKED | No live requests permitted; backend behavior not simulated |
| Full visitor journey from consent through awakening | BLOCKED | Consent-dependent flow cannot be validated under denied-camera scope |
| Original-scene operational regression | NOT RUN | Only candidate scene entered Play Mode; original was preserved by hashes/static contracts |
| Chinese IME / arbitrary assistant glyphs / screen reader | NOT RUN | Actual IME composition and assistive technology not exercised |
| DPI, fullscreen deployment, viewing-distance readability | NOT RUN | Only 1920×1080 Game view; scaled native window is not a physical exhibition trial |
| Steady 60fps, GPU/CPU, allocations and memory soak | NOT RUN | No Player/profiling authorization; frame captures prove neither smooth frame pacing nor sustained performance |
| Production cutover / full release acceptance | BLOCKED | Outside this pass; remaining defects and parity gates still open |

## All-six media evidence

No completion, frame-ready or gesture events were synthesized. The observer subscribed to the actual VideoPlayer loop event; it did not call the completion handler. Existing UI Continue initiated the existing playback sequence, with real decoding and ordinary completion timing.

| Performer | Decoded frame | Real loop event | Decoder error | Release and PostVideo |
| --- | --- | --- | --- | --- |
| GAIA | PASS | PASS | PASS — none | PASS |
| PANDEMIC | PASS | PASS | PASS — none | PASS |
| RE-BIRTH | PASS | PASS | PASS — none | PASS |
| WHITE-GUARDIAN | PASS | PASS | PASS — none | PASS |
| CONSCIENCE | PASS | PASS | PASS — none | PASS |
| SANCTUARY | PASS | PASS | PASS — none | PASS |

The first four configured streams reported 1280×720/24fps; CONSCIENCE and SANCTUARY reported 784×1180/24fps. Portrait-aspect video is centered with side space in the real Game view rather than stretched. These are decoder-reported source dimensions, not a measured application frame rate. Existing sound/volume capability was not tested or implemented.

## Visual comparison findings

Comparison used the saved native Figma exports already in the repository, particularly [Controlled Spatial B GAIA](../Stage2Design/LivingGlass/SpatialAddendum/review-evidence/B-GAIA-en.png), its Chinese/theme counterparts, Gallery Arc, Home, Tutorial, Settings and AI. It did not assume that a browser screenshot proved Unity rendering. No Figma editing/upload or fresh browser verification was needed.

| Visual check | Status | Finding |
| --- | --- | --- |
| Six-presence Arc and crop integrity | PASS | Actual captures preserve the clear six-person composition, asymmetric source crops and alternating depth hierarchy. GAIA's right-biased source crop keeps the face present; no obvious image stretch in the inspected views |
| Heading/art clearance | PASS | Gallery title moved from top 14% to 12.5% to accommodate Unity font metrics; live screenshots show clear separation. Wrappers were not moved |
| Identification panel and theme restraint | PASS | Floating central plate updates name/index/theme; neutral reading surfaces remain readable across the six sampled accents |
| Typography pixel fidelity | FAIL | Unity font metrics and text spacing differ visibly from the Figma reference. Readable tested copy is not evidence of pixel-equivalent typesetting |
| Settings/AI glass hierarchy | PASS | Tinted, sufficiently opaque panels, edge strokes and dimmed background work in actual UI Toolkit; no real blur/refraction claim |
| Settings/Privacy scrollbar styling | FAIL | Native bright default scrollbar dominates the dark window. Reproduce by opening Settings or Chinese Privacy with content overflow; affects both scroll-view styling and its child controls |
| Gallery-to-Focus transient composition | FAIL | Proxy and destination image overlap while the old Gallery and new Focus text crossfade. Actual screenshots at 120/280 ms reveal doubled portrait/text silhouettes; a final-pose geometry check would miss this |
| Camera/consent text TC clipping | PASS | Initial FAIL reproduced in Home; explicit internal toggle-label line height fixed it. Final Home/Privacy Chinese captures show complete text without consent being granted |
| Controlled Spatial B rendering equivalence | NOT RUN | Native Figma boards themselves use 2D optical proxies. Browser perspective tilt and any enhanced custom rendering path have not been validated in Unity |
| Overall approved-design visual fidelity | FAIL | Broad composition is recognizable, but transition ghosting, default scrollbars and type/material differences prevent a final visual acceptance claim |

The current renderer uses simple UI transforms, crop parallax and composited atmospheric color. It has no true UI Toolkit backdrop filtering, refraction, additional capture camera or duplicate video pipeline. Shadows/highlights and glass edge depth remain simpler than the intended premium rendering. Enhanced effects remain a separate, measured technical experiment, not a hidden dependency of this validation.

## Defects, reproduction and exact corrective changes

| ID | Initial status → retest | Reproduction / likely cause | Correction or remaining work |
| --- | --- | --- | --- |
| LGV-01 keyboard selection | FAIL → PASS | In real Gallery, native Tab/Enter reached the root but did not traverse/activate as expected. Raw KeyDown delivery alone did not produce the needed runtime navigation/submit behavior | New presentation OnKey explicitly handles Tab/Shift+Tab in the current scope and submits enabled focused Buttons on Enter/Space. Consumes the event to avoid double activation; text editing/IME retains its separate path |
| LGV-02 modal dismissal after blank click | FAIL → PASS | Open AI, click blank/modal backdrop, then Escape. Before fix focusedElement became empty, so Escape did not reach the root; AI-closed.json actually records overlay=true | New overlay plane and panel are focusable with tabIndex=-1. Actual native backdrop click then Escape closes, restores shell focus and leaves underlying action untriggered |
| LGV-03 heading clearance | FAIL → PASS | English gallery heading crowded portrait top at Unity's actual font metrics | New USS gallery title top 14%→12.5%; live bilingual gallery inspected, hitboxes unchanged |
| LGV-04 TC consent clipping | FAIL → PASS | Chinese Home consent string is vertically clipped by the inherited internal Toggle text size | New USS internal toggle text uses height:auto, min-height:36px, flex-shrink:0 and middle-left alignment. Actual final Chinese Home/Privacy captured and English Home inspected in the native window |
| LGV-05 proxy/destination ghosting | FAIL — open | Confirm GAIA and inspect the 120–280 ms transition; moving portrait proxy and new full portrait are visible at once, with old/new text overlap | Likely overlapping old-gallery, bridge proxy and destination visibility schedules in Render/Bridge composition. Refine ownership/masking and delay destination text reveal; no broad transition rewrite performed in this pass |
| LGV-06 default scrollbar | FAIL — open | Open Settings/Privacy; the white track/thumb and arrow controls clash with the dark material | ScrollView child styles are not comprehensively themed in new USS. Add shared scroller styling and retest keyboard/scroll behavior later |
| LGV-07 final type/material match | FAIL — open | Compare actual Game view against B-GAIA-en and Settings exports | Font metrics, shell/plate spacing and optical-material depth require a deliberate visual refinement pass; do not “fix” by adding an unvalidated heavy blur pass |
| LGV-08 inconsistent screenshot readback | FAIL — evidence path unresolved | Some Home/RE-BIRTH PNGs omit header, camera text or dock that were present in native window inspection and in state geometry. Capture 31 has Home metadata but Gallery pixels because a route mutation followed the asynchronous capture call in the same frame | Likely asynchronous capture/repaint timing; capture-path versus renderer cause is not established. Do not treat absent pixels as proven shell removal, or geometry as visual proof. Exclude 31; retain other affected frames with explicit caveats and retest synchronized native/readback capture later |

Runtime corrective changes are limited to:
- Assets/LivingGlass/Presentation/LivingGlassPresentation.cs — keyboard traversal/submit; modal plane/panel focusability.
- Assets/LivingGlass/UI/Styles/LivingGlass.uss — gallery title spacing; consent internal-label sizing.
- Assets/LivingGlass/Editor/LivingGlassBoundedValidation.cs plus its metadata — explicitly armed Editor-only isolation, observation and fixture helper. It is not a production gesture simulator and never automatically starts Play Mode.

Documentation/evidence tools were added only under Documentation/Stage3LivingGlass. No original scene/build entry, production legacy UI, package, gesture threshold, camera-consent implementation, video importer or completion rule was changed. The three pre-existing shared-script modifications remain as found; this pass did not alter them.

## Console and evidence quality

| Check | Status | Findings |
| --- | --- | --- |
| Application exceptions / decoder errors observed | PASS | None in captured runtime logs or final Console read; all real media rows error=false |
| New project C# compilation errors | PASS | None after the scoped corrections |
| Existing source warnings | PASS | Baseline contains four CS0618 obsolete API warnings and one CS0414 unused-field warning in legacy controller source; not introduced or cleaned up here |
| Editor plugin diagnostics | FAIL — pre-existing | Unity AI NoSubscription diagnostic remains; MCP WebSocket initialization/connection diagnostics also occurred during tooling. They are not passing application behavior and not attributed to this UI change |
| Tool snippet compilation | FAIL — instrumentation, corrected | Early in-memory MCP snippets omitted the UIElements query extension namespace and failed to compile; no project file was generated. Retried using valid helpers; not a project C# build failure |
| Capture integrity | PASS | Curated review uses actual full-HD Game view captures with companion route/locale/state JSON |
| Initial/mislabeled captures | FAIL — evidence quality, excluded | 01 is an initial transparent entrance frame. 03/05 labels do not reliably name the settled route; 31 pixels show Gallery although Home metadata was captured. Historical Settings-motion samples have overlay=false. They remain on disk but are excluded from curated acceptance evidence. Final Settings-open/close-final and AI-open-final are valid motion samples |
| Complete screenshot chrome consistency | FAIL — unresolved | 10/25 Home and 32 Focus readbacks omit chrome seen in native inspection; 22 omits some header/status content. These captures are annotated as inconclusive for shell rendering. Use full-chrome theme/GAIA Focus/modal captures plus native observations, without claiming every readback is visually complete |
| Log copy provenance | NOT RUN for Session2 log | Session2-RuntimeLog.txt repeats the earlier log and is not used as a distinct second-session runtime log. Use timestamped state JSON/native-input evidence for that session and actual Session3/4 logs for media/final retests |

Console baseline and final results are [before](BoundedValidation/ConsoleBefore.json) / [after](BoundedValidation/ConsoleAfter.json). Session3-RuntimeLog.txt records actual media first-frame activity and cancellation. Session4-RuntimeLog.txt records final shortcut selection. Sampled motion timing is approximate and screenshots can be one rendered frame later than state capture; no frame-pacing guarantee follows from these samples.

## Prioritized refinement and remaining gates

| Priority | Status | Next work and acceptance evidence |
| --- | --- | --- |
| P1 | FAIL — visual refinement required | Eliminate Gallery→Focus duplicate portrait/text exposure. Re-record actual transition frames for six identities, interrupted transitions and reduced motion before visual approval |
| P1 | NOT RUN | Complete keyboard tab wrapping, overlay scrolling, control reachability, locale replacement and Chinese IME across all shared families; recheck modal fix under rapid pointer/keyboard handoff |
| P1 | FAIL — validation evidence refinement | Synchronize screenshots with settled rendered frames and compare simultaneous native window/readback results; resolve missing-chrome capture discrepancy before complete visual acceptance |
| P2 | FAIL — visual refinement required | Theme ScrollView track/thumb/arrows; tune typography, floating identification spacing, shell/chrome and material hierarchy against approved exports in actual Game view |
| P2 | NOT RUN | All TC tutorial steps, all-six cancel/preparation/error/missing-clip cases, reduced-mode full journey and longer callback/resource-lifecycle repetition |
| Separate authorization | BLOCKED | Real consent/camera/MediaPipe gestures, target acquisition/loss/recovery and original-scene runtime regression |
| Separate authorization | BLOCKED | Live assistant backend success/error/cancel/late responses, approved privacy/operational service trial |
| Separate authorization | NOT RUN | Windows Player/DPI/fullscreen/viewing-distance trial and hardware frame-pacing/memory profiling against the provisional 1920×1080/60fps target |
| Final gate | BLOCKED | Release acceptance, production build-entry cutover and destructive legacy cleanup; this bounded pass authorizes none of them |

No automatic follow-on implementation, full release testing, production cutover or cleanup was started.
