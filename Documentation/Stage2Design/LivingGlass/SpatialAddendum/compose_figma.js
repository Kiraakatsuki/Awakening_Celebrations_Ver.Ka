// Prefix OLD with the Stage 2B FigmaLedger. New boards derive only from approved Arc.
const created=[],removed=[];const track=n=>{created.push(n.id);return n;};const clone=n=>{const c=n.clone();created.push(c.id,...c.findAll(()=>true).map(x=>x.id));return c;};
const name='Living Glass · Arc spatial addendum';const existing=figma.root.children.find(p=>p.name===name);if(existing&&existing.children.length)return {alreadyExists:true,pageId:existing.id};
await Promise.all([{family:'Source Sans 3',style:'Regular'},{family:'Source Serif 4',style:'Regular'},{family:'Noto Sans TC',style:'Regular'},{family:'Noto Serif TC',style:'Regular'}].map(f=>figma.loadFontAsync(f)));
const sources=await Promise.all(['18:66','18:634','18:401','18:969','18:485','18:1053'].map(id=>figma.getNodeByIdAsync(id)));
const page=existing||track(figma.createPage());page.name=name;await figma.setCurrentPageAsync(page);
const names=['GAIA','PANDEMIC','RE-BIRTH','WHITE-GUARDIAN','CONSCIENCE','SANCTUARY'];const keys=['gaia','pandemic','rebirth','guardian','conscience','sanctuary'];
const atmosphere=['#137F91','#756453','#8B557C','#6A839A','#9B4B2B','#98713B'];
const rgb=h=>({r:parseInt(h.slice(1,3),16)/255,g:parseInt(h.slice(3,5),16)/255,b:parseInt(h.slice(5,7),16)/255});
const collection=(await figma.variables.getLocalVariableCollectionsAsync()).find(c=>c.id===OLD.collectionId);const atmo={};
for(let i=0;i<6;i++){const v=figma.variables.createVariable('spatial/atmosphere/'+names[i],collection,'COLOR');v.scopes=['FRAME_FILL','SHAPE_FILL'];v.setValueForMode(collection.defaultModeId,rgb(atmosphere[i]));atmo[i]=v;}
const accents=await Promise.all(keys.map(k=>figma.variables.getVariableByIdAsync(OLD.variables[k])));
const solid=(v,a=1)=>figma.variables.setBoundVariableForPaint({type:'SOLID',color:{r:1,g:1,b:1},opacity:a},'color',v);
function label(parent,value,loc,x,y,w,size=18){const t=track(figma.createText());t.name='Selection visual state';t.fontName={family:loc==='zh'?'Noto Sans TC':'Source Sans 3',style:'Regular'};t.characters=value;t.fontSize=size;t.lineHeight={unit:'PERCENT',value:150};t.fills=[{type:'SOLID',color:rgb('#C2CBD5')}];t.resize(w,t.height);t.textAutoResize='HEIGHT';parent.appendChild(t);t.x=x;t.y=y;return t;}
const bounds=region=>({x:region.x,y:region.y,width:region.width,height:region.height});
const frames=[];let count=0;
function make(mode,i,loc,phase='targeted',overlay=null){
 const src=overlay?(overlay==='settings'?sources[loc==='zh'?3:2]:sources[loc==='zh'?5:4]):sources[loc==='zh'?1:0];
 const f=clone(src);page.appendChild(f);f.name=`Arc spatial / ${mode} / ${i<0?'NEUTRAL':names[i]} / ${phase}${overlay?' '+overlay:''} / ${loc}`;f.x=200+(count%4)*2120;f.y=200+Math.floor(count/4)*1280;count++;
 const row=f.children.find(n=>n.name==='Six stable interaction regions');const before=row.children.map(bounds);
 row.children.forEach((region,j)=>{const im=region.findAllWithCriteria({types:['FRAME']}).find(n=>Array.isArray(n.fills)&&n.fills.some(p=>p.type==='IMAGE'));if(im){im.fills=im.fills.map(p=>p.type==='IMAGE'?{...p,opacity:phase==='targeted'&&j===i?1:phase==='acquiring'&&j===i?.85:.78}:p);created.push(im.id);}});
 if(i>=0&&phase==='targeted'&&!overlay){const original=row.children[i];removed.push(original.id);const region=original.detachInstance();created.push(region.id,...region.findAll(()=>true).map(n=>n.id));region.name='Stable hit region PER-00'+(i+1);region.clipsContent=false;
 const art=region.children.find(n=>n.type==='FRAME'&&Array.isArray(n.fills)&&n.fills.some(p=>p.type==='IMAGE'));const w=art.width,h=art.height;
 const slot=track(figma.createFrame());slot.name='Non-picking optical slot';slot.resize(w,h);slot.fills=[];slot.clipsContent=false;region.insertChild(0,slot);slot.appendChild(art);
 const scale=mode==='B'?1.020:1.006,lift=mode==='B'?6:2;art.resize(w*scale,h*scale);art.x=-(w*(scale-1))/2;art.y=-(h*(scale-1))/2-lift;art.rotation=mode==='B'?(i<3?.45:-.45):0;art.strokes=[solid(accents[i],.45)];art.strokeWeight=1;
 region.resize(before[i].width,before[i].height);
 }
 if(i>=0&&phase!=='acquiring'){
  const light=track(figma.createFrame());light.name='Localized atmosphere / '+names[i];light.resize(1920,660);light.x=0;light.y=230;light.fills=[{type:'GRADIENT_RADIAL',gradientTransform:[[.9,0,.05],[0,.7,.20]],opacity:overlay?.025:.10,gradientStops:[{position:0,color:{...rgb(atmosphere[i]),a:1},boundVariables:{color:{type:'VARIABLE_ALIAS',id:atmo[i].id}}},{position:1,color:{...rgb(atmosphere[i]),a:0}}]}];f.insertChild(0,light);light.x=0;light.y=230;
 }
 const plate=f.children.find(n=>n.name==='Floating identification');if(plate){const texts=plate.findAllWithCriteria({types:['TEXT']});const identity=plate.children.find(n=>n.name==='Identity and target state');if(identity){const ts=identity.findAllWithCriteria({types:['TEXT']});ts[0].characters=i<0?(loc==='zh'?'六種藝術存在':'Six artistic presences'):names[i];ts[1].characters=loc==='zh'?(phase==='acquiring'?'正在取得目標・尚未選取。':phase==='lossPending'?'追蹤中斷・暫停確認。':i<0?'以自己的步調探索，尚未做出選擇。':'目標已穩定・須確認。'):(phase==='acquiring'?'Acquiring a target · not selected.':phase==='lossPending'?'Tracking interrupted · confirmation suspended.':i<0?'Explore at your own pace. No choice has been made.':'Target stable · confirmation required.');}
 const index=plate.children.find(n=>n.type==='TEXT');if(index){index.characters=i<0?'01—06':'0'+(i+1);if(i>=0&&phase!=='acquiring')index.fills=[solid(accents[i])];}
 if(i>=0&&phase!=='acquiring'){const rim=track(figma.createFrame());rim.name='Contextual glass upper rim';rim.resize(300,1);rim.fills=[solid(accents[i],.42)];plate.appendChild(rim);rim.x=260;rim.y=0;}
 }
 if(i>=0&&phase==='acquiring'){const region=row.children[i],art=region.findAllWithCriteria({types:['FRAME']}).find(n=>Array.isArray(n.fills)&&n.fills.some(p=>p.type==='IMAGE'));if(art){art.strokes=[solid(accents[i],.18)];art.strokeWeight=1;}}
 if(overlay){const panel=f.children.find(n=>n.name=== (overlay==='settings'?'Settings panel':'Assistant panel'));const rim=track(figma.createFrame());rim.name='Restrained contextual accent';rim.resize(688,2);rim.fills=[solid(accents[i],.22)];panel.appendChild(rim);rim.x=36;rim.y=0;}
 label(f,loc==='zh'?(phase==='acquiring'?'正在取得目標・僅局部回饋':phase==='lossPending'?'追蹤中斷・保留上一個氛圍':i<0?'中性藝廊':'目標已穩定・須確認'):(phase==='acquiring'?'Target acquiring · local feedback only':phase==='lossPending'?'Tracking interrupted · previous atmosphere held':i<0?'Neutral gallery':'Target stable · confirmation required'),loc,1320,290,505,17);
 const after=row.children.map(bounds);const unchanged=JSON.stringify(before)===JSON.stringify(after);if(!unchanged)throw new Error('Stable wrapper geometry changed for '+f.name);
 frames.push({id:f.id,name:f.name,mode,performer:i<0?'NEUTRAL':names[i],locale:loc,phase,overlay,width:f.width,height:f.height,regions:after,stableRegionsUnchanged:unchanged,editableText:f.findAllWithCriteria({types:['TEXT']}).length,instances:f.findAllWithCriteria({types:['INSTANCE']}).length,nativePose:'2D optical proxy; browser perspective is a separate unverified prototype effect'});
}
for(const loc of ['en','zh']){for(const mode of ['A','B'])for(let i=0;i<6;i++)make(mode,i,loc);make('B',-1,loc,'neutral');make('B',0,loc,'acquiring');make('B',0,loc,'lossPending');make('B',0,loc,'targeted','settings');make('B',0,loc,'targeted','ai');}
return {pageId:page.id,sourceArcIds:['18:66','18:634'],createdNodeIds:created,removedCopiedInstanceIds:removed,frames,atmosphereVariables:Object.fromEntries(Object.entries(atmo).map(([i,v])=>[names[i],v.id])),scope:'Arc-only native review boards; no production edits, new uploads or browser verification'};
