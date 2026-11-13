# Stage 3.3A — GUID investigation and integrity baseline

**Historical snapshot:** The subsequent [Stage3.3B retirement report](../LegacyRetirementReport.md) records intentional changes to eight files and a current raw count of5. The14-reference investigation and frozen evidence below remain unchanged as provenance; rerunning this stage's hash-bound checker now correctly detects that its baseline has changed. Use the new report for current retirement validation.

**All14 references were traced. Ten are obsolete compatibility-data entries; four are genuine missing sample/tooling/optional dependencies. No active broken Living Glass reference was confirmed among these14. No assets, scenes, importers, packages, runtime code or build configuration were changed.** Runtime functionality was not tested.

The old strict raw-resolution FAIL remains historically correct: all14 GUIDs still fail AssetDatabase lookup. Treating that result as14 active application failures was unsupported. This investigation adds field, ownership and impact evidence without hiding real missing resources.

## Exact14-reference evidence table

[Full JSON](GuidFindings.json) and [CSV](GuidFindings.csv) contain each GUID's referencing files/line numbers, property, expected type, origin, scene scope, obsolete status, native resolution, impact, repair prerequisite and evidence links. [SerializedOccurrences.json](SerializedOccurrences.json) contains every occurrence, including six sample and two recovery scenes for the old override.

File aliases below are exact repository paths:

- R: `Assets/Settings/Renderer2D.asset`.
- B: `Assets/Samples/MediaPipe Unity Plugin/0.16.3/Official Solutions/Common/Scripts/Bootstrap.cs.meta`.
- S: six sample scenes under `Assets/Samples/MediaPipe Unity Plugin/0.16.3/Official Solutions/Scenes/` (Face Detection, Face Landmark Detection, Hand Landmark Detection, Image Segmentation, Object Detection, Pose Landmark Detection) and `Assets/_Recovery/0 (1).unity`, `Assets/_Recovery/1.unity`. Exact paths in the JSON/CSV.
- G: `Assets/Samples/MediaPipe Unity Plugin/0.16.3/Official Solutions/UI/Objects/Global Config Window.prefab`.
- V: `Assets/Samples/MediaPipe Unity Plugin/0.16.3/Official Solutions/Scenes/Legacy/MediaPipe Video/MediaPipe Video.unity`.
- T: `Assets/Settings/Lit2DSceneTemplate.scenetemplate`.
- M: `Assets/TextMesh Pro/Resources/TMP Settings.asset`.

| GUID | Referencing file / serialized property | Expected asset type | Classification | Application significance |
| --- | --- | --- | --- | --- |
| `03cfc4915c15d504a9ed85ecc404e607` | R: probeVolumeResources.probeVolumeFragmentationDebugShader | Shader | Validator false positive | Both via renderer; obsolete/debug migration field, not current rendering input. |
| `24ec0e140fb444a44ab96ee80844e18e` | R: probeVolumeResources.probeSamplingDebugTexture | Texture2D | Validator false positive | Both via renderer; obsolete/debug migration field, not current rendering input. |
| `488fbb6a3005b66c2b657204bce83fcf` | B: MonoImporter.defaultReferences.globalConfigManagerPrefab | Prefab object (exact former field/component type unavailable) | Validator false positive | Both through Bootstrap script importer; former field absent; current AppSettings valid. |
| `53a11f4ebaebf4049b3638ef78dc9664` | R: probeVolumeResources.probeVolumeOffsetDebugShader | Shader | Validator false positive | Both via renderer; obsolete/debug migration field, not current rendering input. |
| `5688ab254e4c0634f8d6c8e0792331ca` | R: m_FallOffLookup | Texture2D | Validator false positive | Both via renderer; field absent; current falloff generated in code. |
| `57d7c4c16e2765b47a4d2069b311bffe` | R: probeVolumeResources.probeSamplingDebugMesh | Mesh | Validator false positive | Both via renderer; obsolete/debug migration field, not current rendering input. |
| `5a0e103d2920f990ab08fe7bada48cc4` | G: MonoBehaviour1409415355137883427.m_Script | MonoScript; intended historical class unknown | Historical or rollback dependency | Neither application closure; one genuinely missing script confirmed on prefab asset. |
| `8236d46ca3a43d373ae6ad281fea8fe0` | S: old ModalButton contents prefab override (8 sites) | GameObject prefab (old ModalButton contents override) | Validator false positive | Neither application; old contents ignored, all eight _contents overrides resolve. |
| `8f96cd657dc40064aa21efcc7e50a2e7` | R: probeVolumeResources.probeVolumeSamplingDebugShader | Shader | Validator false positive | Both via renderer; obsolete/debug migration field, not current rendering input. |
| `a84904e562ecaf887b2dba62111bb901` | B: MonoImporter.defaultReferences.gpuManagerPrefab | Prefab object (exact former field/component type unavailable) | Validator false positive | Both through Bootstrap script importer; former field absent; current AppSettings valid. |
| `b3ab972b21db85d48ade9657efdd4771` | T: dependencies.Array.data[0].dependency | UnityEngine.Object dependency; Texture2D inferred from legacy fileID2800000 | Editor/tooling dependency | Neither application; missing template dependency, embedded preview valid; template use untested. |
| `c41005c129ba4d66911b75229fd70b45` | M: m_defaultSpriteAsset | TMPro.TMP_SpriteAsset | Historical or rollback dependency | Optional global TMP sprite default missing; no current app sprite-tag consumer found. |
| `d5da564da19cb6b7d8e4f97f269edc5d` | V: MonoBehaviour1064799463.m_Script | MonoScript; likely legacy pool component, exact class not proven | Historical or rollback dependency | Neither application; missing script in legacy sample scene; no compatible replacement established. |
| `e5c6678ed2aaa91408dd3df699057aae` | R: probeVolumeResources.probeVolumeDebugShader | Shader | Validator false positive | Both via renderer; obsolete/debug migration field, not current rendering input. |

