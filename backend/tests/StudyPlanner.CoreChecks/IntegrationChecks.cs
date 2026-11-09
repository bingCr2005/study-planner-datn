using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using StudyPlanner.Api.Administration;
using StudyPlanner.Api.Auth;
using StudyPlanner.Api.HocTap;
using StudyPlanner.Application.HocTap;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;
using StudyPlanner.Infrastructure.Data;
using StudyPlanner.Infrastructure.HocTap;

namespace StudyPlanner.CoreChecks;

public static class IntegrationChecks
{
    public static async Task RunAsync(string supplied)
    {
        var config = new NpgsqlConnectionStringBuilder(supplied);
        if (config.Host is not ("localhost" or "127.0.0.1") || config.Database != "postgres")
            throw new InvalidOperationException("CoreChecks only accepts an isolated localhost PostgreSQL admin database named postgres.");
        var name = "studyplanner_core_checks_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(config.ConnectionString); await admin.OpenAsync();
        await using (var command = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", admin)) await command.ExecuteNonQueryAsync();
        config.Database = name;
        WebApplication? app = null;
        Exception? serverError = null;
        var count = 0;
        void Check(bool condition, string label) { if (!condition) throw new Exception("FAIL: " + label); count++; }
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(config.ConnectionString).Options;
        var clock = new KiemThuClock(new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero));
        try
        {
            int owner, stranger, course, otherCourse;
            await using (var db = new ApplicationDbContext(options))
            {
                await db.Database.MigrateAsync();
                NguoiDung User(string email) => new()
                {
                    Email = email, EmailChuanHoa = email.ToUpperInvariant(), HoTen = email, DangHoatDong = true,
                    MuiGio = "Asia/Ho_Chi_Minh", NgayTao = clock.GetUtcNow(), NgayCapNhat = clock.GetUtcNow()
                };
                var a = User("core-a@example.test"); var b = User("core-b@example.test"); b.VaiTro=VaiTroNguoiDung.Admin;
                var hasher = new PasswordHasher<NguoiDung>();
                a.MatKhauBam = hasher.HashPassword(a, Guid.NewGuid().ToString("N"));
                b.MatKhauBam = hasher.HashPassword(b, Guid.NewGuid().ToString("N"));
                db.AddRange(a, b); await db.SaveChangesAsync(); owner = a.Ma; stranger = b.Ma;
                var semester = new HocKy { MaNguoiDung = owner, Ten = "Học kỳ kiểm thử", NgayBatDau = new(2026, 11, 1), NgayKetThuc = new(2026, 12, 31), NgayTao = clock.GetUtcNow(), NgayCapNhat = clock.GetUtcNow() };
                var otherSemester = new HocKy { MaNguoiDung = stranger, Ten = "Học kỳ riêng", NgayBatDau = new(2026, 11, 1), NgayKetThuc = new(2026, 12, 31), NgayTao = clock.GetUtcNow(), NgayCapNhat = clock.GetUtcNow() };
                db.AddRange(semester, otherSemester); await db.SaveChangesAsync();
                var mon = new MonHoc { MaHocKy = semester.Ma, Ten = "CSDL", MauSac = "#3B82F6", NgayTao = clock.GetUtcNow(), NgayCapNhat = clock.GetUtcNow() };
                var monB = new MonHoc { MaHocKy = otherSemester.Ma, Ten = "Riêng B", NgayTao = clock.GetUtcNow(), NgayCapNhat = clock.GetUtcNow() };
                db.AddRange(mon, monB); await db.SaveChangesAsync(); course = mon.Ma; otherCourse = monB.Ma;
            }

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
            builder.Logging.ClearProviders(); builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Configuration["Auth:SigningKey"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseNpgsql(config.ConnectionString));
            builder.Services.AddQuanTri(); builder.Services.AddHocTap();
            builder.Services.AddStudyPlannerXacThuc(builder.Configuration, builder.Environment);
            builder.Services.AddSingleton<TimeProvider>(clock);
            builder.Services.AddHttpContextAccessor();
            // Chỉ host kiểm thử có điểm chèn lỗi; code API sản phẩm không đọc header này.
            builder.Services.AddScoped<IHocTapStore>(sp => new LoiSauLuuStore(new EfHocTapStore(sp.GetRequiredService<ApplicationDbContext>()), sp.GetRequiredService<IHttpContextAccessor>()));
            builder.Services.AddEndpointsApiExplorer(); builder.Services.AddSwaggerGen(o => { o.OperationFilter<MoTaSwagger>(); o.SchemaFilter<MoTaSwagger>(); });
            app = builder.Build(); app.UseExceptionHandler();
            app.Use(async (_, next) => { try { await next(); } catch (Exception error) { serverError = error; throw; } });
            app.UseSwagger(); app.UseAuthentication(); app.UseAuthorization(); app.UseRateLimiter(); app.MapControllers();
            await app.StartAsync();
            var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            using var aClient = new HttpClient { BaseAddress = new(address) };
            using var bClient = new HttpClient { BaseAddress = new(address) };
            using var anonymous = new HttpClient { BaseAddress = new(address) };
            await using (var db = new ApplicationDbContext(options))
            {
                var issuer = app.Services.GetRequiredService<JwtTokenIssuer>();
                aClient.DefaultRequestHeaders.Authorization = new("Bearer", issuer.Issue(await db.DanhSachNguoiDung.SingleAsync(x => x.Ma == owner)).TokenTruyCap);
                bClient.DefaultRequestHeaders.Authorization = new("Bearer", issuer.Issue(await db.DanhSachNguoiDung.SingleAsync(x => x.Ma == stranger)).TokenTruyCap);
            }

            async Task<JsonElement> Send(HttpClient client, HttpMethod method, string path, object? body, int expected,
                long? revision = null, string? key = null, int? failSave = null)
            {
                using var req = new HttpRequestMessage(method, path);
                if (body is not null) req.Content = JsonContent.Create(body);
                if (revision.HasValue) req.Headers.TryAddWithoutValidation("If-Match", $"\"ht-{revision}\"");
                if (key is not null) req.Headers.TryAddWithoutValidation("Idempotency-Key", key);
                if (failSave.HasValue) req.Headers.Add("X-CoreChecks-Fail-Save", failSave.Value.ToString());
                using var response = await client.SendAsync(req);
                var raw = await response.Content.ReadAsStringAsync();
                Check((int)response.StatusCode == expected, $"{method} {path}: expected {expected}, received {(int)response.StatusCode}; {raw}; server error: {serverError}");
                if (expected >= 400) Check(response.Content.Headers.ContentType?.MediaType == "application/problem+json", "Errors use ProblemDetails");
                if (expected is 200 or 201 && path.Contains("/api/v1/")) Check(response.Headers.ETag is not null, "Read/write response exposes ETag");
                return raw.Length == 0 ? default : JsonDocument.Parse(raw).RootElement.Clone();
            }
            async Task<long> Revision(HttpClient client) => (await Send(client, HttpMethod.Get, "/api/v1/availability-windows", null, 200)).GetProperty("phienBan").GetInt64();
            async Task<JsonElement> Write(HttpClient client, HttpMethod method, string path, object? body, int expected, int? fail = null) =>
                await Send(client, method, path, body, expected, await Revision(client), failSave: fail);
            static JsonElement Data(JsonElement e) => e.GetProperty("duLieu");
            static int Id(JsonElement e) => Data(e).GetProperty("ma").GetInt32();
            object Free(bool enabled = true, bool cancel = false) => new { thuTrongTuan = 3, gioBatDau = "19:00:00", gioKetThuc = "22:00:00", ngayBatDau = "2026-11-04", ngayKetThuc = "2026-12-30", dangSuDung = enabled, huyPhienLienQuan = cancel };
            object Class(string start = "07:00:00", string end = "09:00:00", bool overlap = false, bool cancel = false) => new { thuTrongTuan = 3, gioBatDau = start, gioKetThuc = end, phongHoc = "A101", xacNhanTrungLich = overlap, huyPhienLienQuan = cancel };
            object TaskBody(string title, int? minutes = 200, string? due = "2026-11-06T23:59:00+07:00") =>
                new { tieuDe = title, maMonHoc = course, thoiLuongDuKienPhut = minutes, hanHoanThanh = due, mucDoUuTien = "High" };

            await Send(anonymous, HttpMethod.Get, "/api/v1/availability-windows", null, 401);
            await Send(aClient,HttpMethod.Get,"/api/v1/calendar?tu=2026-11-04T00:00:00&den=2026-11-08T00:00:00Z",null,400);
            await Write(aClient,HttpMethod.Post,"/api/v1/tasks",new{tieuDe="Thiếu offset",hanHoanThanh="2026-11-06T23:59:00"},400);
            await Send(aClient, HttpMethod.Post, "/api/v1/availability-windows", Free(), 428);
            await Write(aClient, HttpMethod.Post, "/api/v1/availability-windows", new { thuTrongTuan = 3, gioKetThuc = "22:00:00", ngayBatDau = "2026-11-04", ngayKetThuc = "2026-12-30" }, 400);
            var freeId = Id(await Write(aClient, HttpMethod.Post, "/api/v1/availability-windows", Free(), 201));
            await Send(bClient, HttpMethod.Get, $"/api/v1/availability-windows/{freeId}", null, 404);
            var revisionBeforeNoop = await Revision(aClient);
            var noop = await Write(aClient, HttpMethod.Put, $"/api/v1/availability-windows/{freeId}", Free(), 200);
            Check(noop.GetProperty("phienBan").GetInt64() == revisionBeforeNoop, "No-op availability update preserves revision");
            await Write(aClient, HttpMethod.Post, $"/api/v1/courses/{otherCourse}/class-sessions", Class(), 404);
            var classId = Id(await Write(aClient, HttpMethod.Post, $"/api/v1/courses/{course}/class-sessions", Class(), 201));
            var overlapError = await Write(aClient, HttpMethod.Post, $"/api/v1/courses/{course}/class-sessions", Class("08:00:00", "10:00:00"), 409);
            Check(overlapError.GetProperty("code").GetString() == "CLASS_OVERLAP_CONFIRMATION_REQUIRED", "Class overlap needs explicit confirmation");
            await Write(aClient, HttpMethod.Post, $"/api/v1/courses/{course}/class-sessions", Class("08:00:00", "10:00:00", true), 201);

            var taskId = Id(await Write(aClient, HttpMethod.Post, "/api/v1/tasks", TaskBody("Bài làm riêng"), 201));
            var accentedSearch = Data(await Send(aClient, HttpMethod.Get,
                "/api/v1/tasks?tuKhoa=" + Uri.EscapeDataString("Bài làm riêng"), null, 200));
            Check(accentedSearch.GetProperty("danhSach").EnumerateArray().Any(x => x.GetProperty("ma").GetInt32() == taskId),
                "Vietnamese literal search matches stored text independently of Unicode case conversion in DB collation");
            const string personalTitle = "Việc cá nhân giữa các học kỳ";
            var personalId = Id(await Write(aClient, HttpMethod.Post, "/api/v1/tasks",
                new { tieuDe = personalTitle, loai = "Personal" }, 201));
            var personalSearch = "/api/v1/tasks?tuKhoa=" + Uri.EscapeDataString(personalTitle);
            var allScopes = Data(await Send(aClient, HttpMethod.Get, personalSearch, null, 200));
            Check(allScopes.GetProperty("danhSach").EnumerateArray().Any(x => x.GetProperty("ma").GetInt32() == personalId),
                "Default task listing includes personal work when academic data exists");
            var courseScope = Data(await Send(aClient, HttpMethod.Get, personalSearch + "&maMonHoc=" + course, null, 200));
            Check(courseScope.GetProperty("tongSo").GetInt32() == 0, "Only explicit course filtering excludes personal work");
            await Write(aClient, HttpMethod.Delete, $"/api/v1/tasks/{personalId}", null, 204);
            await Write(aClient, HttpMethod.Patch, $"/api/v1/tasks/{taskId}/progress", new { tienDo = 25 }, 200);
            await Send(bClient, HttpMethod.Get, $"/api/v1/tasks/{taskId}", null, 404);
            var parentId = Id(await Write(aClient, HttpMethod.Post, "/api/v1/tasks", TaskBody("Báo cáo chia nhỏ", null), 201));
            await Write(aClient, HttpMethod.Post, $"/api/v1/tasks/{parentId}/children", new { danhSach = new[] { TaskBody("ERD", 60, null), TaskBody("SQL", 60, null) } }, 201);
            int child1, child2;
            await using (var db = new ApplicationDbContext(options))
            {
                var children = await db.DanhSachNhiemVu.Where(x => x.MaNhiemVuCha == parentId).OrderBy(x => x.Ma).ToListAsync();
                Check(children.Count == 2, "Batch children are persisted"); child1 = children[0].Ma; child2 = children[1].Ma;
            }
            var childRead = Data(await Send(aClient, HttpMethod.Get, $"/api/v1/tasks/{child1}", null, 200));
            Check(childRead.GetProperty("hanHieuLuc").GetDateTimeOffset() == DateTimeOffset.Parse("2026-11-06T23:59:00+07:00"), "Child inherits effective deadline");
            await Write(aClient, HttpMethod.Patch, $"/api/v1/tasks/{child1}/progress", new { tienDo = 60 }, 200);
            Check(Data(await Send(aClient, HttpMethod.Get, $"/api/v1/tasks/{parentId}", null, 200)).GetProperty("tienDo").GetInt32() == 30, "Parent averages direct child progress");
            await Write(aClient, HttpMethod.Patch, $"/api/v1/tasks/{parentId}/progress", new { tienDo = 20 }, 409);
            await Write(aClient, HttpMethod.Patch, $"/api/v1/tasks/{parentId}/parent", new { maNhiemVuCha = child1 }, 409);
            var stale = await Revision(aClient);
            await Write(aClient, HttpMethod.Patch, $"/api/v1/tasks/{child2}/progress", new { tienDo = 10 }, 200);
            await Send(aClient, HttpMethod.Patch, $"/api/v1/tasks/{child2}/progress", new { tienDo = 20 }, 409, stale);
            var calendar = Data(await Send(aClient, HttpMethod.Get, "/api/v1/calendar?tu=2026-11-04T00:00:00%2B07:00&den=2026-11-08T00:00:00%2B07:00", null, 200));
            Check(calendar.GetProperty("suKien").EnumerateArray().Any(e => e.GetProperty("loai").GetString() == "HanNhiemVu" && e.GetProperty("maNguon").GetInt32() == child1), "Calendar renders inherited leaf deadline through SQL projection");
            Check(!calendar.GetProperty("suKien").EnumerateArray().Any(e => e.GetProperty("loai").GetString() == "HanNhiemVu" && e.GetProperty("maNguon").GetInt32() == parentId), "Calendar hides parent deadlines by default");

            int tasksBefore, historyBefore; long versionBefore;
            await using (var db = new ApplicationDbContext(options)) { tasksBefore = await db.DanhSachNhiemVu.CountAsync(); historyBefore = await db.DanhSachLichSuNhiemVu.CountAsync(); versionBefore = await db.DanhSachNguoiDung.Where(x => x.Ma == owner).Select(x => x.PhienBanLapLich).SingleAsync(); }
            await Write(aClient, HttpMethod.Post, "/api/v1/tasks", TaskBody("Phải rollback"), 500, 1);
            await using (var db = new ApplicationDbContext(options))
            {
                Check(await db.DanhSachNhiemVu.CountAsync() == tasksBefore && await db.DanhSachLichSuNhiemVu.CountAsync() == historyBefore, "Task/history rollback after a real SaveChanges followed by injected failure");
                Check(await db.DanhSachNguoiDung.Where(x => x.Ma == owner).Select(x => x.PhienBanLapLich).SingleAsync() == versionBefore, "Rollback preserves revision");
            }

            object PreviewBody() => new { ngayBatDau = "2026-11-04", soNgay = 3, danhSachMaNhiemVu = new[] { taskId } };
            async Task<JsonElement> Preview() => Data(await Send(aClient, HttpMethod.Post, "/api/v1/planning/preview", PreviewBody(), 200));
            int batchesBefore, sessionsBefore;
            await using (var db = new ApplicationDbContext(options)) { batchesBefore = await db.DanhSachLanApDungLich.CountAsync(); sessionsBefore = await db.DanhSachPhienTuHoc.CountAsync(); }
            var preview = await Preview();
            Check(preview.GetProperty("tongPhutCanXep").GetInt32() == 150, "Scheduler uses estimate times remaining progress");
            Check(preview.GetProperty("phien").GetArrayLength() == 3, "150 minutes split into three 50-minute sessions with breaks");
            await using (var db = new ApplicationDbContext(options)) Check(await db.DanhSachLanApDungLich.CountAsync() == batchesBefore && await db.DanhSachPhienTuHoc.CountAsync() == sessionsBefore, "Preview performs no persisted writes");
            object ApplyBody(JsonElement p, int index = 0) => new { tokenPreview = p.GetProperty("tokenPreview").GetString(), phien = new[] { p.GetProperty("phien")[index] } };
            var applyBody = ApplyBody(preview);
            await Send(aClient, HttpMethod.Post, "/api/v1/planning/apply", applyBody, 500, key: "rollback-apply", failSave: 2);
            await using (var db = new ApplicationDbContext(options)) Check(await db.DanhSachLanApDungLich.CountAsync() == batchesBefore && await db.DanhSachPhienTuHoc.CountAsync() == sessionsBefore, "Batch/sessions rollback after second persisted save fails");
            var applied = Data(await Send(aClient, HttpMethod.Post, "/api/v1/planning/apply", applyBody, 200, key: "apply-once"));
            var firstSession = applied.GetProperty("phien")[0].GetProperty("ma").GetInt32();
            var retry = Data(await Send(aClient, HttpMethod.Post, "/api/v1/planning/apply", applyBody, 200, key: "apply-once"));
            Check(retry.GetProperty("daTonTai").GetBoolean() && retry.GetProperty("maLanApDung").GetInt32() == applied.GetProperty("maLanApDung").GetInt32(), "Idempotent retry returns same durable batch");
            await Send(aClient, HttpMethod.Post, "/api/v1/planning/apply", ApplyBody(preview, 1), 409, key: "apply-once");
            await Send(aClient, HttpMethod.Post, "/api/v1/planning/apply", applyBody, 409, key: "old-version");
            var afterReservation = await Preview();
            Check(afterReservation.GetProperty("tongPhutCanXep").GetInt32() == 100 && afterReservation.GetProperty("tongPhutDaDat").GetInt32() == 50, "Existing future reservation is subtracted once");
            await Send(bClient, HttpMethod.Post, "/api/v1/planning/preview", PreviewBody(), 404);
            await Send(aClient, HttpMethod.Post, "/api/v1/planning/apply", new { tokenPreview = "tampered", phien = new[] { preview.GetProperty("phien")[0] } }, 400, key: "tampered");
            var outsideTask = Id(await Write(aClient, HttpMethod.Post, "/api/v1/tasks", TaskBody("Đã đặt ngoài horizon", 100, null), 201));
            object Manual(string from, string to) => new { maNhiemVu = outsideTask, batDauDuKien = from, ketThucDuKien = to };
            await Write(aClient, HttpMethod.Post, "/api/v1/study-sessions", Manual("2026-11-11T19:00:00+07:00", "2026-11-11T19:10:00+07:00"), 400);
            var outsideSession = Id(await Write(aClient, HttpMethod.Post, "/api/v1/study-sessions", Manual("2026-11-11T19:00:00+07:00", "2026-11-11T19:50:00+07:00"), 201));
            await Write(aClient, HttpMethod.Post, "/api/v1/study-sessions", Manual("2026-11-11T19:55:00+07:00", "2026-11-11T20:45:00+07:00"), 409);
            var outsidePreview = Data(await Send(aClient, HttpMethod.Post, "/api/v1/planning/preview", new { ngayBatDau = "2026-11-04", soNgay = 3, danhSachMaNhiemVu = new[] { outsideTask } }, 200));
            Check(outsidePreview.GetProperty("tongPhutCanXep").GetInt32() == 50 && outsidePreview.GetProperty("tongPhutDaDat").GetInt32() == 50, "Reservations beyond preview horizon still reduce remaining demand");
            await Write(aClient, HttpMethod.Post, $"/api/v1/study-sessions/{outsideSession}/cancel", null, 200);

            var freeImpact = Data(await Send(aClient, HttpMethod.Post, "/api/v1/calendar/change-preview", new { loai = "GioRanh", ma = freeId, khungGioRanh = Free(false) }, 200));
            Check(freeImpact.GetProperty("phienBiAnhHuong").EnumerateArray().Any(x => x.GetInt32() == firstSession), "Availability preview identifies future session impact");
            await Write(aClient, HttpMethod.Put, $"/api/v1/availability-windows/{freeId}", Free(false), 409);
            // A second identical window keeps the session covered when the first is disabled.
            var duplicateFree = Id(await Write(aClient, HttpMethod.Post, "/api/v1/availability-windows", Free(), 201));
            await Write(aClient, HttpMethod.Put, $"/api/v1/availability-windows/{freeId}", Free(false), 200);
            await Write(aClient, HttpMethod.Delete, $"/api/v1/availability-windows/{duplicateFree}", null, 409);
            await Write(aClient, HttpMethod.Delete, $"/api/v1/availability-windows/{duplicateFree}?huyPhienLienQuan=true", null, 204);
            Check(Data(await Send(aClient, HttpMethod.Get, $"/api/v1/study-sessions/{firstSession}", null, 200)).GetProperty("trangThai").GetString() == "Cancelled", "Explicit availability removal cancels only affected Planned sessions");
            await Write(aClient, HttpMethod.Put, $"/api/v1/availability-windows/{freeId}", Free(), 200);
            clock.Advance(TimeSpan.FromMinutes(11));
            await Send(aClient, HttpMethod.Post, "/api/v1/planning/apply", applyBody, 200, key: "apply-once");
            Check(Data(await Send(aClient, HttpMethod.Get, $"/api/v1/study-sessions/{firstSession}", null, 200)).GetProperty("trangThai").GetString() == "Cancelled", "Retry after preview expiry does not recreate cancelled session");
            var expiring = await Preview(); clock.Advance(TimeSpan.FromMinutes(11));
            var expired = await Send(aClient, HttpMethod.Post, "/api/v1/planning/apply", ApplyBody(expiring), 409, key: "expired");
            Check(expired.GetProperty("code").GetString() == "PREVIEW_EXPIRED", "A new apply rejects expired preview");

            var racing = await Preview();
            async Task<HttpStatusCode> Race(string key)
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/planning/apply") { Content = JsonContent.Create(ApplyBody(racing)) };
                req.Headers.Add("Idempotency-Key", key);
                using var response = await aClient.SendAsync(req); return response.StatusCode;
            }
            var raceResult = await Task.WhenAll(Race("race-a"), Race("race-b"));
            Check(raceResult.Count(s => s == HttpStatusCode.OK) == 1 && raceResult.Count(s => s == HttpStatusCode.Conflict) == 1, "Two different apply keys racing for the same revision cannot double-book");
            await using (var db = new ApplicationDbContext(options))
            {
                var active = await db.DanhSachPhienTuHoc.Where(x => x.MaNguoiDung == owner && x.TrangThai != TrangThaiPhienTuHoc.Cancelled).OrderBy(x => x.BatDauDuKien).ToListAsync();
                Check(active.Count == 1, "Exactly one racing session persisted");
                var s = active[0]; clock.Set(s.BatDauDuKien);
                await Write(aClient, HttpMethod.Post, $"/api/v1/study-sessions/{s.Ma}/start", null, 200);
                var repeatedStart = await Write(aClient, HttpMethod.Post, $"/api/v1/study-sessions/{s.Ma}/start", null, 200);
                Check(Data(repeatedStart).GetProperty("trangThai").GetString() == "InProgress", "Repeated start is idempotent");
                var metadata=Data(await Send(aClient,HttpMethod.Get,$"/api/v1/tasks/{taskId}",null,200));
                await Write(aClient,HttpMethod.Put,$"/api/v1/tasks/{taskId}",TaskBody("Chỉ đổi tiêu đề khi đang học"),200);
                var versionBeforeNoop=await Revision(aClient);
                await Write(aClient,HttpMethod.Put,$"/api/v1/tasks/{taskId}",TaskBody("Chỉ đổi tiêu đề khi đang học"),200);
                Check(await Revision(aClient)==versionBeforeNoop,"No-op task edit leaves revision unchanged even during active session");
                await Write(aClient, HttpMethod.Post, $"/api/v1/study-sessions/{s.Ma}/finish", null, 200);
                Check(Data(await Send(aClient, HttpMethod.Get, $"/api/v1/tasks/{taskId}", null, 200)).GetProperty("tienDo").GetInt32() == 25, "Finishing session does not silently advance task progress");
            }
            var estimateTask=Id(await Write(aClient,HttpMethod.Post,"/api/v1/tasks",TaskBody("Bỏ ước lượng cần hủy phiên",50,"2026-12-30T23:59:00+07:00"),201));
            var estimateSession=Id(await Write(aClient,HttpMethod.Post,"/api/v1/study-sessions",new{maNhiemVu=estimateTask,batDauDuKien="2026-11-25T19:00:00+07:00",ketThucDuKien="2026-11-25T19:50:00+07:00"},201));
            await Write(aClient,HttpMethod.Put,$"/api/v1/tasks/{estimateTask}",TaskBody("Bỏ ước lượng cần hủy phiên",null,"2026-12-30T23:59:00+07:00"),409);
            await Write(aClient,HttpMethod.Put,$"/api/v1/tasks/{estimateTask}",new{tieuDe="Bỏ ước lượng cần hủy phiên",maMonHoc=course,thoiLuongDuKienPhut=(int?)null,hanHoanThanh="2026-12-30T23:59:00+07:00",mucDoUuTien="High",huyPhienLienQuan=true},200);
            Check(Data(await Send(aClient,HttpMethod.Get,$"/api/v1/study-sessions/{estimateSession}",null,200)).GetProperty("trangThai").GetString()=="Cancelled","Removing an estimate cannot leave future unestimated reservations");
            var swagger=await Send(aClient, HttpMethod.Get, "/swagger/v1/swagger.json", null, 200);
            Check(swagger.GetProperty("paths").TryGetProperty("/api/v1/planning/apply",out _),"Swagger exposes real planning route");
            Check(swagger.GetProperty("paths").GetProperty("/api/v1/tasks/{id}/children").GetProperty("post").GetProperty("responses").TryGetProperty("201",out _),"Swagger documents actual batch creation status201");
            Check(swagger.GetProperty("paths").GetProperty("/api/v1/tasks/{id}").GetProperty("put").GetProperty("parameters").EnumerateArray().Any(x=>x.GetProperty("name").GetString()=="If-Match"),"Swagger documents concurrency precondition");
            var directory=new DirectoryInfo(AppContext.BaseDirectory);
            while(directory is not null && !File.Exists(Path.Combine(directory.FullName,"frontend-preview","checks-http.cjs"))) directory=directory.Parent;
            if(directory is null)throw new InvalidOperationException("Cannot locate the repository client/API contract checks.");
            var js=Path.Combine(directory.FullName,"frontend-preview","checks-http.cjs");
            var nodeInfo=new System.Diagnostics.ProcessStartInfo("node"){RedirectStandardInput=true,RedirectStandardOutput=true,RedirectStandardError=true,UseShellExecute=false,CreateNoWindow=true};
            nodeInfo.ArgumentList.Add(js);
            using(var process=System.Diagnostics.Process.Start(nodeInfo)!)
            {
                await using var authDb=new ApplicationDbContext(options);
                var user=await authDb.DanhSachNguoiDung.SingleAsync(x=>x.Ma==owner);
                var auth=app.Services.GetRequiredService<JwtTokenIssuer>().Issue(user);
                await process.StandardInput.WriteAsync(JsonSerializer.Serialize(new{baseAddress=address,auth},new JsonSerializerOptions(JsonSerializerDefaults.Web)));
                process.StandardInput.Close();
                var outputTask=process.StandardOutput.ReadToEndAsync();var errorTask=process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();var output=await outputTask;var error=await errorTask;
                Check(process.ExitCode==0,"Real client/API contract: "+error);Console.Write(output);
            }
            Console.WriteLine($"PASS: {count} PostgreSQL/HTTP core assertions; temporary database only.");
        }
        finally
        {
            if (app is not null) { await app.StopAsync(); await app.DisposeAsync(); }
            NpgsqlConnection.ClearAllPools();
            await using var drop = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{name}\" WITH (FORCE)", admin);
            await drop.ExecuteNonQueryAsync();
        }
    }
}

