"""Read-only dependency evidence. Writes only this Cleanup directory; never runs Unity.

Native AssetDatabase evidence is supplied separately by the idle Editor. Zero incoming
edges are a triage signal, never a deletion instruction. Run before/after code batches.
"""
from pathlib import Path
from collections import Counter, defaultdict
import csv
import hashlib
import json
import re
import sys

OUT = Path(__file__).resolve().parent
ROOT = OUT.parents[2]
phase = sys.argv[1] if len(sys.argv) > 1 else "after"
if phase not in {"before", "after"}:
    raise SystemExit("Use before or after")
native = json.loads((OUT / "UnityAssetGraph.json").read_text(encoding="utf-8-sig"))
graph = {a["path"]: a for a in native}
closures = [set((OUT / name).read_text(encoding="utf-8-sig").splitlines()) for name in
            ["SampleSceneDependencies.txt", "AwakeningLivingGlassDependencies.txt"]]
old, new = closures
incoming = defaultdict(list)
for a in native:
    for d in a["dependencies"]:
        incoming[d].append(a["path"])

files = [p for top in ("Assets", "Packages", "ProjectSettings", "awakening-ai-backend")
         if (ROOT / top).exists() for p in (ROOT / top).rglob("*")
         if p.is_file() and not any(x in {"node_modules", ".git", "__pycache__"} for x in p.parts)]
hashes = {p.relative_to(ROOT).as_posix(): hashlib.sha256(p.read_bytes()).hexdigest() for p in files}
if phase == "before" and (OUT / "AllFilesBefore.json").exists() and json.loads((OUT / "AllFilesBefore.json").read_text()):
    raise SystemExit("Baseline already exists; will not overwrite")
(OUT / ("AllFilesBefore.json" if phase == "before" else "AllFilesAfter.json")).write_text(json.dumps(hashes, indent=2), encoding="utf-8")
texts = {}
extensions = {".meta", ".unity", ".asset", ".prefab", ".mat", ".uxml", ".uss", ".json", ".cs", ".shader", ".shadergraph", ".hlsl", ".cginc", ".scenetemplate", ".asmdef", ".asmref", ".tss", ".inputactions", ".txt", ".xml"}
for p in files:
    if p.suffix in extensions:
        try:
            texts[p.relative_to(ROOT).as_posix()] = p.read_text(encoding="utf-8-sig")
        except UnicodeError:
            pass
guid_paths = {}
for path, s in texts.items():
    if path.endswith(".meta"):
        m = re.search(r"^guid: ([0-9a-f]{32})", s, re.M)
        if m:
            guid_paths[m[1]] = path[:-5]
raw = defaultdict(set)
for path, s in texts.items():
    # Meta's own top-level GUID is identity, not an incoming dependency.
    if path.endswith(".meta"):
        s = re.sub(r"^guid: [0-9a-f]{32}.*$", "", s, flags=re.M)
    for guid in re.findall(r"\bguid\s*[:=]\s*([0-9a-f]{32})", s):
        raw[guid].add(path)
sourcefiles = {p: s for p, s in texts.items() if p.endswith(".cs")}
dynamic = []
for path, s in sourcefiles.items():
    for number, line in enumerate(s.splitlines(), 1):
        if any(term in line for term in ("Resources.Load", "LoadAssetAtPath", "GetField(", "GetMethod(", "Type.GetType", "Shader.Find", "Addressables", "StreamingAssets", "streamingAssetsPath", ".bytes\"")):
            dynamic.append({"path": path, "line": number, "evidence": line.strip()})
(OUT / "DynamicAndToolingReferences.json").write_text(json.dumps(dynamic, indent=2, ensure_ascii=False), encoding="utf-8")
(OUT / "SerializedGuidReferences.json").write_text(json.dumps({g: sorted(v) for g, v in sorted(raw.items())}, indent=2), encoding="utf-8")
unknown = sorted(g for g in raw if g not in guid_paths and g != "0" * 32)
(OUT / "ExternalGuids.json").write_text(json.dumps(unknown), encoding="utf-8")

# ProjectSettings is an additional root set, not a scene dependency. Follow its
# imported dependencies so pipeline renderer/material/text assets are protected.
settings_closure = {guid_paths[g] for g, refs in raw.items() if g in guid_paths
                    and any(p.startswith("ProjectSettings/") for p in refs)}
pending = list(settings_closure)
while pending:
    for dependency in graph.get(pending.pop(), {}).get("dependencies", []):
        if dependency not in settings_closure:
            settings_closure.add(dependency); pending.append(dependency)
