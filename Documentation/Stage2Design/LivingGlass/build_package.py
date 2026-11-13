"""Design-only inventory, static checks and contact sheets. Never executes Unity/browser."""
from pathlib import Path
import csv, hashlib, json, re, subprocess
from collections import Counter
from PIL import Image,ImageDraw,ImageFont

BASE=Path(__file__).resolve().parent
ROOT=BASE.parents[2]
PARENT=BASE.parent
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def save(name,value):(BASE/name).write_text(json.dumps(value,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
def csvread(p):
    with p.open(encoding='utf-8-sig',newline='') as f:return list(csv.DictReader(f))
def csvwrite(name,rows):
    with (BASE/name).open('w',encoding='utf-8-sig',newline='') as f:
        w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)

tokens={'version':'Living Glass v1 / design approval candidate','creative_direction':'selected by user; production implementation not authorized','target':{'width':1920,'height':1080,'fps':60,'status':'provisional / unmeasured'},'colors':{'floor':'#0C1117','glass':'#243343','reading':'#17212C','text':'#F3F0E8','secondary':'#C2CBD5','muted':'#9CAAB8','edge':'#526475','success':'#9ADBC0','warning':'#F3D8A3','error':'#F1B8AD'},'spacing':[8,12,16,24,32,48,64,96],'radius':{'art':4,'action':14,'identification':24,'dock':28,'window':30},'type':{'en':{'display':'Source Serif 4','body':'Source Sans 3'},'zh-Hant':{'display':'Noto Serif TC','body':'Noto Sans TC'},'browser_zh_fallback':['Noto Sans TC','Microsoft JhengHei','sans-serif'],'sizes':{'display':[64,82],'page_title':[44,48],'panel_title':34,'body':[24,26],'action':22,'performer':[22,24],'status':[18,20]},'line_height_percent':{'display_en':112,'display_zh':130,'body':150,'dense_zh':165}},'motion_ms':{'feedback':120,'target':260,'identity':180,'navigation':300,'panel_enter':360,'panel_exit':220,'gallery_enter':520,'gallery_stagger':20,'stagger_cap':100,'existing_cinematic':650,'existing_activation':3000,'reduced_max':80},'easing':{'standard':[.22,.76,.24,1],'settle':[.18,.82,.22,1],'exit':[.4,0,.8,.25]},'idle':{'artwork_translation_px':3,'period_seconds':18,'hit_region_moves':False},'gallery':{'gutter_fhd':96,'gap_fhd':32,'region_height':464,'depth':{'far':{'art_height':340,'inset':38},'mid':{'art_height':370,'inset':16},'near':{'art_height':400,'inset':0}},'arc_order':['far','mid','near','near','mid','far']},'materials':{'baseline':'tinted/opaque reading surfaces; no native backdrop-filter assumption','enhanced':'separately approved shared bounded source/pass experiment','opaque_fallback':True}}
spatial=load(BASE/'SpatialAddendum/SpatialTokens.json') if (BASE/'SpatialAddendum/SpatialTokens.json').exists() else None
if spatial:
    tokens['version']='Living Glass v1.1 / approved Arc composition; spatial bounds pending'
    tokens['spatial_addendum']=spatial
    tokens['gallery']['approved_composition']='Arc'
    tokens['idle'].update(artwork_translation_px=0,period_seconds=0)
    tokens['motion_ms'].update(target=spatial['bounds']['responseMs'],return_to_rest=spatial['bounds']['returnMs'],theme_blend=spatial['bounds']['themeMs'])
save('DesignTokens.json',tokens)
names=['GAIA','PANDEMIC','RE-BIRTH','WHITE-GUARDIAN','CONSCIENCE','SANCTUARY']
files=['gaia.png','pandemic.png','rebirth.png','guardian.png','conscience.png','sanctuary.png']
accents=['#5BD8ED','#C0AA91','#E0B0C8','#C5E9FA','#FF956A','#F1C57A']
lines=[('An ocean of attention.','將感知，交給海洋。'),('A quieter kind of presence.','在靜謐中，感受存在。'),('A moment of becoming.','與蛻變的此刻相遇。'),('Stillness, held in light.','在光中，安放寧靜。'),('An ember of awareness.','感知，如餘燼微亮。'),('A space to return to.','回到，心中的安所。')]
save('PerformerThemes.json',[dict(id=f'PER-00{i+1}',name=n,accent=accents[i],artwork='../prototype/assets/'+files[i],line_en=lines[i][0],line_zh_Hant=lines[i][1],copy_status='proposed poetic interpretation; not factual assistant knowledge',shared_component_system=True) for i,n in enumerate(names)])
if spatial:
    themed=load(BASE/'PerformerThemes.json')
    for row,palette in zip(themed,spatial['palettes']):row.update(atmosphere_color=palette['atmosphere'],secondary_reflection_color=palette['secondary'])
    save('PerformerThemes.json',themed)

mapping={'Home':('home','Editorial art/reading split','invitation;privacy entry;shell'), 'Privacy':('privacy','Opaque reading window','consent;status;actions'), 'Tutorial':('tutorial','Instruction/progress surface','instruction;progress;skip;return'), 'Attract':('attract','Quiet invitation/art','awakening;status;alternative input'), 'ColorRelease / selection':('gallery','Arc six-presence gallery','stable region;presence;identification;shell'), 'FOCUS-VIEW':('focus','Chapter art window','chapter;play action;theme'), 'ACTIVATION':('activation','Protected luminance passage','chapter;reduced shell'), 'LIVING-PORTRAIT':('portrait','Protected media surface','placeholder;prepare;first frame;completion;cancel'), 'RELEASE':('release','Chapter return','reading actions;return focus'), 'POST-VIDEO-AI':('postvideo','Reflective art/conversation entry','prompt;assistant;continue'), 'AssistantChooser':('chooser','Six contexts','context selector;reading window'), 'AssistantOverlay':('ai','Right glass conversation','modal;editor;request status;minimize'), 'Settings':('settings','Right glass settings','setting row;locale;privacy;unavailable'), 'Closing':('closing','Editorial closing','return;optional takeaway;end policy'), 'Souvenir':('souvenir','Conditional art/content preview','unavailable;real destination/expiry;Done')}
rows=csvread(PARENT/'ScreenFamilyCoverage.csv')
ledger=load(BASE/'FigmaLedger.json')
for r in rows:
    view,composition,components=mapping[r['family']]
    loc=r['locale']
    key='Gallery arc' if view=='gallery' else view
    frame=next(f for f in ledger['frames'] if f['name']==f'Living Glass / {key} / {loc}')
    r.update(living_glass_view=view,living_glass_composition=composition,components=components,figma_node=frame['id'],native_reference=f"https://www.figma.com/design/9VQRtMDk8vFGer7gktEYsB?node-id={frame['id'].replace(':','-')}",coverage_level='shared family visual + state/localization contract; source-bound substeps and exhaustive long/error pixels require acceptance',verification='native structure/export; browser blocked; Unity not run')
assert len(rows)==104 and len({(r['logical_route'],r['locale']) for r in rows})==104
csvwrite('ScreenFamilyCoverage.csv',rows)
states=csvread(PARENT/'DeferredFeatureStates.csv');csvwrite('DeferredFeatureStates.csv',states)
js=(BASE/'prototype/living-glass.js').read_text(encoding='utf-8')
if (BASE/'prototype/spatial-interactions.js').exists():js+='\n'+(BASE/'prototype/spatial-interactions.js').read_text(encoding='utf-8')
copy=[]
for en,zh in re.findall(r"T\('([^']*)','([^']*)'\)",js):
    if (en,zh) not in {(r['en'],r['zh_Hant']) for r in copy}:copy.append(dict(key=f'LG-{len(copy)+1:03}',en=en,zh_Hant=zh,status='design candidate; native TC editorial review pending'))
csvwrite('BilingualCopy.csv',copy)
core=[
 ('assistant','empty','Your conversation begins here.','你的對話，從這裡開始。'),
 ('assistant','waiting','Preparing a response…','正在準備回覆…'),
 ('assistant','cancel','Cancel request','取消請求'),
 ('assistant','cancelled','Request cancelled.','已取消請求。'),
 ('assistant','offline','The assistant is unavailable. Your experience can continue.','助理目前無法使用，你仍可繼續體驗。'),
 ('assistant','network error','The connection was interrupted. Try again when you are ready.','連線中斷，準備好後可重試。'),
 ('assistant','unexpected response','The response could not be displayed. Please try again.','無法顯示回覆，請重試。'),
 ('assistant','retry','Try again','重試'),
 ('assistant','new question','Ask another question','提出另一個問題'),
 ('assistant','no theme','Choose a conversation perspective.','請選擇對話主題。'),
 ('media','preparing','Preparing the portrait…','正在準備流動畫像…'),
 ('media','unavailable','The portrait is unavailable. The artwork remains here.','流動畫像目前無法使用，仍可欣賞此作品。'),
 ('media','cancelled','You have left the portrait.','已離開流動畫像。'),
 ('media','complete','A moment to return.','回到此刻。'),
 ('media','retry unavailable','Please return to the experience or ask for help.','請返回體驗，或開啟說明。'),
 ('navigation','deferred','Available after this moment.','此段落結束後可使用。'),
 ('navigation','ready','Ready to open','已可開啟'),
 ('navigation','open','Open','開啟'),
 ('navigation','return','Return to where you were','返回剛才的體驗'),
 ('navigation','unavailable','This option is not available yet.','此選項尚未提供。'),
 ('camera','off','Camera off','相機已關閉'),
 ('camera','suspended','Capture suspended','已暫停擷取'),
 ('camera','starting','Preparing the camera…','正在準備相機…'),
 ('camera','lost','Tracking lost. Point again to reacquire.','失去追蹤，請重新指向目標。'),
 ('camera','recovered','Tracking recovered. A new target is required.','追蹤已恢復，請重新取得目標。'),
 ('camera','error','Camera unavailable. Use pointer and keyboard.','相機目前無法使用，請改用滑鼠與鍵盤。'),
 ('gallery','target','Targeted, not yet selected.','已指向，尚未選取。'),
 ('gallery','confirm','Confirm this presence','確認此種存在'),
 ('privacy','declined','Gesture interaction was declined for this session.','本次體驗未同意使用手勢互動。'),
 ('privacy','review','Review your camera choice','檢視相機使用選擇')]
csvwrite('CoreStateCopy.csv',[dict(family=f,state=s,en=en,zh_Hant=zh,rule='Only show on actual corresponding state; never synthesize service/media success') for f,s,en,zh in core])

exports=load(BASE/'ExportLedger.json');assert len(exports)==43 and not any('error' in x for x in exports)
index=['# Indexed native designs','','Current refinement: [approved Gallery Arc spatial/theming addendum](SpatialAddendum/VisualIndex.md). Its34 native boards and updated A/B prototype supersede the historical gallery choice below.','','Original Stage2B native editable1920×1080 boards follow. PNGs are native Figma renders, not browser or Unity captures. Constellation/Salon and earlier motion are historical exploration only.','','[Original Living Glass page](https://www.figma.com/design/9VQRtMDk8vFGer7gktEYsB?node-id=18-48) · [Current interactive prototype](prototype/index.html) · [Historical gallery comparison](GalleryComparison.jpg) · [Family overview](FamilyOverview.jpg)','','| Design | Editable Figma | Native PNG |','| --- | --- | --- |']
for f,e in zip(ledger['frames'],exports):
    assert f['id']==e['nodeId']
    index.append(f"| {f['name']} | [Open](https://www.figma.com/design/9VQRtMDk8vFGer7gktEYsB?node-id={f['id'].replace(':','-')}) | [PNG]({e['file']}) |")
motion=load(BASE/'MotionLedger.json')
motion_validation=load(BASE/'MotionValidation.json')
assert [m['timelines'][0]['duration'] for m in motion_validation]==[6,6,14]
assert all(m['animated'] for m in motion_validation)
index+=['','## Native motion timelines','','These are authored timelines with editable layers. They are distinct from interactive state logic and from Unity proof. The chapter uses still art and explicitly authored review-event beats. Open a motion frame in Figma and play its timeline.']
index.append('')
for f in motion['frames']:index.append(f"- [{f['name']} · {f['duration']}s](https://www.figma.com/design/9VQRtMDk8vFGer7gktEYsB?node-id={f['id'].replace(':','-')})")
(BASE/'VisualIndex.md').write_text('\n'.join(index)+'\n',encoding='utf-8')

# Combine native renders into comparison sheets; no image editing of production assets.
fontfile=Path('C:/Windows/Fonts/segoeui.ttf');font=ImageFont.truetype(str(fontfile),22)
def cropfit(path,size):
    with Image.open(path) as im:return im.convert('RGB').resize(size)
sheet=Image.new('RGB',(1984,440),'#0c1117');d=ImageDraw.Draw(sheet)
for i,(key,label) in enumerate([('arc','ARC / RECOMMENDED'),('constellation','CONSTELLATION'),('salon','SALON')]):
    x=16+i*656;d.text((x,14),label,font=font,fill='#f3f0e8');sheet.paste(cropfit(BASE/f'review-evidence/Gallery-{key}-en.png',(640,360)),(x,55))
sheet.save(BASE/'GalleryComparison.jpg',quality=94)
views=['home','privacy','tutorial','attract','Gallery-arc','focus','activation','portrait','playing','release','postvideo','chooser','settings','ai','closing','souvenir','calibration']
overview=Image.new('RGB',(2040,1340),'#0c1117');d=ImageDraw.Draw(overview)
for i,view in enumerate(views):
    x=20+(i%5)*404;y=20+(i//5)*330;d.text((x,y),view.upper(),font=font,fill='#f3f0e8');overview.paste(cropfit(BASE/f'review-evidence/{view}-en.png',(384,216)),(x,y+36))
overview.save(BASE/'FamilyOverview.jpg',quality=94)

# Record inputs without modifying prior reports or runtime files.
source_paths=[ROOT/'AGENTS.md']+sorted((ROOT/'.agents/skills').glob('*/SKILL.md'))
source_paths+=[ROOT/r['path'] for r in load(PARENT/'SourceManifest.json') if r['path'].startswith('Documentation/')]
source_paths+=sorted(p for p in PARENT.glob('*.md'))
source_paths+=[ROOT/p for p in ['ProjectSettings/ProjectVersion.txt','ProjectSettings/GraphicsSettings.asset','ProjectSettings/QualitySettings.asset','Packages/manifest.json','Assets/Settings/UniversalRP.asset','Assets/Settings/Renderer2D.asset','Assets/UI Toolkit/PanelSettings.asset','Assets/Scripts/AwakeningApplicationController.cs','Assets/Scripts/ScreenDocumentManager.cs','Assets/Scripts/RuntimeScreenCatalog.cs','Assets/Scripts/Interaction/GestureRecognitionLayer.cs','Assets/Scripts/Interaction/GestureCursorController.cs']]
save('SourceManifest.json',[dict(path=p.relative_to(ROOT).as_posix(),sha256=hashlib.sha256(p.read_bytes()).hexdigest(),bytes=p.stat().st_size) for p in dict.fromkeys(source_paths)])
missing=[]
for p in BASE.rglob('*.md'):
    for url in re.findall(r'\]\(([^)]+)\)',p.read_text(encoding='utf-8')):
        if re.match(r'^(https?://|#)',url):continue
        if not (p.parent/url.split('#')[0].strip('<>')).resolve().exists():missing.append(dict(file=p.name,target=url))
syntax=subprocess.run(['node','--check',str(BASE/'prototype/living-glass.js')],capture_output=True,text=True)
assert syntax.returncode==0,syntax.stderr
assert not re.search(r'\b(fetch|XMLHttpRequest|WebSocket|getUserMedia|RTCPeerConnection)\s*\(',js)
css=(BASE/'prototype/living-glass.css').read_text(encoding='utf-8')
assert all((BASE/'prototype'/u.strip("'\"")).resolve().is_file() for u in re.findall(r'url\(([^)]+)\)',css))
assert all((BASE/t['artwork']).resolve().is_file() for t in load(BASE/'PerformerThemes.json'))
for old in load(PARENT/'SourceManifest.json'):
    assert hashlib.sha256((ROOT/old['path']).read_bytes()).hexdigest()==old['sha256'],old['path']
asset_ledger=load(PARENT/'figma/AssetUploadLedger.json')
for i,source in enumerate(asset_ledger):
    assert hashlib.sha256((ROOT/source['source']).read_bytes()).digest()==hashlib.sha256((PARENT/'prototype/assets'/files[i]).read_bytes()).digest()
for p in BASE.rglob('*.png'):
    with Image.open(p) as im:im.verify()
native=load(BASE/'NativeValidation.json');assert len(native['frames'])==43
assert all(f['width']==1920 and f['height']==1080 and f['editableText']>0 for f in native['frames'])
allowed={'d0b9dde2bbef820accb280eb544262867133da19','b60d5f32a4d7c19f34310fd2f8c9b7401c5b512c','0c9809f40f73677964b45824c227d85dc2a53bc5','29dbef3b4629075df8200a85af640f7c4bb8561d','1b4373bc801edf6490cb06e74a9d7902af6a78cc','f6a5af1bfcdbbbdd9321d7f6c127f41a6a00ba57'}
assert all(im['hash'] in allowed for f in native['frames'] for im in f['images'])
diff=subprocess.run(['git','diff','--name-only','HEAD','--','Assets','Packages','ProjectSettings','awakening-ai-backend'],cwd=ROOT,text=True,capture_output=True,check=True).stdout.strip()
assert not diff,diff
save('PackageVerification.json',{'native_static_frames':43,'native_motion_frames':len(motion['frames']),'native_images_exported':len(exports),'localized_catalog_records':len(rows),'logical_routes':len(set(r['logical_route'] for r in rows)),'families':len(Counter(r['family'] for r in rows)),'paired_deferred_states':len(states),'living_glass_copy_pairs':len(copy),'core_state_copy_pairs':len(core),'js_syntax':'passed','local_link_failures':missing,'artwork_hashes':'six approved sources only','tracked_production_diff':[],'browser_visual_interaction_checks':'blocked: saved permission / managed policy denial','Unity_runtime_performance':'not run / not measured','native_motion':'authored tracks; no video-frame inspection; not Unity proof'})
print(json.dumps(load(BASE/'PackageVerification.json'),indent=2))
assert not missing,missing
