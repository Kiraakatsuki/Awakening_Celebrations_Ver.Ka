"""Read-only Stage3.4 file/reference inventory; writes only this evidence directory.
Does not launch Unity, runtime, hardware, services or a build. Native resolution is
supplied separately by the idle Editor; absence in this scan is not asset deletion proof.
"""
from pathlib import Path
from collections import Counter, defaultdict
import csv
import difflib
import hashlib
import json
import re
import subprocess

OUT = Path(__file__).resolve().parent
ROOT = OUT.parents[3]
def load(name):
    return json.loads((OUT / name).read_text(encoding="utf-8-sig"))
def save(name, value):
    (OUT / name).write_text(json.dumps(value, indent=2, ensure_ascii=False), encoding="utf-8")
def digest(p):
    return hashlib.sha256(p.read_bytes()).hexdigest()
baseline = load("BaselineHashes.json")
paths = [p for top in ("Assets", "Packages", "ProjectSettings") for p in (ROOT / top).rglob("*") if p.is_file()]
current = {p.relative_to(ROOT).as_posix():digest(p) for p in paths}
save("HashesAfter.json", current)
removed = sorted(baseline.keys() - current.keys())
added = sorted(current.keys() - baseline.keys())
changed = sorted(p for p in baseline.keys() & current.keys() if baseline[p] != current[p])
save("FileDelta.json", dict(removed=removed, added=added, changed=changed))
recovery = load("RecoveryManifest.json")
commit = recovery["recovery_commit"]

def old_bytes(names):
    proc = subprocess.Popen(["git", "cat-file", "--batch"], cwd=ROOT, stdin=subprocess.PIPE, stdout=subprocess.PIPE)
    found = {}
    for path in names:
        proc.stdin.write((commit + ":" + path + "\n").encode()); proc.stdin.flush()
        header = proc.stdout.readline().decode().strip().split()
        if header[-1] == "missing": raise RuntimeError(path + " missing from recovery")
        found[path] = proc.stdout.read(int(header[2])); proc.stdout.read(1)
    proc.stdin.close();proc.wait()
    return found
old = old_bytes(sorted(set(p if p.endswith(".meta") else p+".meta" for p in removed if p.endswith(".meta") or p+".meta" in baseline)
                       | {p for p in baseline if p.endswith(".cs")}))
graph = {r["path"]:r for r in load("NativeGraphBefore.json")}
classification = {r["path"]:r for r in load("DependencyClassification.json")}
native_after = {r["path"]:r for r in load("NativeGraphAfter.json")}
closure = set((OUT / "LivingGlassDependenciesAfter.txt").read_text(encoding="utf-8-sig").splitlines())
def reason(p):
    if p.startswith("Assets/_Recovery/"):return "Imported demo snapshot relocated byte-for-byte to HistoricalRecovery; exact original also archived"
    if p.startswith("Assets/Figma"):return "Retired demo export; no remaining native, source, dynamic or supported tooling consumer after catalog removal"
    if p.startswith("Assets/Fonts/"):return "Legacy export-only SDF or SC font; Living Glass uses preserved Latin TTF and independent TC font assets"
    if p.startswith("Assets/Scenes/SampleScene"):return "Demo explicitly retired after recovery gate; Living Glass sole independent development entry"
    if p.startswith("Assets/Settings/Pipeline"):return "Figma importer-only pipeline configuration; importer and exports retired together"
    if p.startswith("Assets/UI Toolkit/PanelSettings"):return "Original panel; Living Glass has independent panel and text settings"
    return "Legacy presentation/catalog/layout tooling removed after source and serialization audit; shared service authority retained"
ledger=[]
for p in removed:
    asset=p[:-5] if p.endswith(".meta") else p
    meta=asset+".meta"
    m=re.search(rb"^guid: ([0-9a-f]{32})", old.get(meta,b""),re.M)
    incoming=[r["path"] for r in graph.values() if asset in r["dependencies"]]
    retained_incoming=[r["path"] for r in native_after.values() if asset in r["dependencies"]]
    ledger.append(dict(path=p,guid=m[1].decode() if m else "",sha256_before=baseline[p],classification="LEGACY_ONLY",
                       action="MIGRATED_TO_HISTORY" if p.startswith("Assets/_Recovery/") else "RETIRED",
                       reason=reason(p),native_incoming_before=incoming,native_incoming_after=retained_incoming,
                       recovery_commit=commit,recovery_path=p))
save("RemovalLedger.json",ledger)
with (OUT/"RemovalLedger.csv").open("w",newline="",encoding="utf-8-sig") as f:
    w=csv.DictWriter(f,fieldnames=["path","guid","action","classification","reason","sha256_before","recovery_commit","recovery_path"])
    w.writeheader();w.writerows({k:r[k] for k in w.fieldnames} for r in ledger)

final=[]
for p,r in native_after.items():
    c=classification.get(p,{})
    category=c.get("classification","UNRESOLVED")
    detail=c.get("reason","Retained; no deletion inference from lack of direct references")
    if p in closure:category="ACTIVE_LIVING_GLASS";detail="Current primary scene native dependency closure"
    final.append(dict(**r,classification=category,reason=detail))
for r in ledger:
    if not r["path"].endswith(".meta"):
        final.append(dict(path=r["path"],guid=r["guid"],classification="LEGACY_ONLY",reason=r["reason"],retired=True))
final.append(dict(path="Documentation/",classification="HISTORICAL_DOCUMENTATION",reason="Design provenance, historical validation and recovery evidence retained"))
save("FinalDependencyClassification.json",final)

