> **Historical snapshot — superseded for the active repository by [Stage 3.4](FinalLegacyRetirement/Report.md).** SampleScene operational rollback and imported-demo dependency statements below describe the earlier phase. Recovery now uses the verified archival commit/bundle; Living Glass is the sole development entry. Original findings are retained as historical evidence.

# Stage 3.3 cleanup report

**Stage3.3B follow-up:** [Targeted reference retirement](LegacyRetirementReport.md) removed15 obsolete reference occurrences in eight files. Current raw unresolved GUIDs are5, with four known support issues and one retained recovery-only obsolete GUID. Both application scenes and their dependency graphs remain preserved; no whole file or C# code was deleted. Results below describe the earlier cleanup snapshot.

**Stage3.3A follow-up:** [All14 GUIDs are now classified](GUIDInvestigation/Report.md): ten obsolete compatibility entries and four genuine missing sample/tooling/optional dependencies. No active Living Glass defect was confirmed among them. The original strict raw-resolution FAIL below is preserved as historical evidence; no asset repairs or deletions were performed.

**Scoped cleanup completed; repository-wide clean acceptance has an existing GUID-resolution failure and runtime gates remain open.** Two small code refactors were validated. No files were proven eligible for deletion, so none were removed. No production cutover, architecture migration, feature implementation or visual redesign occurred.

Start with [dependency inventory](DependencyInventory.md), [removal decisions](RemovalCandidates.md), [protected dependencies/recovery](ProtectedDependencies.md), [source ownership](SourceOwnership.csv) and [exact patch](ScopedChanges.patch).

## Baseline and existing work

Branch main; commit b9f0972b38008b31e4bd2117ee88cb34ef9ce6fe. Initial tracked changes already existed in AwakeningApplicationController.cs, MediaPipeGestureBridge.cs and the sample HandLandmarkerRunner.cs. Project guidance, Living Glass code/assets/scene and Stage1/2/3 documents were already untracked. All were treated as existing user work.

The [initial status](BaselineGitStatus.txt), [commit](BaselineCommit.txt), [initial tracked binary patch](WorkingChanges.patch), [untracked archive](UntrackedBaseline.zip) and2045-file hash inventory provide recovery evidence. The archive has973 entries,446365022 bytes and passed CRC verification. Ignored Assets recovery/capture files were hashed and retained in place; they are not claimed as archived. This local recovery archive is separate from Unity runtime assets and should be reviewed before any source-control publication.

Unity was in Edit Mode with the clean candidate scene open. The original scene remained the sole enabled build entry. Baseline static checks passed24 checks/104 route-locale rows. Existing Console observations included the Editor AI NoSubscription exception and a GPU-inference fallback warning. Baseline compilation refresh returned to idle; there was no C# compiler error observed. No Console clearing hid previous evidence.

## Dependency and removal outcome

Native dependency inventory:591 imported asset files; physical inventory:593; original/candidate closures274/45 with36 shared paths, including packages. Additional ProjectSettings roots, source/assembly/reflection/resource/tooling paths and hidden manifest provenance were examined.154 assemblies and57 direct package declarations/73 lock entries were recorded.

Final classification:22 active,141 shared,391 rollback,39 unresolved physical asset files.252 zero-native-incoming non-scene entries were triaged. All22 first-party source files have an ownership disposition;51 imported sample C# files remain integration/supporting workflows. Six byte-identical screenshot groups were found and retained as historical evidence.

**Removed files: none.** [RemovalLedger.json](RemovalLedger.json) is empty. Older imported UXML/USS and presentation controllers remain required by SampleScene and its mapping/prebuild workflow. Samples, font/shader infrastructure, dynamic Resources and StreamingAssets are not obsolete solely because a scene graph has no incoming edge. Recovery scenes and manifest backups lack independent disposal proof. All39 unresolved assets and uncertain packages are retained, not described as completed removals.

## Exact corrective changes and ownership improvement

| Changed file | Change / expected behavior |
| --- | --- |
| Assets/Scripts/AwakeningApplicationController.cs |Removed the private nonserialized write-only interactionStylesAdded field and assignment-only conditional. Null guards and stylesheet addition remain identical. No flow, session, consent, gesture, media, request or locale behavior changed. |
| Assets/LivingGlass/Editor/LivingGlassPreviewWindow.cs |Added two internal getter-only properties returning the existing presentation and cloned preview data. Ownership, initialization and destruction remain in this Editor window. |
| Assets/LivingGlass/Editor/LivingGlassConversationChecks.cs |Uses typed preview properties instead of two string field-name reflection lookups. Runtime-flow-null guard retained. |
| Assets/LivingGlass/Editor/LivingGlassStabilizationChecks.cs |Same typed seam replaces two reflection lookups; isolated lifecycle checks remain intact. |

