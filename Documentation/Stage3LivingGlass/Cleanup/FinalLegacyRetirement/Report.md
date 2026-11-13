# Stage3.4 — final legacy demo retirement

Completed the authorized development-entry cutover and high-confidence legacy retirement. Living Glass is the sole intended application entry; the old demo scene, imported layouts/catalog/bindings and their presentation-only owners are retired. The approved view, themes, motion, conversation composition, art/media and capture/backend integration remain byte-preserved except the narrowly recorded flow consolidation. **This is not production-release acceptance.** No Play Mode, camera capture, application assistant call, Player build or profiling was performed.

## 1. Baseline and user work

Branch `main`, HEAD `b9f0972b38008b31e4bd2117ee88cb34ef9ce6fe`. The working tree already contained Stage1–3 documentation/skills and the Living Glass implementation as untracked work, plus modified controller/bridge/runner and eight earlier reference-retirement files. See [exact initial status](BaselineGitStatus.txt), [initial tracked diff](BaselineWorkingChanges.patch), [2038-file hash baseline](BaselineHashes.json) and separately protected backend hashes. None of these states was interpreted as disposable because it was untracked.

AGENTS.md, architecture/UI Toolkit/feature-planning/regression skills, general implementation and package-management guidance, current source/serialization, Stage1–3 design/architecture/validation reports, the conversation report and complete Stage3.3/3.3A/3.3B findings were reviewed. Latest source took precedence over earlier descriptions. The Stage3.4 user instruction explicitly superseded operational SampleScene rollback; shared-service and behavior preservation remains in force.

No reset, clean, forced checkout, history rewrite or main-branch commit was performed. Existing user changes outside the authorized controller/consolidation scope remain preserved.

## 2. Recovery gate — PASS before deletion

Created a separate archival Git commit using a temporary index and raw blobs:

- Commit: **`eb860bc572ad112d2b06d08697eb711110f1032c`**.
- Ref: `refs/archive/awakening-demo-pre-stage34-20261113t175527z`.
- Tree: `46127b59cf5b74c11498bb5356ed9626eb0534b6`.
- Includes all2038 pre-retirement files under Assets/Packages/ProjectSettings, including ignored scene recovery files and matching metadata. Every blob was read back and SHA256-compared before deletion.
- Includes the manifest's external MediaPipe0.16.3 tarball at archival path `RecoveryExternal/com.github.homuler.mediapipe-0.16.3.tgz`.
- Complete verified bundle: `D:/Coding/Unity/Archives/AwakeningCelebrations/PreStage34-20261113T175527Z/AwakeningDemo-PreStage34.bundle`.
- Bundle SHA256: `cfc576629e810601eb31f6f62f0254bfa3f2e5ca063612a62573a4b52d74a6d6`.

[RecoveryManifest.json](RecoveryManifest.json) and [archive procedure](create_recovery.py) record verification. This is an explicit recovery archive of current application dependencies, not a silent commit of unrelated user work to main. Backend secrets/unrelated untracked documents were excluded from the new archive overlay; their original working files remain untouched. Main HEAD and staged work were preserved.

## 3. Dependency evidence and classifications

Inspected scene components/serialized fields, prefab and ProjectSettings dependencies, native AssetDatabase GUID/dependency resolution, source callers, Resources loading, reflection/tooling references, assembly/package ownership, font fallback, all-six art/video mappings, media lifecycle, consent/MediaPipe integration and assistant configuration. Native dependency discovery alone was not treated as proof that C# code or Resources were unused.

Evidence: [native graph before](NativeGraphBefore.json), [after code migration](NativeGraphAfterCode.json), [final native graph](NativeGraphAfter.json), [dynamic/tooling references](DynamicAndToolingReferencesAfter.json), [final classifications](FinalDependencyClassification.json), [45-path primary closure](LivingGlassDependenciesAfter.txt), [source member plan](SourceRefactorPlan.json) and [method review](MethodReview.diff). The final primary closure matches the post-code/pre-asset-retirement closure exactly. No retained native asset points to a retired path. Compiled source no longer contains the retired presentation type/catalog dependencies.

