// Design-only Figma script. No production Unity assets are changed.
const createdNodeIds=[];
const page=figma.currentPage; page.name='00 · Foundations & source artwork';
const fontNames=[{family:'Source Sans 3',style:'Regular'},{family:'Source Sans 3',style:'SemiBold'},{family:'Source Serif 4',style:'Regular'},{family:'Noto Sans TC',style:'Regular'},{family:'Noto Sans TC',style:'Medium'},{family:'Noto Serif TC',style:'Regular'}];
await Promise.all(fontNames.map(f=>figma.loadFontAsync(f)));
const primitives=figma.variables.createVariableCollection('Stage2A / Primitives');
const semantics=figma.variables.createVariableCollection('Stage2A / Semantic');
primitives.renameMode(primitives.modes[0].modeId,'Exploration');semantics.renameMode(semantics.modes[0].modeId,'Exploration');
const vals={stage:'#101419',raised:'#212831',text:'#F3EEE4',secondary:'#C3C8CF',muted:'#A8B0BB',line:'#46505C',gaia:'#31D7F2',pandemic:'#B29A82',rebirth:'#D8A9C1',guardian:'#BFE9FF',conscience:'#FF6A2A',sanctuary:'#F3B95F',success:'#92D9BC',warning:'#F2CA7D',error:'#FFACAC',bStage:'#111C28',bRaised:'#29394A',cStage:'#0A0D10'};
const rgb=h=>({r:parseInt(h.slice(1,3),16)/255,g:parseInt(h.slice(3,5),16)/255,b:parseInt(h.slice(5,7),16)/255});
const vars={};
for(const [name,hex] of Object.entries(vals)){
 const p=figma.variables.createVariable('color/'+name,primitives,'COLOR');p.scopes=[];p.setValueForMode(primitives.modes[0].modeId,{...rgb(hex),a:1});p.setVariableCodeSyntax('WEB','var(--primitive-'+name+')');
 const v=figma.variables.createVariable('color/'+name,semantics,'COLOR');v.scopes=['FRAME_FILL','SHAPE_FILL','TEXT_FILL','STROKE_COLOR'];v.setValueForMode(semantics.modes[0].modeId,{type:'VARIABLE_ALIAS',id:p.id});v.setVariableCodeSyntax('WEB','var(--'+name+')');vars[name]=v.id;
}
for(const n of [8,12,16,24,32,48,64,96]) {const v=figma.variables.createVariable('space/'+n,semantics,'FLOAT');v.scopes=['GAP'];v.setValueForMode(semantics.modes[0].modeId,n);v.setVariableCodeSyntax('WEB','var(--space-'+n+')');vars['space'+n]=v.id;}
for(const n of [12,20,28,40]) {const v=figma.variables.createVariable('radius/'+n,semantics,'FLOAT');v.scopes=['CORNER_RADIUS'];v.setValueForMode(semantics.modes[0].modeId,n);v.setVariableCodeSyntax('WEB','var(--radius-'+n+')');vars['radius'+n]=v.id;}
const textStyles={};
for(const [locale,family,display] of [['en','Source Sans 3','Source Serif 4'],['zh','Noto Sans TC','Noto Serif TC']]) for(const [role,size] of [['display',88],['title',48],['section',30],['body',24],['label',22],['caption',18]]){
 const s=figma.createTextStyle();s.name='Stage2A/'+locale+'/'+role;s.fontName={family:role==='display'?display:family,style:'Regular'};s.fontSize=size;s.lineHeight={unit:'PERCENT',value:role==='display'?112:locale==='zh'?150:140};textStyles[locale+'/'+role]=s.id;
}
function txt(parent,value,size=24){const t=figma.createText();createdNodeIds.push(t.id);t.fontName={family:'Source Sans 3',style:'Regular'};t.fontSize=size;t.characters=value;t.fills=[{type:'SOLID',color:rgb(vals.text)}];parent.appendChild(t);return t;}
const componentIds={};const buttonProperties={};
for(const concept of ['A','B','C']) for(const state of ['Default','Hover','Pressed','Focus','Unavailable','Deferred']){
 const c=figma.createComponent();createdNodeIds.push(c.id);c.name='Button / '+concept+' / '+state;c.description='Stage 2A design study. One semantic action; label is a text property. Unavailable and deferred are distinct states. Not a runtime component.';
 c.layoutMode='HORIZONTAL';c.primaryAxisAlignItems='CENTER';c.counterAxisAlignItems='CENTER';c.resize(224,64);c.primaryAxisSizingMode='FIXED';c.counterAxisSizingMode='FIXED';c.cornerRadius=concept==='B'?32:concept==='C'?12:20;
 c.setBoundVariable('cornerRadius',await figma.variables.getVariableByIdAsync(vars['radius'+(concept==='B'?28:concept==='C'?12:20)]));
 const paint=figma.variables.setBoundVariableForPaint({type:'SOLID',color:rgb(vals.raised)},'color',await figma.variables.getVariableByIdAsync(vars[state==='Default'?'text':state==='Pressed'?'line':'raised']));c.fills=[paint];
 const lab=txt(c,'Continue',22);lab.fills=[{type:'SOLID',color:rgb(state==='Default'?vals.stage:vals.text)}];
 const key=c.addComponentProperty('Label','TEXT','Continue');lab.componentPropertyReferences={characters:key};
 if(state==='Unavailable'){c.opacity=.58;lab.characters='Unavailable';}if(state==='Deferred')lab.characters='After this moment';
 if(state==='Focus'){c.strokes=[{type:'SOLID',color:rgb(vals.text)}];c.strokeWeight=3;}
 c.x=200+(['A','B','C'].indexOf(concept))*300;c.y=400+(['Default','Hover','Pressed','Focus','Unavailable','Deferred'].indexOf(state))*100;
 componentIds[concept+'/'+state]=c.id;buttonProperties[concept+'/'+state]=key;
}
const title=txt(page,'AWAKENING CELEBRATIONS / VER.KA\nStage 2A · Three directions, one protected experience',48);title.x=200;title.y=120;
const sourceIds=['2:7','2:2','2:6','2:3','2:4','2:5'];
for(let i=0;i<sourceIds.length;i++){const n=await figma.getNodeByIdAsync(sourceIds[i]);if(n&&n.type==='FRAME'){n.name='Source artwork / '+['GAIA','PANDEMIC','RE-BIRTH','WHITE-GUARDIAN','CONSCIENCE','SANCTUARY'][i];n.resize(320,240);n.x=1300+(i%3)*360;n.y=400+Math.floor(i/3)*300;}}
return {createdNodeIds,mutatedNodeIds:[page.id,...sourceIds],collections:{primitives:primitives.id,semantic:semantics.id},vars,textStyles,componentIds,buttonProperties,fontNames,scope:'exploration foundations / 18 button state components; native layers; not production approved'};
