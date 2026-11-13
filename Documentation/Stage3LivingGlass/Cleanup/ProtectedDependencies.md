# Protected dependencies and preservation

| Area | Preservation evidence |
| --- | --- |
| SampleScene and metadata |Unchanged stage-baseline hash;274-path native closure retained; eight distinct serialized component-script GUIDs resolve. No runtime claim. |
| AwakeningLivingGlass scene and metadata |Unchanged; active scene clean in idle Edit Mode;45-path native closure retained; seven distinct component-script GUIDs resolve. |
| Production build entry |EditorBuildSettings.asset unchanged; only enabled scene remains SampleScene. No cutover. |
| Packages and ProjectSettings |All bytes unchanged; no upgrade, package removal or renderer migration. |
| Shared controller |Only private nonserialized dead flag/assignment removed. Exact scoped diff verifies prior user modifications remain. Fifteen protected video/assistant method bodies pass the existing checker. |
| Consent/MediaPipe |Gate, runner, bridge, recognizer, cursor, rig/model/prefab metadata unchanged. No capture initialization, consent grant or threshold tuning. |
| Media |All video bytes/importers unchanged; six ordered serialized slots match the original scene. No playback or resource-release retest. |
| Assistant/backend |Integration partial, transport/session/cancellation/late-response protection and backend unchanged. No live request sent. |
| Original UI/localization |Catalog, node bindings,104 UXML/100 USS, Home/doc manager, panels, fonts and art retained. Mapping validator passes. |
| Living Glass design |Runtime presentation, motion, theme, atmosphere, USS and asset configuration unchanged. Controlled Spatial B and fallback behavior not redesigned. |
| Deferred features |Sound/accessibility business preferences, tracking/calibration, Restart, Souvenir, future speech/guide remain deferred. No control activated. |
| History |Stage1/2/3 design/validation evidence retained. Rerun check results copied into Cleanup/Validation, historical artifacts restored byte-for-byte from HistoricalEvidenceBefore. |

[AllFilesBefore.json](AllFilesBefore.json) and [AllFilesAfter.json](AllFilesAfter.json) contain2045 source/asset/package/settings/backend hashes. [AuditAfter.json](AuditAfter.json) records only four intended source edits, zero added/deleted files; the other2041 files match. [CleanupStaticChecks.json](CleanupStaticChecks.json) verifies exact scope, meta uniqueness, UI URL references and graph preservation. Initial [ProtectedHashes.json](ProtectedHashes.json) is also retained.

## Baseline and recovery

Branch main; commit b9f0972b38008b31e4bd2117ee88cb34ef9ce6fe. [BaselineGitStatus.txt](BaselineGitStatus.txt), [BaselineCommit.txt](BaselineCommit.txt), [WorkingChanges.patch](WorkingChanges.patch) preserve initial tracked state. No reset, clean or checkout performed.

[UntrackedBaseline.zip](UntrackedBaseline.zip) contains973 pre-existing untracked files outside Cleanup,446365022 bytes. CRC verification passed. Ignored recovery/capture files were hashed and retained in place; they are not claimed as part of that archive.

Prefer the four exact source copies under [Before](Before/) for this cleanup's rollback. Review for later edits before copying individual files, retain existing meta files, then recompile in Edit Mode. Do not reset the whole repository or blindly extract the archive over newer work. The commit, initial patch and archive are recovery references, not destructive commands.

Returning to the original scene needs no asset migration/build-entry change. Do not load both application scenes additively. Original-scene operation still requires separately authorized runtime testing.