Classifications use ACTIVE_LIVING_GLASS, SHARED_RUNTIME, REQUIRED_EDITOR_OR_BUILD, LEGACY_ONLY, HISTORICAL_DOCUMENTATION and UNRESOLVED. Required sample integration, unique media, historic screenshots and uncertain unreferenced resources remain retained. A zero incoming count was never sufficient for deletion. Classification counts include historical/retired ledger rows, so they are not the count of active application assets.

## 4. Exact retirement ledger

**458 physical imported files were retired or migrated, including230 `.meta` files.** Six physical files were added (three new assets and their metadata); ten existing files changed. The exhaustive [CSV ledger](RemovalLedger.csv) and [JSON ledger](RemovalLedger.json) identify every exact path, original GUID where present, hash, reason, native incoming evidence and recovery commit/path. [FileDelta.json](FileDelta.json) separately lists all additions/modifications/removals; no numerical target drove deletion.

| Dependency-coherent batch | Outcome and evidence |
| --- | --- |
| `Assets/Scenes/SampleScene.unity` + metadata | REMOVED — validated statically. Explicitly authorized; independent new scene already contained the rig, services, clips and panel. |
| Four imported demo recovery snapshots `Assets/_Recovery/0.unity`, `0 (2).unity`, `0 (3).unity`, `0 (4).unity` + metadata | MIGRATED — byte-validated to [HistoricalRecovery](HistoricalRecovery/), also archived at original Git paths. These user recovery snapshots are retained as history outside Unity imports. |
| `Assets/Figma/` plus root metadata | REMOVED —410 physical descendant files plus root meta. Original104 UXML screens, generated USS, manifests/mappings and export-only assets have no remaining consumer. Original performer art under `Assets/Art/` was not removed. |
| Six fonts: NotoSansSC/NotoSerifSC TTF and SDF; SourceSans3/SourceSerif4 SDF | REMOVED — export-only. The Latin TTF originals, new TC font/text settings and any font subassets owned by Living Glass remain. |
| `Assets/UI Toolkit/PanelSettings.asset` + meta | REMOVED — original panel only. Living Glass panel retained; shared `UnityDefaultRuntimeTheme.tss` retained. |
| `Assets/Resources/RuntimeScreenCatalog.asset`, `RuntimeNodeBindings.json`, `HomeInteraction.uss`, `AssistantChatRuntime.uss` + metadata | REMOVED — their legacy controllers/loaders were retired together; active Resources/service loading was checked. |
| `Assets/Settings/Pipeline/` | REMOVED — only importer-owned EditorPipelineManager configuration and folder metadata, after all consumers ended. |

Assets were removed through Unity's asset deletion API, with matching metadata. The temporary source-import pause was closed in `finally`; no import pause remains. No imported asset GUID was reassigned.

## 5. Scene retirement and serialized migration

Old scene GUID `8c9cfa26abfee488c85f1582747f6a02` is retired. Primary scene GUID `12792cd3e2cf3c84b854c8186ea81f3a` is unchanged. The new scene was not recreated, cloned again or broadly reserialized. Its only text changes remove three already-null controller fields: `screenDocumentManager`, `screenCatalog`, `homeController`. [SceneFieldMigration.json](SceneFieldMigration.json) and the static exact-diff check prove this; clips, rig, controller GUID, consent configuration and timing values are unchanged.

No live camera, navigation, video or assistant owner was migrated from another scene or duplicated. The primary scene already had its own single application rig. Native inspection confirms one flow controller, one gesture bridge, one UIDocument, all-six clips and no missing scene scripts.

## 6. Development startup entry

