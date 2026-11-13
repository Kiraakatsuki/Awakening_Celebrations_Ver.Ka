"""Stage3.3B read-only preservation checks; writes only this evidence directory.
Native snapshots are supplied by the idle Editor. No Unity launch, runtime,
hardware, service, asset mutation or GUID suppression is performed here.
"""
from pathlib import Path
import hashlib
import json
import re
import xml.etree.ElementTree as ET

OUT = Path(__file__).resolve().parent
ROOT = OUT.parents[3]

def load(name):
    return json.loads((OUT / name).read_text(encoding="utf-8-sig"))

checks = []

def check(name, passed, detail=None):
    checks.append(dict(check=name, status="PASS" if passed else "FAIL", detail=detail))

baseline = load("BaselineHashes.json")
files = [p for top in ["Assets", "Packages", "ProjectSettings", "awakening-ai-backend"]
         if (ROOT / top).exists() for p in (ROOT / top).rglob("*") if p.is_file()
         and not any(x in {"node_modules", ".git", "__pycache__"} for x in p.parts)]
current = {p.relative_to(ROOT).as_posix(): hashlib.sha256(p.read_bytes()).hexdigest() for p in files}
expected = set(load("ExpectedChangedFiles.json"))
changed = {p for p in baseline if p in current and baseline[p] != current[p]}
check("exact eight-file scope; no asset/code additions or deletions", current.keys() == baseline.keys()
      and changed == expected, sorted(changed))
check("native snapshot corresponds to current file hashes", current == load("HashesAfter.json"))
check("all CSharp and all other2037 files preserved", len(current) == 2045
      and all(current[p] == h for p, h in baseline.items() if p not in expected)
      and not any(p.endswith(".cs") for p in changed))

renderer = "Assets/Settings/Renderer2D.asset"
old = (OUT / "Before" / renderer).read_text(encoding="utf-8")
old_lines = old.splitlines(True)
start = old_lines.index("  probeVolumeResources:\n")
old_without_probe = old_lines[:start] + old_lines[start+7:]
wanted = "".join(line for line in old_without_probe if not line.startswith("  m_FallOffLookup:"))
check("renderer diff contains only the obsolete resource block and falloff line", (ROOT / renderer).read_text(encoding="utf-8") == wanted)
meta = "Assets/Samples/MediaPipe Unity Plugin/0.16.3/Official Solutions/Common/Scripts/Bootstrap.cs.meta"
old = (OUT / "Before" / meta).read_text(encoding="utf-8")
start, end = old.index("  defaultReferences:\n"), old.index("  executionOrder:")
wanted = old[:start] + "  defaultReferences: []\n" + old[end:]
check("Bootstrap diff removes only stale default-reference names", (ROOT / meta).read_text(encoding="utf-8") == wanted)
removals = load("SampleOverrideRemovals.json")
samples_ok = len(removals) == 6
for row in removals:
    old = (OUT / "Before" / row["path"]).read_text(encoding="utf-8")
    new = (ROOT / row["path"]).read_text(encoding="utf-8")
    block = row["removed_block"]
    samples_ok &= ("propertyPath: contents\n" in block and "propertyPath: _contents" not in block
                   and old.count(block) == 1 and old.replace(block, "") == new
                   and old.count("propertyPath: _contents") == new.count("propertyPath: _contents"))
check("six sample diffs remove only dead contents overrides", samples_ok)

meta_changed = [p for p in changed if p.endswith(".meta")]
old_identity = re.search(r"^guid: (\w+)", (OUT / "Before" / meta).read_text(), re.M)[1]
new_identity = re.search(r"^guid: (\w+)", (ROOT / meta).read_text(), re.M)[1]
check("metadata identities preserved", meta_changed == [meta] and old_identity == new_identity)
def graph(name):
    return {r["path"]: (r["guid"], sorted(r["dependencies"])) for r in load(name)}
check("all591 native asset GUID/dependency entries unchanged", graph("NativeGraphBefore.json") == graph("NativeGraphAfter.json"))
check("both scene dependency closures unchanged", all((OUT / (n+"Before.txt")).read_bytes() == (OUT / (n+"After.txt")).read_bytes()
      for n in ["SampleScene", "AwakeningLivingGlass"]))

native = load("BatchB/NativeConfigurationChecks.json")
check("native retired slots empty and Bootstrap identity intact", all(r["objectNull"] and r["instanceID"] == 0 for r in native["rendererLegacy"])
      and not native["rendererDirty"] and native["bootstrapGUID"] == old_identity
      and all(not r["fieldExists"] and r["defaultNull"] for r in native["defaults"])
      and not native["playing"] and not native["dirty"])
raw = load("RawGuidReferencesAfter.json")
resolution = load("RawGuidResolutionAfter.json")
missing = {r["guid"] for r in resolution if not r["path"]}
before = {r["guid"] for r in load("NativeGuidBefore.json") if not r["path"]}
check("no new unresolved GUIDs; exactly nine retired from scan roots", missing.issubset(before)
      and len(missing) == 5 and len(before-missing) == 9, sorted(missing))
retained_old = "8236d46ca3a43d373ae6ad281fea8fe0"
check("remaining obsolete override is confined to protected recovery files", set(raw[retained_old]) == {"Assets/_Recovery/0 (1).unity", "Assets/_Recovery/1.unity"})

bad_ui = []
uxml = list((ROOT / "Assets").rglob("*.uxml"))
uss = list((ROOT / "Assets").rglob("*.uss"))
for p in uxml:
    try:
        ET.fromstring(p.read_text(encoding="utf-8-sig"))
    except ET.ParseError as e:
        bad_ui.append(str(e))
for guid in missing:
    bad_ui.extend(p for p in raw[guid] if p.endswith((".uxml", ".uss", ".tss")))
check("104 UXML parse and100 USS have no unresolved GUID dependencies", len(uxml) == 104 and len(uss) == 100 and not bad_ui, bad_ui)
check("existing24 static checks and original mapping validator pass", all(r["status"] == "PASS" for r in load("BatchB/StaticVerification.json"))
      and not load("BatchB/FigmaMappingChecks.json"))

result = dict(regression_checks=checks, regression_status="PASS" if all(c["status"] == "PASS" for c in checks) else "FAIL",
              strict_raw_guid_resolution=dict(status="FAIL" if missing else "PASS", distinct_guids=len(missing), sources={g: raw[g] for g in sorted(missing)}),
              classified_findings=dict(genuine_support_optional_issues=4, protected_obsolete_recovery_guid=1, confirmed_active_application_broken=0),
              note="Raw missing references are not suppressed; no runtime behavior or visual acceptance was tested.")
(OUT / "RetirementValidation.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
print(json.dumps(dict(regression_checks=len(checks), regression_status=result["regression_status"], failed=[r["check"] for r in checks if r["status"] == "FAIL"], raw_guid_status=result["strict_raw_guid_resolution"]["status"], raw_guid_count=len(missing))))
raise SystemExit(1 if result["regression_status"] != "PASS" or missing else 0)