The shared controller remains the single semantic/service authority for both applications. Living Glass UI, motion, themes and rendering already have distinct owners; no second transport/player/capture/router was introduced. No shared-service extraction was performed. Original-scene compatibility is intentional until parity/cutover approval. The presenter and service facade remain maintainability candidates; their necessary cancellation/serialization behavior was not simplified without evidence.

The exact diff check proves these four changes against stage-specific source backups, preserving previous user edits.2041 of2045 hashed files are unchanged; no additions/deletions in Assets, Packages, ProjectSettings or backend. Native asset paths, GUIDs and dependency edges are identical before/after. Every metadata file is unchanged.

## Validation performed

| Check | Status | Actual result / evidence |
| --- | --- | --- |
| Unity Editor compilation and reload |PASS |Scripts refreshed/compiled; Editor returned idle; updated checker code executed. No C# compilation errors. [Editor state](EditorStateAfter.json), [Console](ConsoleAfter.json). |
| Existing repository checks |PASS |24 checks; protected methods, glyph coverage, metadata, media-slot order, gesture diagnostics and104 coverage rows. [Results](Validation/StaticVerification.json). |
| Route/locale/performer/motion contracts |PASS |104 catalog records/52 bilingual routes;30 chapters; invalid routes rejected; interrupted/cancelled/reduced tracks and crop contract. [Pure checks](PureContractChecks.txt). |
| Original Figma mapping/prebuild logic |PASS |Read-only ValidateCurrentProject invocation returned zero errors. No build triggered. [Result](FigmaMappingChecks.json). |
| Candidate serialized scene |PASS |One flow owner/bridge/UIDocument, required references, six media slots, catalog support and no missing component scripts in active candidate. [Report](Validation/SerializedValidation.txt). |
| Both scenes' script GUIDs |PASS |Eight distinct original and seven candidate script GUIDs all resolve. [Evidence](SceneScriptReferences.json). No original-scene runtime test. |
| Conversation layout/state specimens |PASS, Editor only |48 performer/locale/viewport combinations plus20 isolated state specimens. No real assistant responses or IME verification. [Checks](Validation/ConversationRefinement/EditorChecks.txt), [rows](Validation/ConversationRefinement/LayoutChecks.csv). |
| Stabilization checker |PASS, Editor only |1001 visibility samples; six performers × two locales; repeated/interrupted/overlay/locale/reduced transitions, cleanup and scroll style/Home/End specimens. [Results](Validation/Stage31Stabilization/EditorChecks.txt). Not a new Game view fidelity pass. |
| Cleanup-specific preservation |PASS |10 checks: exact four-file scope,2041 unchanged files, native graph parity, unique meta GUIDs, UI URL/path integrity and scene script GUIDs. [Machine results](CleanupStaticChecks.json). |
| Complete external GUID resolution |FAIL, pre-existing |14 of261 non-local GUIDs do not resolve;247 resolve. No affected serialized/importer/package file changed. See finding below. The verifier intentionally exits1 rather than hiding this failure. |
| Preview teardown |PASS, Editor only |Zero preview windows/owned LivingGlassStaticPreview objects after check completion; clean active scene; Play Mode false. No runtime resource-release claim. |
| Original scene/build/packages/media/backend preservation |PASS |Baseline/after hashes and exact source scope agree; no deleted dependencies. |
| Play Mode / visitor runtime parity |BLOCKED — requires additional permission |No fresh authorization for this phase; not entered. |
| Camera/MediaPipe recognition/live AI |NOT RUN |No hardware initialization, consent grant, live assistant request or backend test. |
| Player builds/performance/release |NOT RUN |No build or profiling; no production-ready/60fps claim. |

Existing test utilities were inspected before execution. Pure checks load catalog data and exercise isolated models; preview checks create only a hidden unsaved presentation owner plus cloned data and assert no flow owner. They do not initialize the application, recognizer, player or transport. They write documentation artifacts only. Scene creation, importer mutation, runtime capture harness and Play Mode tools were not invoked.

