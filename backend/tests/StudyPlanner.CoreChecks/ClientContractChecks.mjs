import assert from 'node:assert/strict'
import fs from 'node:fs'
import path from 'node:path'
import { createRequire } from 'node:module'
import { pathToFileURL } from 'node:url'

// Danh tính kiểm thử riêng được cấp qua đầu vào chuẩn, không truyền thông tin xác thực bằng tham số dòng lệnh.
let input = ''
for await (const chunk of process.stdin) input += chunk
const { baseAddress, auth, frontendRoot } = JSON.parse(input)
assert(fs.existsSync(path.join(frontendRoot, 'src/hoc-tap/api.ts')), 'Select the real frontend repository with STUDYPLANNER_TEST_FRONTEND')
const require = createRequire(path.join(frontendRoot, 'package.json'))
const { createServer } = await import(pathToFileURL(require.resolve('vite')).href)
const vite = await createServer({ root: frontendRoot, define: {
  'import.meta.env.VITE_USE_MOCK_API': '"false"',
  'import.meta.env.VITE_API_BASE_URL': JSON.stringify(baseAddress + '/api/v1'),
}, server: { middlewareMode: true, hmr: false, ws: false }, appType: 'custom' })
let passed = false
const nativeFetch = globalThis.fetch
const calls = []
try {
  globalThis.fetch = (url, options) => {
    const target = new URL(url, baseAddress)
    calls.push({ path: target.pathname, method: options?.method ?? 'GET' })
    return nativeFetch(new URL(target.pathname + target.search, baseAddress), options)
  }
  const { goi, thayDoi } = await vite.ssrLoadModule('/src/hoc-tap/api.ts')
  const token = auth.tokenTruyCap
  const read = path => goi(token, path)
  const write = async (path, method, body) => {
    const { phienBan } = await read('/tasks')
    return goi(token, path, { method, body, version: phienBan })
  }
  const root = await write('/tasks', 'POST', { tieuDe: 'HTTP client: đồ án', loai: 'Personal', mucDoUuTien: 'Medium',
    checklist: [{ tieuDe: 'Đã làm', daXong: true }, { tieuDe: 'Chưa làm', daXong: false }] })
  assert.equal(root.duLieu.tienDo, 50)
  const id = root.duLieu.ma
  const body = { danhSach: [{ tieuDe: 'Slide mới', loai: 'Personal', mucDoUuTien: 'Medium' }] }
  const preview = { path: `/tasks/${id}/change-preview`, body: { thaoTac: 'children', noiDung: body } }
  let questions = 0
  const no = await thayDoi(token, `/tasks/${id}/children`, 'POST', body, root.phienBan, preview, async () => { questions++; return false })
  assert.equal(no, null)
  assert.equal(questions, 1)
  assert.equal(calls.filter(call => call.path.endsWith('/children')).length, 0)
  assert.equal((await read(`/tasks/${id}`)).duLieu.coNhiemVuCon, false)
  const created = await thayDoi(token, `/tasks/${id}/children`, 'POST', body, root.phienBan, preview, async () => true)
  const common = created.duLieu.find(task => task.tieuDe === 'Công việc chung')
  const slide = created.duLieu.find(task => task.tieuDe === 'Slide mới')
  assert(common && slide)
  assert.equal(common.checklist.length, 2)
  assert.equal(common.checklist[0].daXong, true)
  assert.equal((await read(`/tasks/${id}`)).duLieu.tienDo, 25)
  await assert.rejects(write(`/tasks/${id}/complete`, 'POST', {}), error => error.code === 'TASK_PROGRESS_IS_DERIVED')
  await write(`/tasks/${slide.ma}/complete`, 'POST', {})
  assert.equal((await read(`/tasks/${id}`)).duLieu.tienDo, 75)
  await write(`/tasks/${common.ma}`, 'PUT', { tieuDe: common.tieuDe, loai: common.loai, mucDoUuTien: common.mucDoUuTien,
    checklist: common.checklist.map(item => ({ ...item, daXong: true })) })
  const complete = await read(`/tasks/${id}`)
  assert.equal(complete.duLieu.trangThai, 'Completed')
  assert(complete.duLieu.ngayHoanThanh)
  const add = { danhSach: [{ tieuDe: 'Báo cáo bổ sung' }] }
  await assert.rejects(write(`/tasks/${id}/children`, 'POST', add), error => error.code === 'TASK_STRUCTURE_CONFIRMATION_REQUIRED')
  const expansion = await thayDoi(token, `/tasks/${id}/children`, 'POST', add, complete.phienBan,
    { path: `/tasks/${id}/change-preview`, body: { thaoTac: 'children', noiDung: add } }, async () => true)
  assert.equal(expansion.duLieu.length, 1)
  const reopened = await read(`/tasks/${id}`)
  assert.equal(reopened.duLieu.tienDo, 66)
  assert.equal(reopened.duLieu.ngayHoanThanh, null)
  await assert.rejects(goi(token, `/tasks/${slide.ma}/progress`, { method: 'PATCH', version: complete.phienBan, body: { tienDo: 50 } }), error => error.code === 'DATA_VERSION_CONFLICT')
  console.log('PASS: real frontend task client -> HTTP API -> PostgreSQL; cancel/confirm, preserved checklist, rollup, reopen and stale If-Match.')
  passed = true
} finally {
  globalThis.fetch = nativeFetch
  await Promise.race([vite.close(), new Promise(resolve => setTimeout(resolve, 3000))])
  if (passed) process.exit(0)
}