None of the14 is a proven valid external/built-in reference. None is a Figma import dependency. All14 are genuinely absent from AssetDatabase lookup. The ten false positives concern **interpreting stale serialized data as active functional requirements**, not a claim that GUIDToAssetPath returned the wrong result.

Classification totals:0 Active broken reference;3 Historical or rollback dependency;1 Editor/tooling dependency;0 Valid external or built-in reference;10 Validator false positive;0 wholly unclassified entries. The intended class behind each missing sample MonoScript and the original template texture remain unknown; classification of their support-workflow exposure does not resolve that provenance.

## Evidence and impact

### URP: six deprecated probe resources and one removed falloff field

Native schema reflection confirms ProbeVolumeResources still serializes in Renderer2DData, but both the parent and its fields carry Obsolete metadata referring to the replacement ProbeVolumeDebugResources class. Current installed URP17.4 package source has no consumer of this legacy container outside Deprecated.cs. Current APV rendering code reads separate debug resources. Thus native property existence alone would also overstate activity. See [native deprecation proof](DeprecatedFieldProof.json), [schema](RendererSchema.json) and [source hits](RendererSourceHits.json).

The six old properties have missing object references in the live serialized object. This data is real but unconsumed on the inspected current code path; it is not evidence of six failed lighting effects. Both application scenes use the renderer indirectly through ProjectSettings, so scope is recorded as both, with inactive property status explicitly separated.

m_FallOffLookup is absent from current serialized schema and fields. Current Light2DLookupTexture.GetFalloffLookupTexture creates its lookup texture when needed. Assigning a similarly named texture into old YAML would not repair the current renderer. No texture-generation method was invoked during investigation.

No renderer asset was rewritten or marked as repaired. Later Unity reserialization can also discard legacy compatibility data; that is not needed for integrity of the candidate and was not forced here.

### Bootstrap: stale importer default-reference names

Bootstrap.cs declares only the current serialized AppSettings field, not globalConfigManagerPrefab or gpuManagerPrefab. Native MonoImporter.GetDefaultReference returns null for both old names. The local0.16.3 archive contains those same stale meta defaults, with no corresponding asset identities.

Both scenes explicitly reference the same valid HandLandmarkerBootstrap prefab GUIDb7c55e0db5bc46df90a6157eb1521758. Read-only prefab inspection resolves its HandLandmarkerAppSettings, and also the general Bootstrap prefab's AppSettings. No Bootstrap is instantiated in Edit Mode; an empty active-scene bootstrap instance list is expected because the runner instantiates its configured prefab later. See [importer](BootstrapImporter.json), [prefabs](PrefabReadOnlyInspection.json), [both scenes](BothSceneSerializedScope.json) and [runner reference](SceneBootstrapReferences.json).

These two old defaults are not broken consent/capture configuration. This static finding is not a camera-recognition validation or permission grant. The importer and its meta GUID remain unchanged.

### Sample modal override: obsolete contents versus current _contents

The exact targeted Main Canvas component local ID8571076843980202029 is Mediapipe.Unity.Sample.UI.ModalButton. Native SerializedObject finds _contents and does not find contents. The same sample/recovery override lists contain a current _contents assignment immediately after the obsolete contents assignment; all eight assignments resolve to six valid task config-window prefabs, including exact GameObject local fileID7355513348742275121. See [schema](ModalOverrideSchema.json), [pairs](SampleOverridePairs.json) and [current resolutions](CurrentSampleOverrideResolution.json).

