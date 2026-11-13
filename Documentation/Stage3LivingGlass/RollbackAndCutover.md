> **Historical snapshot — superseded for the active repository by [Stage 3.4](Cleanup/FinalLegacyRetirement/Report.md).** SampleScene operational rollback and imported-demo dependency statements below describe the earlier phase. Recovery now uses the verified archival commit/bundle; Living Glass is the sole development entry. Original findings are retained as historical evidence.

# Rollback and cutover

The production build entry still points to `Assets/Scenes/SampleScene.unity`. The parallel scene is not added as a production entry. Returning to the fallback scene does not require deleting or migrating assets.

For presentation rollback, open SampleScene as a single scene in Edit Mode. Its `livingGlass` reference is absent, so the existing Home/documents/imported UI path is selected. Do not load both scenes additively. Operational parity remains unverified until authorized tests run.

For a complete source rollback after reviewing later edits, restore these three files from the captured text backups under `Baseline/`, preserving their existing `.meta` files:

- `AwakeningApplicationController.cs.txt` → `Assets/Scripts/AwakeningApplicationController.cs`.
- `MediaPipeGestureBridge.cs.txt` → `Assets/Scripts/Interaction/MediaPipeGestureBridge.cs`.
- `HandLandmarkerRunner.cs.txt` → its existing MediaPipe sample source path documented in `SceneAndSharedChanges.md`.

The LivingGlass partial-class file also contributes to the controller. For full rollback, close Unity, archive/move the complete new `Assets/LivingGlass` tree with its matching metadata and new scene/metadata outside `Assets`, then restore the three source backups together. Reopen SampleScene and compile. Preserve that archive for recovery; do not delete original dependencies or edit GUIDs. Prefer the reviewed complete change set over restoring one source file in isolation. Do not erase pre-existing untracked Stage1/Stage2/skills files recorded in `BaselineGitStatus.txt`.

No cleanup is executed. Later cleanup candidates include legacy Home path mappings, ScreenDocumentManager, imported navigation binders, old UXML/USS, RuntimeNodeBindings, compatibility fields and obsolete screen helpers. Their dependencies remain required by SampleScene. Cleanup requires a dependency report, passed parity evidence, approved rollback retention and separate destructive-cleanup authorization. Service extraction should retain one operational owner and preserve the tested media/assistant contracts.

Cutover checklist:

- [ ] User accepts the visual direction as rendered in Unity, including optical tilt compromise and TC typography.
- [ ] Full new-scene journey and all six chapters pass in both locales.
- [ ] Original-scene shared-code regression passes.
- [ ] Camera consent/preview/suspension/failure/recovery and single-owner tests pass.
- [ ] Existing gesture semantics and stable targeting pass at Windows DPI/fullscreen settings.
- [ ] Video preparation, real completion, cancellation and configured error/missing-clip behavior pass.
- [ ] Assistant service, failure, retry, cancellation and late-response checks pass.
- [ ] Keyboard, focus restoration, IME, reduced modes and accessibility trials pass.
- [ ] Authorized performance/memory measurements support the agreed hardware target.
- [ ] Deployment privacy notice/backend-retention policy and visitor session-reset policy are resolved.
- [ ] User separately authorizes the production build entry switch.
- [ ] Any destructive cleanup is separately approved after dependency evidence.

No check above is approved merely because a new scene exists or a Figma frame was approved.
