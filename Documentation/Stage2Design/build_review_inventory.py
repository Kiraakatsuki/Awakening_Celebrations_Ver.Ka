"""Design documentation inventory only; no Unity execution or application mutations."""
from pathlib import Path
import csv,json,hashlib,re
BASE=Path(__file__).resolve().parent
ROOT=BASE.parents[1]
def write_csv(name,rows):
    with (BASE/name).open('w',encoding='utf-8-sig',newline='') as f:
        w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
def family(s):
    if s=='Home':return 'Home'
    if s=='Privacy':return 'Privacy'
    if s.startswith('TUTORIAL---'):return 'Tutorial'
    if s.startswith('ATTRACT---'):return 'Attract'
    if s=='COLOR-RELEASE':return 'ColorRelease / selection'
    if s=='AssistantChooser':return 'AssistantChooser'
    if s.startswith('Assistant-'):return 'AssistantOverlay'
    if s.startswith('SETTINGS---'):return 'Settings'
    if s.startswith('CLOSING---'):return 'Closing'
    if s.startswith('SOUVENIR---'):return 'Souvenir'
    for stage in ['FOCUS-VIEW','ACTIVATION','LIVING-PORTRAIT','RELEASE','POST-VIDEO-AI']:
        if '---'+stage+'---' in s:return stage
    raise ValueError(s)
states={
 'Home':'default;locale;consent off;review;review unavailable',
 'Privacy':'notice;consent decision;decline;revocation;copy unavailable',
 'Tutorial':'step;progress;skip;return;pointer/keyboard alternative;camera unavailable',
 'Attract':'waiting;first awakening;returning visitor;tracking lost;alternative input',
 'ColorRelease / selection':'presentation;ready;point target;fist confirm;tracking lost;1-6/pointer',
 'FOCUS-VIEW':'ready;target performer;manual start;no camera;video unavailable',
 'ACTIVATION':'enter;protected sequence;safe exit policy;cancel;reduced motion',
 'LIVING-PORTRAIT':'placeholder;preparing;first frame;playing;complete;failed;cancelled;configured missing-video hold/timer',
 'RELEASE':'entrance;awaiting continue;back;return;tracking status',
 'POST-VIDEO-AI':'empty;prompt;waiting;reply;error;retry;cancel;continue',
 'AssistantChooser':'six themes;focus;selection;no service;return',
 'AssistantOverlay':'empty;editing;waiting;reply;network/format error;retry;cancel;opener restore',
 'Settings':'language;sound/accessibility/tracking/reset deferred;privacy;unavailable reason;return',
 'Closing':'complete;return to selection;optional takeaway;end policy',
 'Souvenir':'unavailable;preview;preparing;ready;expired;offline/error;cancel;Done'
}
patterns={k:'shell;action;reading surface;localized text' for k in states}
patterns.update({'ColorRelease / selection':'shell;performer selector;target status;action','LIVING-PORTRAIT':'cinematic surface;reduced shell;media status','AssistantOverlay':'modal;prompt;question field;conversation;request status','Settings':'modal;setting row;language selector;privacy status','Tutorial':'help instruction;progress;action;shell'})
with (ROOT/'Documentation/TransformationAudit/RouteLocaleInventory.csv').open(encoding='utf-8-sig',newline='') as f:catalog=list(csv.DictReader(f))
rows=[]
for r in catalog:
    fam=family(r['screen']); performer=next((n for n in ['GAIA','PANDEMIC','RE-BIRTH','WHITE-GUARDIAN','CONSCIENCE','SANCTUARY'] if n in r['screen']),'shared')
    rows.append(dict(logical_route=r['screen'],locale=r['locale'],family=fam,performer=performer,source_asset=r['asset'],source_figma_root=r['root'],shared_patterns=patterns[fam],required_states=states[fam],current_stage2a_coverage='representative family artifact + state/localization specification; not all pixel variants finalized',next_gate='selected direction refinement; relevant product/architecture approval',verification='design coverage only; no Unity runtime'))
