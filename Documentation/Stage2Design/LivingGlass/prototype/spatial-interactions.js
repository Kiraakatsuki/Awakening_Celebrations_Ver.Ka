/* Isolated Arc presentation. Depends on the existing prototype's functions at call time. */
const spatialModel=LivingGlassSpatial.create();
let spatialInput='context',spatialTimers=[],spatialSequence=0,spatialPaintFrame=0;
const pointerPose=Array.from({length:6},()=>({x:0,y:0}));
function stopSpatialSequence(){spatialSequence++;spatialTimers.forEach(clearTimeout);spatialTimers=[];}
function queueSpatialPaint(){if(!spatialPaintFrame)spatialPaintFrame=requestAnimationFrame(()=>{spatialPaintFrame=0;paintSpatial();});}
function spatialText(el,value){if(el&&el.textContent!==value)el.textContent=value;}
function settleSpatialEntrance(){
 const nodes=state.view==='gallery'?[...app.querySelectorAll('.view:not(.gallery-ghost) .art-window')]:[];
 const starts=nodes.map(n=>({n,transform:getComputedStyle(n).transform,opacity:getComputedStyle(n).opacity}));
 cancelScene();if(!reduce())for(const start of starts)animateNode(start.n,[{transform:start.transform,opacity:start.opacity},{transform:'none',opacity:1}],{duration:120});
}
function spatialTarget(i,source='pointer',stable=true,reviewOverride=false){
 if(state.view!=='gallery'||state.overlay||document.querySelector('.overlay-plane'))return;
 if(source!=='gesture')stopSpatialSequence();
 if(reviewOverride){spatialModel.release(spatialModel.snapshot().owner);}
 if(source!=='pointer')pointerPose[i]={x:(i-2.5)/2.5*.35,y:-.25};
 const before=spatialModel.snapshot();spatialModel.acquire(i,source);if(stable)spatialModel.stable(i,source);
 const s=spatialModel.snapshot();if(s.revision!==before.revision){if(sceneAnimations.some(a=>a.playState==='running'))settleSpatialEntrance();state.target=s.candidate>=0?s.candidate:s.target;state.gestureTarget=s.gestureTarget;paintSpatial();}
}
function paintSpatial(){
 const s=spatialModel.snapshot(),mode=document.querySelector('#treatment')?.value||'B',reduced=reduce(),opaque=document.querySelector('#opaque').checked;
 const suppressed=s.overlay||!!document.querySelector('.overlay-plane')||document.hidden;
 app.classList.remove('mode-A','mode-B');app.classList.add('mode-'+mode);app.classList.toggle('motion-suppressed',suppressed);app.classList.toggle('reduced',reduced);app.classList.toggle('opaque',opaque);app.classList.toggle('hitbox',document.querySelector('#hitboxes').checked);
 app.dataset.spatialPhase=s.phase;app.dataset.theme=s.theme<0?'neutral':LivingGlassSpatial.palettes[s.theme].name;app.dataset.inputOwner=s.owner||'none';
 const palette=s.theme>=0?LivingGlassSpatial.palettes[s.theme]:null,accent=palette?.accent||'#8BD6E5';
 app.style.setProperty('--context-accent',accent);app.style.setProperty('--theme-strength',palette?'.42':'0');
 const light=app.querySelector('.view:not(.gallery-ghost) .environment-light');if(light){light.style.backgroundColor=palette?.atmosphere||'#173B48';light.style.opacity=palette?(suppressed?'.025':'.10'):'0';light.style.transitionDuration=reduced?'60ms':'280ms';}
 const current=app.querySelector('.view:not(.gallery-ghost)');
 current?.querySelectorAll('.presence').forEach((button,i)=>{
  const active=['targeted','confirmed'].includes(s.phase)&&s.target===i&&!suppressed,acquiring=s.phase==='acquiring'&&s.candidate===i&&!suppressed;
  button.classList.toggle('targeted',active);button.classList.toggle('acquiring',acquiring);
  const surface=button.querySelector('.spatial-surface');if(!surface)return;
  const reflection=surface.querySelector('.portrait-reflection');if(reflection)reflection.style.backgroundColor=palette?.secondary||'#C2CBD5';
  const pos=pointerPose[i],p=LivingGlassSpatial.pose(mode,pos.x,pos.y,reduced,active);
  if(!active&&!reduced&&!suppressed&&mode==='B'&&s.phase==='targeted')p.scale=LivingGlassSpatial.bounds.B.recession;
  for(const[k,v]of Object.entries({'--ss':String(p.scale),'--sx':p.x+'px','--sy':p.y+'px','--rx':p.rx+'deg','--ry':p.ry+'deg','--px':p.px+'px','--py':p.py+'px','--reflection-x':p.px*.5+'px'}))surface.style.setProperty(k,v);
 });
 if(state.view==='gallery'){
  const id=s.phase==='acquiring'?s.candidate:s.target,displayId=id>=0?id:-1;
  state.target=displayId;state.gestureTarget=s.gestureTarget;
  const name=current?.querySelector('#identity-name'),copy=current?.querySelector('#identity-copy'),idx=current?.querySelector('.identity-index'),confirmButton=current?.querySelector('[data-action="confirm"]');
  spatialText(name,displayId>=0?performers[displayId].name:T('Six artistic presences','六種藝術存在'));
  spatialText(copy,s.phase==='acquiring'?T('Acquiring a target · not selected.','正在取得目標・尚未選取。'):displayId>=0?T(performers[displayId].en,performers[displayId].zh):T('Explore at your own pace. No choice has been made.','以自己的步調探索，尚未做出選擇。'));
  if(idx){spatialText(idx,displayId>=0?'0'+(displayId+1):'01—06');idx.style.color=s.phase==='acquiring'?'#C2CBD5':'';}
  if(confirmButton)confirmButton.disabled=displayId<0||!['targeted','context'].includes(s.phase)||suppressed;
  const labels={neutral:['Neutral gallery','中性藝廊'],acquiring:['Target acquiring · local feedback only','正在取得目標・僅局部回饋'],targeted:['Target stable · confirmation required','目標已穩定・須確認'],lossPending:['Tracking interrupted · waiting for target-loss state','追蹤中斷・等待目標遺失狀態'],trackingLost:['Target cleared · returning to neutral','目標已清除・回到中性狀態'],context:['Context retained · acquire a fresh gesture target','保留情境・請重新取得手勢目標'],confirmed:['Performer confirmed','已確認表演者']};
  const status=current?.querySelector('.theme-state');spatialText(status,T(...(labels[s.phase]||labels.neutral)));
  const tracking=current?.querySelector('.tracking');if(tracking){tracking.dataset.status=['lossPending','trackingLost'].includes(s.phase)?'lost':s.phase;tracking.textContent=s.phase==='lossPending'?T('Confirmation suspended. The last atmosphere is held.','已暫停確認，保留上一個氛圍。'):s.phase==='trackingLost'?T('Point again, or use pointer and keyboard.','請重新指向，或使用滑鼠與鍵盤。'):T('Camera off · review events only','相機已關閉・僅為檢視事件');}
 }
 const review=document.querySelector('#spatial-status');spatialText(review,`${mode} / ${s.phase} / ${s.owner||'none'}${reduced?' / '+T('Reduced motion','減少動態效果'):''}`);
}
function handleSpatialReviewEvent(ev){
 if(!['ready','point','fist','loss','cleared','recover','acquire','neutral'].includes(ev))return false;
 stopSpatialSequence();if(state.view!=='gallery')navigate('gallery');
 const i=Number(document.querySelector('#theme').value),before=spatialModel.snapshot();
 if(ev==='neutral'||ev==='ready')spatialModel.neutral();
 if(ev==='acquire')spatialTarget(i,'gesture',false,true);
 if(ev==='point')spatialTarget(i,'gesture',true,true);
 if(ev==='loss')spatialModel.observationGap();
 if(ev==='cleared')spatialModel.targetLost();
 if(ev==='recover')spatialModel.recover();
 if(ev==='fist'){
  const s=spatialModel.snapshot();if(s.gestureTarget<0){log('Fist ignored: acquire and stabilize a fresh gesture target.','未接受握拳：請重新取得並穩定手勢目標。');paintSpatial();return true;}
  confirm(s.gestureTarget,'review fist event');return true;
 }
 paintSpatial();log(`Review event: ${ev}; no detector threshold or camera state changed.`,`檢視事件：${ev}；未變更偵測門檻或相機狀態。`);return true;
}
function setupSpatialReview(){
 document.querySelector('#theme').innerHTML=performers.map((p,i)=>`<option value="${i}">${p.name}</option>`).join('');
 document.addEventListener('keydown',e=>{if(['Tab','ArrowLeft','ArrowRight','ArrowUp','ArrowDown'].includes(e.key))spatialInput='keyboard';if(!e.target.matches('#treatment,#reduced,#opaque,#hitboxes')&&['Tab','Enter',' ','Escape','ArrowLeft','ArrowRight','ArrowUp','ArrowDown'].includes(e.key))stopSpatialSequence();});
 document.addEventListener('pointerdown',e=>{if(e.target.closest('#treatment,#reduced,#opaque,#hitboxes'))return;spatialInput='pointer';stopSpatialSequence();});
 document.addEventListener('focusin',e=>{if(!app.contains(e.target)){spatialModel.release('keyboard');paintSpatial();}});
 app.addEventListener('pointerover',e=>{const b=e.target.closest('.presence');if(!b||b.closest('.gallery-ghost')||state.view!=='gallery'||state.overlay)return;const i=+b.dataset.performer;if(spatialModel.snapshot().target!==i||spatialModel.snapshot().owner!=='pointer')spatialTarget(i,'pointer');});
 app.addEventListener('pointermove',e=>{const b=e.target.closest('.presence');if(!b||b.closest('.gallery-ghost')||spatialModel.snapshot().owner!=='pointer'||spatialModel.snapshot().target!==+b.dataset.performer)return;const r=b.getBoundingClientRect(),i=+b.dataset.performer;pointerPose[i]={x:(e.clientX-r.left)/r.width*2-1,y:(e.clientY-r.top)/r.height*2-1};queueSpatialPaint();});
 app.addEventListener('pointerout',e=>{const from=e.target.closest('.presence'),to=e.relatedTarget?.closest?.('.presence');if(from&&from!==to){spatialModel.release('pointer');paintSpatial();}});
 app.addEventListener('focusin',e=>{const b=e.target.closest('.presence');if(!b||b.closest('.gallery-ghost')||state.view!=='gallery'||state.overlay)return;pointerPose[+b.dataset.performer]={x:(+b.dataset.performer-2.5)/2.5*.35,y:-.25};spatialTarget(+b.dataset.performer,spatialInput==='keyboard'?'keyboard':'context');});
 document.querySelector('.comparison-tools').addEventListener('click',e=>{
  const cmd=e.target.closest('[data-spatial]')?.dataset.spatial;if(!cmd)return;stopSpatialSequence();
  if(state.view!=='gallery')navigate('gallery');const selected=Number(document.querySelector('#theme').value);
  if(cmd==='neutral'){spatialModel.neutral();paintSpatial();return;}
  if(cmd==='acquire'){spatialTarget(selected,'gesture',false,true);return;}
  if(cmd==='stable'){spatialTarget(selected,'gesture',true,true);return;}
  if(cmd==='confirm'){const s=spatialModel.snapshot();if(s.phase==='targeted'&&s.target===selected)confirm(selected,'explicit review action');else log('Stabilize the selected review target first.','請先穩定所選的檢視目標。');return;}
  if(cmd==='next'){const next=(selected+1)%6;document.querySelector('#theme').value=String(next);spatialTarget(next,'gesture',true,true);return;}
  if(cmd==='bridge'){
   spatialModel.gallery(false);paintSpatial();const generation=spatialSequence;
   const step=(delay,fn)=>spatialTimers.push(setTimeout(()=>{if(generation===spatialSequence)fn();},delay));
   step(450,()=>spatialTarget(selected,'gesture',false,true));step(850,()=>spatialTarget(selected,'gesture',true,true));step(1700,()=>confirm(selected,'scripted review bridge'));
   log('Scripted review bridge: neutral → acquisition → stable theme → explicit confirmation → Focus. These delays are not detector thresholds.','腳本式檢視過渡：中性 → 取得目標 → 穩定氛圍 → 明確確認 → 凝視。這些延遲不是偵測門檻。');
  }
 });
 document.querySelector('#theme').addEventListener('change',()=>{stopSpatialSequence();});
 matchMedia('(prefers-reduced-motion: reduce)').addEventListener('change',()=>paintSpatial());
}
