import assert from 'node:assert/strict'
import fs from 'node:fs'
import path from 'node:path'
import { randomUUID } from 'node:crypto'

export const output = path.resolve(process.env.STUDENT_V2_OUTPUT || 'backend/obj/student-v2-live')
export const host = JSON.parse(fs.readFileSync(path.join(output, 'host.json'), 'utf8'))
export const results = []
export const fixtures = []
const health = await (await fetch(host.address + '/__test/health')).json()
export const t0 = new Date(health.now)
const localDay = new Intl.DateTimeFormat('en-CA', { timeZone: 'Asia/Ho_Chi_Minh', year: 'numeric', month: '2-digit', day: '2-digit' })
export const day = (offset = 2) => {
  const parts = Object.fromEntries(localDay.formatToParts(t0).map(p => [p.type, p.value]))
  const date = new Date(Date.UTC(Number(parts.year), Number(parts.month) - 1, Number(parts.day) + offset))
  return date.toISOString().slice(0, 10)
}
export const at = (date = day(), time = '19:00') => `${date}T${time}:00+07:00`
export const mins = p => (Date.parse(p.ketThucDuKien) - Date.parse(p.batDauDuKien)) / 60000
export const sum = rows => rows.reduce((n, p) => n + mins(p), 0)
export const delay = ms => new Promise(resolve => setTimeout(resolve, ms))
export async function test(id, fn, level = 'HTTP + PostgreSQL isolated') {
  const started = Date.now()
  try { await fn(); results.push({ id, status: 'PASS', level, ms: Date.now() - started }); console.log(`PASS ${id}`) }
  catch (e) { results.push({ id, status: 'FAIL', level, error: e.message, ms: Date.now() - started }); console.log(`FAIL ${id}: ${e.message}`) }
  fs.writeFileSync(path.join(output, 'results.json'), JSON.stringify({ t0, D: day(), database: host.database, fixtures, results }, null, 2))
}
export async function fixture(label, academic = true) {
  const response = await fetch(host.address + '/__test/fixture/' + encodeURIComponent(label), { method: 'POST' })
  assert.equal(response.status, 200, 'test account provisioning')
  const auth = await response.json()
  const c = { id: auth.nguoiDung.ma, email: auth.nguoiDung.email, token: auth.tokenTruyCap, expiresAt: Date.parse(auth.hetHanLuc), revision: 0, label }
  c.send = async (method, route, body, expected = 200, extra = {}) => {
    const headers = { Authorization: 'Bearer ' + c.token, ...(method !== 'GET' ? { 'If-Match': `"ht-${c.revision}"` } : {}), ...extra }
    if (body !== undefined) headers['Content-Type'] = 'application/json'
    const r = await fetch(host.address + '/api/v1/' + route, { method, headers, body: body === undefined ? undefined : JSON.stringify(body) })
    const text = await r.text(); let data
    try { data = text ? JSON.parse(text) : null } catch { throw Error(`${method} ${route}: invalid JSON, HTTP ${r.status}`) }
    assert.equal(r.status, expected, `${method} ${route}: ${text.slice(0, 500)}`)
    if (typeof data?.phienBan === 'number') c.revision = data.phienBan
    const tag = r.headers.get('etag')?.match(/ht-(\d+)/)
    if (tag) c.revision = Number(tag[1])
    return data?.duLieu ?? data
  }
  c.state = async () => (await fetch(host.address + '/__test/state/' + c.id)).json()
  if (academic) {
    const s = await c.send('POST', 'semesters', { ten: '[LPTEST-V2] ' + label, ngayBatDau: day(-30), ngayKetThuc: day(150) }, 201)
    c.semester = s.ma
    const m = await c.send('POST', `semesters/${s.ma}/courses`, { ten: '[LPTEST-V2] Web', kyHieu: 'QA-WEB', soTinChi: 3, mauSac: '#2563EB' }, 201)
    c.course = m.ma
  }
  fixtures.push({ label, owner: c.id, semester: c.semester, course: c.course, taskIds: [], availabilityIds: [] })
  return c
}
export const taskBody = (c, estimate = 25, extra = {}) => ({ tieuDe: '[LPTEST-V2] ' + c.label, maHocKy: c.semester ?? null, maMonHoc: c.course ?? null,
  loai: 'Assignment', mucDoUuTien: 'Medium', hanHoanThanh: at(day(7), '23:00'), thoiLuongDuKienPhut: estimate, ...extra })