(OUT / "ProjectSettingsDependencies.txt").write_text("\n".join(sorted(settings_closure)) + "\n", encoding="utf-8")

rows = []
for p in sorted(ROOT.joinpath("Assets").rglob("*")):
    if not p.is_file() or p.suffix == ".meta":
        continue
    path = p.relative_to(ROOT).as_posix()
    guid = graph.get(path, {}).get("guid", next((g for g, v in guid_paths.items() if v == path), ""))
    refs = sorted(raw.get(guid, set())) if guid else []
    if path in old and path in new:
        cls, reason = "KEEP — SHARED", "Native dependency closure of both scenes"
    elif path in old:
        cls, reason = "KEEP — ROLLBACK", "Native dependency closure of protected SampleScene"
    elif path in new or path.startswith("Assets/LivingGlass/"):
        cls, reason = "KEEP — ACTIVE", "Living Glass presentation, configuration, provenance or supporting Editor workflow"
    elif "/Resources/" in path or "/StreamingAssets/" in path:
        cls, reason = "KEEP — SHARED", "Dynamically loadable resource/model; scene-only graph is insufficient"
    elif path.startswith("Assets/Screenshots/"):
        cls, reason = "KEEP — ROLLBACK", "Historical visual validation evidence; no documentation deletion authorized"
    elif path.startswith("Assets/_Recovery/"):
        cls, reason = "UNRESOLVED", "Ignored user/Editor recovery scene; independent recovery value not established"
    elif path.endswith(".cs") or path.endswith(".asmdef") or path.startswith("Assets/Samples/"):
        cls, reason = "KEEP — SHARED", "Compiled code, Editor entry point or imported sample workflow; not scene-only dependencies"
    elif path in settings_closure:
        cls, reason = "KEEP — SHARED", "Transitive ProjectSettings dependency (build/input/render pipeline/tooling)"
    elif path.endswith("/.figma-manifest.json"):
        cls, reason = "KEEP — ROLLBACK", "Read by the original-scene Figma mapping and prebuild validator; hidden from AssetDatabase"
    elif path.endswith("OFL.txt") or " - OFL.txt" in path:
        cls, reason = "KEEP — SHARED", "Font license/provenance must accompany retained font assets"
    else:
        cls, reason = "UNRESOLVED", "No deletion proof across indirect shader/font/tooling and supported workflows"
    rows.append({"path": path, "guid": guid, "classification": cls, "reason": reason,
                 "sample_scene_closure": path in old, "living_glass_closure": path in new,
                 "native_incoming_count": len(incoming[path]), "native_incoming": incoming[path],
                 "serialized_incoming_count": len(refs), "serialized_incoming": refs,
                 "sha256": hashes[path], "bytes": p.stat().st_size})
(OUT / "AssetInventory.json").write_text(json.dumps(rows, indent=2, ensure_ascii=False), encoding="utf-8")
with (OUT / "AssetInventory.csv").open("w", encoding="utf-8-sig", newline="") as f:
    keys = [k for k in rows[0] if k not in {"native_incoming", "serialized_incoming"}]
    w = csv.DictWriter(f, fieldnames=keys, extrasaction="ignore"); w.writeheader(); w.writerows(rows)
zero = [r for r in rows if not r["sample_scene_closure"] and not r["living_glass_closure"] and not r["native_incoming_count"]]
(OUT / "CandidateTriage.json").write_text(json.dumps(zero, indent=2, ensure_ascii=False), encoding="utf-8")
summary = {"phase": phase, "native_assets": len(native), "physical_assets": len(rows), "hashed_files": len(hashes),
           "scene_closures_including_packages": {"SampleScene": len(old), "LivingGlass": len(new), "intersection": len(old & new)},
           "classifications": dict(Counter(r["classification"] for r in rows)),
           "dynamic_tooling_reference_lines": len(dynamic), "external_guids_to_resolve": len(unknown),
           "source_files": len(sourcefiles), "zero_incoming_triage": len(zero)}
if phase == "after":
    before = json.loads((OUT / "AllFilesBefore.json").read_text())
    summary["changed"] = [p for p in before if p in hashes and before[p] != hashes[p]]
    summary["removed"] = sorted(set(before) - hashes.keys())
    summary["added"] = sorted(hashes.keys() - before.keys())
(OUT / ("AuditBefore.json" if phase == "before" else "AuditAfter.json")).write_text(json.dumps(summary, indent=2, ensure_ascii=False), encoding="utf-8")
print(json.dumps(summary, ensure_ascii=False))
