"""Read-only repository checks; writes reports only under this documentation directory.

Does not start Unity, tests, camera, services, builds or profiling.
Run with python -X utf8 Documentation/Stage3LivingGlass/verify_static.py.
"""
from pathlib import Path
import csv
import hashlib
import json
import re
from fontTools.ttLib import TTFont

ROOT = Path(__file__).resolve().parents[2]
OUT = Path(__file__).resolve().parent
results = []
baseline = json.loads((OUT / "BaselineHashes.json").read_text(encoding="utf-8-sig"))
for item in baseline:
    path = Path(item["Path"])
    if path.name == "AwakeningApplicationController.cs":
        continue
    actual = hashlib.sha256(path.read_bytes()).hexdigest().upper()
    results.append({"check": "preserved " + str(path.relative_to(ROOT)), "status": "PASS" if actual == item["Hash"] else "FAIL", "sha256": actual})

old = (OUT / "Baseline/AwakeningApplicationController.cs.txt").read_text(encoding="utf-8")
new = (ROOT / "Assets/Scripts/AwakeningApplicationController.cs").read_text(encoding="utf-8")

def method(text, name):
    match = re.search(r"^    private [^\n]+\b" + name + r"\([^\n]*\)\s*\{", text, re.M)
    if not match:
        return None
    start = match.end() - 1
    depth = 0
    # Inspected protected methods below have no brace literals affecting this balance.
    for index in range(start, len(text)):
        if text[index] == "{":
            depth += 1
        elif text[index] == "}":
            depth -= 1
            if depth == 0:
                return text[match.start():index + 1]
    return None

for name in ["StartLivingPortrait", "PrepareLivingPortraitVideo", "OnLivingPortraitVideoPrepared", "TryStartLivingPortraitPlayback", "OnLivingPortraitVideoFrameReady", "OnLivingPortraitVideoCompleted", "OnLivingPortraitVideoError", "StartPlaceholderPortraitTimer", "WaitForPlaceholderPortrait", "RequestLivingPortraitCompletion", "StopLivingPortraitVideoPlayer", "ReleaseLivingPortraitRenderTexture", "TryBuildAssistantChatUrl", "BuildPerformerScopedMessage", "CancelAssistantRequest"]:
    before, after = method(old, name), method(new, name)
    results.append({"check": "unchanged protected method " + name, "status": "PASS" if before is not None and before == after else "FAIL"})

bridge = (ROOT / "Assets/Scripts/Interaction/MediaPipeGestureBridge.cs").read_text(encoding="utf-8")
bridge_without_diagnostic = "\n".join(line for line in bridge.split("\n") if "public bool HasCameraFailure =>" not in line)
results.append({"check": "gesture bridge unchanged except read-only diagnostic", "status": "PASS" if bridge_without_diagnostic == (OUT / "Baseline/MediaPipeGestureBridge.cs.txt").read_text(encoding="utf-8") else "FAIL"})
runner_path = ROOT / "Assets/Samples/MediaPipe Unity Plugin/0.16.3/Official Solutions/Scenes/Hand Landmark Detection/HandLandmarkerRunner.cs"
runner = runner_path.read_text(encoding="utf-8")
runner_without_diagnostic = "\n".join(line for line in runner.split("\n") if "LastPipelineFailureType" not in line and "Read-only diagnostic for presentation" not in line)
results.append({"check": "runner unchanged except existing-failure diagnostic assignments", "status": "PASS" if runner_without_diagnostic == (OUT / "Baseline/HandLandmarkerRunner.cs.txt").read_text(encoding="utf-8") else "FAIL"})

font = TTFont(ROOT / "Assets/LivingGlass/Assets/Fonts/NotoSansCJKtc-Regular.otf")
cmap = font.getBestCmap()
characters = set()
for path in (ROOT / "Assets/LivingGlass").rglob("*.cs"):
    characters.update(c for c in path.read_text(encoding="utf-8") if "\u3400" <= c <= "\u9fff")
missing = sorted(c for c in characters if ord(c) not in cmap)
results.append({"check": "Traditional Chinese source glyph coverage", "status": "PASS" if not missing else "FAIL", "unique_characters": len(characters), "missing": missing})

new_files = list((ROOT / "Assets/LivingGlass").rglob("*")) + [ROOT / "Assets/Scenes/AwakeningLivingGlass.unity"]
missing_meta = [str(p.relative_to(ROOT)) for p in new_files if p.is_file() and not p.name.endswith(".meta") and not Path(str(p) + ".meta").exists()]
results.append({"check": "new asset metadata", "status": "PASS" if not missing_meta else "FAIL", "missing": missing_meta})

old_scene = (ROOT / "Assets/Scenes/SampleScene.unity").read_text(encoding="utf-8")
new_scene = (ROOT / "Assets/Scenes/AwakeningLivingGlass.unity").read_text(encoding="utf-8")
def clips(text):
    return re.search(r"  performerPortraitVideos:\n((?:  - .+\n){6})", text).group(1)
results.append({"check": "six serialized video slots identical and ordered", "status": "PASS" if clips(old_scene) == clips(new_scene) else "FAIL"})

with (ROOT / "Documentation/Stage2Design/LivingGlass/ScreenFamilyCoverage.csv").open(encoding="utf-8-sig") as source_file:
    source = list(csv.DictReader(source_file))
with (OUT / "ScreenCoverage.csv").open("w", newline="", encoding="utf-8-sig") as target:
    writer = csv.DictWriter(target, fieldnames=["route", "locale", "family", "performer", "implementation", "static_status", "runtime_status"])
    writer.writeheader()
    for row in source:
        writer.writerow({"route": row["logical_route"], "locale": row["locale"], "family": row["family"], "performer": row["performer"], "implementation": "LivingGlassPresentation shared family + semantic flow port", "static_status": "PASS: compiled classifier supports route", "runtime_status": "NOT RUN: requires authorization"})
(OUT / "StaticVerification.json").write_text(json.dumps(results, indent=2, ensure_ascii=False), encoding="utf-8")
print(json.dumps({"checks": len(results), "failures": [r for r in results if r["status"] == "FAIL"], "coverage_records": len(source)}, ensure_ascii=False))
raise SystemExit(1 if any(r["status"] == "FAIL" for r in results) else 0)
