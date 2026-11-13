"""Current Living Glass static regression gate after authorized demo retirement.
Run the collector and idle-Editor native checks before this check. Never launches
Unity, Play Mode, hardware, services, test runners or builds. Raw GUID failures stay
visible and produce exit1, separately from newly introduced regression status.
The pre-retirement checker remains byte-preserved in Before/Documentation/.
"""
from pathlib import Path
import csv
import hashlib
import json
import re
import xml.etree.ElementTree as ET
from fontTools.ttLib import TTFont

OUT=Path(__file__).resolve().parent
ROOT=OUT.parents[3]
def load(name):return json.loads((OUT/name).read_text(encoding="utf-8-sig"))
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
checks=[]
def check(name,ok,detail=None):checks.append(dict(check=name,status="PASS" if ok else "FAIL",detail=detail))
baseline=load("BaselineHashes.json");current=load("HashesAfter.json");delta=load("FileDelta.json")
actual={p.relative_to(ROOT).as_posix():sha(p) for top in ("Assets","Packages","ProjectSettings") for p in (ROOT/top).rglob("*") if p.is_file()}
check("evidence file hashes match the current complete scan roots",actual==current)
expected={"Assets/LivingGlass/Core/LivingGlassRoutes.cs","Assets/LivingGlass/Editor/LivingGlassBoundedValidation.cs","Assets/LivingGlass/Editor/LivingGlassContractChecks.cs","Assets/LivingGlass/Editor/LivingGlassPreviewWindow.cs","Assets/LivingGlass/Integration/LivingGlassFlowIntegration.cs","Assets/Scenes/AwakeningLivingGlass.unity","Assets/Scripts/AwakeningApplicationController.cs","Packages/manifest.json","Packages/packages-lock.json","ProjectSettings/EditorBuildSettings.asset"}
check("only ten reviewed existing files changed",set(delta["changed"])==expected,delta["changed"])
ledger=load("RemovalLedger.json")
check("every retirement has exact recovery path, hash and GUID evidence",len(ledger)==len(delta["removed"])==458 and {r["path"] for r in ledger}==set(delta["removed"]) and all(r["sha256_before"]==baseline[r["path"]] and r["recovery_commit"]==load("RecoveryManifest.json")["recovery_commit"] and not r["native_incoming_after"] for r in ledger))
check("all other pre-existing files preserved",all(current[p]==h for p,h in baseline.items() if p not in expected and p not in delta["removed"]))
check("six new files are only enum, route contract and independent Editor validator with metadata",set(delta["added"])=={p+s for p in ["Assets/LivingGlass/Core/AppLanguage.cs","Assets/LivingGlass/Editor/RouteContract.json","Assets/LivingGlass/Editor/LivingGlassProjectValidation.cs"] for s in ("",".meta")})
protected=[p for p in baseline if p.startswith(("Assets/Art/","Assets/StreamingAssets/","Assets/Samples/","Assets/Scripts/Interaction/"))]
check("art, videos/importers, models, capture/consent/recognition/cursor and vendor sources preserved",all(current.get(p)==baseline[p] for p in protected),len(protected))
backend=load("ProtectedBackendHashes.json")
check("backend files and local configuration preserved without exposing contents",all(sha(ROOT/p)==h for p,h in backend.items()),len(backend))
view=[p for p in baseline if p.startswith("Assets/LivingGlass/") and not p.startswith("Assets/LivingGlass/Editor/") and p not in expected]
check("approved view, motion, glass, themes, fonts, component styling and media configuration preserved",all(current.get(p)==baseline[p] for p in view),len(view))
check("four user recovery snapshots preserved byte-for-byte outside imported assets",all(sha(OUT/"HistoricalRecovery"/Path(r["path"]).name)==r["sha256_before"] for r in ledger if r["action"]=="MIGRATED_TO_HISTORY"))
scene="Assets/Scenes/AwakeningLivingGlass.unity"
old=(OUT/"Before"/scene).read_text(encoding="utf-8-sig");new=(ROOT/scene).read_text(encoding="utf-8-sig")
remove=load("SceneFieldMigration.json")
wanted="".join(line for line in old.splitlines(True) if line.strip() not in remove)
check("primary scene diff removes only three obsolete null legacy fields",new==wanted)
def clips(s):return re.search(r"  performerPortraitVideos:\n((?:  - .+\n){6})",s)[1]
check("all six serialized video slots unchanged and ordered",clips(old)==clips(new))
settings="ProjectSettings/EditorBuildSettings.asset"
oldsettings=(OUT/"Before"/settings).read_text();newsettings=(ROOT/settings).read_text()
guid=re.search(r"^guid: (\w+)",(ROOT/(scene+".meta")).read_text(),re.M)[1]
wantedsettings=oldsettings.replace("Assets/Scenes/SampleScene.unity",scene).replace("8c9cfa26abfee488c85f1582747f6a02",guid)
check("serialized development entry is Living Glass and unrelated config objects preserved",newsettings==wantedsettings and newsettings.count("enabled: 1")==1)
check("demo scene/catalog/controllers/exports absent from imported project",all(not (ROOT/p).exists() for p in ["Assets/Scenes/SampleScene.unity","Assets/Scenes/SampleScene.unity.meta","Assets/Figma","Assets/Figma.meta","Assets/Scripts/ScreenDocumentManager.cs","Assets/Scripts/HomeLanguageController.cs","Assets/Scripts/RuntimeScreenCatalog.cs"]))
fixture=json.loads((ROOT/"Assets/LivingGlass/Editor/RouteContract.json").read_text())["entries"]
gold=load("RouteContractBefore.json")
pairs=lambda rows:sorted((r["route"],r["locale"]) for r in rows)
check("independent104 locale fixture matches the exact pre-retirement contract",len(fixture)==104 and len({r["route"] for r in fixture})==52 and pairs(fixture)==pairs(gold))
check("AppLanguage serialized numeric contract preserved",(ROOT/"Assets/LivingGlass/Core/AppLanguage.cs").read_text().find("English = 0")>=0 and (ROOT/"Assets/LivingGlass/Core/AppLanguage.cs").read_text().find("Chinese = 1")>=0)
changes=load("MethodPreservation.json")
unchanged={r["name"] for r in changes["unchanged"]}
critical={"TryConnectGestureBridge","UpdateGestureTarget","OnPointingChanged","OnTrackingChanged","SetTargetedPerformer","ClearTargetedPerformer","ContinueFromPrivacy","FadeFocusIntoActivation","PlayActivationSequence","StartLivingPortrait","PrepareLivingPortraitVideo","OnLivingPortraitVideoPrepared","TryStartLivingPortraitPlayback","OnLivingPortraitVideoFrameReady","OnLivingPortraitVideoCompleted","OnLivingPortraitVideoError","StartPlaceholderPortraitTimer","WaitForPlaceholderPortrait","RequestLivingPortraitCompletion","StopLivingPortraitVideoPlayer","ReleaseLivingPortraitRenderTexture","TryBuildAssistantChatUrl","BuildPerformerScopedMessage","SubmitAssistantMessage","SendAssistantRequest","CancelAssistantRequest","OnDestroy","PlayEnvironmentalMusic","StopEnvironmentalMusic","LivingLanguage","LivingConsentChoice","LivingGlassCloseOverlay"}
check("protected service/consent/gesture/media/request/disposal method AST text unchanged",critical.issubset(unchanged),sorted(critical-unchanged))
nodes=load("SourceMembersAfter.json")
check("Roslyn method evidence matches current source spans",all((ROOT/r["path"]).read_bytes().decode("utf-8")[r["start"]:r["start"]+r["length"]]==r["text"] for r in nodes))
main=(ROOT/"Assets/Scripts/AwakeningApplicationController.cs").read_text()
port=(ROOT/"Assets/LivingGlass/Integration/LivingGlassFlowIntegration.cs").read_text()
check("active flow has no legacy controller/catalog/path-binding dependencies",not any(n in main+port for n in ["ScreenDocumentManager","HomeLanguageController","RuntimeScreenCatalog","RuntimeNodeBindings","screenUnbind","activeBindingScreen","overlayStack","BindPath("]))
check("assistant lifecycle and cinematic coroutines remain in the single serialized controller",all(n in main for n in ["UnityWebRequest","assistantSessionSequence","CancelAssistantRequest","VideoPlayer","RenderTexture","RequestLivingPortraitCompletion"]) and "GestureCameraConsentGate.SetConsent(false)" in main)
font=TTFont(ROOT/"Assets/LivingGlass/Assets/Fonts/NotoSansCJKtc-Regular.otf")
characters={c for p in (ROOT/"Assets/LivingGlass").rglob("*.cs") for c in p.read_text(encoding="utf-8") if "\u3400"<=c<="\u9fff"}
missing=sorted(c for c in characters if ord(c) not in font.getBestCmap())
check("Traditional Chinese source glyph coverage",not missing,dict(unique_characters=len(characters),missing=missing))
bad=[]
for p in (ROOT/"Assets").rglob("*.uxml"):
    try:ET.fromstring(p.read_text(encoding="utf-8-sig"))
    except ET.ParseError as e:bad.append(str(e))