Rerun artifacts were copied into Cleanup/Validation and previous generated evidence restored byte-for-byte from HistoricalEvidenceBefore. No new screenshots were captured and no Figma visual-fidelity claim is inferred from these checks.

## Existing failures and remaining risk

Fourteen unresolved GUIDs are listed exactly in [ExternalGuidResolution.json](ExternalGuidResolution.json), with reference sources in [SerializedGuidReferences.json](SerializedGuidReferences.json). They were detected before edits; unchanged asset/importer/package hashes establish they were not introduced by cleanup.

| Group | Count | Exposure / disposition |
| --- | --- | --- |
| Renderer2D debug/probe resources and falloff lookup |7 |ProjectSettings reaches this renderer. Legacy serialized data versus current required resource behavior must be investigated; do not remove fields or assume harmlessness. |
| Bootstrap.cs.meta default prefab references |2 |Importer defaults are unresolved; actual scene bootstrap configuration and hardware behavior are not established by this pass. Preserve. |
| Sample prefab override reference |1 |Imported sample scenes; future sample-workflow repair scope. |
| Global Config Window and legacy MediaPipe Video script GUIDs |2 |Serialized script references do not resolve. Neither preserved scene's own component-script scan reports missing scripts. Sample workflow impact remains untested. |
| Scene-template resource and TMP default sprite |2 |Editor/font infrastructure; future targeted integrity review. |

Final Console contains four CS0618 warnings at existing FindObject(s)OfType/unityBackgroundScaleMode call sites, the pre-existing Editor AI NoSubscription exception, and an MCP WebSocket reconnect warning during reload. None is a new application compiler error. Existing obsolete API calls were not replaced casually because sorting/font/layout behavior could change. The NoSubscription issue belongs to installed Editor tooling, not a tested visitor assistant backend. [Baseline Console](ConsoleBaseline.json) and [after Console](ConsoleAfter.json) are retained.

Full camera, live assistant, performance, release acceptance and the latest conversation runtime/IME checks remain unverified. Earlier bounded validation is historical evidence, not authorization or regression proof for this stage.

## Major cleanup item status

| Item | Final category |
| --- | --- |
| Dead private write-only state |REFACTORED — validated (compilation/static; runtime not run) |
| Editor preview/checker dependency seam |REFACTORED — validated (compilation and isolated Editor checks) |
| Scenes, original UI/legacy compatibility, single service owners, media and design history |RETAINED — required |
| File removals |No REMOVED — validated entries; none met proof requirements |
|39 unresolved assets, uncertain packages, further facade/presenter extraction |DEFERRED — insufficient evidence |
| Repository-wide external GUID integrity |FAILED — validation did not pass;14 pre-existing unresolved GUIDs |
| Runtime parity and hardware/service validation |BLOCKED — requires additional permission |

## Rollback

Restore only the four changed source files from Before/ at the same relative paths after reviewing newer edits; keep existing meta files. Recompile in idle Edit Mode and rerun the inspected static/Editor checks. ScopedChanges.patch shows the complete cleanup delta. Do not reset/clean the repository, revert prior stages wholesale, overwrite all untracked work, or change the build entry. The initial commit, WorkingChanges.patch and973-entry archive provide wider recovery references. No asset-deletion rollback is required because no file was removed.

## Prioritized next actions — not executed

1. Investigate the14 existing GUID failures in a separate targeted integrity task. Confirm renderer and bootstrap fields' actual role before any asset/package correction; preserve the original application throughout.
2. Obtain fresh bounded runtime authorization if parity testing proceeds. Proposed offline plan: candidate scene up to6minutes for bilingual Home/gallery → Focus/return, rapid Settings/AI dismissals and latest conversation scrolling/keyboard behavior; original scene up to2minutes for Home/privacy/tutorial/gallery and overlay navigation. Inspect setup first, deny consent, disable capture initialization, isolate live assistant transport, stop at the agreed total bound and exit cleanly. No hardware, live backend, Player build or profiling. No existing permission is reused.
3. Establish camera/live AI/performance acceptance under their own explicit scope before considering cutover or legacy dependency deletion.
4. After parity evidence, evaluate one lifecycle extraction at a time with an impact/serialization plan and focused contracts. Do not split the shared controller merely to match a target diagram. Further removals need new per-file independence proof.
5. Keep speech/guide and all deferred feature functionality in separately authorized work after integration/acceptance. This cleanup implemented none of them.

Work stops at this handoff. The safe cleanup changes are validated within permitted scope; complete clean-reference/release acceptance is not claimed.
