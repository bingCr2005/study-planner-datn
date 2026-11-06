// Semantic client checks without browser dependencies or production data.
const assert = require('node:assert/strict');
const fs = require('node:fs');
const vm = require('node:vm');
const {webcrypto} = require('node:crypto');
const elements = new Map();
const storage = new Map();
const element = () => ({innerHTML:'',style:{},hidden:true,open:false,addEventListener(){},removeEventListener(){},setAttribute(){},close(){this.open=false;},showModal(){this.open=true;},querySelector(){return element();}});
let fetchImpl;
const context = vm.createContext({console,Intl,Date,Map,Set,URL,URLSearchParams,FormData,Response,crypto:webcrypto,
  setTimeout(){return 0;},clearTimeout(){},navigator:{clipboard:{writeText:async()=>{}}},location:{hash:'',origin:'http://127.0.0.1:4173'},
  document:{getElementById(id){if(!elements.has(id))elements.set(id,element());return elements.get(id);},addEventListener(){},createElement:element,body:{append(){}}},
  window:{addEventListener(){}},sessionStorage:{getItem:k=>storage.get(k)||null,setItem:(k,v)=>storage.set(k,v),removeItem:k=>storage.delete(k)},
  fetch:(...args)=>fetchImpl(...args)});
vm.runInContext(fs.readFileSync(__dirname+'/app.js','utf8')+'\nglobalThis.client={state,asUtc,localValue,dateKey,addDays,weekStart,esc,api,performTask,applyPlan};',context);
const c=context.client;
let assertions=0;
function check(condition,message){assert.ok(condition,message);assertions++;}
function response(body,status=200){return new Response(status===204?null:JSON.stringify(body),{status,headers:{'Content-Type':'application/json'}});}
(async()=>{
  c.state.auth={tokenTruyCap:'test-only',nguoiDung:{ma:1,muiGio:'Asia/Ho_Chi_Minh'}};
  check(c.asUtc('2026-11-04T19:30')==='2026-11-04T12:30:00.000Z','Account timezone conversion is independent of host timezone');
  check(c.localValue('2026-11-04T12:30:00Z')==='2026-11-04T19:30','UTC roundtrip returns account local input');
  check(c.dateKey('2026-11-04T19:30:00Z')==='2026-11-05','Date grouping follows account day, not UTC day');
  check(c.weekStart('2026-11-08')==='2026-11-02','Sunday belongs to the week beginning Monday');
  check(c.addDays('2028-02-28',1)==='2028-02-29','Calendar navigation includes leap day');
  c.state.auth.nguoiDung.muiGio='America/New_York';
  assert.throws(()=>c.asUtc('2026-03-08T02:30'),/không tồn tại/);assertions++;
  assert.throws(()=>c.asUtc('2026-11-01T01:30'),/hai cách/);assertions++;
  c.state.auth.nguoiDung.muiGio='Asia/Ho_Chi_Minh';
  check(c.esc('<img src=x onerror="alert(1)">').includes('&lt;img'),'API text is escaped before rendering HTML');
  let calls=[];
  c.state.revision=7;
  fetchImpl=async(path,options)=>{calls.push({path,options});return response({phienBan:8,duLieu:{ma:12}});};
  const created=await c.api('/api/v1/tasks',{method:'POST',body:{tieuDe:'Test'}});
  check(calls[0].options.headers['If-Match']==='"ht-7"','Mutations send original revision');
  check(created.ma===12&&c.state.revision===8,'Response envelope updates revision and unwraps data');
  fetchImpl=async()=>{calls.push({});return response({status:409,code:'DATA_VERSION_CONFLICT',detail:'Dữ liệu cũ'},409);};
  const before=calls.length;
  await assert.rejects(()=>c.api('/api/v1/tasks/12',{method:'PUT',body:{}}),e=>e.status===409);assertions++;
  check(calls.length===before+1,'409 is surfaced without automatically replaying a write');
  c.state.revision=9;c.state.detailRevision=9;c.state.nodes.set(12,{ma:12,_revision:9});calls=[];
  fetchImpl=async(path,options)=>{calls.push({path,options});return response({phienBan:10,duLieu:{maPhienBiHuy:[]}});};
  await assert.rejects(()=>c.performTask('progress',{tienDo:50},{id:12}),e=>e.status===409);assertions++;
  check(calls.length===1&&calls[0].path.endsWith('change-preview'),'A newer impact preview cannot silently upgrade a stale edit revision');
  c.state.revision=10;c.state.detailRevision=10;c.state.nodes.set(12,{ma:12,_revision:10});
  fetchImpl=async path=>path.endsWith('change-preview')?response({phienBan:10,duLieu:{maPhienBiHuy:[]}}):response(null,204);
  check((await c.performTask('delete',{}, {id:12}))!==null,'204 deletion is success, not interpreted as user cancelling confirmation');
  c.state.preview={tokenPreview:'signed-test',phien:[{maNhiemVu:12,batDauDuKien:'2026-11-06T12:00:00Z',ketThucDuKien:'2026-11-06T12:50:00Z'}]};
  c.state.planSelected=new Set([0]);c.state.applyAttempt=null;calls=[];
  vm.runInContext('confirmDialog=async()=>true;',context);
  fetchImpl=async(path,options)=>{calls.push({path,options});throw new Error('Simulated connection loss after server may have committed');};
  await assert.rejects(()=>c.applyPlan(),e=>e.status===0);assertions++;
  const key=c.state.applyAttempt.key;
  check(JSON.parse(storage.get('studyflow.pendingApply.v1')).attempt.key===key,'Uncertain request is kept in sessionStorage before the network call so reload can recover the key');
  fetchImpl=async(path,options)=>{calls.push({path,options});return response({phienBan:11,duLieu:{maLanApDung:1,daTonTai:true,phien:[]}});};
  await c.applyPlan(true);
  check(calls[0].options.headers['Idempotency-Key']===key&&calls[1].options.headers['Idempotency-Key']===key,'Retry after uncertain apply reuses the same persisted-in-session request key');
  check(calls[0].options.body===calls[1].options.body,'Retry sends exactly the same selected slot payload');
  check(c.state.applyAttempt===null&&c.state.preview===null,'Confirmed apply resolves pending attempt and clears draft');
  check(!storage.has('studyflow.pendingApply.v1'),'Confirmed apply clears the persisted pending request');
  storage.set('studyflow.auth.v1','test');
  fetchImpl=async()=>response({status:401,title:'Đăng nhập lại'},401);
  await assert.rejects(()=>c.api('/api/v1/tasks'),e=>e.status===401);assertions++;
  check(c.state.auth===null&&!storage.has('studyflow.auth.v1'),'401 removes expired session instead of retaining access token');
  console.log(`PASS ${assertions} frontend semantic checks. No browser, database or external account was used.`);
})().catch(error=>{console.error(error);process.exitCode=1;});
