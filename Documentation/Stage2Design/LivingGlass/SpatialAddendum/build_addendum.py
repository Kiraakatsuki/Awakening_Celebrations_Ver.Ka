"""Inventory and validate isolated design artifacts. No browser, Unity, camera or service execution."""
from pathlib import Path
import csv,hashlib,json,re,subprocess
from PIL import Image,ImageDraw,ImageFont
BASE=Path(__file__).resolve().parent
LG=BASE.parent
ROOT=LG.parents[2]
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def save(name,obj):(BASE/name).write_text(json.dumps(obj,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
def csvwrite(name,rows):
 with (BASE/name).open('w',encoding='utf-8-sig',newline='') as f:
  w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)

result=subprocess.run(['node','-e',"const {palettes,bounds}=require('../prototype/spatial-model.js');process.stdout.write(JSON.stringify({palettes,bounds}));"],cwd=BASE,text=True,capture_output=True,check=True)
source=json.loads(result.stdout)
tokens={'version':'Gallery Arc spatial addendum v1','approval':{'composition':'Gallery Arc approved by user','treatment':'B preferred conditionally','exact_bounds':'proposed; final visual review/approval pending','production':'not authorized'},'palettes':source['palettes'],'bounds':source['bounds'],'lighting':{'environment_opacity':.10,'overlay_environment_opacity':.025,'identification_rim_opacity':.42,'overlay_rim_opacity':.22,'environment_planes':1},'geometry':{'hit_wrappers_move':False,'text_and_focus_move':False,'native_arc_row':{'x':96,'y':340,'width':1728,'height':464},'perspective_css_px':1200,'native_pose':'2D optical proxy; not CSS/Unity perspective proof'},'reduced':{'spatial_transforms':False,'parallax':False,'recession':False,'reflection':False,'max_color_opacity_ms':60,'functional_state_changes':False},'timing_source':'model bounds; not detector dwell/loss/cooldown values'}
save('SpatialTokens.json',tokens)
states=[
 ('neutral','No eligible target','Neutral gallery','中性藝廊','none','no theme','none','confirmation unavailable'),
 ('acquiring','Candidate supplied, stability not confirmed','Target acquiring · local feedback only','正在取得目標・僅局部回饋','candidate source','hold prior stable theme, or neutral','one local rim; no pose/environment change','fist cannot commit'),
 ('targeted','Source supplies stable target','Target stable · confirmation required','目標已穩定・須確認','keyboard / pointer / gesture priority','blend current color directly to performer in280ms','one selected pose240ms; subordinate light/reflection','fresh gesture fist or explicit source-supported alternative'),
 ('targetChanged','Different candidate then stable event','A new presence, one continuous atmosphere','另一種存在，氛圍連續流動','winning source only','direct old→new; never pass through neutral','retarget current pose/color, no stacked animations','old gesture target invalidated; new target required'),
 ('lossPending','Raw gesture observation gap','Tracking interrupted · waiting for target-loss state','追蹤中斷・等待目標遺失狀態','gesture only; higher-priority input unaffected','hold last atmosphere','suspend acquisition/confirmation; settle pose calmly','fist blocked immediately; pointer alternative remains'),
 ('trackingLost','Existing source reports target cleared','Target cleared · returning to neutral','目標已清除・回到中性狀態','source-semantic loss','fade to neutral280ms','rest340ms; no invented loss timer','fresh point/stable target required'),
 ('recovered','Source readiness recovers','Context retained · acquire a fresh gesture target','保留情境・請重新取得手勢目標','none until fresh input','retain context if loss was not cleared; otherwise neutral','no target animation until fresh stable event','readiness alone cannot confirm'),
 ('confirmed','Eligible fist or explicit portrait/key activation','Performer confirmed','已確認表演者','committed transition','latch performer palette into Focus','one650ms art/gallery/Focus bridge','exactly once; cancel visual scope without changing media rules'),
 ('overlayOpening','Available Settings/AI/Help/Privacy action','Quiet controls, your context retained','從容調整，保留此刻情境','top modal','retain2.5% light /22% narrow panel rim; neutral reading fill','gallery settles<=120ms; panel dominates360ms','invalidate gesture acquisition; scope input immediately'),
 ('overlayOpen','Top panel owns focus','Return to exactly where you were','回到剛才的體驗','top modal','quiet context only','no gallery tilt/parallax/reflection','Tab scoped; Escape dismisses; deferred functions stay unavailable'),
 ('overlayClosing','Close/Escape/minimize','Returning to your experience','返回原本的體驗','modal until removal','context retained','panel exit220ms; no competing gallery event','restore actual opener or resume control; fresh gesture needed'),
 ('reduced','OS preference or review override','Reduced motion, same experience','減少動態效果，保留相同體驗','unchanged semantic priority','<=60ms color/opacity; opaque fallback independent','no tilt/scale/parallax/recession/stagger/proxy','same valid confirmations/cancellation/state sequence'),
 ('reflow','Committed locale/layout replacement','Reacquire your target','請重新取得目標','invalidate stale gesture acquisition','retain visual context, not a fresh target','freeze during reflow; recalculate wrapper mapping later','fresh source stability; no translated-text binding')]
csvwrite('SelectionStateMatrix.csv',[dict(state=k,trigger=tr,en=en,zh_Hant=zh,input_owner=owner,theme=theme,motion=motion,confirmation_recovery=policy,verification='pure-model checks where applicable; browser/Unity pending') for k,tr,en,zh,owner,theme,motion,policy in states])
csvwrite('SpatialCopy.csv',[dict(key='ARC-'+k,en=en,zh_Hant=zh,status='design copy; TC editorial review pending') for k,_,en,zh,*_ in states])

ledger=load(BASE/'FigmaLedger.json');exports=load(BASE/'ExportLedger.json');assert len(ledger['frames'])==34 and len(exports)==34
assert all(f['stableRegionsUnchanged'] and len(f['regions'])==6 for f in ledger['frames'])
canonical=ledger['frames'][0]['regions'];assert all(f['regions']==canonical for f in ledger['frames'])
assert not any('error' in x for x in exports)
native=load(BASE/'NativeValidation.json')
assert len(native['frames'])==34 and all(f['editableText']>0 and f['width']==1920 and f['height']==1080 for f in native['frames'])
asset_ledger=load(LG.parent/'figma/AssetUploadLedger.json')
approved_hashes={a['response']['imageHash'] for a in asset_ledger}
assert all(im['hash'] in approved_hashes for f in native['frames'] for im in f['images'])
byid={e['nodeId']:e for e in exports}
md=['# Arc spatial visual index','','Gallery Arc is approved. These are native editable design boards; B poses use2D optical proxies, not a captured browser or Unity perspective effect. Native text/art/components remain editable.','','[Updated interactive prototype](../prototype/index.html) · [A/B comparison](Comparison.jpg) · [Six-theme overview](SixThemes.jpg) · [Figma addendum page](https://www.figma.com/design/9VQRtMDk8vFGer7gktEYsB?node-id=33-2)','','| Treatment / performer / state / locale | Native Figma | PNG |','| --- | --- | --- |']
for f in ledger['frames']:
 e=byid[f['id']];md.append(f"| {f['name']} | [Open](https://www.figma.com/design/9VQRtMDk8vFGer7gktEYsB?node-id={f['id'].replace(':','-')}) | [PNG]({e['file']}) |")
(BASE/'VisualIndex.md').write_text('\n'.join(md)+'\n',encoding='utf-8')
font=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',23)
def img(f,size):
 with Image.open(BASE/byid[f['id']]['file']) as im:
  bg=Image.new('RGBA',im.size,'#0c1117');bg.alpha_composite(im.convert('RGBA'));return bg.convert('RGB').resize(size)
def frame(mode,performer,loc='en'):return next(f for f in ledger['frames'] if f['mode']==mode and f['performer']==performer and f['locale']==loc and f['phase']=='targeted' and not f['overlay'])
comparison=Image.new('RGB',(1960,630),'#0c1117');draw=ImageDraw.Draw(comparison)
for j,(mode,label) in enumerate([('A','A / RESTRAINED OPTICAL'),('B','B / CONTROLLED SPATIAL · PREFERRED')]):
 x=20+j*970;draw.text((x,16),label,font=font,fill='#f3f0e8');comparison.paste(img(frame(mode,'GAIA'),(950,534)),(x,63))
comparison.save(BASE/'Comparison.jpg',quality=95)
themes=Image.new('RGB',(1980,850),'#0c1117');draw=ImageDraw.Draw(themes)
for i,p in enumerate(source['palettes']):
 x=15+(i%3)*655;y=15+(i//3)*420;draw.text((x,y),p['name'],font=font,fill=p['accent']);themes.paste(img(frame('B',p['name']),(640,360)),(x,y+42))
themes.save(BASE/'SixThemes.jpg',quality=95)

# Preserve the existing source/route coverage; this addendum does not redefine app functionality.
for name in ['living-glass.js','spatial-model.js','spatial-interactions.js']:
 syntax=subprocess.run(['node','--check',str(LG/'prototype'/name)],text=True,capture_output=True)
 assert syntax.returncode==0,syntax.stderr
for cssname in ['living-glass.css','spatial.css']:
 css=(LG/'prototype'/cssname).read_text(encoding='utf-8')
 assert all((LG/'prototype'/u.strip("'\"")).resolve().is_file() for u in re.findall(r'url\(([^)]+)\)',css))
for p in BASE.rglob('*.png'):
 with Image.open(p) as im:im.verify()
missing=[]
for p in BASE.rglob('*.md'):
 for url in re.findall(r'\]\(([^)]+)\)',p.read_text(encoding='utf-8')):
  if re.match(r'^(https?://|#)',url):continue
  if not (p.parent/url.split('#')[0].strip('<>')).resolve().exists():missing.append(dict(file=p.name,target=url))
assert not missing,missing
original=load(LG/'SourceManifest.json')
for item in original:
 if item['path'].startswith(('Assets/','Packages/','ProjectSettings/','.agents/')) or item['path']=='AGENTS.md':
  assert hashlib.sha256((ROOT/item['path']).read_bytes()).hexdigest()==item['sha256'],item['path']
diff=subprocess.run(['git','diff','--name-only','HEAD','--','Assets','Packages','ProjectSettings','awakening-ai-backend'],cwd=ROOT,text=True,capture_output=True,check=True).stdout.strip();assert not diff,diff
save('SourceManifest.json',[dict(path=p.relative_to(ROOT).as_posix(),sha256=hashlib.sha256(p.read_bytes()).hexdigest()) for p in [*sorted((BASE/'baseline').glob('*')),ROOT/'Assets/Scripts/Interaction/GestureRecognitionLayer.cs',ROOT/'Assets/Scripts/Interaction/MediaPipeGestureBridge.cs',ROOT/'Assets/Scripts/AwakeningApplicationController.cs',*sorted((ROOT/'.agents/skills').glob('*/SKILL.md'))]])
save('ModelCheckRecord.json',{'command':'node --test Documentation/Stage2Design/LivingGlass/SpatialAddendum/spatial-model.test.cjs','result':'14 passed / 0 failed in the recorded execution','scope':'pure presentation state and numeric bounds; no DOM/browser/Unity','model_sha256':hashlib.sha256((LG/'prototype/spatial-model.js').read_bytes()).hexdigest(),'test_sha256':hashlib.sha256((BASE/'spatial-model.test.cjs').read_bytes()).hexdigest()})
save('PackageVerification.json',{'approved_composition':'Arc','treatment':'B preferred; caps pending approval','native_boards':34,'native_exports':34,'six_regions_unchanged_in_every_native_board':True,'syntax':'passed for three prototype scripts','links':'all local addendum links resolve','pure_model_checks':'14 passed in recorded node --test run; no browser/Unity execution','browser_QA':'blocked by existing saved-policy denial; no retry/workaround in addendum','Unity_performance':'not run / not measured','production_diff':[]})
print(json.dumps(load(BASE/'PackageVerification.json'),indent=2))
