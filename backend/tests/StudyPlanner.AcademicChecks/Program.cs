using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
using StudyPlanner.Api.Academic;
using StudyPlanner.Api.Administration;
using StudyPlanner.Api.Auth;
using StudyPlanner.Application.Administration;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;
using StudyPlanner.Infrastructure.Data;

var supplied = Environment.GetEnvironmentVariable("STUDYPLANNER_TEST_POSTGRES");
if (string.IsNullOrWhiteSpace(supplied)) throw new InvalidOperationException("Set STUDYPLANNER_TEST_POSTGRES to isolated local PostgreSQL.");
var config = new NpgsqlConnectionStringBuilder(supplied);
if (config.Host is not ("localhost" or "127.0.0.1") || config.Database != "postgres") throw new InvalidOperationException("Local test DB only.");
var database = "studyplanner_academic_checks_" + Guid.NewGuid().ToString("N");
await using var admin = new NpgsqlConnection(config.ConnectionString); await admin.OpenAsync();
await using (var command = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", admin)) await command.ExecuteNonQueryAsync();
config.Database = database; config.Pooling = false;
var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(config.ConnectionString).Options;
WebApplication? app = null;
var checks = 0;
void Check(bool condition, string label) { if (!condition) throw new Exception("FAIL: " + label); checks++; }
try
{
    await using (var db = new ApplicationDbContext(options)) await db.Database.MigrateAsync();
    var failure = new DeleteFailure();
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
    builder.Logging.ClearProviders(); builder.WebHost.UseUrls("http://127.0.0.1:0");
    builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseNpgsql(config.ConnectionString).AddInterceptors(failure));
    builder.Services.AddAdministration(); builder.Services.AddAcademic();
    builder.Services.AddStudyPlannerAuth(builder.Configuration, builder.Environment);
    builder.Services.AddEndpointsApiExplorer(); builder.Services.AddSwaggerGen();
    app = builder.Build(); app.UseExceptionHandler(); app.UseSwagger();
    app.UseAuthentication(); app.UseAuthorization(); app.UseRateLimiter(); app.MapControllers(); await app.StartAsync();
    var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
    using var client = new HttpClient { BaseAddress = new Uri(address) };
    var swagger = await client.GetFromJsonAsync<JsonElement>("/swagger/v1/swagger.json");
    Check(swagger.GetProperty("paths").GetProperty("/api/v1/semesters").TryGetProperty("post", out _), "Swagger semesters route");
    Check(swagger.GetProperty("paths").GetProperty("/api/v1/courses/{id}").GetProperty("delete").GetProperty("responses").TryGetProperty("204", out _), "Swagger empty deletion response");
    Check(!swagger.GetProperty("components").GetProperty("schemas").GetProperty("CourseRequest").GetProperty("properties").TryGetProperty("maHocKy", out _), "Swagger course update cannot reassign semester");
    // PostgreSQL stores microseconds; use whole milliseconds for exact round trips.
    var now = DateTimeOffset.FromUnixTimeMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    var a = new User { Email = "a@example.test", EmailChuanHoa = "A@EXAMPLE.TEST", HoTen = "A", MatKhauBam = Guid.NewGuid().ToString(), NgayTao = now, NgayCapNhat = now };
    var b = new User { Email = "b@example.test", EmailChuanHoa = "B@EXAMPLE.TEST", HoTen = "B", MatKhauBam = Guid.NewGuid().ToString(), NgayTao = now, NgayCapNhat = now };
    await using (var db = new ApplicationDbContext(options)) { db.Users.AddRange(a, b); await db.SaveChangesAsync(); }
    var issuer = app.Services.GetRequiredService<JwtTokenIssuer>();
    var tokenA = issuer.Issue(a).TokenTruyCap; var tokenB = issuer.Issue(b).TokenTruyCap;
    void Actor(string? token) => client.DefaultRequestHeaders.Authorization = token == null ? null : new AuthenticationHeaderValue("Bearer", token);
    async Task<JsonElement> Send(HttpMethod method, string path, object? body, HttpStatusCode expected)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body != null) request.Content = JsonContent.Create(body);
        using var response = await client.SendAsync(request);
        var text = await response.Content.ReadAsStringAsync();
        Check(response.StatusCode == expected, $"{method} {path}: expected {(int)expected}, got {(int)response.StatusCode}");
        if (expected == HttpStatusCode.NoContent) Check(text.Length == 0, "204 has no body");
        if (expected == HttpStatusCode.Created) Check(response.Headers.Location != null, "201 includes Location");
        return text.Length == 0 ? default : JsonDocument.Parse(text).RootElement.Clone();
    }
    Task<JsonElement> Get(string path, HttpStatusCode status = HttpStatusCode.OK) => Send(HttpMethod.Get, path, null, status);
    Task<JsonElement> Post(string path, object body, HttpStatusCode status = HttpStatusCode.Created) => Send(HttpMethod.Post, path, body, status);
    Task<JsonElement> Put(string path, object body, HttpStatusCode status = HttpStatusCode.OK) => Send(HttpMethod.Put, path, body, status);
    Task<JsonElement> Delete(string path, HttpStatusCode status = HttpStatusCode.NoContent) => Send(HttpMethod.Delete, path, null, status);
    object Semester(string name = "Học kỳ thử") => new { ten = name, ngayBatDau = "2026-09-01", ngayKetThuc = "2027-01-31" };
    object Course(string name = "Môn thử") => new { ten = name, kyHieu = " CS01 ", soTinChi = 3, giangVien = " GV ", mauSac = "#53a88c" };
    await Get("/api/v1/semesters", HttpStatusCode.Unauthorized);
    Actor(tokenA);
    var semester = await Post("/api/v1/semesters", Semester()); var sid = semester.GetProperty("ma").GetInt32();
    var path = "/api/v1/semesters/" + sid;
    await Post("/api/v1/semesters", new { ten = " ", ngayBatDau = "2026-09-01", ngayKetThuc = "2026-09-30" }, HttpStatusCode.BadRequest);
    await Post("/api/v1/semesters", new { ten = "Sai ngày", ngayBatDau = "2026-09-01", ngayKetThuc = "2026-08-30" }, HttpStatusCode.BadRequest);
    await Post("/api/v1/semesters", new { ten = "Thiếu ngày" }, HttpStatusCode.BadRequest);
    await Get("/api/v1/semesters?Trang=0", HttpStatusCode.BadRequest);
    await Get("/api/v1/semesters?KichThuocTrang=101", HttpStatusCode.BadRequest);
    await Get("/api/v1/semesters/0", HttpStatusCode.BadRequest);
    await Get("/api/v1/semesters/not-an-id", HttpStatusCode.BadRequest);
    await Post("/api/v1/semesters", Semester("Học kỳ trùng ngày"));
    var page = await Get("/api/v1/semesters?Trang=2&KichThuocTrang=1");
    Check(page.GetProperty("danhSach").GetArrayLength() == 1 && page.GetProperty("trang").GetInt32() == 2 && page.GetProperty("tongSo").GetInt32() == 2, "pagination contract");
    Check((await Get("/api/v1/semesters?TuKhoa=không-tồn-tại")).GetProperty("tongSo").GetInt32() == 0, "search filter");
    await Put(path, Semester("Đã sửa"));
    var course = await Post(path + "/courses", Course()); var cid = course.GetProperty("ma").GetInt32();
    var cp = "/api/v1/courses/" + cid;
    Check(course.GetProperty("mauSac").GetString() == "#53A88C" && course.GetProperty("kyHieu").GetString() == "CS01", "color and trimming");
    foreach (var color in new[] { "sage", "#123", "red" }) await Post(path + "/courses", new { ten = "Sai màu", mauSac = color }, HttpStatusCode.BadRequest);
    await Post(path + "/courses", new { ten = "Sai tín chỉ", soTinChi = 0 }, HttpStatusCode.BadRequest);
    await Put(cp, new { ten = "Chuyển môn", maHocKy = sid + 1 }, HttpStatusCode.BadRequest);
    await Put(cp, Course("Đã sửa môn"));
    var minimal = await Post(path + "/courses", new { ten = "Môn tối thiểu" });
    var secondId = minimal.GetProperty("ma").GetInt32();
    Check(minimal.GetProperty("soTinChi").ValueKind == JsonValueKind.Null, "optional course fields");
    Check((await Get(path + "/courses?Trang=2&KichThuocTrang=1")).GetProperty("tongSo").GetInt32() == 2, "courses pagination");
    Actor(tokenB);
    Check((await Get("/api/v1/semesters")).GetProperty("tongSo").GetInt32() == 0, "owner filtered list");
    foreach (var target in new[] { path, cp, path + "/courses", path + "/deletion-preview", cp + "/deletion-preview" }) await Get(target, HttpStatusCode.NotFound);
    await Post(path + "/courses", Course(), HttpStatusCode.NotFound);
    await Put(path, Semester(), HttpStatusCode.NotFound); await Put(cp, Course(), HttpStatusCode.NotFound);
    await Delete(path, HttpStatusCode.NotFound); await Delete(cp, HttpStatusCode.NotFound);
    Actor(tokenA);
    int parentId, childId;
    var frontendRoot = Environment.GetEnvironmentVariable("STUDYPLANNER_FE_ROOT");
    if (!string.IsNullOrWhiteSpace(frontendRoot))
    {
        var start = new System.Diagnostics.ProcessStartInfo("node") { WorkingDirectory = frontendRoot, UseShellExecute = false, CreateNoWindow = true };
        start.ArgumentList.Add(Path.Combine(frontendRoot, "tests", "academic-live.mjs"));
        start.Environment["TEST_ACADEMIC_API_URL"] = address.TrimEnd('/');
        start.Environment["TEST_ACADEMIC_TOKEN"] = tokenA;
        start.Environment["TEST_ACADEMIC_OTHER_TOKEN"] = tokenB;
        using var frontend = System.Diagnostics.Process.Start(start)!;
        await frontend.WaitForExitAsync(); Check(frontend.ExitCode == 0, "FE adapters through Vite proxy");
    }
    else Console.WriteLine("FE adapter check skipped: set STUDYPLANNER_FE_ROOT.");
    await using (var db = new ApplicationDbContext(options))
    {
        var parent = new TaskItem { MaNguoiDung = a.Ma, MaHocKy = sid, MaMonHoc = cid, TieuDe = "Cha", Loai = TaskType.Assignment, HanHoanThanh = now.AddDays(10), NgayTao = now, NgayCapNhat = now };
        var child = new TaskItem { MaNguoiDung = a.Ma, MaHocKy = sid, MaMonHoc = cid, ParentTask = parent, TieuDe = "Con", Loai = TaskType.Revision, TienDo = 50, TrangThai = TaskItemStatus.InProgress, HanHoanThanh = now.AddDays(11), NgayTao = now, NgayCapNhat = now };
        db.Tasks.AddRange(parent, child, new TaskItem { MaNguoiDung = a.Ma, MaHocKy = sid, TieuDe = "Việc chung kỳ", NgayTao = now, NgayCapNhat = now });
        db.ClassSessions.Add(new ClassSession { MaMonHoc = cid, ThuTrongTuan = 1, GioBatDau = new(8, 0), GioKetThuc = new(9, 0), NgayTao = now, NgayCapNhat = now });
        await db.SaveChangesAsync(); parentId = parent.Ma; childId = child.Ma;
    }
    var preview = await Get(cp + "/deletion-preview");
    Check(preview.GetProperty("soBuoiHoc").GetInt32() == 1 && preview.GetProperty("soNhiemVuBoLienKet").GetInt32() == 2, "course impact from persisted tasks and schedule");
    failure.Enabled = true;
    await Delete(cp, HttpStatusCode.InternalServerError);
    await using (var db = new ApplicationDbContext(options))
        Check(await db.Courses.AnyAsync(x => x.Ma == cid) && await db.Tasks.CountAsync(x => x.MaMonHoc == cid) == 2 && await db.TaskHistories.CountAsync() == 0 && await db.ClassSessions.CountAsync() == 1, "failed delete rolls back history, links and schedules");
    failure.Enabled = false;
    await Delete(cp); await Get(cp, HttpStatusCode.NotFound);
    await using (var db = new ApplicationDbContext(options))
    {
        var child = await db.Tasks.SingleAsync(x => x.Ma == childId);
        Check(child.MaMonHoc == null && child.MaHocKy == sid && child.MaNhiemVuCha == parentId && child.TienDo == 50 && child.TrangThai == TaskItemStatus.InProgress && child.HanHoanThanh == now.AddDays(11), "course deletion keeps tree, semester, deadlines and progress");
        Check(await db.TaskHistories.CountAsync() == 2 && await db.ClassSessions.CountAsync() == 0, "history created and schedules removed");
    }
    preview = await Get(path + "/deletion-preview");
    Check(preview.GetProperty("soMonHoc").GetInt32() == 1 && preview.GetProperty("soNhiemVuBoLienKet").GetInt32() == 3, "semester impact includes standalone and detached course tasks");
    failure.Enabled = true;
    await Delete(path, HttpStatusCode.InternalServerError);
    await using (var db = new ApplicationDbContext(options))
        Check(await db.Semesters.AnyAsync(x => x.Ma == sid) && await db.Tasks.CountAsync(x => x.MaHocKy == sid) == 3 && await db.TaskHistories.CountAsync() == 2, "failed semester delete rolls back all link changes and histories");
    failure.Enabled = false;
    await Delete(path);
    await using (var db = new ApplicationDbContext(options))
    {
        Check(await db.Tasks.CountAsync() == 3 && await db.Tasks.AllAsync(x => x.MaHocKy == null && x.MaMonHoc == null), "semester deletion preserves all tasks without links");
        Check(await db.TaskHistories.CountAsync() == 5 && !await db.Courses.AnyAsync(x => x.Ma == secondId), "semester delete records every link change and removes courses");
        Check((await db.Tasks.SingleAsync(x => x.Ma == childId)).MaNhiemVuCha == parentId, "parent link survives semester deletion");
    }
    await Delete(path, HttpStatusCode.NotFound);
    Actor(null); await Delete("/api/v1/courses/1", HttpStatusCode.Unauthorized);
    Console.WriteLine($"PASS: {checks} academic HTTP/JWT/PostgreSQL checks, ownership, validation, pagination, preview, 204, retained task tree, history and rollback.");
}
finally
{
    if (app != null) await app.DisposeAsync();
    NpgsqlConnection.ClearAllPools();
    await using var drop = new NpgsqlCommand($"DROP DATABASE \"{database}\" WITH (FORCE)", admin); await drop.ExecuteNonQueryAsync();
}

sealed class DeleteFailure : SaveChangesInterceptor
{
    public bool Enabled { get; set; }
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData data, InterceptionResult<int> result, CancellationToken ct = default)
    {
        if (Enabled && data.Context!.ChangeTracker.Entries<Course>().Any(x => x.State == EntityState.Deleted))
            throw new InvalidOperationException("Injected deletion failure");
        return ValueTask.FromResult(result);
    }
}
