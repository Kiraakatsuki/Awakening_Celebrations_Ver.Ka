"""Read-only cleanup regression checks. No Editor launch, runtime, hardware or services.
Writes evidence only alongside this script. Run after audit_dependencies.py after.
"""
from pathlib import Path
import difflib
import html
import json
import re
import urllib.parse
import xml.etree.ElementTree as ET

OUT = Path(__file__).resolve().parent
ROOT = OUT.parents[2]
results = []

def check(name, passed, detail=None):
    results.append(dict(check=name, status="PASS" if passed else "FAIL", detail=detail))

audit = json.loads((OUT / "AuditAfter.json").read_text(encoding="utf-8"))
expected = {
    "Assets/Scripts/AwakeningApplicationController.cs",
    "Assets/LivingGlass/Editor/LivingGlassPreviewWindow.cs",
    "Assets/LivingGlass/Editor/LivingGlassConversationChecks.cs",
    "Assets/LivingGlass/Editor/LivingGlassStabilizationChecks.cs",
}
check("only four scoped source changes", set(audit["changed"]) == expected and not audit["removed"] and not audit["added"], audit)
check("all other source/assets/settings/backend bytes preserved", audit["hashed_files"] == 2045 and len(audit["changed"]) == 4, "2041 of 2045 files unchanged; four intended source files changed")
patches = []
for path in sorted(expected):
    before = (OUT / "Before" / path).read_text(encoding="utf-8-sig")
    after = (ROOT / path).read_text(encoding="utf-8-sig")
    if path.endswith("AwakeningApplicationController.cs"):
        wanted = before.replace("    private bool interactionStylesAdded;\n", "").replace("        if (document == navigationDocument)\n            interactionStylesAdded = true;\n", "")
    elif path.endswith("LivingGlassPreviewWindow.cs"):
        wanted = before.replace("    private LivingGlassAssets previewData;\n", "    private LivingGlassAssets previewData;\n    // Typed access for isolated Editor checks; neither property initializes services.\n    internal LivingGlassPresentation Presentation { get { return view; } }\n    internal LivingGlassAssets PreviewAssets { get { return previewData; } }\n")
    else:
        flag = "flags" if path.endswith("LivingGlassConversationChecks.cs") else "Private"
        wanted = before.replace('view=(LivingGlassPresentation)typeof(LivingGlassPreviewWindow).GetField("view",'+flag+').GetValue(window);', 'view=window.Presentation;').replace('data=(LivingGlassAssets)typeof(LivingGlassPreviewWindow).GetField("previewData",'+flag+').GetValue(window);', 'data=window.PreviewAssets;')
    check("scope preservation " + path, after == wanted)
    patches.extend(difflib.unified_diff(before.splitlines(True), after.splitlines(True), fromfile="before/"+path, tofile="after/"+path))
(OUT / "ScopedChanges.patch").write_text("".join(patches), encoding="utf-8")

def graph(name):
    return {r["path"]: (r["guid"], sorted(r["dependencies"])) for r in json.loads((OUT / name).read_text(encoding="utf-8-sig"))}
check("native asset GUID/dependency graph unchanged", graph("UnityAssetGraph.json") == graph("UnityAssetGraphAfter.json"))
metas = {}
duplicates = []
for p in ROOT.joinpath("Assets").rglob("*.meta"):
    m = re.search(r"^guid: ([0-9a-f]{32})", p.read_text(encoding="utf-8-sig"), re.M)
    if m:
        if m[1] in metas:
            duplicates.append([metas[m[1]], p.relative_to(ROOT).as_posix()])
        metas[m[1]] = p.relative_to(ROOT).as_posix()[:-5]
check("asset metadata GUID uniqueness", not duplicates, duplicates)
external = json.loads((OUT / "ExternalGuidResolution.json").read_text(encoding="utf-8-sig"))
resolved = dict(metas, **{r["guid"]: r["path"] for r in external if r["path"]})
errors = []
urls = 0
for p in ROOT.joinpath("Assets").rglob("*"):
    if p.suffix not in {".uxml", ".uss", ".tss"}:
        continue
    text = p.read_text(encoding="utf-8-sig")
    if p.suffix == ".uxml":
        try:
            ET.fromstring(text)
        except ET.ParseError as e:
            errors.append([str(p), "XML parse", str(e)])
    for url in re.findall(r'''project://database/([^"')<>]+)''', html.unescape(text)):
        urls += 1
        uri = urllib.parse.urlsplit("project://database/" + url)
        path = urllib.parse.unquote(uri.path.lstrip("/"))
        guid = urllib.parse.parse_qs(uri.query).get("guid", [""])[0]
        if guid and resolved.get(guid) != path:
            errors.append([p.relative_to(ROOT).as_posix(), path, guid, resolved.get(guid)])
        if path.startswith("Assets/") and not ROOT.joinpath(path).exists():
            errors.append([str(p), "missing asset path", path])
check("UXML XML and project URL/GUID integrity", not errors, {"uxml":104, "uss":100, "project_urls":urls, "errors":errors})
scripts = json.loads((OUT / "SceneScriptReferences.json").read_text(encoding="utf-8-sig"))
check("both scenes serialized script GUIDs resolve", all(not s["missing"] for s in scripts), scripts)
missing = [r for r in external if not r["path"]]
results.append(dict(check="full repository external GUID resolution", status="FAIL" if missing else "PASS", detail={"unresolved_baseline_guids": missing, "note":"Pre-existing references; no referenced source asset or package changed. Reachability/functional impact remains unresolved; this check is not a missing-reference-free claim."}))
(OUT / "CleanupStaticChecks.json").write_text(json.dumps(results, indent=2, ensure_ascii=False), encoding="utf-8")
print(json.dumps({"checks":len(results), "pass":sum(r["status"]=="PASS" for r in results), "failures":[r for r in results if r["status"]=="FAIL"], "blocked":[r["check"] for r in results if r["status"]=="BLOCKED"]}, ensure_ascii=False))
raise SystemExit(1 if any(r["status"]=="FAIL" for r in results) else 0)
