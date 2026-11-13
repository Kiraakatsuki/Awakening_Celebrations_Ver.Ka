// Prefix with const STATE=<StateLedger>, CONCEPT='A'|'B'|'C'. Native editable exploration.
const made=[];const track=n=>{made.push(n.id);return n;};
await Promise.all(STATE.fontNames.map(f=>figma.loadFontAsync(f)));
const name={A:'A · Living Light',B:'B · Fluid Glass',C:'C · Cinematic Spatial'}[CONCEPT];
let page=figma.root.children.find(p=>p.name===name);
if(page&&page.children.length) return {alreadyExists:true,pageId:page.id};
if(!page)page=track(figma.createPage());page.name=name;await figma.setCurrentPageAsync(page);
const vars={};await Promise.all(Object.entries(STATE.vars).map(async([k,id])=>{vars[k]=await figma.variables.getVariableByIdAsync(id);}));
const paint=(k,a=1)=>figma.variables.setBoundVariableForPaint({type:'SOLID',color:{r:0,g:0,b:0},opacity:a},'color',vars[k]);
function box(parent,n,x,y,w,h,k='stage',a=1,r=0){const f=track(figma.createFrame());f.name=n;f.resize(w,h);f.fills=[paint(k,a)];f.cornerRadius=r;parent.appendChild(f);f.x=x;f.y=y;return f;}
function column(parent,n,x,y,w,gap=20){const f=track(figma.createAutoLayout('VERTICAL'));f.name=n;f.fills=[];f.itemSpacing=gap;f.resize(w,100);f.primaryAxisSizingMode='AUTO';f.counterAxisSizingMode='FIXED';parent.appendChild(f);f.x=x;f.y=y;return f;}
function text(parent,value,loc,role='body',width=700,color='text',size){const t=track(figma.createText());t.name=value.slice(0,50);t.fontName={family:loc==='zh'?'Noto Sans TC':'Source Sans 3',style:'Regular'};t.characters=value;t.textStyleId=STATE.textStyles[loc+'/'+role];if(size)t.fontSize=size;t.fills=[paint(color)];t.resize(width,t.height);t.textAutoResize='HEIGHT';parent.appendChild(t);return t;}
function atText(parent,v,loc,role,x,y,w,col='text',size){const t=text(parent,v,loc,role,w,col,size);t.x=x;t.y=y;return t;}
const hashes=['d0b9dde2bbef820accb280eb544262867133da19','b60d5f32a4d7c19f34310fd2f8c9b7401c5b512c','0c9809f40f73677964b45824c227d85dc2a53bc5','29dbef3b4629075df8200a85af640f7c4bb8561d','1b4373bc801edf6490cb06e74a9d7902af6a78cc','f6a5af1bfcdbbbdd9321d7f6c127f41a6a00ba57'];
const performers=['GAIA','PANDEMIC','RE-BIRTH','WHITE-GUARDIAN','CONSCIENCE','SANCTUARY'];
const accents=['gaia','pandemic','rebirth','guardian','conscience','sanctuary'];
function art(parent,i,x,y,w,h,r=0){const n=box(parent,'Artwork / '+performers[i],x,y,w,h,'stage',1,r);n.fills=[{type:'IMAGE',imageHash:hashes[i],scaleMode:'FILL'}];n.clipsContent=true;return n;}
const buttons={};for(const state of ['Default','Hover','Focus','Unavailable','Deferred'])buttons[state]=await figma.getNodeByIdAsync(STATE.componentIds[CONCEPT+'/'+state]);
function button(parent,label,loc,x,y,w=240,state='Default'){const n=buttons[state].createInstance();made.push(n.id,...n.findAllWithCriteria({types:['TEXT']}).map(t=>t.id));n.setProperties({[STATE.buttonProperties[CONCEPT+'/'+state]]:label});n.resize(w,64);parent.appendChild(n);n.x=x;n.y=y;for(const t of n.findAllWithCriteria({types:['TEXT']})){t.fontName={family:loc==='zh'?'Noto Sans TC':'Source Sans 3',style:'Regular'};t.fontSize=22;}return n;}
const L=(loc,en,zh)=>loc==='zh'?zh:en;
function shell(root,loc,reduced=false){
 if(reduced){box(root,'Reduced shell / edge outside artwork',0,1000,1920,80,'stage');atText(root,'AWAKENING / '+L(loc,'Living Portrait','流動畫像'),loc,'caption',64,1024,600,'secondary');atText(root,L(loc,'Settings & AI after this moment','此段落結束後可開啟設定與 AI'),loc,'caption',730,1024,650,'muted');atText(root,L(loc,'Esc · Leave portrait','Esc · 離開畫像'),loc,'label',1560,1020,300);return;}
 const x=CONCEPT==='B'?420:CONCEPT==='C'?920:920;
 if(CONCEPT==='B')box(root,'Floating shell',x,968,1080,80,'bRaised',.94,40);else box(root,'Shell',0,984,1920,96,'stage',.98);
 atText(root,'VER.KA',loc,'label',CONCEPT==='B'?470:64,CONCEPT==='B'?992:1014,200,'secondary');
 const actions=[L(loc,'Settings','設定'),L(loc,'Ask AI','詢問 AI'),L(loc,'Help','說明'),L(loc,'Privacy','隱私')];
 actions.forEach((a,i)=>button(root,a,loc,(CONCEPT==='B'?670:950)+i*(CONCEPT==='B'?196:215),CONCEPT==='B'?976:1000,CONCEPT==='B'?180:195,'Hover'));
}
function header(root,loc,tag){atText(root,'AWAKENING CELEBRATIONS',loc,'label',64,42,700);atText(root,tag,loc,'caption',64,86,800,'muted');atText(root,'English  /  繁體中文',loc,'label',1550,42,330,'secondary');}
function scene(root,loc){
 if(CONCEPT==='C'){art(root,0,0,0,1920,984);box(root,'Reading field',0,0,850,984,'cStage',.92);}
 else if(CONCEPT==='B'){art(root,0,560,0,1360,984);box(root,'Tinted reading field',0,0,700,984,'bStage',.94);}
 else{art(root,0,1040,140,816,760,28);}
 header(root,loc,L(loc,'SIX PRESENCES · ONE SHARED AWAKENING','六種存在・共同甦醒'));
}
function home(root,loc){
 const cinematic=CONCEPT==='C';art(root,5,cinematic?0:CONCEPT==='B'?510:880,cinematic?0:130,cinematic?1920:CONCEPT==='B'?1350:980,cinematic?984:790,cinematic?0:CONCEPT==='B'?40:28);
 if(cinematic)box(root,'Dark reading field',0,0,930,984,'cStage',.88);
 if(CONCEPT==='B')box(root,'Light-bearing reading surface',72,220,860,674,'bRaised',.92,40);
 header(root,loc,L(loc,'AN INTERACTIVE EXHIBITION','互動展演體驗'));
 const c=column(root,'Home content',CONCEPT==='B'?124:96,CONCEPT==='B'?280:245,CONCEPT==='B'?720:730,26);
 text(c,L(loc,'A presence.\nAn awakening.','與存在相遇。\n讓感知甦醒。'),loc,'display',730,'text',CONCEPT==='B'?80:88);
 text(c,L(loc,'Six performers. A world revealed through your movement.','六位表演者，以你的動作，開啟感知世界。'),loc,'body',640,'secondary');
 text(c,L(loc,'Your camera stays off until you choose gesture interaction.','相機會保持關閉，直到你選擇手勢互動。'),loc,'label',640,'secondary');
 button(root,L(loc,'Review & begin','閱讀並開始'),loc,CONCEPT==='B'?124:96,760,300);
 atText(root,L(loc,'Camera off · Your choice, this session only','相機已關閉・你的選擇僅限本次體驗'),loc,'caption',CONCEPT==='B'?124:96,852,710,'secondary');
 shell(root,loc);
}
function selection(root,loc){header(root,loc,L(loc,'COLOR RELEASE','色彩釋放'));atText(root,L(loc,'Who will you awaken?','你想喚醒哪種存在？'),loc,'display',80,148,1500,'text',72);
 atText(root,L(loc,'Point to explore. Make a fist to choose. Or click / press 1–6.','指向以探索，握拳以選擇。也可點擊或按 1–6。'),loc,'body',84,250,1500,'secondary');
 if(CONCEPT==='A')for(let i=0;i<6;i++){const x=80+i*294;art(root,i,x,360,274,450,20);atText(root,performers[i],loc,'label',x,834,278,accents[i],i===3?20:24);atText(root,'0'+(i+1)+' / '+L(loc,'Choose','選擇'),loc,'caption',x,880,270,'muted');}
 if(CONCEPT==='B')for(let i=0;i<6;i++){const x=104+(i%3)*584,y=340+Math.floor(i/3)*292;const f=box(root,'Glass performer surface',x,y,552,264,'bRaised',.94,28);art(f,i,8,8,224,248,24);atText(f,'0'+(i+1),loc,'caption',264,30,240,'muted');atText(f,performers[i],loc,'section',264,78,264,accents[i],i===3?23:30);atText(f,L(loc,'Meet this presence','遇見此刻的存在'),loc,'label',264,160,245,'secondary');}
 if(CONCEPT==='C'){art(root,0,650,330,1190,610);for(let i=0;i<6;i++){atText(root,'0'+(i+1)+'   '+performers[i],loc,'section',96,360+i*84,520,i===0?'gaia':'secondary');}atText(root,L(loc,'GAIA · Targeted, not yet selected','GAIA・已指向，尚未選取'),loc,'label',740,870,900,'text');}
 shell(root,loc);
}
function overlay(root,loc,type){scene(root,loc);box(root,'Modal scrim',0,0,1920,984,'stage',CONCEPT==='B'?.30:.46);
 const x=CONCEPT==='A'?1080:CONCEPT==='B'?360:64,w=CONCEPT==='B'?1200:CONCEPT==='C'?760:776;
 const p=box(root,type+' panel',x,130,w,800,CONCEPT==='B'?'bRaised':'raised',.97,CONCEPT==='B'?40:28);
 if(CONCEPT==='B')p.effects=[{type:'BACKGROUND_BLUR',radius:32,visible:true}];
 const c=column(p,'Panel content',48,40,w-96,20);
 text(c,L(loc,type==='Settings'?'Your experience':'A different perspective',type==='Settings'?'你的體驗':'從另一個角度，看見'),loc,'title',w-96);
 text(c,L(loc,type==='Settings'?'Quiet controls. Return to exactly where you were.':'GAIA / Assistant · Camera not required',type==='Settings'?'安靜調整，回到剛才的體驗。':'GAIA／AI 助理・不需開啟相機'),loc,'label',w-96,'secondary');
 if(type==='Settings'){
  const rows=[['Language','語言','English / 繁體中文'],['Sound','聲音',L(loc,'Designed · deferred','已設計・功能暫緩')],['Accessibility','無障礙',L(loc,'Motion · effects · text','動態・效果・文字')],['Camera & privacy','相機與隱私',L(loc,'Capture suspended here','此處暫停擷取')],['End experience','結束體驗',L(loc,'Reset policy to approve','重設規則待核准')]];
  for(const [en,zh,val]of rows){const row=track(figma.createAutoLayout('VERTICAL'));row.name=en;row.itemSpacing=4;row.fills=[];c.appendChild(row);text(row,L(loc,en,zh),loc,'section',w-100);text(row,val,loc,'caption',w-100,'secondary');}
  text(c,L(loc,'Optional calibration belongs in Help, only if needed.','校正僅在有需要時，由說明入口提供。'),loc,'caption',w-96,'muted');
 }else{
  text(c,L(loc,'What would you like to explore?','你想探索什麼？'),loc,'section',w-96);
  for(const [en,zh]of [['Meet GAIA','認識 GAIA'],['About this experience','關於這段體驗'],['How to interact','如何互動']]){const row=box(c,'Suggestion / '+en,0,0,w-96,66,'stage',.7,12);atText(row,L(loc,en,zh),loc,'label',22,16,w-140);}
  text(c,L(loc,'Your conversation begins here.','從這裡開始對話。'),loc,'body',w-96,'secondary');
  text(c,L(loc,'No conversation or network request is active in this design study.','此設計稿未啟動對話或網路請求。'),loc,'caption',w-96,'muted');
  const field=box(p,'Question field',48,626,w-96,72,'stage',1,20);atText(field,L(loc,'Ask a question…','輸入問題…'),loc,'label',24,20,w-180,'secondary');
 }
 button(p,L(loc,'Close · Esc','關閉・Esc'),loc,48,712,250,'Hover');shell(root,loc);
}
function cinema(root,loc){art(root,0,CONCEPT==='A'?64:0,CONCEPT==='A'?64:0,CONCEPT==='A'?1792:1920,CONCEPT==='A'?872:1000,CONCEPT==='A'?28:0);
 if(CONCEPT==='B')box(root,'Quiet cinematic title surface',64,64,480,156,'bRaised',.9,28);
 if(CONCEPT==='C')box(root,'Title contrast veil',0,0,650,260,'cStage',.80);
 atText(root,'GAIA',loc,'display',CONCEPT==='B'?96:100,CONCEPT==='B'?82:92,640,'text',72);
 atText(root,L(loc,'Living Portrait','流動畫像'),loc,'body',CONCEPT==='B'?100:104,CONCEPT==='B'?170:190,650,'secondary');
 shell(root,loc,true);
}
const frames=[];
for(const [row,loc]of ['en','zh'].entries())for(const [col,kind]of ['Home','Selection','Settings','AI','Cinematic'].entries()){
 const root=box(page,CONCEPT+' / '+kind+' / '+loc,200+col*2120,200+row*1300,1920,1080,CONCEPT==='B'?'bStage':CONCEPT==='C'?'cStage':'stage');root.clipsContent=true;
 if(kind==='Home')home(root,loc);if(kind==='Selection')selection(root,loc);if(kind==='Settings'||kind==='AI')overlay(root,loc,kind);if(kind==='Cinematic')cinema(root,loc);
 frames.push({id:root.id,name:root.name,width:root.width,height:root.height,nodeCount:root.findAll(()=>true).length,imageNodes:root.findAllWithCriteria({types:['FRAME']}).filter(n=>Array.isArray(n.fills)&&n.fills.some(p=>p.type==='IMAGE')).length});
}
return {createdNodeIds:made,pageId:page.id,concept:CONCEPT,frames,fontFamilies:['Source Sans 3','Source Serif 4','Noto Sans TC','Noto Serif TC'],editable:true,notes:'Artwork is discrete content; all text and UI are editable. Background blur is Figma reference only, not a proven Unity effect.'};