The old override is ineffective on the inspected current schema. It does not imply a missing active Hand Landmark configuration window. No sample or recovery scene was opened or modified to remove the entry.

### Two genuine missing sample scripts

The Global Config Window prefab asset has one missing MonoBehaviour confirmed by GameObjectUtility without instantiating it. Its root script reference5a0e103d2920f990ab08fe7bada48cc4 remains genuinely unresolved. No expected class name or compatible source is established. Empty serialized fields do not justify deleting the component or assuming it was decoration.

The legacy MediaPipe Video scene has unresolved scriptd5da564da19cb6b7d8e4f97f269edc5d with _poolSize:10. This suggests a historical pool component but does not identify its class conclusively. The current package Experimental.TextureFramePool is IDisposable, not a MonoBehaviour, so assigning it is invalid. The legacy scene was not opened or run.

Neither item is in either application scene's native dependency closure, and current application/source/resource loading has no path to these missing scripts. Both orphan references also ship in the local MediaPipe0.16.3 archive; this cleanup did not lose their assets. Their sample functionality remains broken or uncertain, not validated. See [archive comparison](MediaPipeArchiveMatches.json) and [prefab inspection](PrefabReadOnlyInspection.json).

### Editor scene-template texture dependency

SceneTemplateAsset.dependencies[0].dependency is a live missing Object reference. The old fileID2800000 suggests Texture2D, but the property itself accepts Object, so the exact intended asset is unproven. The embedded Lit2D preview texture resolves separately; it must not be assumed to be the missing dependency. Application scenes do not reference this template. New Scene template instantiation was not tested. See [native tooling assets](ToolingAssets.json).

### TMP optional default sprite

TMP_Settings.m_defaultSpriteAsset is a current optional field with a missing TMP_SpriteAsset. UGUI package conversion records identify EmojiOne.asset for this GUID; those are conversion metadata, not an AssetDatabase alias or runtime remap. The installed Essential Resources/Examples bundles do not contain that exact asset identity. A different bundled Default Sprite Asset exists and is not an equivalent replacement. See [conversion matches](PackageConversionMatches.json) and [bundle inspection](BundledSpriteEvidence.json).

The current application uses UI Toolkit/TextCore presentation; no first-party source consumer of TMP_Settings, TextMeshPro or sprite tags was found. Assistant text remains plain. TMP sprite fallback code would use the default when a supported TMP consumer requests it, so this is a genuine optional/sample capability gap, not proof that ordinary bilingual app text is broken. Its runtime consequences have not been exercised. See [TMP source paths](TMPSourceHits.json).

## Where the original assets were sought

All14 GUIDs were queried again in the live AssetDatabase. Exact identity searches covered project/package metadata, installed package cache sources/metadata, loose Editor Resources metadata, the local MediaPipe0.16.3 tarball and both installed TMP unitypackage bundles. No exact missing asset identity was found. Local Git history search found the stale Bootstrap metadata in the initial commit but did not recover the missing assets/scripts. No remote source or service was contacted.

A related name is not an identity match: current TextureFramePool, an embedded preview texture, newer probe shaders and a different default sprite were all rejected as arbitrary replacements. No GUID was classified as valid simply because a package mentions it. [Native lookup](NativeResolutionNow.json), [Editor loose metadata search](EditorResourceMetaSearch.json) and [Git search](GuidGitHistory.txt) preserve the scope. Unexamined remote/upstream history may still contain the intended assets.

## Smallest repair proposals — none applied

There is no confirmed broken active application reference among these14 requiring an asset repair now. Four support/optional issues remain actionable only with their intended provenance/capability decision:

| Confirmed support defect | Exact proposed action | Serialized impact / prerequisite |
| --- | --- | --- |
| Global Config Window missing script |Recover the matching historical MonoScript and original .meta GUID5a0e103d2920f990ab08fe7bada48cc4. |Preserve prefab component fileID1409415355137883427; no reassignment/removal. Expected class is unknown, so safe repair is blocked on matching source. |
| Legacy MediaPipe Video missing script |Recover matching MonoScript GUIDd5da564da19cb6b7d8e4f97f269edc5d and compatible serialized _poolSize. |Preserve component fileID1064799463 and value10. A migration to the current non-MonoBehaviour pool needs its own lifecycle design; not an integrity substitution. |
| Scene-template missing dependency |In an approved isolated Editor experiment, use the verified SceneTemplateService.CreateTemplateFromScene(existing.templateScene, temporarySceneTemplatePath), compare dependency arrays, then propose the minimal original-asset diff. |Only the original template dependency entry/list should change if proven stale; retain its identity and preview. Do not substitute preview or modify either application scene. If the texture remains required, recover exact GUIDb3ab972b21db85d48ade9657efdd4771 first. No template generation or rewrite performed. Native API reflection confirmed CreateTemplateFromScene(SceneAsset, string); no UpdateDependencies API was assumed. |
| TMP default sprite missing |Recover exact EmojiOne sprite asset GUIDc41005c129ba4d66911b75229fd70b45 plus verified matching atlas/material dependencies if that capability is retained. |Keep references by restoring correct identity. If sprite support is intentionally omitted, separately approve clearing only m_defaultSpriteAsset to an explicit optional null. Do not import all examples or choose a different sprite silently. |

