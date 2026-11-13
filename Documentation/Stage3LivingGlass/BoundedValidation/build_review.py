"""Summarize existing actual Play Mode captures. Never starts Unity or touches assets."""
from pathlib import Path
import hashlib, html, json, math
from PIL import Image, ImageDraw, ImageFont

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
SHOTS = HERE / 'Screenshots'
font = ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf', 20)
small = ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf', 15)

snapshots = {}
for path in HERE.glob('*.json'):
    value = json.loads(path.read_text(encoding='utf-8-sig'))
    if isinstance(value, dict) and 'rigInactive' in value:
        snapshots[path.stem] = value

unsafe = [n for n, x in snapshots.items() if x['cameraConsent'] or x['sessionConsent'] or not x['rigInactive'] or not x['requestAbsent']]
protected = []
for old in json.loads((HERE / 'ProtectedBefore.json').read_text(encoding='utf-8-sig')):
    digest = hashlib.sha256((ROOT / old['path']).read_bytes()).hexdigest()
    protected.append(dict(path=old['path'], sha256=digest, status='PASS' if digest == old['sha256'] else 'FAIL'))

gallery = {n:x for n,x in snapshots.items() if x['route'] == 'COLOR-RELEASE' and len([e for e in x['elements'] if e['name'].startswith('presence-')]) == 6}
def wrappers(x):
    return {e['name']:tuple(e['bounds'][k] for k in ('x','y','width','height')) for e in x['elements'] if e['name'].startswith('presence-')}
reference = wrappers(snapshots['Theme-runtime-0'])
drift = {n:wrappers(x) for n,x in gallery.items() if wrappers(x) != reference}
apertures = [e for x in gallery.values() for e in x['elements'] if e['name'].startswith('aperture-')]
summary = dict(snapshotCount=len(snapshots), unsafeSnapshots=unsafe,
               fullHDAll=all((x['width'],x['height']) == (1920,1080) for x in snapshots.values()),
               gallerySamples=len(gallery), wrapperDriftSamples=list(drift), referenceWrappers=reference,
               maximumApertureScale=max(e['scale'] for e in apertures),
               maximumApertureTranslation=max(max(abs(e['translation']['x']),abs(e['translation']['y'])) for e in apertures),
               protectedFiles=protected,
               note='Snapshots are sampled evidence plus an Editor update safety guard; they do not prove camera/gesture/backend/performance operation.')
(HERE/'EvidenceSummary.json').write_text(json.dumps(summary,indent=2),encoding='utf-8')

