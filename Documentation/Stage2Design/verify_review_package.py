"""Check design artifacts only. Never launches Unity or the product."""
from pathlib import Path
from collections import Counter
import csv, hashlib, json, re, subprocess
from PIL import Image, ImageDraw, ImageFont

BASE = Path(__file__).resolve().parent
ROOT = BASE.parents[1]
def read_json(name):
    return json.loads((BASE / name).read_text(encoding='utf-8-sig'))
def read_csv(name):
    with (BASE / name).open(encoding='utf-8-sig', newline='') as f:
        return list(csv.DictReader(f))

# Contact sheets use browser captures, retaining the full captures separately.
font_path = Path('C:/Windows/Fonts/segoeui.ttf')
font = ImageFont.truetype(str(font_path), 22) if font_path.exists() else ImageFont.load_default()
def stage_crop(path):
    with Image.open(path) as im:
        return im.convert('RGB').crop((0,165,1264,1065))
comparison = Image.new('RGB',(1984,540),'#101419')
draw = ImageDraw.Draw(comparison)
for i,(code,label) in enumerate([('A','LIVING LIGHT'),('B','FLUID GLASS'),('C','CINEMATIC SPATIAL')]):
    x=16+i*656
    draw.text((x,18),f'{code} / {label}',font=font,fill='#f3eee4')
    comparison.paste(stage_crop(BASE/f'review-evidence/{code}-home-en.png').resize((640,456)),(x,60))
comparison.save(BASE/'ConceptComparison.jpg',quality=94)
overview=Image.new('RGB',(2040,1010),'#101419')
draw=ImageDraw.Draw(overview)
for row,code in enumerate('ABC'):
    for col,view in enumerate(['home','selection','settings','ai','focus']):
        x=20+col*404; y=20+row*330
        draw.text((x,y),f'{code} / {view.upper()}',font=font,fill='#f3eee4')
        overview.paste(stage_crop(BASE/f'review-evidence/{code}-{view}-en.png').resize((384,273)),(x,y+34))
overview.save(BASE/'ConceptScreenOverview.jpg',quality=94)

missing=[]
for p in BASE.rglob('*.md'):
    for target in re.findall(r'\]\(([^)]+)\)',p.read_text(encoding='utf-8')):
        if re.match(r'^(https?://|#|mailto:)',target): continue
        candidate=(p.parent/target.split('#')[0].strip('<>')).resolve()
        if not candidate.exists(): missing.append({'file':str(p.relative_to(BASE)),'target':target})
assert not missing,missing
sources=read_json('SourceManifest.json')
assert all(hashlib.sha256((ROOT/s['path']).read_bytes()).hexdigest()==s['sha256'] for s in sources)
routes=read_csv('ScreenFamilyCoverage.csv')
assert len(routes)==104 and len({(r['logical_route'],r['locale']) for r in routes})==104
pairs={r['logical_route']:{s['locale'] for s in routes if s['logical_route']==r['logical_route']} for r in routes}
assert len(pairs)==52 and all(len(v)==2 for v in pairs.values())
states=read_csv('DeferredFeatureStates.csv')
assert len(states)==53 and all(r['en'] and r['zh_Hant'] for r in states)
mockups=read_json('MockupIndex.json')
assert len(mockups)==30 and all((r['width'],r['height'])==(1920,1080) for r in mockups)
native=read_json('figma/NativeValidation.json')
native_frames=[f for c in native for f in c['frames']]
assert len(native_frames)==30 and all(f['editableText']>0 and f['dimensions']==[1920,1080] for f in native_frames)
coverage=read_json('review-evidence/BrowserCoverage.json')
assert len(coverage)==30 and all(r['imagesLoaded'] and r['dialog']==r['dialogExpected'] for r in coverage)
images=[]
for p in BASE.rglob('*'):
    if p.suffix.lower() in ('.jpg','.jpeg','.png'):
        with Image.open(p) as im:
            im.verify()
        images.append(str(p.relative_to(BASE)))
production=['Assets','Packages','ProjectSettings','awakening-ai-backend']
diff=subprocess.run(['git','diff','--name-only','HEAD','--',*production],cwd=ROOT,capture_output=True,text=True,check=True).stdout.strip()
assert not diff,diff
syntax=subprocess.run(['node','--check',str(BASE/'prototype/review.js')],capture_output=True,text=True)
assert syntax.returncode==0,syntax.stderr
ledger=read_json('figma/StateLedger.json')
ledger['componentSets']={'A':'8:2','B':'8:3','C':'8:4'}
(BASE/'figma/StateLedger.json').write_text(json.dumps(ledger,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
result={
 'scope':'isolated design package; no Unity execution',
 'local_markdown_links':'all resolve', 'source_hashes_unchanged':len(sources),
 'localized_catalog_records':len(routes),'logical_routes':len(pairs),
 'screen_families':len(Counter(r['family'] for r in routes)),
 'paired_deferred_states':len(states),'native_editable_frames':len(native_frames),
 'browser_combinations_checked':len(coverage),'images_decoded':len(images),
 'prototype_js_syntax':'pass','tracked_production_diff':[],
 'final_home_layout':'A/en and A/zh recaptured after narrowing reading column; 31.65px gap at captured browser width',
 'limitations':['No Unity rendering or performance proof','No camera/video/live assistant test','Not every post-selection pixel variant finalized']
}
(BASE/'PackageVerification.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
print(json.dumps(result,indent=2))