`EditorBuildSettings.scenes` now contains only enabled `Assets/Scenes/AwakeningLivingGlass.unity`, resolving its original GUID. Changes were made with the Editor API and saved; the final check also verifies the on-disk file, not only the in-memory setting. Unrelated `m_configObjects` entries for App UI and input actions are unchanged. Other ProjectSettings are byte-preserved. **Development-entry cutover is complete; no Windows build or release cutover qualification occurred.**

## 7. Figma importer decision

Removed `com.unity.ui.figma` through Unity Package Manager `Client.Remove`, after proving that the export/catalog tooling and pipeline configuration were its last project consumers. UPM reported Success and removed only `com.unity.ui.figma` and its exclusive `com.unity.pipeline` dependency. No package was added or version-changed. See [PackageDelta.json](PackageDelta.json) and [UPM outcome](FigmaPackageRemoval.json).

`com.unity.dt.app-ui` remains because `com.unity.ai.inference` requires it. MediaPipe stays0.16.3 at its original local package source; URP remains17.4.0; Unity remains6000.4.7f1. No other dependency chain was removed, and no manual manifest editing was used. Historical Figma provenance and Stage2 prototypes/specifications remain in Documentation.

## 8. Legacy C# retired

The following six scripts and matching metadata were removed after caller/serialized/tooling checks:

- `Assets/Scripts/HomeLanguageController.cs`.
- `Assets/Scripts/ScreenDocumentManager.cs`.
- `Assets/Scripts/RuntimeScreenCatalog.cs`.
- `Assets/Editor/FigmaRuntimeMappingValidator.cs`.
- `Assets/Editor/Phase4RuntimeScreenshotCapture.cs`.
- `Assets/LivingGlass/Editor/LivingGlassSceneBuilder.cs` — obsolete one-time clone/generation workflow, no longer appropriate after demo retirement.

The original controller MonoBehaviour was **retained and consolidated**, not replaced or renamed. Imported sample integration scripts remain available. Full script GUIDs are in the removal ledger.

## 9. Consolidation and code smells addressed

| Problem and concrete impact | Smallest change and preserved contract |
| --- | --- |
| Main controller selected between two unrelated presentation paths, retaining legacy fields, overlay stack, callbacks and binding snapshots | Requires the existing Living Glass view; removes37 obsolete presentation/helper methods and legacy-only state/events. Same serialized component and active flow authority. |
| Private wrappers dispatched to the Living Glass branch but carried dead demo bodies | Active commands call the same Living Glass operation directly. `LivingDone` still reaches `LivingGlassHome(true)`; ShowHome public signature remains and preserves its Living Glass behavior. |
| Duplicate performer/stage mapping and custom array search | Controller aliases canonical LivingGlassRoutes arrays and delegates pure chapter mapping. Golden104 locale pairs and30 chapters verify the unchanged vocabulary/order. |
| Legacy-only assistant screen ID/placeholder state and view-active fallback obscured request ownership | Removes unused state and the resulting unused private chat-binding argument at all three callers; view-active check retains the exact prior Living Glass condition. Session generations, transport, cancellation and late-response protection remain. |
| Catalog language enum/build tooling coupled new scene to retired imported assets | Moves explicit English0/Chinese1 enum into Core; retains exact golden route data in independent Editor fixture. Adds read-only primary-scene/prebuild validation. |
| Preview and bounded-test utility referenced obsolete scene builder | References independent scene/asset constants; authorization guards retained. No runtime harness execution. |

The reviewed20 changed method declarations primarily remove dead branches/wrappers;62 method declarations are text-identical, including critical gesture, consent, video, assistant, audio and disposal methods. See [MethodPreservation.json](MethodPreservation.json), [Roslyn before](SourceMembersBefore.json), [after](SourceMembersAfter.json) and [diff](MethodReview.diff). The current-source span check prevents stale AST evidence from being mistaken for proof.

