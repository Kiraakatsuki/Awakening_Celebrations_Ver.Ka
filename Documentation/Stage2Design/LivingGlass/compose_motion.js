// Separate native Figma motion specimens. Static design boards remain componentized.
const page=await figma.getNodeByIdAsync('18:48');await figma.setCurrentPageAsync(page);
await Promise.all([{family:'Source Sans 3',style:'Regular'},{family:'Source Serif 4',style:'Regular'},{family:'Noto Sans TC',style:'Regular'},{family:'Noto Serif TC',style:'Regular'}].map(f=>figma.loadFontAsync(f)));
const created=[],mutated=[],removed=[],frames=[];
const track=n=>{created.push(n.id,...n.findAll(()=>true).map(c=>c.id));return n;};
const ease={type:'CUSTOM_CUBIC_BEZIER',easingFunctionCubicBezier:{x1:.22,y1:.76,x2:.24,y2:1}};
function field(n,key,values){n.applyManualKeyframeTrack({type:'PROPERTY',name:key},{keyframes:values.map(([t,v])=>({timelinePosition:t,value:key==='SCALE_XY'?{type:'VECTOR',value:{x:v,y:v}}:{type:'FLOAT',value:v},easing:ease}))});mutated.push(n.id);}
const arc=await figma.getNodeByIdAsync('18:66'),settings=await figma.getNodeByIdAsync('18:401'),ai=await figma.getNodeByIdAsync('18:485');
for(const reduced of [false,true]){
 const f=track(arc.clone());page.appendChild(f);f.name=reduced?'Motion / Reduced gallery and overlays':'Motion / Gallery and overlays';f.x=reduced?2320:200;f.y=16000;
 const row=f.children.find(n=>n.name==='Six stable interaction regions');
 for(const [i,inst]of [...row.children].entries()){
  if(reduced)continue;
  // Plugin API cannot keyframe instance descendants; detach only this motion copy.
  removed.push(inst.id);const standalone=track(inst.detachInstance());
  const art=standalone.findAllWithCriteria({types:['FRAME']}).find(n=>n.name.startsWith('Artwork'));
  field(art,'OPACITY',[[i*.02,.4],[.52+i*.02,1]]);field(art,'TRANSLATION_Y',[[i*.02,18],[.52+i*.02,0]]);
 }
 const scrim=track(settings.children.find(n=>n.name==='Modal scrim').clone());f.appendChild(scrim);
 field(scrim,'OPACITY',[[0,0],[1.6,0],[reduced?1.66:1.82,.38],[2.8,.38],[reduced?2.86:3.02,0],[3.5,0],[reduced?3.56:3.72,.38],[4.7,.38],[reduced?4.76:4.92,0]]);
 for(const [src,name,start,enter,end,exit]of [[settings,'Settings panel',1.6,1.96,2.8,3.02],[ai,'Assistant panel',3.5,3.86,4.7,4.92]]){
  const panel=track(src.children.find(n=>n.name===name).clone());f.appendChild(panel);
  field(panel,'OPACITY',[[0,0],[start,0],[reduced?start+.06:enter,1],[end,1],[reduced?end+.06:exit,0]]);
  if(!reduced){field(panel,'TRANSLATION_Y',[[0,65],[start,65],[enter,0],[end,0],[exit,65]]);field(panel,'SCALE_XY',[[0,.94],[start,.94],[enter,1],[end,1],[exit,.94]]);}
 }
 f.setTimelineDuration(f.timelines[0].id,6);mutated.push(f.id);frames.push({id:f.id,name:f.name,duration:6,reduced});
}
const f=track(arc.clone());page.appendChild(f);f.name='Motion / GAIA chapter event storyboard';f.x=4440;f.y=16000;
for(const [id,start,enter,end,exit]of [['18:549',1.5,2.15,3.2,3.85],['18:570',3.2,3.85,6.85,7.5],['18:586',6.85,7.5,8.5,8.65],['18:603',8.5,8.65,10.0,10.65],['18:611',10.0,10.65,12.0,12.65]]){
 const src=await figma.getNodeByIdAsync(id),n=track(src.clone());f.appendChild(n);n.x=0;n.y=0;
 field(n,'OPACITY',[[0,0],[start,0],[enter,1],[end,1],[exit,0]]);
}
f.setTimelineDuration(f.timelines[0].id,14);mutated.push(f.id);frames.push({id:f.id,name:f.name,duration:14,semantic:'authored review events, still artwork only; no real media first-frame or completion'});
return {createdNodeIds:created,mutatedNodeIds:mutated,removedMotionInstanceIds:removed,frames,verification:'Native tracks authored; static boards untouched. No video export: local frame extractor unavailable. Not a browser/Unity runtime test.'};
