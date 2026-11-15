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
                    MuiGio = "Asia/Ho_Chi_Minh", PhienHocPhut = 50, NghiGiuaPhienPhut = 10, GioiHanHocMoiNgayPhut = 720,
                    NgayTao = clock.GetUtcNow(), NgayCapNhat = clock.GetUtcNow()
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
            // Chỉ máy chủ kiểm thử có điểm chèn lỗi; API sản phẩm không đọc tiêu đề yêu cầu này.
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
            var parentPath = $"/api/v1/tasks/{parentId}";
            Check(Data(await Send(aClient, HttpMethod.Get, parentPath, null, 200)).GetProperty("tienDo").GetInt32() == 30, "Parent progress is the mean of all immediate children");
            var derivedError = await Write(aClient, HttpMethod.Patch, parentPath + "/progress", new { tienDo = 20 }, 409);
            Check(derivedError.GetProperty("code").GetString() == "TASK_PROGRESS_IS_DERIVED", "A group cannot override progress derived from children");
            await Write(aClient, HttpMethod.Post, parentPath + "/complete", new { }, 409);
            await Write(aClient, HttpMethod.Post, parentPath + "/reopen", new { tienDo = 0 }, 409);
            object ParentChecklist(bool allDone) => new { tieuDe = "Bài lớn", maMonHoc = course, hanHoanThanh = "2026-11-06T23:59:00+07:00",
                checklist = new[] { new { tieuDe = "Một", daXong = true }, new { tieuDe = "Hai", daXong = allDone }, new { tieuDe = "Ba", daXong = allDone } } };
            var checklistError = await Write(aClient, HttpMethod.Put, parentPath, ParentChecklist(false), 409);
            Check(checklistError.GetProperty("code").GetString() == "TASK_CHECKLIST_REQUIRES_LEAF", "A parent cannot have a second checklist-based progress source");
            await Write(aClient, HttpMethod.Post, $"/api/v1/tasks/{child1}/complete", new { }, 200);
            Check(Data(await Send(aClient, HttpMethod.Get, parentPath, null, 200)).GetProperty("tienDo").GetInt32() == 50, "Completed children are still included in group progress");
            var complete = Data(await Write(aClient, HttpMethod.Post, $"/api/v1/tasks/{child2}/complete", new { }, 200));
            Check(Data(await Send(aClient, HttpMethod.Get, parentPath, null, 200)).GetProperty("trangThai").GetString() == "Completed", "A group completes only after all children complete");
            await Write(aClient, HttpMethod.Post, $"/api/v1/tasks/{child1}/reopen", new { tienDo = 60 }, 200);
            var reopenedParent = Data(await Send(aClient, HttpMethod.Get, parentPath, null, 200));
            Check(reopenedParent.GetProperty("tienDo").GetInt32() == 80 && reopenedParent.GetProperty("ngayHoanThanh").ValueKind == JsonValueKind.Null, "Reopening a child clears the parent's completion state and timestamp");
            var activeList = Data(await Send(aClient, HttpMethod.Get, "/api/v1/tasks?cheDo=all", null, 200)).GetProperty("danhSach");
            Check(activeList.EnumerateArray().Any(task => task.GetProperty("ma").GetInt32() == parentId), "Reopened parent returns to the active list");
            await Write(aClient, HttpMethod.Patch, $"/api/v1/tasks/{parentId}/parent", new { maNhiemVuCha = child1 }, 409);
            var stale = await Revision(aClient);
            await Write(aClient, HttpMethod.Patch, $"/api/v1/tasks/{child2}/progress", new { tienDo = 10 }, 200);
            await Send(aClient, HttpMethod.Patch, $"/api/v1/tasks/{child2}/progress", new { tienDo = 20 }, 409, stale);
            var calendar = Data(await Send(aClient, HttpMethod.Get, "/api/v1/calendar?tu=2026-11-04T00:00:00%2B07:00&den=2026-11-08T00:00:00%2B07:00", null, 200));
            Check(calendar.GetProperty("suKien").EnumerateArray().Any(e => e.GetProperty("loai").GetString() == "HanNhiemVu" && e.GetProperty("maNguon").GetInt32() == child1), "Calendar renders inherited leaf deadline through SQL projection");
            Check(!calendar.GetProperty("suKien").EnumerateArray().Any(e => e.GetProperty("loai").GetString() == "HanNhiemVu" && e.GetProperty("maNguon").GetInt32() == parentId), "Calendar hides parent deadlines by default");

            async Task<JsonElement> ReadTask(int id) => Data(await Send(aClient, HttpMethod.Get, $"/api/v1/tasks/{id}", null, 200));
            async Task<int> NewTask(string title, int? estimate = null) => Id(await Write(aClient, HttpMethod.Post, "/api/v1/tasks", TaskBody(title, estimate, null), 201));
            object AddTasks(bool confirm, params string[] titles) => new { danhSach = titles.Select(title => TaskBody(title, null, null)).ToArray(), xacNhanDoiCauTruc = confirm };
            async Task<JsonElement> AddChildren(int id, bool confirm, params string[] titles) => Data(await Write(aClient, HttpMethod.Post, $"/api/v1/tasks/{id}/children", AddTasks(confirm, titles), 201));
            static int NamedId(JsonElement children, string title) => children.EnumerateArray().Single(task => task.GetProperty("tieuDe").GetString() == title).GetProperty("ma").GetInt32();
            async Task CompleteTask(int id) => await Write(aClient, HttpMethod.Post, $"/api/v1/tasks/{id}/complete", new { }, 200);
            bool ContainsId(JsonElement array, int id) => array.EnumerateArray().Any(item => item.GetInt32() == id);
            var thesisId = await NewTask("Đồ án nhiều cấp");
            var thesisChildren = await AddChildren(thesisId, false, "Slide", "Báo cáo");
            var slideId = NamedId(thesisChildren, "Slide"); var reportId = NamedId(thesisChildren, "Báo cáo");
            var slideChildren = await AddChildren(slideId, false, "Nội dung", "Thiết kế");
            var contentId = NamedId(slideChildren, "Nội dung"); var designId = NamedId(slideChildren, "Thiết kế");
            await CompleteTask(contentId);
            Check((await ReadTask(slideId)).GetProperty("tienDo").GetInt32() == 50 && (await ReadTask(thesisId)).GetProperty("tienDo").GetInt32() == 25,
                "Updating a grandchild recalculates both intermediate and root progress");
            await CompleteTask(reportId); await CompleteTask(designId);
            Check((await ReadTask(thesisId)).GetProperty("trangThai").GetString() == "Completed", "All descendant work completes the root");
            var structuralBody = AddTasks(false, "Tập thuyết trình");
            var structuralRevision = await Revision(aClient);
            var structuralPreview = Data(await Send(aClient, HttpMethod.Post, $"/api/v1/tasks/{slideId}/change-preview", new { thaoTac = "children", noiDung = structuralBody }, 200));
            Check(ContainsId(structuralPreview.GetProperty("maNhiemVuMoLai"), slideId) && ContainsId(structuralPreview.GetProperty("maNhiemVuMoLai"), thesisId),
                "Preview identifies every completed ancestor that adding work will reopen");
            Check(await Revision(aClient) == structuralRevision && (await ReadTask(thesisId)).GetProperty("tienDo").GetInt32() == 100,
                "Structural preview never persists reopening or bumps revision");
            var missingConfirmation = await Write(aClient, HttpMethod.Post, $"/api/v1/tasks/{slideId}/children", structuralBody, 409);
            Check(missingConfirmation.GetProperty("code").GetString() == "TASK_STRUCTURE_CONFIRMATION_REQUIRED", "Adding work to completed tasks requires explicit confirmation");
            Check(await Revision(aClient) == structuralRevision && Data(await Send(aClient, HttpMethod.Get, $"/api/v1/tasks/{slideId}/tree", null, 200)).GetArrayLength() == 3,
                "Unconfirmed structure change leaves tree and revision unchanged");
            var practiceId = NamedId(await AddChildren(slideId, true, "Tập thuyết trình"), "Tập thuyết trình");
            Check((await ReadTask(slideId)).GetProperty("tienDo").GetInt32() == 66 && (await ReadTask(thesisId)).GetProperty("tienDo").GetInt32() == 83,
                "Confirmed added work lowers progress through all ancestor levels");
            Check((await ReadTask(thesisId)).GetProperty("ngayHoanThanh").ValueKind == JsonValueKind.Null, "Confirmed expansion clears stale root completion timestamp");
            await Write(aClient, HttpMethod.Post, $"/api/v1/tasks/{practiceId}/trash", new { }, 200);
            Check((await ReadTask(thesisId)).GetProperty("tienDo").GetInt32() == 100, "Trashing pending work recalculates active ancestors");
            await Write(aClient, HttpMethod.Post, $"/api/v1/tasks/trash/{practiceId}/restore", null, 204);
            Check((await ReadTask(thesisId)).GetProperty("tienDo").GetInt32() == 83 && (await ReadTask(slideId)).GetProperty("tienDo").GetInt32() == 66,
                "Restoring pending work reopens and recalculates the original ancestor chain");
            await CompleteTask(practiceId);
            Check((await ReadTask(thesisId)).GetProperty("tienDo").GetInt32() == 100, "Recompleted descendants complete ancestors again");
            await using (var db = new ApplicationDbContext(options))
                Check(await db.DanhSachLichSuNhiemVu.AnyAsync(entry => entry.MaNhiemVu == thesisId && entry.NguonThayDoi == "System"), "Ancestor progress changes have system-authored history");

            var checklistRoot = Id(await Write(aClient, HttpMethod.Post, "/api/v1/tasks", new {
                tieuDe = "Checklist cần chia nhỏ", maMonHoc = course, thoiLuongDuKienPhut = 120,
                checklist = new[] { new { tieuDe = "Đã đọc tài liệu", daXong = true }, new { tieuDe = "Còn luyện tập", daXong = false } }
            }, 201));
            Check((await ReadTask(checklistRoot)).GetProperty("tienDo").GetInt32() == 50, "Leaf progress still comes from its checklist");
            await Write(aClient, HttpMethod.Patch, $"/api/v1/tasks/{checklistRoot}/progress", new { tienDo = 80 }, 409);
            var checklistPreview = Data(await Send(aClient, HttpMethod.Post, $"/api/v1/tasks/{checklistRoot}/change-preview", new { thaoTac = "children", noiDung = AddTasks(false, "Bài thực hành") }, 200));
            Check(ContainsId(checklistPreview.GetProperty("maChecklistChuyenThanhBuoc"), checklistRoot), "Preview discloses preserving a leaf checklist in a child");
            await Write(aClient, HttpMethod.Post, $"/api/v1/tasks/{checklistRoot}/children", AddTasks(false, "Bài thực hành"), 409);
            var checklistChildren = await AddChildren(checklistRoot, true, "Bài thực hành");
            var preservedChecklistId = NamedId(checklistChildren, "Công việc chung");
            var preservedChecklist = await ReadTask(preservedChecklistId);
            Check(preservedChecklist.GetProperty("checklist").GetArrayLength() == 2 && preservedChecklist.GetProperty("checklist")[0].GetProperty("daXong").GetBoolean()
                && !preservedChecklist.GetProperty("checklist")[1].GetProperty("daXong").GetBoolean(), "Checklist conversion preserves every label and completion flag");
            Check(preservedChecklist.GetProperty("thoiLuongDuKienPhut").GetInt32() == 120 && (await ReadTask(checklistRoot)).GetProperty("tienDo").GetInt32() == 25
                && (await ReadTask(checklistRoot)).GetProperty("checklist").GetArrayLength() == 0, "Converted checklist has one progress source and retains its estimate");
            await Send(bClient, HttpMethod.Get, $"/api/v1/tasks/{preservedChecklistId}", null, 404);
            await Send(bClient, HttpMethod.Post, $"/api/v1/tasks/{checklistRoot}/children", AddTasks(true, "Không được thêm"), 404, await Revision(bClient));

            var manualRoot = await NewTask("Việc đã làm một phần", 90);
            await Write(aClient, HttpMethod.Patch, $"/api/v1/tasks/{manualRoot}/progress", new { tienDo = 60 }, 200);
            var manualPreview = Data(await Send(aClient, HttpMethod.Post, $"/api/v1/tasks/{manualRoot}/change-preview", new { thaoTac = "children", noiDung = AddTasks(false, "Phần mới") }, 200));
            Check(ContainsId(manualPreview.GetProperty("maTienDoChuyenThanhBuoc"), manualRoot), "Preview discloses preserving manually recorded progress before first split");
            await Write(aClient, HttpMethod.Post, $"/api/v1/tasks/{manualRoot}/children", AddTasks(false, "Phần mới"), 409);
            var manualChildren = await AddChildren(manualRoot, true, "Phần mới");
            var preservedWork = await ReadTask(NamedId(manualChildren, "Phần việc đã thực hiện"));
            Check(preservedWork.GetProperty("tienDo").GetInt32() == 60 && preservedWork.GetProperty("thoiLuongDuKienPhut").GetInt32() == 90
                && (await ReadTask(manualRoot)).GetProperty("tienDo").GetInt32() == 30, "First split preserves prior manual work instead of resetting or double counting it");

            var rollbackRoot = Id(await Write(aClient, HttpMethod.Post, "/api/v1/tasks", new { tieuDe = "Không mất checklist khi lỗi", maMonHoc = course,
                checklist = new[] { new { tieuDe = "Giữ nguyên", daXong = true } } }, 201));
            var rollbackRevision = await Revision(aClient);
            await Write(aClient, HttpMethod.Post, $"/api/v1/tasks/{rollbackRoot}/children", AddTasks(true, "Phải rollback"), 500, 1);
            var afterFailedSplit = await ReadTask(rollbackRoot);
            Check(afterFailedSplit.GetProperty("checklist").GetArrayLength() == 1 && afterFailedSplit.GetProperty("tienDo").GetInt32() == 100
                && !afterFailedSplit.GetProperty("coNhiemVuCon").GetBoolean() && await Revision(aClient) == rollbackRevision,
                "Failure after persisted structural conversion rolls back checklist, children, completion and revision");

            var oldRoot = await NewTask("Nhánh nguồn"); var newRoot = await NewTask("Nhánh đích");
            var movingChildren = await AddChildren(oldRoot, false, "Chuyển việc đã xong", "Việc còn lại");
            var movingId = NamedId(movingChildren, "Chuyển việc đã xong");
            await CompleteTask(movingId);
            await Write(aClient, HttpMethod.Patch, $"/api/v1/tasks/{movingId}/parent", new { maNhiemVuCha = newRoot }, 200);
            Check((await ReadTask(oldRoot)).GetProperty("tienDo").GetInt32() == 0 && (await ReadTask(newRoot)).GetProperty("tienDo").GetInt32() == 100,
                "Reparenting recalculates both source and destination trees");
            var pendingMove = await NewTask("Chuyển việc chưa xong");
            var reparentError = await Write(aClient, HttpMethod.Patch, $"/api/v1/tasks/{pendingMove}/parent", new { maNhiemVuCha = newRoot }, 409);
            Check(reparentError.GetProperty("code").GetString() == "TASK_STRUCTURE_CONFIRMATION_REQUIRED", "Reparenting pending work into a completed group requires confirmation");
            await Write(aClient, HttpMethod.Patch, $"/api/v1/tasks/{pendingMove}/parent", new { maNhiemVuCha = newRoot, xacNhanDoiCauTruc = true }, 200);
            Check((await ReadTask(newRoot)).GetProperty("tienDo").GetInt32() == 50 && (await ReadTask(newRoot)).GetProperty("ngayHoanThanh").ValueKind == JsonValueKind.Null,
                "Confirmed reparent reopens completed destination groups");
            var lastRoot = await NewTask("Xóa bước cuối"); var lastChild = NamedId(await AddChildren(lastRoot, false, "Bước duy nhất"), "Bước duy nhất");
            await CompleteTask(lastChild); await Write(aClient, HttpMethod.Delete, $"/api/v1/tasks/{lastChild}", null, 204);
            var emptyGroup = await ReadTask(lastRoot);
            Check(emptyGroup.GetProperty("tienDo").GetInt32() == 0 && emptyGroup.GetProperty("trangThai").GetString() == "NotStarted"
                && !emptyGroup.GetProperty("coNhiemVuCon").GetBoolean(), "Deleting the last child clears former derived progress rather than leaving false completion");

            var restoreRoot = await NewTask("Khôi phục vào cha đã có phần việc riêng");
            var restoreChild = NamedId(await AddChildren(restoreRoot, false, "Bước khôi phục"), "Bước khôi phục");
            await Write(aClient, HttpMethod.Post, $"/api/v1/tasks/{restoreChild}/trash", new { }, 200);
            await Write(aClient, HttpMethod.Patch, $"/api/v1/tasks/{restoreRoot}/progress", new { tienDo = 40 }, 200);
            await Write(aClient, HttpMethod.Post, $"/api/v1/tasks/trash/{restoreChild}/restore", null, 204);
            var restoredBranch = Data(await Send(aClient, HttpMethod.Get, $"/api/v1/tasks/{restoreRoot}/tree", null, 200));
            Check(restoredBranch.GetArrayLength() == 3 && NamedId(restoredBranch, "Phần việc đã thực hiện") > 0
                && (await ReadTask(restoreRoot)).GetProperty("tienDo").GetInt32() == 20,
                "Restore preserves a former parent's newly recorded manual work before attaching the old child");

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
            // Khung trùng thứ hai vẫn bao phủ phiên học khi tắt khung thứ nhất.
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
                var s = active[0]; clock.Set(s.BatDauDuKien.AddMinutes(-10));
                var actualStart = clock.GetUtcNow();
                var batchPath = $"/api/v1/study-sessions?maLanApDung={s.MaLanApDung}";
                var batch = Data(await Send(aClient, HttpMethod.Get, batchPath, null, 200));
                Check(batch.GetArrayLength() == 1 && batch[0].GetProperty("ma").GetInt32() == s.Ma, "Batch view returns the sessions of the applied plan");
                await Send(bClient, HttpMethod.Get, batchPath, null, 404);
                var sessionPath = $"/api/v1/study-sessions/{s.Ma}";
                var started = Data(await Write(aClient, HttpMethod.Post, sessionPath + "/start", null, 200));
                Check(started.GetProperty("batDauBamGio").GetDateTimeOffset() == actualStart, "Starting early stores the actual server time");
                Check(started.GetProperty("batDauDuKien").GetDateTimeOffset() == s.BatDauDuKien &&
                    started.GetProperty("ketThucDuKien").GetDateTimeOffset() == s.KetThucDuKien, "Starting early preserves the planned interval");
                clock.Advance(TimeSpan.FromMinutes(5));
                var repeatedStart = Data(await Write(aClient, HttpMethod.Post, sessionPath + "/start", null, 200));
                Check(repeatedStart.GetProperty("trangThai").GetString() == "InProgress" &&
                    repeatedStart.GetProperty("batDauBamGio").GetDateTimeOffset() == actualStart, "Repeated start keeps the original stopwatch");
                var running = Data(await Send(aClient, HttpMethod.Get, sessionPath, null, 200));
                Check(running.GetProperty("batDauBamGio").GetDateTimeOffset() == actualStart, "Session details restore the running stopwatch");
                var metadata=Data(await Send(aClient,HttpMethod.Get,$"/api/v1/tasks/{taskId}",null,200));
                await Write(aClient,HttpMethod.Put,$"/api/v1/tasks/{taskId}",TaskBody("Chỉ đổi tiêu đề khi đang học"),200);
                var versionBeforeNoop=await Revision(aClient);
                await Write(aClient,HttpMethod.Put,$"/api/v1/tasks/{taskId}",TaskBody("Chỉ đổi tiêu đề khi đang học"),200);
                Check(await Revision(aClient)==versionBeforeNoop,"No-op task edit leaves revision unchanged even during active session");
                var finished = Data(await Write(aClient, HttpMethod.Post, sessionPath + "/finish", null, 200));
                Check(finished.GetProperty("soGiayDaHoc").GetInt64() == 300 &&
                    finished.GetProperty("batDauBamGio").ValueKind == JsonValueKind.Null, "Finishing stops the stopwatch and records elapsed seconds");
                clock.Advance(TimeSpan.FromSeconds(1));
                var repeatedFinish = Data(await Write(aClient, HttpMethod.Post, sessionPath + "/finish", null, 200));
                Check(repeatedFinish.GetProperty("soGiayDaHoc").GetInt64() == 300, "Repeated finish preserves recorded duration");
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
            while(directory is not null && !File.Exists(Path.Combine(directory.FullName,"backend","tests","StudyPlanner.CoreChecks","ClientContractChecks.mjs"))) directory=directory.Parent;
            if(directory is null)throw new InvalidOperationException("Cannot locate ClientContractChecks.mjs in this repository.");
            var frontendRoot=Environment.GetEnvironmentVariable("STUDYPLANNER_TEST_FRONTEND") ?? Path.Combine(directory.Parent!.FullName,"Study_Planner_FE");
            if(!File.Exists(Path.Combine(frontendRoot,"src","hoc-tap","api.ts")))throw new InvalidOperationException("Set STUDYPLANNER_TEST_FRONTEND to the frontend repository with installed dependencies.");
            var js=Path.Combine(directory.FullName,"backend","tests","StudyPlanner.CoreChecks","ClientContractChecks.mjs");
            var nodeInfo=new System.Diagnostics.ProcessStartInfo("node"){RedirectStandardInput=true,RedirectStandardOutput=true,RedirectStandardError=true,UseShellExecute=false,CreateNoWindow=true};
            nodeInfo.ArgumentList.Add(js);
            using(var process=System.Diagnostics.Process.Start(nodeInfo)!)
            {
                await using var authDb=new ApplicationDbContext(options);
                var user=await authDb.DanhSachNguoiDung.SingleAsync(x=>x.Ma==owner);
                var auth=app.Services.GetRequiredService<JwtTokenIssuer>().Issue(user);
                await process.StandardInput.WriteAsync(JsonSerializer.Serialize(new{baseAddress=address,auth,frontendRoot},new JsonSerializerOptions(JsonSerializerDefaults.Web)));
                process.StandardInput.Close();
                var outputTask=process.StandardOutput.ReadToEndAsync();var errorTask=process.StandardError.ReadToEndAsync();
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                try { await process.WaitForExitAsync(timeout.Token); }
                catch (OperationCanceledException) { process.Kill(true); throw new InvalidOperationException("Frontend/API contract checks timed out."); }
                var output=await outputTask;var error=await errorTask;
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
    public IQueryable<NhiemVu> NhiemVuTrongThungRac() => inner.NhiemVuTrongThungRac();
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
