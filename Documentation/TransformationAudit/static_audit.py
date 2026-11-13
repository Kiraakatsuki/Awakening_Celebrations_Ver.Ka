"""Stage 1 filesystem-only evidence. Reads assets; writes only beside this script.
Run from project root: python Documentation/TransformationAudit/static_audit.py
No Unity, camera, network, package import, or application execution.
"""
from pathlib import Path
import collections
import csv
import json
import re
import subprocess
import xml.etree.ElementTree as ET
from urllib.parse import unquote

ROOT = Path(__file__).resolve().parents[2]
OUT = Path(__file__).resolve().parent
FIGMA = ROOT / 'Assets/Figma/Awakening-Celebrations---Visual-Redesign'
def read(p):
    return p.read_text(encoding='utf-8-sig')
def tag(e):
    return e.tag.rsplit('}', 1)[-1]
def children(e):
    return [c for c in e if tag(c) not in ('Style', 'Template')]
def resolve(root, path):
    e = root
    for part in path.split('/')[1:]:
        cs = children(e)
        i = int(part) - 1
        if i < 0 or i >= len(cs):
            return None
        e = cs[i]
    return e
def remap(sid, lang, path):
    if sid == 'SOUVENIR---QR-DELIVERY' and path in ('1/7/7', '1/7/8'):
        return path.replace('1/7/', '1/6/')
    if sid == 'SETTINGS---EXPERIENCE-CONTROLS' and lang == 'en':
        if path == '1/7/35': return '1/5/36'
        if path == '1/9': return '1/7'
        if path == '1/10' or path.startswith('1/10/'): return '1/8' + path[4:]
        if path.startswith('1/8/'): return '1/6/' + path[4:]
        if path.startswith('1/7/'): return '1/5/' + path[4:]
    return path

metas = {}
for base in ('Assets', 'Packages'):
    for p in (ROOT / base).rglob('*.meta'):
        m = re.search(r'^guid: ([a-f0-9]{32})', read(p), re.M)
        if m: metas[m[1]] = p.with_suffix('').relative_to(ROOT).as_posix()
catalog = re.findall(r'- screenId: "([^"]+)"\s+language: (\w+)\s+figmaRootNodeId: "([^"]+)"\s+asset: \{[^}]*guid: ([a-f0-9]+)', read(ROOT / 'Assets/Resources/RuntimeScreenCatalog.asset'))
manifest = json.loads(read(FIGMA / '.figma-manifest.json'))['screens']
snapshot = json.loads(read(ROOT / 'Assets/Resources/RuntimeNodeBindings.json'))['screens']
by_root = {s['rootNodeId']: s for s in snapshot}
def mapping(s):
    return (s['rootNodeId'], s['assetName'], [tuple(n.get(k, '') for k in ('figmaNodeId','path','tag','classes')) for n in s['nodes']])
drift, inventory, missing_refs, text_rows = [], [], [], []
xml_errors = []
absolute = total = small_text = 0
for sid, lang, rid, guid in catalog:
    path = metas.get(guid, '')
    if not path or not (ROOT / path).exists():
        inventory.append(dict(screen=sid, locale=lang, root=rid, asset=path, status='missing'))
        continue
    try: tree = ET.parse(ROOT / path).getroot()
    except ET.ParseError as exc:
        xml_errors.append([path, str(exc)])
        continue
    roots = children(tree)
    root = roots[0] if len(roots) == 1 else tree
    bad = 0
    for node in by_root.get(rid, {}).get('nodes', []):
        rp = remap(sid, lang, node['path'])
        e = resolve(root, rp)
        actual = tag(e) if e is not None else 'MISSING'
        if actual != node['tag']:
            bad += 1
            drift.append(dict(screen=sid, locale=lang, node=node['figmaNodeId'], canonical=node['path'], runtime=rp, expected=node['tag'], actual=actual))
    inventory.append(dict(screen=sid, locale=lang, root=rid, asset=path, status='resolved', structural_drift=bad))
    for e in tree.iter():
        style = e.attrib.get('style', '')
        total += 1
        absolute += bool(re.search(r'position:\s*absolute', style))
        if tag(e) == 'Label':
            size = re.search(r'font-size:\s*([\d.]+)px', style)
            small_text += bool(size and float(size[1]) < 18)
            text_rows.append(dict(screen=sid, locale=lang, text=e.attrib.get('text', ''), font_px=size[1] if size else 'stylesheet/default'))
for p in FIGMA.glob('*.uxml'):
    for ref in re.findall(r'project://database/([^?"&]+)', read(p)):
        decoded = unquote(ref)
        if decoded.startswith('Assets/') and not (ROOT / decoded).exists(): missing_refs.append([p.name, decoded])
