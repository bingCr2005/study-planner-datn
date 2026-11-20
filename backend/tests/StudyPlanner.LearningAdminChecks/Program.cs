using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using StudyPlanner.Api.Administration;
using StudyPlanner.Api.Auth;
using StudyPlanner.Api.HocTap;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;
using StudyPlanner.Infrastructure.Data;

var supplied = Environment.GetEnvironmentVariable("STUDYPLANNER_TEST_POSTGRES");
if (string.IsNullOrWhiteSpace(supplied)) throw new Exception("An isolated localhost test PostgreSQL is required.");
var config = new NpgsqlConnectionStringBuilder(supplied);
if (config.Host is not ("localhost" or "127.0.0.1") || config.Database != "postgres") throw new Exception("Only localhost postgres test admin database is allowed.");
var name = "studyplanner_learning_" + Guid.NewGuid().ToString("N");
await using var adminConnection = new NpgsqlConnection(config.ConnectionString); await adminConnection.OpenAsync();
await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", adminConnection)) await create.ExecuteNonQueryAsync();
config.Database = name;
var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(config.ConnectionString).Options;
var clock = new TestClock(); var now = clock.GetUtcNow(); var count = 0;
void Check(bool pass, string label) { if (!pass) throw new Exception("FAIL: " + label); count++; }
try
{
    NguoiDung admin, student, locked; NhiemVu root, child, personal; int semesterId;
    await using (var db = new ApplicationDbContext(options))
    {
        await db.Database.MigrateAsync();
        NguoiDung User(string email, VaiTroNguoiDung role = VaiTroNguoiDung.Student, bool active = true) => new() {
            Email = email, EmailChuanHoa = email.ToUpperInvariant(), HoTen = email, VaiTro = role, DangHoatDong = active,
            MatKhauBam = "private-password-fixture", NgayTao = now, NgayCapNhat = now };
        admin = User("admin@learning.test", VaiTroNguoiDung.Admin); student = User("student@learning.test"); locked = User("locked@learning.test", active: false);
        db.AddRange(admin, student, locked); await db.SaveChangesAsync();
        HocKy Semester(int owner) => new() { MaNguoiDung = owner, Ten = "Học kỳ kiểm thử", NgayBatDau = new(2026, 9, 1), NgayKetThuc = new(2027, 1, 1) };
        var semester = Semester(student.Ma); var lockedSemester = Semester(locked.Ma); var adminSemester = Semester(admin.Ma);
        db.AddRange(semester, lockedSemester, adminSemester); await db.SaveChangesAsync(); semesterId = semester.Ma;
        NhiemVu TaskItem(int owner, int? term, string title) => new() { MaNguoiDung = owner, MaHocKy = term, TieuDe = title,
            Loai = term.HasValue ? LoaiNhiemVu.Assignment : LoaiNhiemVu.Personal,
            MoTa = "private-description-fixture", ChecklistJson = "[{\"tieuDe\":\"private-checklist-fixture\",\"daXong\":false}]", NgayTao = now, NgayCapNhat = now };
        root = TaskItem(student.Ma, semester.Ma, "Đồ án học tập"); root.HanHoanThanh = now.AddDays(-1);
        personal = TaskItem(student.Ma, null, "private-task-fixture");
        var scopedPersonal = TaskItem(student.Ma, semester.Ma, "private-scoped-personal-fixture"); scopedPersonal.Loai = LoaiNhiemVu.Personal;
        var completed = TaskItem(student.Ma, semester.Ma, "Đã xong"); completed.TrangThai = TrangThaiNhiemVu.Completed; completed.TienDo = 100; completed.NgayHoanThanh = now.AddDays(-2);
        var trashed = TaskItem(student.Ma, semester.Ma, "private-trashed-fixture"); trashed.NgayXoa = now;
        db.AddRange(root, personal, scopedPersonal, completed, trashed, TaskItem(locked.Ma, lockedSemester.Ma, "Việc sinh viên bị khóa"), TaskItem(admin.Ma, adminSemester.Ma, "private-admin-task-fixture"));
        for (var i = 0; i < 22; i++) db.Add(TaskItem(student.Ma, semester.Ma, "Bài học " + i));
        await db.SaveChangesAsync();
        child = TaskItem(student.Ma, semester.Ma, "Bước học tập"); child.MaNhiemVuCha = root.Ma; db.Add(child); await db.SaveChangesAsync();
        PhienTuHoc Session(NhiemVu task, string? note = null) => new() { MaNguoiDung = task.MaNguoiDung, MaNhiemVu = task.Ma,
            BatDauDuKien = now.AddHours(-3), KetThucDuKien = now.AddHours(-2), TrangThai = TrangThaiPhienTuHoc.Completed, GhiChu = note };
        db.AddRange(Session(root, "private-session-note-fixture"), Session(personal), Session(trashed));
        PhienTapTrung Focus(NhiemVu? task, long ms, bool manual = false) => new() { MaNguoiDung = student.Ma, MaNhiemVu = task?.Ma,
            TieuDe = "private-focus-title-fixture", GhiChu = "private-focus-note-fixture", TrangThai = "Completed", NhapThuCong = manual,
            BatDau = now.AddHours(-4), KetThuc = now.AddHours(-3), SoMiliGiayDaHoc = ms };
        db.AddRange(Focus(root, 60000), Focus(child, 120000, true), Focus(personal, 900000), Focus(null, 900000), Focus(trashed, 900000));
        await db.SaveChangesAsync();
    }
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
    builder.Logging.ClearProviders(); builder.WebHost.UseUrls("http://127.0.0.1:0");
    builder.Configuration["Auth:SigningKey"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseNpgsql(config.ConnectionString));
    builder.Services.AddQuanTri(); builder.Services.AddHocTap(); builder.Services.AddStudyPlannerXacThuc(builder.Configuration, builder.Environment);
    builder.Services.AddSingleton<TimeProvider>(clock);
    await using var app = builder.Build(); app.UseExceptionHandler(); app.UseAuthentication(); app.UseAuthorization(); app.UseRateLimiter(); app.MapControllers(); await app.StartAsync();
    try
    {
        var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        using var client = new HttpClient { BaseAddress = new(address) }; using var studentClient = new HttpClient { BaseAddress = new(address) }; using var anonymous = new HttpClient { BaseAddress = new(address) };
        var issuer = app.Services.GetRequiredService<JwtTokenIssuer>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", issuer.Issue(admin).TokenTruyCap);
        studentClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", issuer.Issue(student).TokenTruyCap);
        const string prefix = "/api/v1/admin/learning/";
        var range = "?tu=" + Uri.EscapeDataString(now.AddDays(-7).ToString("O")) + "&den=" + Uri.EscapeDataString(now.AddDays(1).ToString("O"));
        async Task<JsonElement> Get(string path)
        {
            using var response = await client.GetAsync(prefix + path); var text = await response.Content.ReadAsStringAsync();
            Check(response.IsSuccessStatusCode, path + " response " + response.StatusCode + " " + text);
            foreach (var forbidden in new[] { "private-", "moTa", "checklist", "ghiChu", "matKhau", "tepDinhKem", "khoaLuuTru" })
                Check(!text.Contains(forbidden, StringComparison.OrdinalIgnoreCase), path + " excludes " + forbidden);
            return JsonDocument.Parse(text).RootElement.Clone();
        }
        foreach (var endpoint in new[] { "summary", "tasks", $"students/{student.Ma}", $"students/{student.Ma}/sessions" + range, $"students/{student.Ma}/focus-summary" + range })
        {
            Check((int)(await anonymous.GetAsync(prefix + endpoint)).StatusCode == 401, "anonymous blocked " + endpoint);
            Check((int)(await studentClient.GetAsync(prefix + endpoint)).StatusCode == 403, "student blocked " + endpoint);
        }
        var summary = await Get("summary");
        Check(summary.GetProperty("tongNhiemVu").GetInt32() == 25, "root count excludes personal, child, trashed and admin tasks");
        Check(summary.GetProperty("hoanThanh").GetInt32() == 1 && summary.GetProperty("quaHan").GetInt32() == 1 && summary.GetProperty("soSinhVienCoNhiemVu").GetInt32() == 2, "whole database totals include locked student");
        var tasks = await Get("tasks"); Check(tasks.GetProperty("danhSach").GetArrayLength() == 20 && tasks.GetProperty("tongSo").GetInt32() == 25, "pagination has independent total");
        var page2 = await Get("tasks?Trang=2"); Check(page2.GetProperty("danhSach").GetArrayLength() == 5, "page two has remainder");
        var allIds = tasks.GetProperty("danhSach").EnumerateArray().Concat(page2.GetProperty("danhSach").EnumerateArray()).Select(t => t.GetProperty("ma").GetInt32()).ToArray();
        Check(allIds.Distinct().Count() == 25, "stable pages do not repeat records");
        var overdue = await Get("tasks?GomBuocNho=true&ChiQuaHan=true");
        Check(overdue.GetProperty("tongSo").GetInt32() == 2 && overdue.GetProperty("danhSach").EnumerateArray().All(t => t.GetProperty("hanHieuLuc").GetDateTimeOffset() == now.AddDays(-1)), "children use inherited deadlines in filter and response");
        var completedPage = await Get("tasks?TrangThai=Completed"); Check(completedPage.GetProperty("tongSo").GetInt32() == 1, "completed filter includes completed tasks");
        var studentSummary = await Get($"summary?MaSinhVien={student.Ma}&MaHocKy={semesterId}"); Check(studentSummary.GetProperty("tongNhiemVu").GetInt32() == 24, "student semester scope");
        await Get($"students/{locked.Ma}");
        var noMatch = await Get("tasks?TuKhoa=unmatched"); Check(noMatch.GetProperty("tongSo").GetInt32() == 0, "real empty page");
        var sessions = await Get($"students/{student.Ma}/sessions" + range); Check(sessions.GetProperty("tongSo").GetInt32() == 1, "sessions omit personal/trashed task and notes");
        var focus = await Get($"students/{student.Ma}/focus-summary" + range);
        Check(focus.GetProperty("soPhien").GetInt32() == 2 && focus.GetProperty("tongMiliGiay").GetInt64() == 180000 && focus.GetProperty("bamGioMiliGiay").GetInt64() == 60000 && focus.GetProperty("nhapTayMiliGiay").GetInt64() == 120000, "focus separates measured/manual, excludes personal/free/trashed");
        foreach (var path in new[] { "tasks?Trang=0", "tasks?KichThuocTrang=101", "tasks?Trang=2147483647", "tasks?TrangThai=Invalid", "tasks?MaSinhVien=-1", $"students/{student.Ma}/sessions?tu=2026-11-20T00:00:00Z&den=2026-11-19T00:00:00Z" })
            Check((int)(await client.GetAsync(prefix + path)).StatusCode == 400, "invalid filter rejected " + path);
        foreach (var path in new[] { $"students/{admin.Ma}", "students/2147483647", $"tasks?MaSinhVien={student.Ma}&MaHocKy=2147483647" })
            Check((int)(await client.GetAsync(prefix + path)).StatusCode == 404, "out of scope rejected " + path);
        foreach (var method in new[] { HttpMethod.Post, HttpMethod.Put, HttpMethod.Delete, HttpMethod.Patch })
            Check((int)(await client.SendAsync(new HttpRequestMessage(method, prefix + "tasks") { Content = JsonContent.Create(new { tienDo = 100 }) })).StatusCode == 405, "no admin task write " + method);
        Check((int)(await client.GetAsync($"/api/v1/tasks/{root.Ma}")).StatusCode == 404, "admin cannot bypass existing owner API");
        await using var verify = new ApplicationDbContext(options);
        Check((await verify.DanhSachNhiemVu.SingleAsync(t => t.Ma == root.Ma)).TienDo == 0 && (await verify.DanhSachNguoiDung.SingleAsync(u => u.Ma == student.Ma)).PhienBanLapLich == 0, "all admin reads leave data and revision unchanged");
        Console.WriteLine($"PASS: {count} assertions for admin learning HTTP/PostgreSQL authorization, privacy, scopes, totals, inherited deadlines, pagination, read-only behavior and actual/manual time.");
    }
    finally { await app.StopAsync(); }
}
finally
{
    NpgsqlConnection.ClearAllPools();
    await using var drop = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{name}\" WITH (FORCE)", adminConnection); await drop.ExecuteNonQueryAsync();
}
sealed class TestClock : TimeProvider { public override DateTimeOffset GetUtcNow() => new(2026, 11, 18, 9, 0, 0, TimeSpan.Zero); }