No meaningful lifecycle was simplified merely because it looked complicated. The controller still owns working media/request/audio lifecycles; further decomposition without authorized runtime validation was intentionally deferred. No speculative performance optimization, architecture framework, new package or public/serialized rename was added.

## 10. Shared services and behavior boundaries

All `Assets/Art`, StreamingAssets, imported sample sources/configuration and `Assets/Scripts/Interaction` files are unchanged against this phase's baseline. This includes prior user bridge/runner edits and Stage3.3B metadata corrections. All-six videos and importer metadata remain intact. Seven backend/config files are hash-preserved without exposing credentials. Living Glass presentation, motion, glass, assets, theme data, TC font, composer/transcript and illustration files are unchanged.

Spatial B bounds, stable hitboxes, gesture thresholds, fist confirmation, first awakening, cinematic durations, camera eligibility/consent, video completion/error/cancellation and request context/late replies were not redesigned. Static preservation is evidence of controlled scope, **not proof of post-cleanup runtime equivalence**. User-reported pre-cleanup operational success remains a behavior requirement rather than this phase's test result.

## 11. Active architecture

See [Architecture.md](Architecture.md) for the concise responsibility/lifetime map. One flow/session/controller authority commands one reusable presentation; route vocabulary is independent of the retired Figma catalog. The existing gate/runner remains the camera owner, the controller retains one decoder/request/audio lifecycle, and the view/motion system retains focus/visual-tree/resource ownership. There is no alternate demo UI path.

## 12. Measured repository statistics

Source: [Statistics.json](Statistics.json), baseline hashes, native graphs and raw recovery-tree files. Counts exclude Library/Temp/node_modules; the broad scan includes the embedded MCP package. Physical retirement size is41,555,592 bytes, including historical recovery migration; this is not a Player memory or build-size measurement.

| Measure | Before | After |
| --- | ---: | ---: |
| Physical files in Assets/Packages/ProjectSettings |2038|1586|
| Imported non-folder Assets graph entries |591|368|
| First-party C# files, excluding imported Samples and Packages |22|18|
| C# files under Assets, including MediaPipe samples |73|69|
| Controller lines, raw splitlines |3056|1544|
| Assets UXML layouts |104|0|
| Assets USS files |100|1|

The full scan has396→392 C# files and101681→98929 lines because embedded MCP code is included. Package UXML/USS remain; zero application UXML is expected because Living Glass composes native reusable screen families in C#. Six legacy scripts were removed and two C# assets added. There are no claimed measured runtime/performance improvements.

## 13. Validation batches and results

| Gate | Result | Evidence and limitation |
| --- | --- | --- |
| Recovery/raw blob/bundle verification | PASS | [manifest](RecoveryManifest.json); before destruction. |
| Post-code compilation and configuration | PASS after correction | Two Editor harness references to deleted SceneBuilder initially produced CS0103; corrected to independent validator constant and recompiled before advancing. [correction](CodeMigrationCompileCorrection.json). No other code-error workaround. |
| Post-code/post-asset pure and scene checks | PASS | [code batch](CodeMigrationChecks/), [asset batch](AssetRetirementChecks/). |
| Final Unity compilation | PASS | `scriptCompilationFailed=false`, `isCompiling=false`; [EditorState](FinalChecks/EditorState.json). All compiled assets/packages, no Player build. |
| Current static regression gate | PASS —34 checks | [Validation.json](Validation.json), [checker](verify_current.py). Covers exact scope, GUID/meta/scene graph, persisted startup, locale/glyphs, service methods, protected files, package delta and isolated results. |
| Route/motion/crop contracts | PASS |52 routes/104 locale records,30 chapters, invalid IDs, interruption/cancellation/reduced final pose; [pure results](FinalChecks/PureContractChecks.txt). |
| Native primary scene references | PASS | [serialized results](FinalChecks/SerializedValidation.txt). |
| Conversation isolated Editor cases | PASS |48 six-performer/locale/viewport layouts +20 state specimens; [CSV](FinalChecks/ConversationRefinement/LayoutChecks.csv). No real reply, IME or backend verification. |
| Stabilization isolated Editor checks | PASS |1001 motion-ownership samples;12 performer/locale interruption/tree checks; bilingual scroll/style and reduced views. [results](FinalChecks/Stage31Stabilization/EditorChecks.txt). No runtime input/media. |
| Strict raw GUID resolution | FAIL — pre-existing five | Exact identities/owners below; no suppression or arbitrary replacement. Checker intentionally exits1 while these remain. |
| Newly introduced missing-reference regression | PASS — none found | Raw scan plus native resolution; five outside primary closure. |
| Post-retirement runtime/visual/camera/live-service/Player/performance | NOT RUN | Separate authorization required. |

