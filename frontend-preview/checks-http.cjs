// Client/API contract checks with real HTTP, a temporary backend fixture, and a minimal DOM stub.
// Credentials arrive on stdin from CoreChecks; never log them or persist them to disk.
const fs=require('node:fs'),vm=require('node:vm'),assert=require('node:assert/strict');
const {webcrypto}=require('node:crypto');
let input='';process.stdin.setEncoding('utf8');process.stdin.on('data',s=>input+=s);
process.stdin.on('end',async()=>{
  try{
    const setup=JSON.parse(input),base=new URL(setup.baseAddress);
    assert.ok(['127.0.0.1','localhost'].includes(base.hostname),'Only the temporary local test host is allowed');
    const elements=new Map(),storage=new Map();
    const element=()=>({innerHTML:'',style:{},hidden:true,open:false,addEventListener(){},removeEventListener(){},setAttribute(){},close(){},showModal(){},querySelector(){return element();}});
    const context=vm.createContext({console,Intl,Date,Map,Set,URL,URLSearchParams,FormData,Response,crypto:webcrypto,
      setTimeout(){return 0;},clearTimeout(){},navigator:{clipboard:{writeText:async()=>{}}},location:{hash:'',origin:base.origin},
      document:{getElementById(id){if(!elements.has(id))elements.set(id,element());return elements.get(id);},addEventListener(){},createElement:element,body:{append(){}}},
      window:{addEventListener(){}},sessionStorage:{getItem:k=>storage.get(k)||null,setItem:(k,v)=>storage.set(k,v),removeItem:k=>storage.delete(k)},
      fetch:(path,options)=>fetch(new URL(path,base),options)});
    vm.runInContext(fs.readFileSync(__dirname+'/app.js','utf8')+'\nglobalThis.client={state,api,render};',context);
    const c=context.client;c.state.auth=setup.auth;c.state.view='all';let count=0;
    const check=(value,message)=>{assert.ok(value,message);count++;};
    const page=await c.api('/api/v1/tasks?cheDo=all&kichThuocTrang=20');
    check(Array.isArray(page.danhSach)&&Number.isInteger(page.tongSo),'Client consumes server paging envelope');
    check(Number.isInteger(c.state.revision),'Client receives real revision/ETag');
    const created=await c.api('/api/v1/tasks',{method:'POST',body:{tieuDe:'Kiểm tra client <API thật>',loai:'Personal',mucDoUuTien:'Medium'}});
    check(created.ma>0&&created.tienDo===0,'Client writes root task with If-Match');
    const children=await c.api(`/api/v1/tasks/${created.ma}/children`,{method:'POST',body:{danhSach:[{tieuDe:'Bước client',thoiLuongDuKienPhut:30}]}});
    check(Array.isArray(children)&&children.length===1,'Client reads typed batch-created response');
    await c.api(`/api/v1/tasks/${children[0].ma}/progress`,{method:'PATCH',body:{tienDo:25}});
    const parent=await c.api(`/api/v1/tasks/${created.ma}`);
    check(parent.tienDo===25&&parent.coNhiemVuCon,'Server progress propagation is visible to client');
    const tree=await c.api(`/api/v1/tasks/${created.ma}/tree`);
    check(tree.length===2&&tree.some(x=>x.maNhiemVuCha===created.ma),'Client receives real flat subtree');
    c.state.tasks=[parent];c.state.nodes.set(parent.ma,parent);c.state.total=1;c.render();
    check(elements.get('app').innerHTML.includes('Kiểm tra client &lt;API thật&gt;'),'Real returned title is escaped when rendered');
    const history=await c.api(`/api/v1/task-history?maNhiemVu=${created.ma}`);
    check(history.danhSach.some(x=>x.hanhDong==='Created'),'Client history matches persisted server snapshot');
    const impact=await c.api(`/api/v1/tasks/${created.ma}/deletion-preview`);
    check(impact.soNhiemVu===2,'Client reads actual deletion preview');
    await c.api(`/api/v1/tasks/${created.ma}`,{method:'DELETE'});
    let gone=false;try{await c.api(`/api/v1/tasks/${created.ma}`);}catch(e){gone=e.status===404;}
    check(gone,'Client handles real 204 deletion and 404 after deletion');
    const retained=await c.api(`/api/v1/task-history?maNhiemVu=${created.ma}`);
    check(retained.danhSach.some(x=>x.hanhDong==='Deleted'),'History survives real delete');
    console.log(`PASS ${count} real HTTP client/API checks; no browser rendering claimed.`);
  }catch(e){console.error(e.message);process.exitCode=1;}
});
