"""Stage3.3A read-only integrity baseline; no Unity launch, hardware or services.

Checks the reviewed native snapshots against current source/package hashes. A stale
snapshot is a failure, never a cached pass. Writes only beside this script. Known
support defects remain FAIL; raw GUID absence is reported separately from impact.
"""
from pathlib import Path
from collections import Counter
import hashlib
import json
import re

OUT = Path(__file__).resolve().parent
ROOT = OUT.parents[3]

def load(name):
    return json.loads((OUT / name).read_text(encoding="utf-8-sig"))

checks = []

def check(name, passed, detail=None):
    checks.append(dict(check=name, status="PASS" if passed else "FAIL", detail=detail))

baseline = load("BaselineHashes.json")
current_paths = {p.relative_to(ROOT).as_posix() for top in ["Assets", "Packages", "ProjectSettings", "awakening-ai-backend"]
                 if (ROOT / top).exists() for p in (ROOT / top).rglob("*") if p.is_file()
                 and not any(part in {"node_modules", ".git", "__pycache__"} for part in p.parts)}
changed = [path for path, digest in baseline.items()
           if not (ROOT / path).exists() or hashlib.sha256((ROOT / path).read_bytes()).hexdigest() != digest]
check("all2045 application/asset/config/backend files unchanged", len(baseline) == 2045 and not changed
      and current_paths == set(baseline), dict(changed=changed, added=sorted(current_paths - baseline.keys()), removed=sorted(baseline.keys() - current_paths)))
inputs = load("EvidenceInputHashes.json")
stale = [path for path, digest in inputs.items()
         if not (ROOT / path).exists() or hashlib.sha256((ROOT / path).read_bytes()).hexdigest() != digest]
check("reviewed source/package evidence is current", not stale, stale)

findings = load("GuidFindings.json")
ids = set(OUT.joinpath("GuidPatterns.txt").read_text().splitlines())
native = load("NativeResolutionNow.json")
check("exact14 original GUIDs covered once", len(ids) == len(findings) == len(native) == 14
      and ids == {r["guid"] for r in findings} == {r["guid"] for r in native})
occurrences = load("SerializedOccurrences.json")
bad_context = []
for row in occurrences:
    lines = (ROOT / row["path"]).read_text(encoding="utf-8-sig").splitlines()
    if row["guid"] not in lines[row["line"] - 1]:
        bad_context.append(row)
check("reference source locations still match", not bad_context, bad_context)

schema = {r["property"]: r for r in load("RendererSchema.json")["properties"]}
deprecated = load("DeprecatedFieldProof.json")[0]
check("legacy probe fields carry native Obsolete metadata", bool(deprecated["obsolete"])
      and all(child["obsolete"] for child in deprecated["children"])
      and sum(r["property"].startswith("probeVolumeResources.") for r in findings) == 6)
check("old falloff field absent from current serialized schema", not schema["m_FallOffLookup"]["exists"])
defaults = {r["name"]: r for r in load("BootstrapImporter.json")}
check("old Bootstrap importer defaults have no fields", all(not defaults[n]["fieldExists"]
      for n in ["globalConfigManagerPrefab", "gpuManagerPrefab"]))
modal = load("ModalOverrideSchema.json")[0]
check("sample ModalButton consumes _contents, not contents", not modal["oldFieldExists"] and modal["currentFieldExists"])
pairs = load("SampleOverridePairs.json")
resolved = {r["guid"]: r["path"] for r in load("CurrentSampleOverrideResolution.json")}
file_ids = {r["path"]: r["fileId"] for r in load("CurrentOverrideFileIds.json") if r["valid"]}
check("all eight obsolete override sites have valid current overrides", len(pairs) == 8
      and all(resolved.get(r["current_guid"]) and file_ids.get(resolved[r["current_guid"]]) == r["current_fileID"] for r in pairs))
scenes = load("BothSceneSerializedScope.json")
check("both scenes directly contain none of these14 GUIDs", len(scenes) == 2
      and all(not row["directFourteenGuids"] for row in scenes)
      and scenes[0]["bootstrapAssignments"] == scenes[1]["bootstrapAssignments"])
prefabs = load("PrefabReadOnlyInspection.json")
check("current bootstrap prefabs retain valid AppSettings", sum(bool(p.get("value"))
      for r in prefabs for p in r.get("properties", []) if p["name"] == "_appSettings") == 2)

counts = Counter(r["category"] for r in findings)
support = [r["guid"] for r in findings if r["genuine_missing_live_reference"]]
raw = [r["guid"] for r in native if not r["path"]]
check("four known support/optional references resolve", not support, support)
summary = dict(scope="Only the investigated14 references; not whole-project runtime acceptance",
               checks=checks, categories=dict(counts), raw_unresolved_guids=raw,
               genuine_support_optional_defects=support,
               confirmed_active_application_broken=[r["guid"] for r in findings if r["active_application_broken"]],
               runtime="NOT RUN", repairs_applied=0,
               note="Ten obsolete entries are false positives only when interpreted as active runtime defects. All14 GUIDs remain genuinely non-resolving; four support defects stay FAIL.")
(OUT / "IntegrityBaseline.json").write_text(json.dumps(summary, indent=2, ensure_ascii=False), encoding="utf-8")
print(json.dumps(dict(checks=len(checks), passed=sum(r["status"] == "PASS" for r in checks),
                     failed=[r["check"] for r in checks if r["status"] == "FAIL"],
                     raw_unresolved=len(raw), categories=dict(counts),
                     confirmed_active_broken=len(summary["confirmed_active_application_broken"]))))
raise SystemExit(1 if any(r["status"] == "FAIL" for r in checks) else 0)
