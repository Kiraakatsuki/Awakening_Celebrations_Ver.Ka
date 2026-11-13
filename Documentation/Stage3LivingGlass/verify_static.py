"""Current static gate. Historical checker: Cleanup/FinalLegacyRetirement/Before/Documentation/Stage3LivingGlass/verify_static.py.
No Unity launch, Play Mode, hardware, services or builds. Requires fresh native evidence.
Exit1 remains intentional while the five known raw GUID failures remain unresolved.
"""
from pathlib import Path
import csv
import json
import runpy

OUT = Path(__file__).resolve().parent
phase = OUT / "Cleanup/FinalLegacyRetirement"
try:
    runpy.run_path(str(phase / "verify_current.py"), run_name="__main__")
except SystemExit as result:
    code = result.code
else:
    code = 0
report = json.loads((phase / "Validation.json").read_text(encoding="utf-8"))
(OUT / "StaticVerification.json").write_text(json.dumps(report["checks"], indent=2, ensure_ascii=False), encoding="utf-8")
fixture = json.loads((OUT.parents[1] / "Assets/LivingGlass/Editor/RouteContract.json").read_text())["entries"]
with (OUT / "ScreenCoverage.csv").open("w", newline="", encoding="utf-8-sig") as target:
    writer = csv.DictWriter(target, fieldnames=["route", "locale", "implementation", "static_status", "runtime_status"])
    writer.writeheader()
    for row in fixture:
        writer.writerow(dict(**row, implementation="LivingGlassPresentation shared families + single flow owner", static_status="PASS: native pure route contract", runtime_status="NOT RUN: post-retirement runtime authorization required"))
raise SystemExit(code)