The old24-check script assumed SampleScene/catalog/package preservation and is now a historical contract, saved intact under `Before/Documentation/Stage3LivingGlass/verify_static.py`. The active `Documentation/Stage3LivingGlass/verify_static.py` delegates to the current34-check gate and preserves the strict raw FAIL/nonzero exit. Equivalent route/media/protected-source/font checks remain, with fresh native evidence and an unchanged pre-retirement golden fixture. Historical reports were copied before current checks and original isolated-check outputs restored, rather than overwriting historical test claims. This is a scope migration of validation, not a GUID filter.

## 14. Console findings

[ConsoleSnapshot.json](FinalChecks/ConsoleSnapshot.json) records the full post-package compilation:14 CS0618 warnings, comprising eight in unchanged embedded MCP compatibility helpers, four in unchanged MediaPipe sample sources and two in preserved controller video-style operations. The [final incremental compile snapshot](FinalChecks/ConsoleFinal.json), after removing the unused private chat argument, has only the two controller warnings, the existing Unity Editor AI `NoSubscription` error and one MCP WebSocket initialization warning. Two earlier MCP disconnect warnings are retained in the package snapshot; connection recovered. No final C# compilation errors were observed. Application runtime was not started, so runtime error absence is not an acceptance result. Editor package background diagnostics must not be confused with application assistant requests.

## 15. Pre-existing GUID issues carried forward

| GUID | Owner/property | Classification and active significance |
| --- | --- | --- |
| `5a0e103d2920f990ab08fe7bada48cc4` | MediaPipe `UI/Objects/Global Config Window.prefab`, `m_Script` | Historical sample support: missing original MonoScript. Outside primary closure; exact intended source unknown. Retained, no arbitrary replacement. |
| `d5da564da19cb6b7d8e4f97f269edc5d` | MediaPipe `Scenes/Legacy/MediaPipe Video/MediaPipe Video.unity`, `m_Script` | Historical sample support: incompatible with substituting current texture pool class. Retained. |
| `b3ab972b21db85d48ade9657efdd4771` | `Assets/Settings/Lit2DSceneTemplate.scenetemplate`, `dependencies[0]` | Editor template missing texture; neither primary scene nor release-path proof. Retained pending isolated provenance experiment. |
| `c41005c129ba4d66911b75229fd70b45` | `Assets/TextMesh Pro/Resources/TMP Settings.asset`, `m_defaultSpriteAsset` | Optional missing TMP sprite default; primary UI does not use it. Settings left unchanged. |
| `8236d46ca3a43d373ae6ad281fea8fe0` | Only `Assets/_Recovery/0 (1).unity` and `1.unity`, old `contents` override | Obsolete inactive serialized override, not current `_contents`; these MediaPipe recovery files remain byte-preserved. |

Compare [raw reference sites](RawGuidReferencesAfter.json), [native resolution](NativeGuidResolutionAfter.json) and [carried classifications](CarriedGuidFindings.csv) with Stage3.3B evidence and Stage3.3A [findings](../GUIDInvestigation/GuidFindings.csv). No identity/classification was silently retired or reclassified as resolved. Four genuine support/optional issues and one obsolete recovery reference remain; zero confirmed active-app broken references among this set.