assert len(rows)==104 and len({(r['logical_route'],r['locale']) for r in rows})==104
write_csv('ScreenFamilyCoverage.csv',rows)
feature={
 'Sound':('聲音',[('unavailable','Sound controls are not available yet.','聲音控制尚未提供。'),('current','Current sound setting','目前聲音設定'),('applying','Applying sound setting…','正在套用聲音設定…'),('applied','Sound setting updated.','聲音設定已更新。'),('error','Sound could not be updated. Your previous setting is retained.','無法更新聲音，已保留原設定。'),('cancel','Keep current sound setting.','保留目前聲音設定。')]),
 'Accessibility':('無障礙',[('unavailable','Accessibility preferences are not available yet.','無障礙偏好尚未提供。'),('motion','Reduce motion','減少動態效果'),('effects','Use opaque reading surfaces','使用不透明閱讀介面'),('text','Readable text size','易讀文字大小'),('applying','Applying preference…','正在套用偏好…'),('applied','Preference updated.','偏好已更新。'),('error','This preference could not be applied.','無法套用此偏好。')]),
 'Camera':('相機',[('off','Camera off','相機已關閉'),('declined','Gesture interaction was declined for this session.','本次體驗未同意使用手勢互動。'),('suspended','Capture suspended','已暫停擷取'),('starting','Preparing the camera…','正在準備相機…'),('ready','Camera ready','相機已就緒'),('no hand','Show your hand, or use pointer and keyboard.','請將手伸到相機前，或使用滑鼠與鍵盤。'),('lost','Tracking lost. Your selection has not changed.','失去追蹤，選擇未變更。'),('error','Camera unavailable. Use pointer and keyboard.','相機目前無法使用，請改用滑鼠與鍵盤。'),('preview','Preview visibility does not change camera permission.','預覽是否顯示不會改變相機權限。')]),
 'EndRestart':('結束與重新開始',[('confirm','End this experience?','要結束本次體驗嗎？'),('consequence','The approved session reset will clear the items listed here.','核准的重設操作會清除下列體驗資料。'),('cancel','Keep experiencing','繼續體驗'),('pending','Ending safely…','正在安全結束…'),('ended','Experience ended.','體驗已結束。'),('error','The experience could not finish closing. Try the safe recovery action.','無法完成結束，請使用安全復原操作。')]),
 'Help':('說明',[('ready','Move at your own pace.','以自己的步調探索。'),('point','Point, then make a fist to select.','指向後握拳選擇。'),('palms','Show both open palms to begin.','張開雙手以開始。'),('back','Hold one open palm to request Back.','張開單手並保持，以請求返回。'),('alternative','Click or use Tab, Enter and 1–6 where available.','可點擊，或於支援處使用 Tab、Enter 與 1–6。'),('replay','Replay the interaction guide','重新觀看操作說明'),('return','Return to your experience','返回原本的體驗')]),
 'Calibration':('校正',[('optional','Alignment, only if needed.','僅在需要時，調整對位。'),('unavailable','Calibration is not available.','校正目前無法使用。'),('consent','Review camera permission before continuing.','繼續前，請先確認相機權限。'),('acquire','Point at the target and hold as instructed.','依照指示，指向目標並保持。'),('evaluate','Checking alignment…','正在檢查對位…'),('success','Alignment completed.','對位已完成。'),('retry','Alignment could not be confirmed. Try again.','無法確認對位，請重試。'),('cancel','Cancel and retain the previous alignment.','取消並保留原本的對位。')]),
 'Souvenir':('紀念留存',[('unavailable','Delivery unavailable. No QR code is provided.','留存服務未提供，沒有 QR 碼。'),('preview','Carry a moment with you.','帶走此刻的感受。'),('preparing','Preparing your approved takeaway…','正在準備核准的紀念內容…'),('ready','Your takeaway is ready.','紀念內容已就緒。'),('expired','This takeaway link has expired.','紀念內容連結已過期。'),('offline','Delivery is offline. You can still finish the experience.','留存服務離線，你仍可完成體驗。'),('error','The takeaway could not be prepared.','無法準備紀念內容。'),('retry','Try again','重試'),('cancel','Cancel delivery','取消留存'),('done','Done','完成')])}
state_rows=[]
for key,(zh,ss) in feature.items():
 for state,en,trad in ss:
  state_rows.append(dict(feature=key,feature_zh=zh,state=state,en=en,zh_Hant=trad,implementation='deliberately deferred; separate D1 authorization',display_rule='Actual service/state confirmation required; never simulate success' if state in ['applied','success','ready','ended'] else 'Design specification only; prototype uses truthful unavailable/review states',cancel_policy='retain prior settings/context; no new consent; release owned operations'))
