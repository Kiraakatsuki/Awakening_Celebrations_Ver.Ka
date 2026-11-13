/* Pure presentation/input arbitration for the isolated design prototype. No DOM, timers or services. */
(function(root,factory){const api=factory();if(typeof module==='object'&&module.exports)module.exports=api;else root.LivingGlassSpatial=api;})(typeof globalThis!=='undefined'?globalThis:this,function(){
 'use strict';
 const palettes=[
  {name:'GAIA',accent:'#5BD8ED',atmosphere:'#137F91',secondary:'#3BAFAE'},
  {name:'PANDEMIC',accent:'#C0AA91',atmosphere:'#756453',secondary:'#9A8163'},
  {name:'RE-BIRTH',accent:'#E0B0C8',atmosphere:'#8B557C',secondary:'#B77EA5'},
  {name:'WHITE-GUARDIAN',accent:'#C5E9FA',atmosphere:'#6A839A',secondary:'#A5B6C4'},
  {name:'CONSCIENCE',accent:'#FF956A',atmosphere:'#9B4B2B',secondary:'#C87546'},
  {name:'SANCTUARY',accent:'#F1C57A',atmosphere:'#98713B',secondary:'#BB9456'}
 ];
 const bounds={A:{scale:1.006,lift:2,translateX:0,rotateX:0,rotateY:0,parallaxX:0,parallaxY:0,recession:1},B:{scale:1.020,lift:6,translateX:4,rotateX:1,rotateY:1.4,parallaxX:3,parallaxY:2,recession:.996},responseMs:240,returnMs:340,themeMs:280,confirmationMs:650,reducedMs:60};
 const clamp=(v,lo=-1,hi=1)=>Math.min(hi,Math.max(lo,Number.isFinite(v)?v:0));
 function pose(mode='B',x=0,y=0,reduced=false,active=true){const b=bounds[mode]||bounds.B;x=clamp(x);y=clamp(y);if(reduced||!active)return {scale:1,x:0,y:0,rx:0,ry:0,px:0,py:0,recession:1};return {scale:b.scale,x:x*b.translateX,y:-b.lift+(mode==='B'?y:0),rx:-y*b.rotateX,ry:x*b.rotateY,px:x*b.parallaxX,py:y*b.parallaxY,recession:b.recession};}
 function create(){let s={phase:'neutral',candidate:-1,target:-1,theme:-1,owner:null,gestureTarget:-1,overlay:false,committed:-1,revision:0};const valid=i=>Number.isInteger(i)&&i>=0&&i<6;const snapshot=()=>({...s});function touch(){s.revision++;return snapshot();}
  function accept(source,explicit=false){if(s.overlay||s.phase==='confirmed')return false;if(explicit)return true;if(s.owner==='keyboard'&&source!=='keyboard')return false;if(s.owner==='pointer'&&source==='gesture')return false;return true;}
  return {snapshot,
   acquire(i,source='gesture'){if(!valid(i)||!accept(source))return snapshot();s.candidate=i;s.owner=source;s.gestureTarget=-1;s.phase='acquiring';return touch();},
   stable(i,source='gesture'){if(!valid(i)||!accept(source)||s.candidate!==i||s.owner!==source)return snapshot();s.target=i;s.theme=i;s.phase='targeted';s.gestureTarget=source==='gesture'?i:-1;return touch();},
   observationGap(){if(s.owner!=='gesture'||s.overlay||s.phase==='confirmed')return snapshot();s.gestureTarget=-1;s.candidate=-1;s.phase='lossPending';return touch();},
   targetLost(){if(s.owner!=='gesture'||s.overlay||s.phase==='confirmed')return snapshot();s.gestureTarget=-1;s.candidate=-1;s.target=-1;s.theme=-1;s.owner=null;s.phase='trackingLost';return touch();},
   recover(){if(s.phase!=='lossPending'&&s.phase!=='trackingLost')return snapshot();s.gestureTarget=-1;s.target=-1;s.candidate=-1;s.owner=null;s.phase='context';return touch();},
   release(source){if(s.owner!==source||s.phase==='confirmed'||s.overlay)return snapshot();s.gestureTarget=-1;s.candidate=-1;s.owner=null;s.phase=s.theme>=0?'context':'neutral';return touch();},
   confirm(i,source='gesture'){if(!valid(i)||s.overlay||s.phase==='confirmed')return {accepted:false,state:snapshot()};const permitted=source==='gesture'?s.phase==='targeted'&&s.owner==='gesture'&&s.gestureTarget===i&&s.target===i:true;if(!permitted)return {accepted:false,state:snapshot()};s.target=i;s.theme=i;s.candidate=i;s.owner=source;s.gestureTarget=-1;s.committed=i;s.phase='confirmed';return {accepted:true,state:touch()};},
   suspend(){s.overlay=true;s.gestureTarget=-1;s.candidate=-1;s.owner=null;return touch();},
   resume(){s.overlay=false;if(s.phase!=='confirmed')s.phase=s.theme>=0?'context':'neutral';s.gestureTarget=-1;s.candidate=-1;s.owner=null;return touch();},
   invalidate(){s.gestureTarget=-1;s.candidate=-1;s.owner=null;if(s.phase!=='confirmed')s.phase=s.theme>=0?'context':'neutral';return touch();},
   gallery(keepTheme=false){s={phase:keepTheme&&s.theme>=0?'context':'neutral',candidate:-1,target:-1,theme:keepTheme?s.theme:-1,owner:null,gestureTarget:-1,overlay:false,committed:-1,revision:s.revision+1};return snapshot();},
   neutral(){if(s.overlay||s.phase==='confirmed')return snapshot();s.phase='neutral';s.target=-1;s.candidate=-1;s.theme=-1;s.owner=null;s.gestureTarget=-1;return touch();}
  };
 }
 return {palettes,bounds,pose,create};
});
