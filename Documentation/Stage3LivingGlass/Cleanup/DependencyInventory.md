# Stage 3.3 dependency inventory

Current repository evidence supersedes historical claims that gesture, video or assistant integrations are absent. Analysis and validation used idle Unity6000.4.7f1 Edit Mode; no application session was started.

| Evidence | Scope / result |
| --- | --- |
| [Native graph](UnityAssetGraph.json), [after graph](UnityAssetGraphAfter.json) |591 imported Assets files, with GUIDs and direct AssetDatabase edges. Graphs match after cleanup. |
| [Asset inventory](AssetInventory.json), [CSV](AssetInventory.csv) |593 physical non-meta files, including two hidden Figma manifest/backup files omitted by AssetDatabase. Every row has path, GUID where applicable, incoming counts and reference sources, classification/reason, bytes and SHA-256. |
| [Original closure](SampleSceneDependencies.txt), [candidate closure](AwakeningLivingGlassDependencies.txt) |274 and45 paths respectively,36 shared. Counts include root scenes and package dependencies. These alone are insufficient deletion proof. |
| [ProjectSettings dependencies](ProjectSettingsDependencies.txt) |Additional build/input/render/tooling roots followed transitively through native asset edges. |
| [Serialized references](SerializedGuidReferences.json), [external resolution](ExternalGuidResolution.json) |Raw serialized GUID references across Assets, ProjectSettings and Packages; meta identity declarations excluded.261 non-local GUIDs queried natively:247 resolve,14 do not. |
| [Dynamic/tooling references](DynamicAndToolingReferences.json) |356 remaining source lines identify resource loading, generated asset paths, streaming models, reflection and related entry points. |
| [Compiled assemblies](CompiledAssemblies.json) |154 Editor-reported assemblies, source files and references, including packages. Scene-only searches cannot establish dead compiled code. |
| [Package inventory](PackageInventory.json) |57 direct declarations,73 lock records. All retained; no package or lock migration. |
| [Source ownership](SourceOwnership.csv) |22 first-party runtime/Editor files reviewed for responsibility and cleanup exposure;51 additional sample C# files retained. Native Assets contains73 C# files. |
| [Duplicate content](DuplicateContentGroups.json) |Six byte-identical historical screenshot groups. Duplication does not establish that capture provenance or links may be discarded. |
| [Before hashes](AllFilesBefore.json), [after hashes](AllFilesAfter.json) |2045 source/asset/package/settings/backend files, including ignored Assets recovery/evidence. Four intended source changes; no additions/deletions in these roots. |

Final asset classification:22 KEEP — ACTIVE,141 KEEP — SHARED,391 KEEP — ROLLBACK,39 UNRESOLVED. No asset is REPLACE or REMOVE — PROVEN OBSOLETE. Initial triage had43 unresolved entries; tracing ProjectSettings and manifest provenance reclassified four retained assets. Code-only sub-file refactors are recorded separately.

## Current ownership

| Area | Authority / preservation decision |
| --- | --- |
| Flow/session/awakening/navigation |One shared AwakeningApplicationController, including the Living Glass semantic partial. Original-scene compatibility is still required. |
| Living Glass UI |LivingGlassPresentation owns reusable families, shell, overlays, local focus and visual ownership in one UIDocument. No second navigation stack. |
| Routes/themes/motion/material |LivingGlassRoutes, LivingGlassAssets, LivingGlassMotion, LivingGlassAtmosphere, shared USS and gesture illustration. No extra camera/render pass or animation framework. |
| Original UI |HomeLanguageController, ScreenDocumentManager, catalog, node bindings,104 imported UXML/100 USS, art/fonts and panel. Protected original scene and prebuild validator require these. |
| Camera/gestures |Consent gate → registered runner/bootstrap → bridge → recognizer/cursor → controller. Modified sample code is integration code, not disposable examples. Preview visibility is separate from consent. |
| Living Portrait |Six Resources clips and ordered scene slots; same single player/completion/cancellation/RenderTexture owner. Media/importers untouched. |
| Assistant |Same transport/session/cancellation/late-response protection and backend. Editor AI packages are separate from the visitor assistant. |
| Locale/fonts |52 logical routes × en/zh, CJK fonts/subassets and authored strings. No font replacement from apparent filename duplication. |
| Audio |Existing controller/configuration/missing-clip behavior retained. No sound functionality activated. |
| Tooling |Mapping prebuild callback, scene builder, isolated preview/checkers and permission-bound runtime harness. Unity callbacks/menu methods are not dead from ordinary call counts. |
| Dynamic dependencies |Resources catalog/bindings/styles, StreamingAssets hand model, generated performer image paths and baseline panel/font loads. No Addressables declaration or application loading path found. |
| Shader/font infrastructure |Native asset edges plus Resources, material, include and name-based paths. Unknown dependence means retention. |
| Design history |Stage1/2 proposals, Figma provenance, prototypes, checkpoints and validation evidence retained. Historical status does not establish current runtime success. |

## Findings and limits

Fourteen existing GUIDs fail AssetDatabase resolution: seven renderer debug/falloff resources, two Bootstrap importer default references, a sample prefab reference, a scene-template dependency, TMP default sprite, and two sample script references. Complete external GUID resolution is explicitly FAIL in [CleanupStaticChecks.json](CleanupStaticChecks.json). They are unchanged by cleanup; functional reachability remains unresolved. No blanket missing-reference-free claim is made.

Both preserved scenes' serialized component-script GUIDs resolve: eight distinct original-scene scripts and seven candidate-scene scripts. This is static integrity evidence, not runtime acceptance.

Visible serialized/imported graphs, compiled sources and identifiable dynamic entry points have been reviewed. This is not proof against arbitrary external/reflection usage. All insufficiently proven candidates remain intact.