write_csv('DeferredFeatureStates.csv',state_rows)
tokens={'status':'proposed / not approved / design only','viewport':[1920,1080],'frameRateTarget':60,'color':{'stage':'#101419','raised':'#212831','text':'#F3EEE4','secondary':'#C3C8CF','muted':'#A8B0BB','line':'#46505C','GAIA':'#31D7F2','PANDEMIC':'#B29A82','RE-BIRTH':'#D8A9C1','WHITE-GUARDIAN':'#BFE9FF','CONSCIENCE':'#FF6A2A','SANCTUARY':'#F3B95F','success':'#92D9BC','warning':'#F2CA7D','error':'#FFACAC'},'space':[8,12,16,24,32,48,64,96],'radius':[12,20,28,40],'type':{'en':{'body':'Source Sans 3','display':'Source Serif 4'},'zh-Hant':{'body':'Noto Sans TC','display':'Noto Serif TC'},'sizes':{'display':[72,88],'title':[44,48],'section':[28,30],'body':24,'control':22,'caption':[18,20]},'local_prototype_zh_fallback':['Noto Sans TC','Microsoft JhengHei','sans-serif']},'motion_ms':{'feedback':110,'exit':160,'panel_enter':220,'navigation':280,'cinematic_existing':650,'activation_existing':3000,'reduced_max':80},'material':{'baseline':'tint and opaque reading regions','enhancement':'shared bounded backdrop, separately approved','native_backdrop_filter_assumed':False}}
(BASE/'DesignTokens.json').write_text(json.dumps(tokens,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
docs=['PremiumExperienceAudit.md','CurrentArchitectureAssessment.md','FeatureImplementationMatrix.md','GlobalNavigationRedesign.md','PremiumDesignSystem.md','MotionInteractionSpecification.md','TargetRuntimeArchitecture.md','TechnicalDebtAssessment.md','TransformationRoadmap.md','TransformationAudit/ValidationAndPerformancePlan.md']
inputs=[ROOT/'AGENTS.md']+list((ROOT/'.agents/skills').glob('*/SKILL.md'))+[ROOT/'Documentation'/p for p in docs]
sources=[dict(path=p.relative_to(ROOT).as_posix(),sha256=hashlib.sha256(p.read_bytes()).hexdigest(),bytes=p.stat().st_size) for p in inputs]
(BASE/'SourceManifest.json').write_text(json.dumps(sources,indent=2)+'\n',encoding='utf-8')
ledger=json.loads((BASE/'figma/ScreenLedger.json').read_text(encoding='utf-8'))
md=['# Visual mockup index','','All frames are proposed native editable Figma designs at 1920×1080. Each concept has Home, Selection, Settings, AI and GAIA cinematic views in both locales. Local PNG evidence and browser prototypes are references, not Unity renders.','','[Open full Figma exploration](https://www.figma.com/design/9VQRtMDk8vFGer7gktEYsB) · [Interactive prototype](prototype/index.html)','','[Three-direction comparison](ConceptComparison.jpg) · [Fifteen-view overview](ConceptScreenOverview.jpg)','','| Concept / view / locale | Editable reference | Browser study |','| --- | --- | --- |']
idx=[]
for c in ledger['concepts']:
 for f in c['frames']:
  url='https://www.figma.com/design/9VQRtMDk8vFGer7gktEYsB?node-id='+f['id'].replace(':','-')
  parts=[p.strip() for p in f['name'].split('/')]
  view={'Home':'home','Selection':'selection','Settings':'settings','AI':'ai','Cinematic':'focus'}[parts[1]]
  local='review-evidence/'+parts[0]+'-'+view+'-'+parts[2]+'.png'
  md.append('| '+f['name']+' | [Open frame]('+url+') | [PNG]('+local+') |');idx.append(dict(name=f['name'],nodeId=f['id'],url=url,width=f['width'],height=f['height'],browserStudy=local))
md+=['','## Local visual evidence','','- [Living Light Home](mockups/A-Home-en.png) — native Figma composition export.','- Browser review captures are in `review-evidence/`; these include the external design-review toolbar and must not be mistaken for runtime Unity screenshots.','- [Existing artwork contact sheet](references/ExistingArtworkContactSheet.jpg) — source-reference sheet, not a new design.','','Select Direction, View and Locale in the standalone prototype to review all three treatments and optional/deferred interfaces. Post-selection family refinement is tracked in the coverage plan.']
(BASE/'VisualMockupIndex.md').write_text('\n'.join(md)+'\n',encoding='utf-8');(BASE/'MockupIndex.json').write_text(json.dumps(idx,indent=2)+'\n',encoding='utf-8')
print(json.dumps({'catalog_records':len(rows),'families':len(set(r['family'] for r in rows)),'bilingual_deferred_states':len(state_rows),'mockup_frames':len(idx),'inputs_read':len(sources)},indent=2))
