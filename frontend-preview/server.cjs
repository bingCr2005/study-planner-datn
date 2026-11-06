// Loopback-only dev server. API target is configured by the operator, never by browser input.
const http = require('node:http');
const https = require('node:https');
const fs = require('node:fs');
const path = require('node:path');
const target = new URL(process.env.STUDYPLANNER_API_URL || 'http://127.0.0.1:5059');
if (!['http:', 'https:'].includes(target.protocol) || target.username || target.password || !['127.0.0.1', 'localhost', '[::1]'].includes(target.hostname)) {
  throw new Error('STUDYPLANNER_API_URL phải là HTTP(S) loopback, không chứa thông tin đăng nhập.');
}
const routes = new Map([
  ['/', ['index.html', 'text/html; charset=utf-8']],
  ['/index.html', ['index.html', 'text/html; charset=utf-8']],
  ['/styles.css', ['styles.css', 'text/css; charset=utf-8']],
  ['/app.js', ['app.js', 'text/javascript; charset=utf-8']]
]);
const server = http.createServer((req, res) => {
  const url = new URL(req.url, 'http://localhost');
  res.setHeader('X-Content-Type-Options', 'nosniff');
  if (url.pathname.startsWith('/api/') || url.pathname === '/health/live') {
    const headers = {...req.headers, host: target.host};
    delete headers.connection;
    const proxy = (target.protocol === 'https:' ? https : http).request(new URL(url.pathname + url.search, target), {
      method: req.method, headers, timeout: 30000
    }, upstream => {
      const responseHeaders = {...upstream.headers, 'cache-control': 'no-store'};
      delete responseHeaders.connection;
      res.writeHead(upstream.statusCode, responseHeaders);
      upstream.pipe(res);
    });
    proxy.on('timeout', () => proxy.destroy());
    proxy.on('error', () => {
      if (res.headersSent) return res.destroy();
      res.writeHead(502, {'Content-Type': 'application/problem+json; charset=utf-8'});
      res.end(JSON.stringify({status:502,code:'API_UNAVAILABLE',title:'Chưa kết nối được backend',detail:'Khởi động API ở cổng đã cấu hình rồi thử lại. Yêu cầu ghi có thể đã tới server; không gửi lại apply bằng khóa mới.'}));
    });
    req.on('aborted', () => proxy.destroy());
    req.pipe(proxy);
    return;
  }
  const route = routes.get(url.pathname);
  if (!route || !['GET', 'HEAD'].includes(req.method)) {
    res.writeHead(404, {'Content-Type':'text/plain; charset=utf-8'});
    return res.end('Không tìm thấy tài nguyên.');
  }
  fs.readFile(path.join(__dirname, route[0]), (error, body) => {
    if (error) { res.writeHead(500); return res.end('Không thể đọc tài nguyên.'); }
    res.writeHead(200, {'Content-Type':route[1], 'Cache-Control':'no-store', 'Referrer-Policy':'same-origin',
      'Content-Security-Policy':"default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; connect-src 'self'; base-uri 'none'; frame-ancestors 'none'; form-action 'self'"});
    res.end(req.method === 'HEAD' ? undefined : body);
  });
});
server.on('error', error => { process.stderr.write(`${error.message}\n`); process.exitCode = 1; });
const port = Number(process.env.STUDYPLANNER_UI_PORT || 4173);
server.listen(port, '127.0.0.1', () => process.stdout.write(`Local: http://127.0.0.1:${port}\n`));