Ten stale entries need no active-runtime repair. A future metadata normalization could remove individual old field/override entries, but this stage deliberately preserves them and their owning files.

**Additional provably removable files: none.** Obsolete property data does not make Renderer2D, Bootstrap metadata, Main Canvas, sample scenes or historical recovery scenes disposable. The missing assets themselves cannot be deleted because they are absent. No destructive cleanup/cutover occurred.

## Validator correction and reliable baseline

The Stage3.3 raw resolver correctly detected14 absent GUIDs but did not distinguish ignored YAML, importer fields, obsolete serialized fields, optional resources or reachable requirements. It also consumes cached snapshots. The old FAIL and evidence remain unchanged rather than being retrospectively turned into PASS.

[verify_guid_baseline.py](verify_guid_baseline.py) adds a separate classified integrity baseline. It checks current hashes of all2045 application/configuration/backend files, relevant package/source inputs and reviewed native snapshots; stale evidence fails instead of silently surviving a package/code upgrade. It checks the exact14-set, source locations, native schema/deprecation facts, valid current overrides, both scene bootstrap assignments and current AppSettings. Source/package hash binding is a snapshot freshness check, not a substitute for re-exporting native evidence after an environment change.

Run from the repository root: `python -X utf8 Documentation/Stage3LivingGlass/Cleanup/GUIDInvestigation/verify_guid_baseline.py`.

Expected baseline:11 checks PASS,1 FAIL for the four genuine support/optional missing dependencies; raw unresolved14; confirmed active application defects0. Exit1 is intentional. This preserves unresolved support debt while preventing raw reference absence from being reported as14 active application failures. See [IntegrityBaseline.json](IntegrityBaseline.json).

Before later deletion decisions, re-export relevant native schema/resolution snapshots in idle Edit Mode, review source/package changes and refresh classification evidence. Do not add a GUID-only suppression list, substitute same-name assets, or treat this report as removal authorization.

## Validation and preservation

- PASS: exact14 traced and classified; all2045 baseline application/asset/settings/backend hashes unchanged.
- PASS: native schema, deprecation attributes, live missing-reference state, prefab missing-script count and current override resolutions inspected without scene loading/instantiation.
- PASS: both scene files and production build entry preserved; no importer, package, media, gesture, assistant or runtime source changes.
- PASS: diagnostic Python syntax and classified baseline consistency checks; known support failures remain visible.
- FAIL: four genuine support/optional reference issues; strict raw lookup still reports14 unresolved. No repair is claimed.
- NOT RUN: project recompilation (no application C#/asset repair was made). Existing compiled Editor types were inspected and in-memory C# diagnostic snippets compiled/executed; this is not a new full-project compilation claim.
- NOT RUN: Play Mode, camera/MediaPipe initialization, live assistant/backend, scene-template instantiation, legacy sample playback, TMP sprite rendering, Player builds and profiling.

This targeted pass did not repeat the68 conversation checks or general cleanup. No new screenshots were necessary or captured. Existing Stage3.3 compilation and runtime results remain historical evidence. No application release-readiness claim follows from this baseline.

## Console and Editor state

The final read-only Console snapshot contains the pre-existing Editor AI NoSubscription exception and two MCP WebSocket closure warnings. Tool transport briefly lost its connection; probes that had already written evidence were inspected rather than rerun blindly. No C# compiler error was observed. These logs do not establish application runtime behavior. [Console](ConsoleAfter.json) and [Editor state](EditorStateAfter.json) are retained; Editor is idle with Play Mode false.

## Recommended next decisions

Keep the application and rollback graphs unchanged. Resolve whether the old Global Config/MediaPipe Video samples, generic scene template and TMP sprite capability remain supported before approving their separate repairs or metadata normalization. Exact missing-script provenance is required before a compatible restoration can be proposed as executable work. Camera/live assistant/performance acceptance still need their own authorization and evidence.
