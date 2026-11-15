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
using StudyPlanner.Api.Controllers;
using StudyPlanner.Api.HocTap;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;
using StudyPlanner.Infrastructure.Data;

namespace StudyPlanner.CoreChecks;

public static class FocusHttpChecks
{
    public static async Task RunAsync(string supplied)
    {
        var config = new NpgsqlConnectionStringBuilder(supplied);
        if (config.Host is not ("localhost" or "127.0.0.1") || config.Database != "postgres")
            throw new InvalidOperationException("Focus checks require an isolated localhost postgres admin database.");
        var name = "studyplanner_focus_checks_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(config.ConnectionString); await admin.OpenAsync();
        await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", admin)) await create.ExecuteNonQueryAsync();
        config.Database = name;
        WebApplication? app = null;
        var count = 0;
        void Check(bool passed, string text) { if (!passed) throw new Exception("FAIL focus HTTP: " + text); count++; }
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(config.ConnectionString).Options;
        var clock = new KiemThuClock(new DateTimeOffset(2026, 11, 12, 9, 0, 0, TimeSpan.Zero));
        try
        {
            NguoiDung owner, stranger; NhiemVu personal, academic; PhienTuHoc planned, legacy;
            await using (var db = new ApplicationDbContext(options))
            {
                await db.Database.MigrateAsync();
                NguoiDung User(string email) => new() { Email = email, EmailChuanHoa = email.ToUpperInvariant(), HoTen = email,
                    MatKhauBam = "unused-fixture", DangHoatDong = true, MuiGio = "Asia/Ho_Chi_Minh", NgayTao = clock.GetUtcNow(), NgayCapNhat = clock.GetUtcNow() };
                owner = User("focus@example.test"); stranger = User("other-focus@example.test"); db.AddRange(owner, stranger); await db.SaveChangesAsync();
                var semester = new HocKy { MaNguoiDung = owner.Ma, Ten = "Học kỳ tập trung", NgayBatDau = new(2026, 11, 1), NgayKetThuc = new(2026, 12, 31) };
                db.Add(semester); await db.SaveChangesAsync();
                var course = new MonHoc { MaHocKy = semester.Ma, Ten = "Cơ sở dữ liệu" }; db.Add(course); await db.SaveChangesAsync();
                personal = new() { MaNguoiDung = owner.Ma, TieuDe = "Đọc sách cá nhân", TienDo = 25, TrangThai = TrangThaiNhiemVu.InProgress, ThoiLuongDuKienPhut = 100 };
                academic = new() { MaNguoiDung = owner.Ma, MaHocKy = semester.Ma, MaMonHoc = course.Ma, TieuDe = "Ôn CSDL", ThoiLuongDuKienPhut = 100 };
                db.AddRange(personal, academic); await db.SaveChangesAsync();
                PhienTuHoc Planned() => new() { MaNguoiDung = owner.Ma, MaNhiemVu = academic.Ma, TrangThai = TrangThaiPhienTuHoc.Planned,
                    BatDauDuKien = clock.GetUtcNow().AddHours(1), KetThucDuKien = clock.GetUtcNow().AddHours(2) };
                planned = Planned(); legacy = Planned(); db.AddRange(planned, legacy); await db.SaveChangesAsync();
            }
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
            builder.Logging.ClearProviders(); builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Configuration["Auth:SigningKey"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseNpgsql(config.ConnectionString));
            builder.Services.AddQuanTri(); builder.Services.AddHocTap(); builder.Services.AddStudyPlannerXacThuc(builder.Configuration, builder.Environment);
            builder.Services.AddControllers().AddApplicationPart(typeof(TapTrungController).Assembly);
            builder.Services.AddSingleton<TimeProvider>(clock);
            app = builder.Build(); app.UseExceptionHandler(); app.UseAuthentication(); app.UseAuthorization(); app.UseRateLimiter(); app.MapControllers(); await app.StartAsync();
            var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            using var client = new HttpClient { BaseAddress = new(address) };
            using var other = new HttpClient { BaseAddress = new(address) };
            using var anonymous = new HttpClient { BaseAddress = new(address) };
            var issuer = app.Services.GetRequiredService<JwtTokenIssuer>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", issuer.Issue(owner).TokenTruyCap);
            other.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", issuer.Issue(stranger).TokenTruyCap);
            long version = 0;
            async Task<JsonElement> Send(HttpClient who, string path, object? body = null, int expected = 200, long? explicitVersion = null)
            {
                using var request = new HttpRequestMessage(body is null ? HttpMethod.Get : HttpMethod.Post, "/api/v1/" + path);
                if (body is not null) { request.Content = JsonContent.Create(body); request.Headers.TryAddWithoutValidation("If-Match", $"\"ht-{explicitVersion ?? version}\""); }
                using var response = await who.SendAsync(request);
                var text = await response.Content.ReadAsStringAsync();
                Check((int)response.StatusCode == expected, path + ": expected " + expected + ", got " + (int)response.StatusCode + "; " + text);
                if (!response.IsSuccessStatusCode) return default;
                var json = JsonDocument.Parse(text).RootElement.Clone();
                if (who == client) version = json.GetProperty("phienBan").GetInt64();
                Check(response.Headers.ETag is not null, "ETag on " + path);
                return json.GetProperty("duLieu");
            }
            object Start(int? task = null, int? session = null, string mode = "Pomodoro", int target = 60) => new { maNhiemVu = task, maPhienTuHoc = session, cheDo = mode, mucTieuGiay = target };
            object Note(string? text = null) => new { ghiChu = text };
            await Send(anonymous, "focus/state", expected: 401);
            var initial = await Send(client, "focus/state"); Check(initial.GetProperty("hienTai").ValueKind == JsonValueKind.Null, "empty state");
            var first = await Send(client, "focus/start", Start(personal.Ma)); var id = first.GetProperty("ma").GetInt32();
            Check(first.GetProperty("maNhiemVu").GetInt32() == personal.Ma, "personal task is supported");
            var duplicate = await Send(client, "focus/start", Start(personal.Ma)); Check(duplicate.GetProperty("ma").GetInt32() == id, "retry start returns the same timer");
            await Send(client, "focus/start", Start(academic.Ma), 409);
            clock.Advance(TimeSpan.FromSeconds(20));
            var paused = await Send(client, $"focus/{id}/pause", Note("Ghi chú khi học"));
            Check(paused.GetProperty("soMiliGiayDaHoc").GetInt64() == 20000, "pause stores actual segment");
            var pausedVersion = version; await Send(client, $"focus/{id}/pause", Note("Ghi chú khi học")); Check(version == pausedVersion, "pause retry is idempotent");
            await Send(client, $"study-sessions/{planned.Ma}/start", new { }, 409);
            await Send(other, $"focus/{id}/resume", Note(), 404, 0);
            clock.Advance(TimeSpan.FromMinutes(5));
            var pausedState = await Send(client, "focus/state"); Check(pausedState.GetProperty("hienTai").GetProperty("soMiliGiayDaHoc").GetInt64() == 20000, "reload while paused excludes break");
            await Send(client, $"focus/{id}/resume", Note("Ghi chú khi học")); clock.Advance(TimeSpan.FromSeconds(100));
            var expired = await Send(client, "focus/state");
            Check(expired.GetProperty("hienTai").ValueKind == JsonValueKind.Null, "elapsed countdown finalizes on reopening");
            var saved = expired.GetProperty("lichSu")[0];
            Check(saved.GetProperty("soMiliGiayDaHoc").GetInt64() == 60000 && saved.GetProperty("ghiChu").GetString() == "Ghi chú khi học", "countdown caps duration and keeps notes");
            Check(expired.GetProperty("tongQuan").GetProperty("pomoHomNay").GetInt32() == 1, "only full Pomodoro counts");
            await using (var db = new ApplicationDbContext(options))
            {
                Check(await db.DanhSachKhoangThoiGianHoc.CountAsync(d => d.MaPhienTapTrung == id) == 2, "two real segments persist");
                Check(!await db.DanhSachKhoangThoiGianHoc.AnyAsync(d => d.KetThucThucTe == null), "countdown closes its real stopwatch");
                Check((await db.DanhSachNhiemVu.FindAsync(personal.Ma))!.TienDo == 25, "clock never increases task progress");
            }
            var free = await Send(client, "focus/start", Start(mode: "Stopwatch")); var freeId = free.GetProperty("ma").GetInt32();
            clock.Advance(TimeSpan.FromSeconds(15));
            var done = await Send(client, $"focus/{freeId}/finish", Note("Tập trung tự do"));
            Check(done.GetProperty("maNhiemVu").ValueKind == JsonValueKind.Null && done.GetProperty("soMiliGiayDaHoc").GetInt64() == 15000, "free stopwatch saves short sessions");
            await Send(client, $"focus/{freeId}/finish", Note("Tập trung tự do"));
            var discard = await Send(client, "focus/start", Start(academic.Ma)); var discardId = discard.GetProperty("ma").GetInt32();
            clock.Advance(TimeSpan.FromSeconds(10)); await Send(client, $"focus/{discardId}/discard", Note());
            var afterDiscard = await Send(client, "focus/state"); Check(afterDiscard.GetProperty("tongSo").GetInt32() == 2, "discard is excluded from history");
            await using (var db = new ApplicationDbContext(options)) Check(!await db.DanhSachKhoangThoiGianHoc.AnyAsync(d => d.MaPhienTapTrung == discardId), "discard removes only its own measured segments");
            var manualStart = clock.GetUtcNow().AddDays(-1); var manualEnd = manualStart.AddMinutes(25);
            var manualBody = new { maNhiemVu = academic.Ma, cheDo = "Pomodoro", mucTieuGiay = 1500, batDau = manualStart, ketThuc = manualEnd, ghiChu = "Buổi hôm qua" };
            var manual = await Send(client, "focus/records", manualBody);
            Check(manual.GetProperty("nhapThuCong").GetBoolean(), "manual record retains its source");
            await Send(client, "focus/records", manualBody, 409);
            await Send(client, "focus/records", new { batDau = clock.GetUtcNow(), ketThuc = clock.GetUtcNow().AddMinutes(1) }, 400);
            await Send(client, "focus/start", Start(target: 30), 400);
            await Send(client, "focus/start", Start(), 409, 0);
            await Send(client, $"focus/{freeId}/note", Note(new string('x', 2001)), 400);
            var linked = await Send(client, "focus/start", Start(session: planned.Ma, mode: "Stopwatch")); var linkedId = linked.GetProperty("ma").GetInt32();
            Check(linked.GetProperty("maNhiemVu").GetInt32() == academic.Ma, "planned session uses its academic task");
            clock.Advance(TimeSpan.FromSeconds(12)); await Send(client, $"focus/{linkedId}/pause", Note());
            clock.Advance(TimeSpan.FromSeconds(40)); await Send(client, $"focus/{linkedId}/resume", Note());
            clock.Advance(TimeSpan.FromSeconds(8)); await Send(client, $"study-sessions/{planned.Ma}/finish", new { });
            var linkedState = await Send(client, "focus/state");
            Check(linkedState.GetProperty("hienTai").ValueKind == JsonValueKind.Null, "legacy finish also closes the focus group");
            Check(linkedState.GetProperty("lichSu").EnumerateArray().Single(p => p.GetProperty("ma").GetInt32() == linkedId).GetProperty("soMiliGiayDaHoc").GetInt64() == 20000, "linked pause time is excluded");
            await Send(client, $"study-sessions/{legacy.Ma}/start", new { }); clock.Advance(TimeSpan.FromSeconds(10));
            var legacyState = await Send(client, "focus/state");
            Check(legacyState.GetProperty("maPhienDangHocCu").GetInt32() == legacy.Ma, "state discovers a stopwatch started before the focus page");
            var adopted = await Send(client, "focus/start", Start(session: legacy.Ma, mode: "Stopwatch")); var adoptedId = adopted.GetProperty("ma").GetInt32();
            await Send(client, $"focus/{adoptedId}/finish", Note());
            var last = await Send(client, "focus/state");
            Check(last.GetProperty("lichSu").EnumerateArray().Single(p => p.GetProperty("ma").GetInt32() == adoptedId).GetProperty("soMiliGiayDaHoc").GetInt64() == 10000, "existing running session is adopted without losing time");
            Check(last.GetProperty("tongQuan").GetProperty("tongPomo").GetInt32() == 2 && last.GetProperty("tongQuan").GetProperty("pomoHomNay").GetInt32() == 1, "summary uses the account day and completed full Pomodoros");
            await Send(client, "focus/state?trang=0", expected: 400);
            var foreignState = await Send(other, "focus/state"); Check(foreignState.GetProperty("tongSo").GetInt32() == 0, "history is scoped to owner");
            var raceVersion = version;
            async Task<int> Race()
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/focus/start") { Content = JsonContent.Create(Start()) };
                req.Headers.TryAddWithoutValidation("If-Match", $"\"ht-{raceVersion}\"");
                using var response = await client.SendAsync(req); return (int)response.StatusCode;
            }
            var race = await Task.WhenAll(Race(), Race());
            Check(race.Count(s => s == 200) == 1 && race.Count(s => s == 409) == 1, "two tabs cannot create two active timers");
            var racingState = await Send(client, "focus/state");
            await Send(client, $"focus/{racingState.GetProperty("hienTai").GetProperty("ma").GetInt32()}/discard", Note());
            await using (var db = new ApplicationDbContext(options))
            {
                db.Remove((await db.DanhSachNhiemVu.FindAsync(personal.Ma))!); await db.SaveChangesAsync();
            }
            var preserved = await Send(client, "focus/state");
            Check(preserved.GetProperty("lichSu").EnumerateArray().Any(p => p.GetProperty("tieuDe").GetString() == "Đọc sách cá nhân" && p.GetProperty("maNhiemVu").ValueKind == JsonValueKind.Null), "history snapshot survives task deletion");
            await Send(client, "focus/records", new { cheDo = "Stopwatch", mucTieuGiay = 1500,
                batDau = "2026-11-11T17:30:00Z", ketThuc = "2026-11-11T17:55:00Z", ghiChu = "Sau nửa đêm giờ tài khoản" });
            var statsVersion = version;
            await Send(anonymous, "focus/statistics", expected: 401);
            var statsDay = await Send(client, "focus/statistics?soNgay=1&ngay=2026-11-12");
            Check(statsDay.GetProperty("tongHop").GetProperty("soPhien").GetInt32() == 5 &&
                statsDay.GetProperty("tongHop").GetProperty("tongMiliGiay").GetInt64() == 1605000, "day statistics use account midnight rather than UTC midnight");
            var statsWeek = await Send(client, "focus/statistics?soNgay=7&ngay=2026-11-12");
            Check(statsWeek.GetProperty("theoNgay").GetArrayLength() == 7 && statsWeek.GetProperty("tuNgay").GetString() == "2026-11-06", "week statistics fill every day");
            Check(statsWeek.GetProperty("tongHop").GetProperty("soPhien").GetInt32() == 6 &&
                statsWeek.GetProperty("tongHop").GetProperty("tongMiliGiay").GetInt64() == 3105000, "range totals include manual and timed records, exclude discarded groups");
            var academicGroup = statsWeek.GetProperty("theoNhiemVu").EnumerateArray().Single(p => p.GetProperty("maNhiemVu").ValueKind == JsonValueKind.Number && p.GetProperty("maNhiemVu").GetInt32() == academic.Ma);
            Check(academicGroup.GetProperty("soPhien").GetInt32() == 3 && academicGroup.GetProperty("tongMiliGiay").GetInt64() == 1530000, "distribution combines all sessions of a task");
            var statsMonth = await Send(client, "focus/statistics?soNgay=30&ngay=2026-11-12");
            Check(statsMonth.GetProperty("theoNgay").GetArrayLength() == 30, "month range contains 30 daily entries");
            var taskSummary = await Send(client, $"focus/tasks/{academic.Ma}/summary");
            Check(taskSummary.GetProperty("soPhien").GetInt32() == 3 && taskSummary.GetProperty("soPomodoro").GetInt32() == 1 &&
                taskSummary.GetProperty("tongMiliGiay").GetInt64() == 1530000, "task preview summary is computed across its complete history");
            await Send(other, $"focus/tasks/{academic.Ma}/summary", expected: 404);
            await Send(client, $"focus/tasks/{personal.Ma}/summary", expected: 404);
            await Send(client, "focus/statistics?soNgay=2", expected: 400);
            await Send(client, "focus/statistics?soNgay=1&ngay=2026-11-13", expected: 400);
            var emptyStats = await Send(client, "focus/statistics?soNgay=1&ngay=2026-10-01");
            Check(emptyStats.GetProperty("tongHop").GetProperty("soPhien").GetInt32() == 0 && emptyStats.GetProperty("theoNgay").GetArrayLength() == 1, "empty range is real zero data with its day retained");
            var privateStats = await Send(other, "focus/statistics?soNgay=30");
            Check(privateStats.GetProperty("tongHop").GetProperty("soPhien").GetInt32() == 0, "statistics never include another owner's records");
            Check(version == statsVersion, "statistics and task previews never change task or timer revisions");
            Console.WriteLine($"PASS: {count} focus HTTP/PostgreSQL assertions; migration, ownership, stopwatch, countdown, pause/resume, notes, manual records, discard and legacy session integration.");
        }
        finally
        {
            if (app is not null) { await app.StopAsync(); await app.DisposeAsync(); }
            NpgsqlConnection.ClearAllPools();
            await using var drop = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{name}\" WITH (FORCE)", admin); await drop.ExecuteNonQueryAsync();
        }
    }
}