extensions={".meta",".unity",".asset",".prefab",".mat",".uxml",".uss",".json",".cs",".shader",".shadergraph",".hlsl",".cginc",".scenetemplate",".asmdef",".asmref",".tss",".inputactions",".txt",".xml"}
raw=defaultdict(set);identities=defaultdict(list);dynamic=[]
for p in paths:
    if p.suffix not in extensions:continue
    try:s=p.read_text(encoding="utf-8-sig")
    except UnicodeError:continue
    path=p.relative_to(ROOT).as_posix()
    if p.suffix==".meta":
        m=re.search(r"^guid: ([0-9a-f]{32})",s,re.M)
        if m:identities[m[1]].append(path[:-5])
        s=re.sub(r"^guid: [0-9a-f]{32}.*$","",s,flags=re.M)
    for guid in re.findall(r"\bguid\s*[:=]\s*([0-9a-f]{32})",s):raw[guid].add(path)
    if p.suffix==".cs":
        for n,line in enumerate(s.splitlines(),1):
            if any(t in line for t in ("Resources.Load","LoadAssetAtPath","GetField(","GetMethod(","Type.GetType","Shader.Find","Addressables","streamingAssetsPath",".bytes\"")):
                dynamic.append(dict(path=path,line=n,evidence=line.strip()))
save("RawGuidReferencesAfter.json",{g:sorted(v) for g,v in sorted(raw.items())})
save("DuplicateGuidsAfter.json",{g:v for g,v in identities.items() if len(v)>1})
save("DynamicAndToolingReferencesAfter.json",dynamic)
save("ExternalGuidsAfter.json",sorted(g for g in raw if g not in identities and g!="0"*32))
before=load("SourceMembersBefore.json");after=load("SourceMembersAfter.json")
def key(r):return (r["path"],r["parent"],r["kind"],r["name"])
before={key(r):r for r in before if r["kind"]=="MethodDeclarationSyntax"}
after={key(r):r for r in after if r["kind"]=="MethodDeclarationSyntax"}
diff=dict(unchanged=[],changed=[],removed=[],added=[])
for k,r in before.items():
    dest="removed" if k not in after else "unchanged" if r["text"]==after[k]["text"] else "changed"
    diff[dest].append(dict(path=r["path"],name=r["name"]))
for k,r in after.items():
    if k not in before:diff["added"].append(dict(path=r["path"],name=r["name"]))
save("MethodPreservation.json",diff)
review=[]
for row in diff["changed"]:
    k=next(k for k in before if k[0]==row["path"] and k[3]==row["name"])
    review.extend(difflib.unified_diff(before[k]["text"].splitlines(),after[k]["text"].splitlines(),fromfile=row["name"]+" before",tofile=row["name"]+" after",n=2))
(OUT/"MethodReview.diff").write_text("\n".join(review),encoding="utf-8")
before_cs=[p for p in baseline if p.endswith(".cs")];after_cs=[p for p in current if p.endswith(".cs")]
first=lambda p:not p.startswith(("Assets/Samples/","Packages/"))
source=ROOT/"Assets/Scripts/AwakeningApplicationController.cs"
stats=dict(files_before=len(baseline),files_after=len(current),removed_physical_files=len(removed),added_physical_files=len(added),changed_files=len(changed),
           removed_bytes=sum(len(old[p]) if p in old else 0 for p in removed),
           imported_assets_before=len(graph),imported_assets_after=len(native_after),scene_closure_after=len(closure),
           csharp_before=len(before_cs),csharp_after=len(after_cs),firstparty_csharp_before=sum(map(first,before_cs)),firstparty_csharp_after=sum(map(first,after_cs)),
           assets_csharp_before=sum(p.startswith("Assets/") for p in before_cs),assets_csharp_after=sum(p.startswith("Assets/") for p in after_cs),
           csharp_lines_before=sum(len(old[p].splitlines()) for p in before_cs),csharp_lines_after=sum(len((ROOT/p).read_bytes().splitlines()) for p in after_cs),
           controller_lines_before=len(old["Assets/Scripts/AwakeningApplicationController.cs"].splitlines()),controller_lines_after=len(source.read_bytes().splitlines()),
           uxml_before=sum(p.endswith(".uxml") for p in baseline),uxml_after=sum(p.endswith(".uxml") for p in current),uss_before=sum(p.endswith(".uss") for p in baseline),uss_after=sum(p.endswith(".uss") for p in current),
           assets_uxml_before=sum(p.startswith("Assets/") and p.endswith(".uxml") for p in baseline),assets_uxml_after=sum(p.startswith("Assets/") and p.endswith(".uxml") for p in current),assets_uss_before=sum(p.startswith("Assets/") and p.endswith(".uss") for p in baseline),assets_uss_after=sum(p.startswith("Assets/") and p.endswith(".uss") for p in current),
           method_counts={k:len(v) for k,v in diff.items()},classification_counts=dict(Counter(r["classification"] for r in final)))
# Byte-size measurement comes from the recovery tree, including removed binary fonts.
size_text=subprocess.check_output(["git","ls-tree","-rl",commit,"--","Assets","Packages","ProjectSettings"],cwd=ROOT).decode()
sizes={line.split("\t",1)[1]:int(line.split("\t",1)[0].split()[-1]) for line in size_text.splitlines()}
stats["removed_bytes"]=sum(sizes[p] for p in removed)
save("Statistics.json",stats)
print(json.dumps(dict(removed=len(removed),added=added,changed=changed,statistics=stats),ensure_ascii=False))