internal sealed class KiemThuClock(DateTimeOffset now) : TimeProvider
{
    private DateTimeOffset current = now;
    public override DateTimeOffset GetUtcNow() => current;
    public void Advance(TimeSpan amount) => current = current.Add(amount);
    public void Set(DateTimeOffset value) => current = value;
}

internal sealed class LoiSauLuuStore(IHocTapStore inner, IHttpContextAccessor context) : IHocTapStore
{
    private int saveCount;
    public IQueryable<T> TruyVan<T>() where T : class => inner.TruyVan<T>();
    public Task<List<T>> DanhSachAsync<T>(IQueryable<T> query, CancellationToken ct) => inner.DanhSachAsync(query, ct);
    public Task<T?> DauTienAsync<T>(IQueryable<T> query, CancellationToken ct) => inner.DauTienAsync(query, ct);
    public Task<int> DemAsync<T>(IQueryable<T> query, CancellationToken ct) => inner.DemAsync(query, ct);
    public void Them<T>(T entity) where T : class => inner.Them(entity);
    public void Xoa<T>(T entity) where T : class => inner.Xoa(entity);
    public async Task LuuAsync(CancellationToken ct)
    {
        await inner.LuuAsync(ct);
        if (int.TryParse(context.HttpContext?.Request.Headers["X-CoreChecks-Fail-Save"], out var fail) && ++saveCount == fail)
            throw new InvalidOperationException("Injected failure after persisted SaveChanges, before commit.");
    }
    public Task<List<NhiemVu>> CayAsync(int chu, int ma, CancellationToken ct) => inner.CayAsync(chu, ma, ct);
    public Task<IHocTapTransaction> BatDauAsync(bool ghi, CancellationToken ct) => inner.BatDauAsync(ghi, ct);
    public Task<NguoiDung> NguoiDungAsync(int chu, bool khoa, CancellationToken ct) => inner.NguoiDungAsync(chu, khoa, ct);
}
