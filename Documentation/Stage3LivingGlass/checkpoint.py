"""Archive the reviewed source candidate and original shared sources; never modifies Unity assets."""
from pathlib import Path
import csv
import hashlib
import json
import zipfile

OUT = Path(__file__).resolve().parent
ROOT = OUT.parents[1]
checkpoint = OUT / "Checkpoints"
checkpoint.mkdir(exist_ok=True)
shared = [Path("Assets/Scripts/AwakeningApplicationController.cs"), Path("Assets/Scripts/Interaction/MediaPipeGestureBridge.cs"), Path("Assets/Samples/MediaPipe Unity Plugin/0.16.3/Official Solutions/Scenes/Hand Landmark Detection/HandLandmarkerRunner.cs")]
candidate = [p for p in (ROOT / "Assets/LivingGlass").rglob("*") if p.is_file()]
candidate += [ROOT / "Assets/LivingGlass.meta", ROOT / "Assets/Scenes/AwakeningLivingGlass.unity", ROOT / "Assets/Scenes/AwakeningLivingGlass.unity.meta"]
for relative in shared:
    candidate.extend([ROOT / relative, ROOT / (str(relative) + ".meta")])
manifest = []
with zipfile.ZipFile(checkpoint / "ImplementationCandidate.zip", "w", zipfile.ZIP_DEFLATED) as archive:
    for path in sorted(candidate):
        relative = path.relative_to(ROOT).as_posix()
        archive.write(path, relative)
        manifest.append({"path": relative, "sha256": hashlib.sha256(path.read_bytes()).hexdigest()})
with zipfile.ZipFile(checkpoint / "BaselineSharedSources.zip", "w", zipfile.ZIP_DEFLATED) as archive:
    for relative in shared:
        backup = OUT / "Baseline" / (relative.name + ".txt")
        archive.writestr(relative.as_posix(), backup.read_bytes())
        archive.write(ROOT / (str(relative) + ".meta"), str(relative) + ".meta")
(checkpoint / "CandidateManifest.json").write_text(json.dumps(manifest, indent=2), encoding="utf-8")

with (OUT / "ScreenCoverage.csv").open(encoding="utf-8-sig") as source:
    coverage = list(csv.DictReader(source))
with (OUT / "RuntimeAcceptanceMatrix.csv").open("w", newline="", encoding="utf-8-sig") as target:
    writer = csv.DictWriter(target, fieldnames=["scenario", "route", "locale", "scope", "expected", "status"])
    writer.writeheader()
    for row in coverage:
        writer.writerow({"scenario": "Route / focus / localization / Back", "route": row["route"], "locale": row["locale"], "scope": "Play Mode; camera consent scenario where required", "expected": "Correct family, content, focus and capability policy; existing session rules preserved", "status": "NOT RUN"})
    extra = [
        ("Consent denied / granted / revoked / regranted", "Explicit Play Mode + user camera consent", "No capture before permission or on suspended routes; immediate revoke; one pipeline"),
        ("Point/fist/palm/wave/raised/clap", "Consented hardware gestures", "Existing meanings and serialized thresholds; no universal shell fist click"),
        ("Tracking gap / loss / recovery / input handoff", "Consented hardware gestures", "Initial programmatic focus does not seize hand targeting; deliberate input ownership; stable wrappers and existing release/stability/cooldown semantics"),
        ("Rapid overlay open-close-reopen / modal Tab / focus restore", "Play Mode, camera and network off", "No click-through, duplicate actions, stale callbacks or trapped focus"),
        ("Minimize/resume and locale replacement", "Play Mode; live network only if separately authorized", "Requests canceled; fresh-view behavior explicitly labelled; experience context retained"),
        ("All-six video prepare/first frame/real end/cancel/error/fallback", "Play Mode media verification", "Single operation; real completion; configured fallback only; texture/event cleanup"),
        ("Assistant success/error/retry/cancel/late reply", "Separately authorized live assistant or approved isolated transport fixture", "Same HTTPS contract; correct performer; no detached reply; no invented memory"),
        ("Chinese IME / long replies / keyboard / assistive technology", "Play Mode and deployment accessibility review", "Editing does not navigate; focus/readability remain usable"),
        ("Repeated cycles / original-scene regression", "Play Mode", "No duplicate ownership, resource leak or shared-code behavioral regression"),
        ("Frame pacing / CPU / GPU / allocations / decode / memory", "Separately authorized profiling and Player workload", "Measured evidence against provisional FHD60 target"),
    ]
    for scenario, scope, expected in extra:
        writer.writerow({"scenario": scenario, "route": "shared", "locale": "en + zh", "scope": scope, "expected": expected, "status": "NOT RUN"})
print(f"Archived {len(manifest)} candidate files and three baseline shared sources; wrote 114 pending runtime scenarios.")