scene = read(ROOT / 'Assets/Scenes/SampleScene.unity')
video_block = re.search(r'performerPortraitVideos:\s*\n((?:  - .*\n)+)', scene)[1]
videos = [metas.get(g, 'UNRESOLVED:' + g) for g in re.findall(r'guid: ([a-f0-9]+)', video_block)]
doc_blocks = [b for b in re.split(r'--- !u!', scene) if 'UnityEngine.UIElements.UIDocument' in b]
docs = [dict(id=re.search(r'114 &(-?\d+)', b)[1], enabled=re.search(r'm_Enabled: (\d+)', b)[1], order=re.search(r'm_SortingOrder: ([\d.-]+)', b)[1]) for b in doc_blocks]
def write_csv(name, rows):
    if not rows: return
    fields = list(dict.fromkeys(k for r in rows for k in r))
    with (OUT / name).open('w', encoding='utf-8-sig', newline='') as f:
        w = csv.DictWriter(f, fieldnames=fields); w.writeheader(); w.writerows(rows)
write_csv('RouteLocaleInventory.csv', inventory)
write_csv('StructuralDrift.csv', drift)
write_csv('LocalizedTextInventory.csv', text_rows)
font_scan = {'status': 'not run: optional fontTools unavailable'}
try:
    from fontTools.ttLib import TTFont
except ImportError:
    pass
else:
    deficits, cmaps = [], {}
    for p in FIGMA.glob('*.uxml'):
        for e in ET.parse(p).iter():
            txt = e.get('text', '')
            han = {c for c in txt if '\u3400' <= c <= '\u9fff'}
            m = re.search(r'project://database/([^?]+\.(?:ttf|otf))', e.get('style', ''))
            if not han or not m:
                continue
            key = unquote(m[1])
            fp = ROOT / key
            if key.startswith('Packages/com.unity.dt.app-ui/'):
                caches = list((ROOT / 'Library/PackageCache').glob('com.unity.dt.app-ui*'))
                if caches:
                    fp = caches[0] / key.split('Packages/com.unity.dt.app-ui/', 1)[1]
            if not fp.exists():
                continue
            if key not in cmaps:
                with TTFont(fp) as font:
                    cmaps[key] = set(font.getBestCmap())
            missing = sorted(c for c in han if ord(c) not in cmaps[key])
            if missing:
                deficits.append(dict(asset=p.name, font=key, missing=''.join(missing), text=txt))
    write_csv('FontCoverage.csv', deficits)
    font_scan = dict(status='assigned font cmap only; fallback not assessed', labels_with_deficits=len(deficits), assets_with_deficits=len({r['asset'] for r in deficits}))
summary = dict(
    date='2026-11-13', verification='filesystem static only; no Unity compilation/runtime/visual verification',
    commit=subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=ROOT, text=True).strip(),
    catalog_entries=len(catalog), logical_routes=len(set(r[0] for r in catalog)),
    locales=dict(collections.Counter(r[1] for r in catalog)),
    duplicate_route_locale_keys=[k for k,v in collections.Counter((r[0],r[1]) for r in catalog).items() if v>1],
    unpaired_routes=[s for s in set(r[0] for r in catalog) if {r[1] for r in catalog if r[0]==s}!={'en','zh'}],
    manifest_rows=len(manifest), snapshot_rows=len(snapshot), manifest_equals_snapshot=manifest==snapshot,
    manifest_mapping_differences=[s['rootNodeId'] for s in manifest if s['rootNodeId'] not in by_root or mapping(s)!=mapping(by_root[s['rootNodeId']])],
    uxml_count=len(list(FIGMA.glob('*.uxml'))), uss_count=len(list(FIGMA.glob('*.uss'))),
    xml_errors=xml_errors, unresolved_catalog_assets=[r for r in inventory if r['status']=='missing'],
    missing_local_uxml_references=missing_refs,
    structural_path_tag_mismatches=len(drift), drift_by_route=dict(collections.Counter(r['screen']+'/'+r['locale'] for r in drift)),
    note='Structural drift scans all snapshot nodes including decoration, applying only controller remaps. It is not a count of broken controls and does not prove semantic identity.',
    inline_elements=total, inline_absolute_elements=absolute, labels_under_18px=small_text,
    scene_documents=docs, video_slots=videos,
    first_party_test_files=[p.relative_to(ROOT).as_posix() for p in (ROOT/'Assets').rglob('*Test*.cs') if 'Samples' not in p.parts],
    font_scan=font_scan,
)
(OUT/'StaticEvidence.json').write_text(json.dumps(summary, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
print(json.dumps({k:v for k,v in summary.items() if k not in ('scene_documents','drift_by_route')}, ensure_ascii=False, indent=2))
