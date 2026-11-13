# Removal candidates and decisions

**No file qualified as REMOVE — PROVEN OBSOLETE. No asset, script, metadata, directory or historical document was deleted.** [RemovalLedger.json](RemovalLedger.json) is intentionally empty. Retention is not a claim that every file is necessary forever.

[CandidateTriage.json](CandidateTriage.json) records252 zero-native-incoming, non-scene-closure entries. It supplies exact paths, GUIDs, native/serialized incoming counts and source paths, classification and retention reason. Counts measure different relationships and must not be summed as unique usages. [AssetInventory.json](AssetInventory.json) covers all physical assets and scene membership.

| Category | Classification / final status | Evidence / premature-deletion impact |
| --- | --- | --- |
| Original UXML/USS, catalog and artwork |KEEP — ROLLBACK / RETAINED — required |Original scene/catalog closure and mapping prebuild workflow; removal breaks restoration/bindings/tooling. |
| Original controllers and compatibility branches |KEEP — ROLLBACK or SHARED / RETAINED — required |Absent livingGlass selects original UI; same facade owns shared services. No cutover approved. |
| Duplicate-looking fonts/SDFs/TMP shaders/includes |UNRESOLVED or SHARED / DEFERRED or RETAINED |Distinct GUIDs, fallback/material/include and dynamic resource paths. No replacement equivalence or shader-stripping proof. Licenses retained. |
| Captures and six duplicate screenshot groups |KEEP — ROLLBACK / RETAINED — required |Historical validation/provenance and document paths. Equal pixels do not erase capture chronology. |
| Six ignored recovery scenes |UNRESOLVED / DEFERRED — insufficient evidence |Recovery value unknown; ignored/untracked status is not disposal proof. |
| Hidden .figma-manifest.json |KEEP — ROLLBACK / RETAINED — required |Explicit mapping validator path; invisible to imported-asset-only analysis. |
| Hidden .figma-manifest.json.bak |UNRESOLVED / DEFERRED — insufficient evidence |Design-tool backup/rollback provenance unknown. |
| MediaPipe samples/utilities |KEEP — SHARED / RETAINED — required |Compiled/supporting workflows, bootstrap/defaults, application integration. Existing unresolved references must not be hidden through deletion. |
| Packages/fallbacks |SHARED or UNRESOLVED / RETAINED or DEFERRED |57 declarations plus transitive/tooling/platform support. No package migration authorized. |
| Private reflected methods, Unity callbacks/menu entries |KEEP — SHARED / RETAINED — required |Bounded validation/lifecycle/tooling entry points, not ordinary direct-call-only functions. |

## Code-only cleanup performed

1. Removed the private, nonserialized interactionStylesAdded field and its assignment-only branch. Exactly two source occurrences, no read, serialized binding or reflection-name use. ApplyInteractionStylesTo retains its null guards and stylesheet add. No behavior consumes this state.
2. Replaced four string field-name reflection lookups between the isolated preview and two checkers with internal getter-only Presentation/PreviewAssets properties. They return the same owned instances. Preview initialization/disposal and the runtime-flow-null safety probes remain unchanged.

No serialized/public runtime API rename, meta GUID change, service extraction or duplicate subsystem. Compilation, existing protected contracts,68 conversation layout/state specimens and all-six/bilingual stabilization checks validate the permitted scope. Runtime operation was not tested.

Rollback: restore the shared controller from Before/Assets/Scripts/AwakeningApplicationController.cs and the three affected Editor sources from Before/Assets/LivingGlass/Editor/ after reviewing newer edits; preserve metadata. [ScopedChanges.patch](ScopedChanges.patch) is the precise diff.

Future deletion proposals must first record exact path/GUID, incoming sources/types, dynamic/tooling/rollback independence, impact, verification and recovery source. Current unresolved files lack this proof and are retained.