raw=load("RawGuidReferencesAfter.json");resolution=load("NativeGuidResolutionAfter.json")
check("native GUID evidence covers every external raw identity",set(load("ExternalGuidsAfter.json"))=={r["guid"] for r in resolution})
unresolved={r["guid"] for r in resolution if not r["path"]}
known={"5a0e103d2920f990ab08fe7bada48cc4","8236d46ca3a43d373ae6ad281fea8fe0","b3ab972b21db85d48ade9657efdd4771","c41005c129ba4d66911b75229fd70b45","d5da564da19cb6b7d8e4f97f269edc5d"}
check("native resolution reports exactly the same five pre-existing unresolved GUIDs",unresolved==known,sorted(unresolved))
check("remaining obsolete override is confined to two unchanged MediaPipe recovery files",set(raw["8236d46ca3a43d373ae6ad281fea8fe0"])=={"Assets/_Recovery/0 (1).unity","Assets/_Recovery/1.unity"})
check("remaining UI documents parse and have no unresolved asset/style GUIDs",not bad and not any(p.endswith((".uxml",".uss",".tss")) for g in unresolved for p in raw[g]),bad)
check("no duplicate imported GUID identities",not load("DuplicateGuidsAfter.json"))
assets=[p for p in (ROOT/"Assets/LivingGlass").rglob("*") if p.is_file() and p.suffix!=".meta"]
check("all Living Glass files have metadata",all(Path(str(p)+".meta").exists() for p in assets))
graph=load("NativeGraphAfter.json");retired={r["path"].removesuffix(".meta") for r in ledger}
check("no retained native asset points into the retired set",not any(d in retired for r in graph for d in r["dependencies"]))
check("primary native dependency closure preserved after removal",(OUT/"LivingGlassDependenciesAfterCode.txt").read_bytes()==(OUT/"LivingGlassDependenciesAfter.txt").read_bytes())
check("five raw failures are outside the active scene dependency closure",not any(p in set((OUT/"LivingGlassDependenciesAfter.txt").read_text().splitlines()) for g in unresolved for p in raw[g]))
packages=load("PackageDelta.json")
before_lock=json.loads((OUT/"Before/Packages/packages-lock.json").read_text())["dependencies"]
after_lock=json.loads((ROOT/"Packages/packages-lock.json").read_text())["dependencies"]
before_manifest=json.loads((OUT/"Before/Packages/manifest.json").read_text())
after_manifest=json.loads((ROOT/"Packages/manifest.json").read_text())
before_manifest["dependencies"].pop("com.unity.ui.figma")
for name in ("com.unity.ui.figma","com.unity.pipeline"):before_lock.pop(name)
check("UPM removed only Figma and its exclusive pipeline without version drift",before_lock==after_lock and before_manifest==after_manifest and set(packages["removed"])=={"com.unity.pipeline","com.unity.ui.figma"} and not packages["added"] and not packages["changed_versions"] and packages["app_ui_retained"])
editor=load("FinalChecks/EditorState.json")
check("Unity completed compilation in clean Edit Mode with no consent or lingering preview",not any(editor[k] for k in ["playing","compiling","compilerErrors","dirty","consent","previewCount"]) and editor["scene"]==scene)
check("native primary-scene and pure contract checks passed",(OUT/"FinalChecks/SerializedValidation.txt").read_text().count("PASS:")==7 and (OUT/"FinalChecks/PureContractChecks.txt").read_text().count("PASS:")==5)
conversation=OUT/"FinalChecks/ConversationRefinement"
rows=list(csv.DictReader((conversation/"LayoutChecks.csv").open(encoding="utf-8-sig")))
check("all68 isolated conversation layout/state specimens passed",len(rows)==68 and (conversation/"EditorChecks.txt").read_text().startswith("PASS:"))
check("isolated bilingual transition/tree/scroll/reduced checks passed",(OUT/"FinalChecks/Stage31Stabilization/EditorChecks.txt").read_text().rstrip().endswith("PASS: Editor-only model/tree/style checks; runtime retest NOT RUN."))
report=dict(regression_status="PASS" if all(r["status"]=="PASS" for r in checks) else "FAIL",checks=checks,
            strict_raw_guid_resolution=dict(status="FAIL" if unresolved else "PASS",distinct_guids=len(unresolved),sources={g:raw[g] for g in sorted(unresolved)}),
            confirmed_new_active_missing_references=sorted(unresolved-known),runtime="NOT RUN",note="Compilation/static/isolated Editor checks do not prove operational equivalence or release readiness.")
(OUT/"Validation.json").write_text(json.dumps(report,indent=2,ensure_ascii=False),encoding="utf-8")
print(json.dumps(dict(checks=len(checks),regression_status=report["regression_status"],failed=[r["check"] for r in checks if r["status"]=="FAIL"],raw_guid_status=report["strict_raw_guid_resolution"]["status"],raw_guid_count=len(unresolved))))
raise SystemExit(1 if report["regression_status"]!="PASS" or unresolved else 0)
