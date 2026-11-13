# Active Living Glass architecture after Stage3.4

This describes the checked-out implementation after authorized demo retirement. Earlier architecture diagrams describe migration history. The development entry is `Assets/Scenes/AwakeningLivingGlass.unity`; the old demo is recoverable through the separate archival ref/bundle in [Report.md](Report.md). No runtime or release acceptance is inferred from this map.

| Responsibility | Current owner | Ownership and lifetime |
| --- | --- | --- |
| Startup, routes, history, first awakening, visitor/performer state | `Assets/Scripts/AwakeningApplicationController.cs` | One serialized MonoBehaviour, original GUID retained. Requires Living Glass presentation/document; no legacy fallback. Session consent starts denied. |
| UI commands, global overlay policy, camera eligibility, chat binding | `Assets/LivingGlass/Integration/LivingGlassFlowIntegration.cs` | Partial of the same controller, not a second service. Adds/removes chat handlers with the view; retains session/performer context and request cancellation. |
| Route/performer vocabulary and locale enum | `Core/LivingGlassRoutes.cs`, `Core/AppLanguage.cs` | Pure route classification and one performer/stage ordering. English0/Chinese1 preserved. No imported screen catalog. |
| Screen families, shell, windows, transcript/composer, bilingual copy | `Presentation/LivingGlassPresentation.cs` | One persistent UIDocument; semantic commands into the flow. Owns presentation tree replacement, focus scopes, stable target wrappers and outgoing/proxy cleanup. Unchanged in this phase. |
| Logical navigation versus visual completion | Controller versus presentation/motion | Flow commits state; visual animation does not fabricate video completion or recognition. Controller transition guards and cinematic coroutines preserved. |
| Visual tracks, interruption, reduced motion, crop | `Motion/LivingGlassMotion.cs` | Keyed cancellable animation; view shutdown clears visual work. Unchanged. |
| Performer assets, palettes, optical bounds, effect settings | `Themes/LivingGlassAssets.cs`, `Assets/LivingGlassAssets.asset` | Six shared-layout themes; original18 art references, fonts and configured modes unchanged. |
| Glass/layout/text material | `UI/Styles/LivingGlass.uss`, `UI/LivingGlassPanel.asset`, associated text settings | Independent Living Glass resources. Retains shared Unity runtime theme. No native backdrop blur or performance claim added. |
| Lightweight atmosphere | `Rendering/LivingGlassAtmosphere.cs` | View-owned optical composition, not another camera/video pipeline. Unchanged. |
| Gesture tutorial artwork | `UI/Components/LivingGlassGestureIllustration.cs` | Presentation only. Unchanged. |
| Camera consent/capture | Existing `GestureCameraConsentGate`, registered `HandLandmarkerRunner`, bootstrap/AppSettings | Single existing owner under imported MediaPipe sample directories. Session-only gate; preview visibility does not grant capture. All source/config bytes preserved. |
| Recognition and input arbitration | `MediaPipeGestureBridge`, `GestureRecognitionLayer`, `GestureCursorController`; controller target state; integration port pointer ownership | Existing semantics, dwell/release/cooldown thresholds and stable wrappers retained. No new recognizer/adapter or retuning. |
| Living Portrait preparation/playback/completion/cancellation | Controller's existing VideoPlayer event/coroutine/RenderTexture lifecycle | Same six clips/order/importers, first-frame handling, real completion events, missing-media policy and texture disposal. No duplicate decoder. |
| AI transport/session/cancellation | Controller's `AssistantSession`, request generation, `UnityWebRequest` coroutine; integration chat bindings | Transport, message context, cancellation and stale-response checks retained. View-active guard now uses only its pre-existing Living Glass branch. No backend change. |
| Audio | Controller's existing clip/source/fallback ownership | Same environmental audio lifecycle and disposal. No voice, STT or TTS. |
| Static/build validation | `Editor/LivingGlassProjectValidation.cs`, `LivingGlassBuildValidator` | Inspects sole entry, scene scripts, art/fonts/panel,52 routes and media references. Prebuild validation replaces the retired Figma mapping dependency; no build was run. |
| Independent route regression contract | `Editor/RouteContract.json`, `LivingGlassContractChecks.cs` | Exact104 original route/locale pairs; golden data has no Figma GUID/path dependency. Pure motion/crop checks retained. |
| Isolated Editor layout checks | Preview, ConversationChecks, StabilizationChecks | View-only owners with null flow and cloned configuration; destroyed on completion. No capture, transport or application session. |
| Bounded runtime evidence tooling | `Editor/LivingGlassBoundedValidation.cs` | Retained for separately authorized retests only. References independent scene constant; authorization/idle guards retained. Not executed in Stage3.4. |

Paths without `Assets/` above are relative to `Assets/LivingGlass/`. The current main controller is1544 lines, down from3056; its37 obsolete presentation/helper methods were removed. It still owns several proven operational lifecycles. Splitting those lifecycles into new services without runtime validation would add risk; it is explicitly remaining debt, not an implied completed service migration.

There is no `HomeLanguageController`, `ScreenDocumentManager`, `RuntimeScreenCatalog`, imported node-binding adapter or alternate overlay stack in the active architecture. Existing public/serialized contracts still required by Living Glass were retained. No dependency-injection framework, speech framework or speculative interface layer was introduced.

Future work should start with a separately authorized post-retirement smoke pass. Any subsequent service extraction should move one owned lifecycle at a time with cancellation/disposal tests and authorized runtime evidence. Future voice and performer-knowledge work remains separately scoped.