def sheet(output, title, entries, columns=2):
    thumb=(768,432); cell_h=476; top=88
    canvas=Image.new('RGB',(columns*thumb[0],top+math.ceil(len(entries)/columns)*cell_h),'#0b1117')
    draw=ImageDraw.Draw(canvas); draw.text((20,12),title,fill='#f3f0e8',font=font)
    draw.text((20,43),'Actual Unity Editor Play Mode / 1920 x 1080 sources. No performance or gesture claim.',fill='#b5c6d0',font=small)
    for i,(name,label) in enumerate(entries):
        path=SHOTS/(name+'.png')
        image=Image.open(path).convert('RGB').resize(thumb,Image.Resampling.LANCZOS)
        x=(i%columns)*thumb[0]; y=top+(i//columns)*cell_h
        canvas.paste(image,(x,y)); draw.text((x+12,y+438),label,fill='#f3f0e8',font=small)
    canvas.save(HERE/output)

themes=[('Theme-runtime-'+str(i),name+' / semantic target command; stable wrappers') for i,name in enumerate(['GAIA','PANDEMIC','RE-BIRTH','WHITE-GUARDIAN','CONSCIENCE','SANCTUARY'])]
sheet('RuntimeSixThemes.jpg','Six actual performer-reactive gallery themes',themes)
sheet('RuntimeOverview.jpg','Runtime visual review',[
    ('25-Home-final-en','Home EN content / readback omits chrome; see report'),
    ('28-Home-zh-consent-fixed','Home Traditional Chinese / final label sizing'),
    ('06-Gallery-neutral-en','Neutral gallery / denied consent readiness fixture'),
    ('Gallery-zh','Traditional Chinese gallery / readiness fixture'),
    ('21-Settings-final-en','Settings / default scrollbar remains a refinement defect'),
    ('16-AI-GAIA-offline','AI conversation / offline, requests disabled'),
    ('Tutorial-en-3','Tutorial Point EN / layout fixture, not recognition'),
    ('Tutorial-zh-point','Tutorial Point Traditional Chinese'),
    ('Reduced-opaque-zh','Reduced motion + opaque surface / cloned in-memory config'),
    ('Media-decoded-0','GAIA / actual locally decoded video frame'),
    ('Media-decoded-4','CONSCIENCE / actual portrait-aspect video frame'),
    ('Media-cancel-return','Real video Back cancellation returned to Focus')])
sheet('MotionFrames.jpg','Actual motion samples, not a simulated animation',[
    ('Enter-bridge-'+str(ms),'Gallery to Focus / approximately '+str(ms)+' ms') for ms in [20,120,280,480,750]
]+[('Settings-open-final-'+str(ms),'Settings opening / approximately '+str(ms)+' ms') for ms in [20,120,280,480]]+[
    ('AI-open-final-'+str(ms),'Assistant opening / approximately '+str(ms)+' ms') for ms in [20,120,280]],columns=3)

reference_path=ROOT/'Documentation/Stage2Design/LivingGlass/SpatialAddendum/review-evidence/B-GAIA-en.png'
comparison=Image.new('RGB',(1536,520),'#0b1117'); d=ImageDraw.Draw(comparison)
for x,path,label in [(0,reference_path,'Stage 2B Controlled Spatial B / native Figma optical proxy'),(768,SHOTS/'Theme-runtime-0.png','Actual Unity Play Mode / GAIA targeted')]:
    comparison.paste(Image.open(path).convert('RGB').resize((768,432),Image.Resampling.LANCZOS),(x,54))
    d.text((x+10,12),label,fill='#f3f0e8',font=small)
d.text((12,493),'Compare artwork, Arc proportions, type metrics, shell, plate spacing and material differences. No pixel-equivalence claim.',fill='#b5c6d0',font=small)
comparison.save(HERE/'RuntimeVsFigma.jpg')

excluded={'01-Startup-en','03-Settings-zh','05-Home-en-settled','31-Home-en-consent-fixed'}
cards=[]
for path in sorted(SHOTS.glob('*.png')):
    name=path.stem
    if name in excluded or name.startswith('Settings-motion-'): continue
    state=snapshots.get(name,{})
    info=' | '.join(str(state.get(k,'')) for k in ('route','locale','focus'))
    if name in {'10-Home-en-retest','22-Backdrop-Escape-fixed','25-Home-final-en','32-Numeric-Rebirth-Focus'}:
        info += ' | Readback omits some chrome visible in native window; inconclusive capture.'
    cards.append('<figure><a href="Screenshots/'+html.escape(path.name)+'"><img loading="lazy" src="Screenshots/'+html.escape(path.name)+'"></a><figcaption>'+html.escape(name)+'<br><small>'+html.escape(info)+'</small></figcaption></figure>')
(HERE/'Review.html').write_text('''<!doctype html><html lang="en"><meta charset="utf-8"><title>Living Glass bounded Play Mode evidence</title>
<style>body{margin:36px;background:#0b1117;color:#eef0ed;font:17px/1.5 system-ui}a{color:#73d5de}header{max-width:1050px}main{display:grid;grid-template-columns:repeat(auto-fit,minmax(460px,1fr));gap:24px}figure{margin:0;padding:10px;background:#17232e;border-radius:14px}img{width:100%;height:auto}small{color:#bcc8d0}</style>
<header><h1>Bounded Unity Play Mode review</h1><p>13 November 2026 · Unity 6000.4.7f1 · actual 1920×1080 Game view captures. Camera denied, capture rig inactive and assistant endpoint blank during every session. Downstream gallery/tutorial/Attract readiness uses explicitly labelled presentation fixtures. Local video frames and completion events are real.</p>
<p><a href="../ValidationAndAcceptance.md">Acceptance report</a> · <a href="RuntimeOverview.jpg">Overview</a> · <a href="RuntimeSixThemes.jpg">Six themes</a> · <a href="MotionFrames.jpg">Motion frames</a> · <a href="RuntimeVsFigma.jpg">Figma comparison</a> · <a href="EvidenceSummary.json">Machine summary</a></p>
<p>Frame sequences are sampled actual motion, not a frame-rate recording. Screenshots cannot certify 60fps. Initial-entrance and mislabeled early samples remain on disk but are excluded here. Historical Settings-motion samples had no overlay and are excluded. Use Settings-open/close-final and AI-open-final instead.</p></header><main>'''+''.join(cards)+'</main></html>',encoding='utf-8')
print(json.dumps({k:v for k,v in summary.items() if k not in ['protectedFiles','referenceWrappers']},indent=2))