## 16. Recovery instructions

Prefer recovery in a **new sibling project**, not restoring selected legacy owners into the current project. Verify bundle SHA256 against the manifest, clone the verified bundle into a new empty folder and check out archival commit `eb860bc572ad112d2b06d08697eb711110f1032c`. The nonstandard `refs/archive/…` ref is listed in the bundle; explicitly fetch that ref if the clone does not create a checkout. Preserve matching metadata. Do not merge or reset current user work.

The snapshot includes pre-retirement Assets/Packages/ProjectSettings and tracked history. Restore its archived MediaPipe tarball to the original local path if absent, or deliberately update only that recovered copy's local package source. Library/Temp and package caches regenerate; other remote packages still require their recorded sources. Backend secrets and unrelated untracked documents were not added to the archive; retain the unchanged original backend outside this restoration. Full operational restoration is NOT RUN, so archive/bundle integrity must not be represented as a tested runtime rollback.

For an individual file inspection use `git show <recovery-commit>:<ledger recovery_path>` or a Git client, then write the exact blob/metadata to a separate review folder. Avoid PowerShell text redirection for binary assets/byte-exact restoration. Never use destructive reset/clean. Source/config backups in `Before/` and four exact `HistoricalRecovery/` scene pairs are also available, but full dependency recovery should use the coherent archival tree.

## 17. Remaining debt and retained uncertainty

- Controller still has several coherent but coupled flow/media/request/audio lifecycles. No broad rewrite was justified without runtime regression permission.
-100 retained imported graph rows are UNRESOLVED for deletion eligibility, not100 broken active references. Unique artwork/vendor samples/resources are retained rather than speculatively removed.
- The five support/recovery GUID issues remain accurately visible. No further sample/template/TMP deletion or repair was necessary to retire the demo.
- Existing CS0618/Editor AI diagnostics remain; changing proven video style or third-party code just to eliminate warnings was out of scope.
- Project skills and historical documents contain old inventory leads; historical markers/current architecture links clarify that current source is authoritative. No design history was erased.

## 18. Deferred validation and next stage

Next is a **separately authorized bounded post-cleanup operational smoke pass** in AwakeningLivingGlass: startup/sole scene, full first-awakening/return journey, six performer selections, en/zh changes, gallery/focus/protected transitions, Settings/AI modal input/focus, conversation composer/scroll/IME, actual video prepare/completion/cancel/error and clean shutdown/re-entry. First run offline with capture denied and assistant transport isolated, after inspecting the harness. It must not infer gesture/backend success from commands or fixtures.

Camera consent/single-owner recognition testing and live assistant request/failure/retry/cancel/late-response checks each need their own explicit hardware/network scope. Windows Player acceptance, DPI/window/fullscreen accessibility, final visual comparison and performance/memory measurement remain subsequent gates. Only then assess production release. Speech, guide, performer AI knowledge, backend expansion, Souvenir and new Settings functionality remain unimplemented and separately authorized future work.

## 19. Completion decision

**REMOVED — statically validated:** original demo, exclusive exports/panel/fonts/catalog/styles, six legacy/tooling scripts, importer and its exclusive pipeline. **REFACTORED — statically/isolated-Editor validated:** single active flow presentation path, canonical routes/enum, independent validation tooling. **RETAINED — required or uncertain:** performer resources, integration/services, user history, package dependencies and all five classified issues. **NOT RUN / requires fresh permission:** post-cleanup runtime, hardware, live service and release qualification.

Stage3.4 cleanup and development-entry transition are complete at the permitted static/isolated-Editor level. The project is ready for the next bounded operational acceptance task; it is **not yet qualified for final Windows production release**. No voice integration, further deletion, runtime session or production-release testing was started automatically.