export async function task(c, estimate = 25, extra = {}) {
  const row = await c.send('POST', 'tasks', taskBody(c, estimate, extra), 201)
  fixtures.find(f => f.owner === c.id).taskIds.push(row.ma)
  return row
}
export async function availability(c, date = day(), from = '19:00', to = '21:00') {
  const weekday = new Date(date + 'T12:00:00Z').getUTCDay() || 7
  const row = await c.send('POST', 'availability-windows', { thuTrongTuan: weekday, gioBatDau: from + ':00', gioKetThuc: to + ':00', ngayBatDau: date, ngayKetThuc: date, dangSuDung: true }, 201)
  fixtures.find(f => f.owner === c.id).availabilityIds.push(row.ma)
  return row
}
export const preview = (c, ids, extra = {}) => c.send('POST', 'planning/preview', { ngayBatDau: day(), soNgay: 7, maHocKy: c.semester ?? null, maMonHoc: c.course ?? null, danhSachMaNhiemVu: ids, capNhatKeHoach: false, ...extra })
export const selection = p => p.phien.map(({ maNhiemVu, batDauDuKien, ketThucDuKien }) => ({ maNhiemVu, batDauDuKien, ketThucDuKien }))
export const apply = (c, p, rows = selection(p), key = randomUUID(), status = 200) => c.send('POST', 'planning/apply', { tokenPreview: p.tokenPreview, phien: rows }, status, { 'Idempotency-Key': key })
export async function apiChecks() {
  await test('LP-02/API-01/LP-A/LP-05 backend', async () => {
    const c = await fixture('30min'); const n = await task(c, 30); await availability(c)
    const before = await c.state(); const p = await preview(c, [n.ma])
    assert.deepEqual(p.phien.map(mins), [25, 5]); assert.equal(sum(p.phien), 30)
    assert.equal(Date.parse(p.phien[1].batDauDuKien) - Date.parse(p.phien[0].ketThucDuKien), 300000)
    assert.deepEqual(await c.state(), before, 'preview is read-only')
    const invalid = await apply(c, p, selection(p).slice(1), randomUUID(), 400)
    assert.equal(invalid.code, 'SHORT_SESSION_REQUIRES_REMAINDER'); assert.deepEqual(await c.state(), before)
    await apply(c, p); assert.equal((await c.state()).sessions.length, 2)
  })
  await test('LP-03/LP-B', async () => {
    const c = await fixture('50min'); const n = await task(c, 50); await availability(c, day(), '19:00', '19:55')
    const p = await preview(c, [n.ma]); assert.deepEqual(p.phien.map(mins), [25, 25]); assert.equal(p.tongPhutChuaXep, 0)
  })
  for (const [label, estimate, progress, window, expected] of [['LP-06/AV-SHORT-OK',10,0,'19:10',10], ['LP-E',25,80,'19:55',5], ['LP-06B/LP-E2/AV-SHORT',25,0,'19:10',0], ['LP-C',60,0,'19:25',25]]) {
    await test(label, async () => {
      const c = await fixture(label); const n = await task(c, estimate)
      if (progress) await c.send('PATCH', `tasks/${n.ma}/progress`, { tienDo: progress })
      await availability(c, day(), '19:00', window); const p = await preview(c, [n.ma])
      assert.equal(sum(p.phien), expected); if (expected && expected < 15) await apply(c, p)
      if (!expected) assert.equal(p.phien.length, 0)
    })
  }
  await test('LP-08/LP-F', async () => {
    const c = await fixture('warnings'); const missing = await task(c, null); const overdue = await task(c,25,{ hanHoanThanh: new Date(t0.getTime()-3600000).toISOString() }); const valid = await task(c)
    await availability(c,day(),'19:00','19:25'); const p=await preview(c,[missing.ma,overdue.ma,valid.ma])
    assert.deepEqual(p.phien.map(p=>p.maNhiemVu),[valid.ma]); assert(p.canhBao.some(w=>w.maLyDo==='MISSING_ESTIMATE'))
  })
  await test('LP-09/LP-D', async () => {
    const c=await fixture('progress-reserved'); const n=await task(c,120); await c.send('PATCH',`tasks/${n.ma}/progress`,{tienDo:50}); await availability(c)
    let p=await preview(c,[n.ma]); assert.equal(sum(p.phien),60)
    await apply(c,p,selection(p).slice(0,1)); p=await preview(c,[n.ma]); assert.equal(p.tongPhutDaDat,25); assert.equal(p.tongPhutCanXep,35); assert.equal(sum(p.phien),35)
  })
  await test('LP-10/STAT-1 tree', async()=>{
    const c=await fixture('tree'); const p=await task(c,null); const children=await c.send('POST',`tasks/${p.ma}/children`,{danhSach:[taskBody(c,25,{hanHoanThanh:null,tieuDe:'[LPTEST-V2] child25'}),taskBody(c,50,{hanHoanThanh:null,tieuDe:'[LPTEST-V2] child50'})]},201)
    await availability(c); const x=await preview(c,[p.ma,...children.map(n=>n.ma)]); assert.equal(sum(x.phien),75); assert(x.phien.every(s=>s.maNhiemVu!==p.ma));
    await c.send('POST',`tasks/${children[0].ma}/complete`,{}); const root=await c.send('GET',`tasks/${p.ma}`); assert.equal(root.tienDo,50)
    await c.send('PATCH',`tasks/${p.ma}/progress`,{tienDo:30},409)
  })
  await test('LP-11/STAT-1 checklist',async()=>{
    const c=await fixture('checklist'); const n=await task(c,100,{checklist:[{tieuDe:'a',daXong:true},{tieuDe:'b',daXong:false},{tieuDe:'c',daXong:false},{tieuDe:'d',daXong:false}]}); assert.equal(n.tienDo,25)
    await availability(c); assert.equal(sum((await preview(c,[n.ma])).phien),75); await c.send('PATCH',`tasks/${n.ma}/progress`,{tienDo:50},409)
  })
  for(const kind of ['class','event']) await test(kind==='class'?'LP-12':'LP-13',async()=>{
    const c=await fixture(kind);const n=await task(c);await availability(c,day(),'19:00','20:30')
    if(kind==='class') await c.send('POST',`courses/${c.course}/class-sessions`,{thuTrongTuan:new Date(day()+'T12:00Z').getUTCDay()||7,gioBatDau:'19:00:00',gioKetThuc:'20:00:00',ngayApDungTu:day(),ngayApDungDen:day()},201)
    else await task(c,null,{tieuDe:'[LPTEST-V2] Busy event',maHocKy:null,maMonHoc:null,loai:'Personal',batDauSuKien:at(),ketThucSuKien:at(day(),'20:00')})
    const p=await preview(c,[n.ma]);assert.equal(Date.parse(p.phien[0].batDauDuKien),Date.parse(at(day(),'20:00')))
  })
  await test('LP-14/NV-4',async()=>{
    const c=await fixture('exception'); const n=await task(c); const av=await availability(c)
    await c.send('PUT',`availability-windows/${av.ma}/exceptions/${day()}`,{boQua:true});assert.equal((await preview(c,[n.ma])).phien.length,0)
    await c.send('DELETE',`availability-windows/${av.ma}/exceptions/${day()}`); assert.equal(sum((await preview(c,[n.ma])).phien),25)
  })
  await test('LP-15',async()=>{
    const c=await fixture('overlap'); const n=await task(c,100);await availability(c,day(),'19:00','20:00');await availability(c,day(),'19:30','21:00')
    const p=await preview(c,[n.ma]);assert.equal(sum(p.phien),100);const rows=p.phien;for(let i=1;i<rows.length;i++)assert(Date.parse(rows[i].batDauDuKien)>=Date.parse(rows[i-1].ketThucDuKien)+300000)
  })
  await test('LP-16/LP-20/LP-J stale',async()=>{
    const c=await fixture('stale');const n=await task(c,50);await availability(c);const p=await preview(c,[n.ma]);const before=await c.state()
    await c.send('PUT','study-settings',{phienHocPhut:50,nghiGiuaPhienPhut:10,gioiHanHocMoiNgayPhut:180})
    assert.equal((await apply(c,p,undefined,randomUUID(),409)).code,'DATA_VERSION_CONFLICT');assert.deepEqual(await c.state(),before)
  })
  await test('LP-18/API-02/API-04/LP-I',async()=>{
    const c=await fixture('replace');const n=await task(c,50);await availability(c);await apply(c,await preview(c,[n.ma]));const old=(await c.state()).sessions
    const p=await preview(c,[n.ma],{capNhatKeHoach:true});assert.deepEqual(p.maPhienThayThe,[...old.map(s=>s.ma)].sort((a,b)=>a-b));assert.equal(p.phienThayThe.length,2)
    await apply(c,p,selection(p).slice(0,1));const state=await c.state();assert.equal(state.sessions.filter(s=>s.trangThai==='Cancelled').length,2);assert.equal(state.sessions.filter(s=>s.trangThai==='Planned').length,1)
  })
  await test('LP-21/LP-22/LP-L/API-05 concurrent retry',async()=>{
    const c=await fixture('retry');const n=await task(c);await availability(c);const p=await preview(c,[n.ma]);const key=randomUUID()
    const replies=await Promise.all([apply(c,p,undefined,key),apply(c,p,undefined,key)]);assert.equal(replies.filter(r=>r.daTonTai).length,1)
    const before=await c.state();assert.equal(before.sessions.length,1);assert.equal(before.batches,1)
    assert.equal((await apply(c,p,[],key,400)).code,'VALIDATION_ERROR')
    const modified=selection(p).map(s=>({...s,ketThucDuKien:new Date(Date.parse(s.ketThucDuKien)-60000).toISOString()}));await apply(c,p,modified,key,400);assert.deepEqual(await c.state(),before)
  })
  await test('LP-23',async()=>{
    const c=await fixture('reserved25');const n=await task(c,50);await availability(c);const p=await preview(c,[n.ma]);await apply(c,p,selection(p).slice(0,1));const next=await preview(c,[n.ma]);assert.equal(next.tongPhutDaDat,25);assert.equal(next.tongPhutCanXep,25);assert.equal(sum(next.phien),25)
  })
  await test('LP-24 no availability',async()=>{
    const c=await fixture('empty-free');const n=await task(c);const p=await preview(c,[n.ma]);assert.equal(p.phien.length,0);assert(p.canhBao.length>0)
  })
  await test('LP-26/LP-28/F-1/F-3',async()=>{
    const c=await fixture('focus');const n=await task(c);await availability(c);const batch=await apply(c,await preview(c,[n.ma]));const planned=batch.phien[0]
    const f=await c.send('POST','focus/start',{maPhienTuHoc:planned.ma,cheDo:'Stopwatch',mucTieuGiay:1500});await delay(300)
    const paused=await c.send('POST',`focus/${f.ma}/pause`,{});await delay(400);const still=await c.send('GET','focus/state');assert.equal(still.hienTai.soMiliGiayDaHoc,paused.soMiliGiayDaHoc)
    const other=await task(c);await c.send('POST','focus/start',{maNhiemVu:other.ma,cheDo:'Stopwatch'},409)
    await c.send('POST',`focus/${f.ma}/resume`,{});await delay(300);const done=await c.send('POST',`focus/${f.ma}/finish`,{});assert(done.soMiliGiayDaHoc>0);assert.equal((await c.send('GET',`tasks/${n.ma}`)).tienDo,0)
    await c.send('PATCH',`tasks/${n.ma}/progress`,{tienDo:50});assert.equal((await c.send('GET',`tasks/${n.ma}`)).tienDo,50);assert.equal((await c.state()).sessions[0].trangThai,'Completed')
  })
  await test('LP-29/NV-1/NV-2',async()=>{
    const c=await fixture('complete-cancel');const n=await task(c,50);await availability(c);await apply(c,await preview(c,[n.ma]));const before=await c.state()
    await c.send('POST',`tasks/${n.ma}/complete`,{},409);assert.deepEqual(await c.state(),before)
    await c.send('POST',`tasks/${n.ma}/complete`,{huyPhienLienQuan:true});const after=await c.state();assert.equal(after.tasks[0].tienDo,100);assert.equal(after.sessions.filter(s=>s.trangThai==='Cancelled').length,2)
  })
  await test('LP-30/F-4/F-4B/F-5',async()=>{
    const c=await fixture('manual-time');const n=await task(c);const end=new Date(t0.getTime()-3600000);const start=new Date(end.getTime()-1800000)
    const body={maNhiemVu:n.ma,cheDo:'Stopwatch',mucTieuGiay:1500,batDau:start.toISOString(),ketThuc:end.toISOString()}
    const f=await c.send('POST','focus/records',body);const corrected=await c.send('PUT',`focus/${f.ma}/time`,{batDau:start.toISOString(),ketThuc:new Date(end.getTime()-60000).toISOString()});assert.equal(corrected.banGhi.soMiliGiayDaHoc,1740000);assert.equal(corrected.lichSu.length,1)
    const before=await c.state();await c.send('POST','focus/records',{...body,batDau:new Date(end.getTime()-25*3600000).toISOString()},400)
    await c.send('PUT',`focus/${f.ma}/time`,{batDau:start.toISOString(),ketThuc:new Date(Date.now()+3600000).toISOString()},400)
    await c.send('PUT',`focus/${f.ma}/time`,{batDau:end.toISOString(),ketThuc:start.toISOString()},400)
    await c.send('POST','focus/records',body,409);assert.deepEqual(await c.state(),before);assert.equal(before.tasks[0].tienDo,0)
  })
  await test('API-06/API-07/AC-1',async()=>{
    const a=await fixture('owner');const b=await fixture('stranger');const n=await task(a);const av=await availability(a);await apply(a,await preview(a,[n.ma]));const before=await a.state()
    for(const route of [`tasks/${n.ma}`,`courses/${a.course}`,`availability-windows/${av.ma}`,`study-sessions/${before.sessions[0].ma}`])await b.send('GET',route,undefined,404)
    await b.send('POST','planning/preview',{ngayBatDau:day(),soNgay:1,danhSachMaNhiemVu:[n.ma]},404)
    await a.send('GET','admin/priority-rules',undefined,403);assert.equal((await fetch(host.address+'/api/v1/tasks')).status,401);assert.deepEqual(await a.state(),before)
  })
  for(const type of ['course','semester']) await test(type==='course'?'HK-1':'HK-2',async()=>{
    const c=await fixture('delete-'+type);const n=await task(c);const route=type==='course'?`courses/${c.course}`:`semesters/${c.semester}`
    await c.send('GET',route+'/deletion-preview');await c.send('DELETE',route,undefined,204);const kept=await c.send('GET',`tasks/${n.ma}`);assert.equal(kept.maMonHoc,null);assert.equal(kept.maHocKy,type==='course'?c.semester:null);assert((await c.state()).history>1)
  })
  await test('NOTIF-1',async()=>{
    const c=await fixture('notification');const n=await task(c,25,{hanHoanThanh:new Date(Date.now()+1800000).toISOString(),nhacTruocGio:1});const inbox=await c.send('GET','notifications');assert(inbox.danhSach.some(row=>row.maNhiemVu===n.ma))
  })
  await test('INPUT-1',async()=>{
    const c=await fixture('invalid');const before=await c.state();for(const change of [{tieuDe:''},{tieuDe:'x'.repeat(201)},{thoiLuongDuKienPhut:0},{thoiLuongDuKienPhut:-1}])await c.send('POST','tasks',taskBody(c,25,change),400);assert.deepEqual(await c.state(),before)
  })
}
