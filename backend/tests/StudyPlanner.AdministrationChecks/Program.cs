using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using StudyPlanner.Api.Administration;
using StudyPlanner.Application.Administration;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;
using StudyPlanner.Infrastructure.Data;

// Chỉ nhận cấu hình PostgreSQL kiểm thử tường minh, không đọc appsettings/Neon của API.
var supplied = Environment.GetEnvironmentVariable("STUDYPLANNER_TEST_POSTGRES");
if (string.IsNullOrWhiteSpace(supplied))
    throw new InvalidOperationException("Cần đặt STUDYPLANNER_TEST_POSTGRES trỏ PostgreSQL cục bộ dành cho test.");
var adminConfig = new NpgsqlConnectionStringBuilder(supplied);
if (adminConfig.Host is not ("127.0.0.1" or "localhost") || adminConfig.Database != "postgres")
    throw new InvalidOperationException("Chỉ chấp nhận localhost và database quản trị postgres; không dùng Neon.");
var databaseName = "studyplanner_crud_checks_" + Guid.NewGuid().ToString("N");
var testConfig = new NpgsqlConnectionStringBuilder(supplied) { Database = databaseName, Pooling = false };
await using var admin = new NpgsqlConnection(adminConfig.ConnectionString);
await admin.OpenAsync();
await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", admin))
    await create.ExecuteNonQueryAsync();
WebApplication? app = null;
var checks = 0;
void Check(bool condition, string label)
{
    if (!condition) throw new InvalidOperationException("FAIL: " + label);
    checks++; Console.WriteLine("PASS: " + label);
}
async Task<WebApplication> StartAsync(string environment)
{
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = environment });
    builder.Logging.ClearProviders();
    builder.WebHost.UseUrls("http://127.0.0.1:0");
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(testConfig.ConnectionString));
    builder.Services.AddQuanTri();
    builder.Services.AddAuthentication("CrudFixture")
        .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, CrudFixtureAuthentication>("CrudFixture", _ => { });
    builder.Services.AddAuthorization(options => options.AddPolicy("Admin", policy => policy.RequireRole("Admin")));
    builder.Services.AddEndpointsApiExplorer(); builder.Services.AddSwaggerGen();
    var host = builder.Build();
    host.UseExceptionHandler(); host.UseSwagger(); host.UseAuthentication(); host.UseAuthorization(); host.MapControllers();
    await host.StartAsync(); return host;
}
HttpClient Client(WebApplication host) => new()
{
    BaseAddress = new Uri(host.Services.GetRequiredService<IServer>()
        .Features.Get<IServerAddressesFeature>()!.Addresses.Single())
};
async Task CheckRetiredCatalogRoutes(HttpClient client, string actor)
{
    foreach (var resource in new[] { "schools", "faculties", "cohorts", "student-classes" })
    {
        foreach (var (method, suffix) in new[] { (HttpMethod.Get, ""), (HttpMethod.Get, "/1"),
            (HttpMethod.Post, ""), (HttpMethod.Put, "/1"), (HttpMethod.Delete, "/1") })
        {
            using var request = new HttpRequestMessage(method, "/api/v1/admin/" + resource + suffix)
                { Content = JsonContent.Create(new { }) };
            using var response = await client.SendAsync(request);
            Check(response.StatusCode == HttpStatusCode.NotFound, actor + ": retired catalog route is absent: " + method + " " + resource + suffix);
        }
        using var lookup = await client.GetAsync("/api/v1/catalogs/" + resource);
        Check(lookup.StatusCode == HttpStatusCode.NotFound, actor + ": retired catalog lookup is absent: " + resource);
    }
}
try
{
    var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(testConfig.ConnectionString).Options;
    await using (var db = new ApplicationDbContext(dbOptions)) await db.Database.MigrateAsync();
    app = await StartAsync("Development");
    using var client = Client(app);
    async Task<JsonElement> Send(HttpMethod method, string path, object? body, HttpStatusCode expected)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body != null) request.Content = JsonContent.Create(body);
        using var response = await client.SendAsync(request);
        var text = await response.Content.ReadAsStringAsync();
        // Không ghi request chứa mật khẩu hoặc chuỗi kết nối vào output.
        Check(response.StatusCode == expected, $"{method} {path}: {(int)expected}");
        if (expected == HttpStatusCode.Created) Check(response.Headers.Location?.OriginalString.Contains("/api/v1/admin/users/", StringComparison.Ordinal) == true, "201 includes versioned user Location");
        if ((int)expected >= 400)
        {
            Check(response.Content.Headers.ContentType?.MediaType == "application/problem+json", "ProblemDetails content type");
            using var error = JsonDocument.Parse(text);
            Check(error.RootElement.TryGetProperty("code", out _) && error.RootElement.TryGetProperty("traceId", out _), "Error code and traceId");
        }
        return text.Length == 0 ? default : JsonDocument.Parse(text).RootElement.Clone();
    }
    Task<JsonElement> Post(string path, object body, HttpStatusCode expected = HttpStatusCode.Created) => Send(HttpMethod.Post, path, body, expected);
    Task<JsonElement> Put(string path, object body, HttpStatusCode expected = HttpStatusCode.OK) => Send(HttpMethod.Put, path, body, expected);
    Task<JsonElement> Get(string path, HttpStatusCode expected = HttpStatusCode.OK) => Send(HttpMethod.Get, path, null, expected);
    Task<JsonElement> Delete(string path, HttpStatusCode expected = HttpStatusCode.NoContent) => Send(HttpMethod.Delete, path, null, expected);
    int Id(JsonElement value) => value.GetProperty("ma").GetInt32();

    await CheckRetiredCatalogRoutes(client, "Admin fixture");
    await Get("/api/v1/admin/users/0", HttpStatusCode.BadRequest);
    await Get("/api/v1/admin/users/999999", HttpStatusCode.NotFound);
    await Get("/api/v1/admin/users/not-an-id", HttpStatusCode.BadRequest);

    // Mật khẩu giả lập chỉ tồn tại trong bộ nhớ, không hardcode vào ví dụ/tài liệu.
    var password = Guid.NewGuid().ToString("N") + "aA1!";
    var user = await Post("/api/v1/admin/users", new { email = "Student@Example.test", hoTen = "Sinh viên thử", matKhau = password,
        tenTruong = "  Trường tự khai  ", tenLop = "  CNTT01  ", maSoSinhVien = "  SV001  ", vaiTro = "Admin" });
    var userId = Id(user);
    Check(user.GetProperty("tenTruong").GetString() == "Trường tự khai" && user.GetProperty("tenLop").GetString() == "CNTT01",
        "Self-declared school/class names are trimmed without requiring catalog IDs");
    Check(user.GetProperty("ngayTao").GetDateTimeOffset() == user.GetProperty("ngayCapNhat").GetDateTimeOffset(), "Create timestamps match");
    Check(!user.TryGetProperty("maLop", out _) && !user.TryGetProperty("id", out _) && !user.TryGetProperty("fullName", out _),
        "User response hides legacy catalog links and English aliases");
    Check(user.GetProperty("vaiTro").GetString() == "Student", "Client cannot assign Admin");
    Check(!user.TryGetProperty("matKhauBam", out _) && !user.TryGetProperty("matKhau", out _), "User DTO excludes credentials");
    var disabled = await Post("/api/v1/admin/users", new { email = "disabled@example.test", hoTen = "Chưa kích hoạt", matKhau = password,
        tenTruong = "   ", tenLop = "   ", dangHoatDong = false });
    var disabledId = Id(disabled);
    Check(!(await Get($"/api/v1/admin/users/{disabledId}")).GetProperty("dangHoatDong").GetBoolean(), "Create persists explicit inactive status");
    Check(disabled.GetProperty("tenTruong").ValueKind == JsonValueKind.Null && disabled.GetProperty("tenLop").ValueKind == JsonValueKind.Null,
        "Blank optional profile names are normalized to null");
    await Delete($"/api/v1/admin/users/{disabledId}");
    await Post("/api/v1/admin/users", new { email = "student@example.test", hoTen = "Trùng", matKhau = password }, HttpStatusCode.Conflict);
    await Post("/api/v1/admin/users", new { email = "other@example.test", hoTen = "Trùng mã", matKhau = password, maSoSinhVien = "SV001" }, HttpStatusCode.Conflict);
    await Post("/api/v1/admin/users", new { email = "not-an-email", hoTen = "Sai email", matKhau = password }, HttpStatusCode.BadRequest);
    await Post("/api/v1/admin/users", new { email = "long@example.test", hoTen = "Tên trường quá dài", matKhau = password, tenTruong = new string('a', 201) }, HttpStatusCode.BadRequest);
    await Post("/api/v1/admin/users", new { email = "long-class@example.test", hoTen = "Tên lớp quá dài", matKhau = password, tenLop = new string('a', 201) }, HttpStatusCode.BadRequest);
    await Post("/api/v1/admin/users", new { email = "blank@example.test", hoTen = " ", matKhau = password }, HttpStatusCode.BadRequest);
    await Post("/api/v1/admin/users", new { email = "short@example.test", hoTen = "Thiếu mật khẩu", matKhau = "" }, HttpStatusCode.BadRequest);
    int legacyClassId;
    await using (var db = new ApplicationDbContext(dbOptions))
    {
        var saved = await db.DanhSachNguoiDung.SingleAsync(x => x.Ma == userId);
        Check(saved.EmailChuanHoa == "STUDENT@EXAMPLE.TEST" && saved.MaSoSinhVien == "SV001", "Normalized identity fields persisted");
        Check(new PasswordHasher<NguoiDung>().VerifyHashedPassword(saved, saved.MatKhauBam, password) != PasswordVerificationResult.Failed,
            "Stored password verifies through Identity");
        Check(!await db.DanhSachTruong.AnyAsync() && !await db.DanhSachLopSinhVien.AnyAsync(),
            "Self-declared school/class names do not create catalog entities");
        // Danh mục chỉ được seed trực tiếp làm dữ liệu legacy; không gọi lại API đã rút.
        var now = DateTimeOffset.UtcNow;
        var school = new Truong { KyHieu = "LEGACY", Ten = "Trường danh mục cũ", NgayTao = now, NgayCapNhat = now };
        var cohort = new KhoaHoc { Ten = "LEGACY-K2026", NamBatDau = 2026, NamKetThuc = 2030, NgayTao = now, NgayCapNhat = now };
        var faculty = new Khoa { Truong = school, KyHieu = "IT", Ten = "Khoa cũ", NgayTao = now, NgayCapNhat = now };
        var legacyClass = new LopSinhVien { Khoa = faculty, KhoaHoc = cohort, Ten = "Lớp danh mục cũ", NgayTao = now, NgayCapNhat = now };
        db.DanhSachLopSinhVien.Add(legacyClass); await db.SaveChangesAsync();
        legacyClassId = legacyClass.Ma; saved.MaLop = legacyClassId; await db.SaveChangesAsync();
    }
    await Put($"/api/v1/admin/users/{userId}", new { email = "student@example.test", hoTen = "Đã sửa", tenTruong = "Trường mới",
        tenLop = (string?)null, maSoSinhVien = (string?)null, nganhHoc = "CNTT", dangHoatDong = false });
    var inactive = await Get($"/api/v1/admin/users/{userId}");
    Check(!inactive.GetProperty("dangHoatDong").GetBoolean(), "Explicit false persists");
    Check(inactive.GetProperty("tenTruong").GetString() == "Trường mới" && inactive.GetProperty("tenLop").ValueKind == JsonValueKind.Null,
        "PUT persists the self-declared school/class fields");
    await using (var db = new ApplicationDbContext(dbOptions))
        Check((await db.DanhSachNguoiDung.SingleAsync(x => x.Ma == userId)).MaLop == legacyClassId,
            "Updating the core profile preserves the legacy class FK instead of silently clearing it");
    var page = await Get("/api/v1/admin/users?trang=1&kichThuocTrang=1&dangHoatDong=false&tuKhoa=student");
    Check(page.GetProperty("tongSo").GetInt32() == 1 && page.GetProperty("danhSach").GetArrayLength() == 1 &&
        page.GetProperty("trang").GetInt32() == 1 && page.GetProperty("kichThuocTrang").GetInt32() == 1,
        "Filtering/pagination contract");
    await Get("/api/v1/admin/users?kichThuocTrang=101", HttpStatusCode.BadRequest);
    await Get("/api/v1/admin/users?trang=2147483647&kichThuocTrang=100", HttpStatusCode.BadRequest);
    var activated = await Send(HttpMethod.Patch, $"/api/v1/admin/users/{userId}/activate", null, HttpStatusCode.OK);
    Check(activated.GetProperty("thongBao").GetString() == "Kích hoạt tài khoản thành công." && activated.GetProperty("dangHoatDong").GetBoolean(), "Activation returns success message and state");
    Check((await Get($"/api/v1/admin/users/{userId}")).GetProperty("dangHoatDong").GetBoolean(), "Dedicated activation endpoint updates status");
    var deactivated = await Send(HttpMethod.Patch, $"/api/v1/admin/users/{userId}/deactivate", null, HttpStatusCode.OK);
    Check(deactivated.GetProperty("thongBao").GetString() == "Vô hiệu hóa tài khoản thành công." && !deactivated.GetProperty("dangHoatDong").GetBoolean(), "Deactivation returns success message and state");
    Check(!(await Get($"/api/v1/admin/users/{userId}")).GetProperty("dangHoatDong").GetBoolean(), "Deactivation persisted in database");
    await Send(HttpMethod.Patch, "/api/v1/admin/users/999999/activate", null, HttpStatusCode.NotFound);
    using (var retiredActive = await client.PatchAsync($"/api/v1/admin/users/{userId}/active", null))
        Check(retiredActive.StatusCode == HttpStatusCode.NotFound, "Old active route is removed");
    var foreignUserId = Id(await Post("/api/v1/admin/users", new { email = "foreign@example.test", hoTen = "Người khác", matKhau = password }));
    int taskId, foreignChildId;
    await using (var db = new ApplicationDbContext(dbOptions))
    {
        var now = DateTimeOffset.UtcNow;
        var task = new NhiemVu { MaNguoiDung = userId, TieuDe = "Việc trước khi xóa", NgayTao = now, NgayCapNhat = now };
        db.DanhSachNhiemVu.Add(task); await db.SaveChangesAsync(); taskId = task.Ma;
        var foreignChild = new NhiemVu { MaNguoiDung = foreignUserId, MaNhiemVuCha = taskId,
            TieuDe = "Dữ liệu fixture cố ý sai chủ", NgayTao = now, NgayCapNhat = now };
        db.DanhSachNhiemVu.Add(foreignChild); await db.SaveChangesAsync(); foreignChildId = foreignChild.Ma;
        db.DanhSachThongBao.Add(new ThongBao { MaNguoiDung = userId, MaNhiemVu = taskId, NoiDung = "Nhắc thử",
            ThoiDiemNhac = now.AddDays(1), NgayTao = now, NgayCapNhat = now });
        await db.SaveChangesAsync();
        // Ép bước ghi lịch sử lỗi để kiểm tra rollback, chỉ trên DB test do chương trình tạo.
        await db.Database.ExecuteSqlRawAsync("CREATE FUNCTION fail_test_history() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN RAISE EXCEPTION 'test failure'; END $$; CREATE TRIGGER fail_history BEFORE INSERT ON lich_su_nhiem_vu FOR EACH ROW EXECUTE FUNCTION fail_test_history();");
    }
    await Delete($"/api/v1/admin/users/{userId}", HttpStatusCode.Conflict);
    await using (var db = new ApplicationDbContext(dbOptions))
    {
        Check(await db.DanhSachNhiemVu.AnyAsync(x => x.Ma == foreignChildId), "User delete does not cascade another owner's inconsistent child");
        db.DanhSachNhiemVu.Remove(await db.DanhSachNhiemVu.SingleAsync(x => x.Ma == foreignChildId)); await db.SaveChangesAsync();
    }
    await Delete($"/api/v1/admin/users/{foreignUserId}");
    await Delete($"/api/v1/admin/users/{userId}", HttpStatusCode.InternalServerError);
    await using (var db = new ApplicationDbContext(dbOptions))
    {
        Check(await db.DanhSachNguoiDung.AnyAsync(x => x.Ma == userId) && await db.DanhSachNhiemVu.AnyAsync(x => x.Ma == taskId), "Failed history write rolls user/task deletion back");
        await db.Database.ExecuteSqlRawAsync("DROP TRIGGER fail_history ON lich_su_nhiem_vu; DROP FUNCTION fail_test_history();");
        await db.Database.ExecuteSqlRawAsync("CREATE FUNCTION fail_test_user_delete() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN RAISE EXCEPTION 'test failure'; END $$; CREATE TRIGGER fail_user_delete BEFORE DELETE ON nguoi_dung FOR EACH ROW EXECUTE FUNCTION fail_test_user_delete();");
    }
    await Delete($"/api/v1/admin/users/{userId}", HttpStatusCode.InternalServerError);
    await using (var db = new ApplicationDbContext(dbOptions))
    {
        Check(await db.DanhSachNguoiDung.AnyAsync(x => x.Ma == userId) && !await db.DanhSachLichSuNhiemVu.AnyAsync(x => x.MaNhiemVu == taskId),
            "Later delete failure rolls previously saved history back");
        await db.Database.ExecuteSqlRawAsync("DROP TRIGGER fail_user_delete ON nguoi_dung; DROP FUNCTION fail_test_user_delete();");
    }
    await Delete($"/api/v1/admin/users/{userId}");
    await Get($"/api/v1/admin/users/{userId}", HttpStatusCode.NotFound);
    await using (var db = new ApplicationDbContext(dbOptions))
    {
        Check(!await db.DanhSachNhiemVu.AnyAsync(x => x.MaNguoiDung == userId) && !await db.DanhSachThongBao.AnyAsync(x => x.MaNguoiDung == userId), "User delete cascades task and reminder");
        var history = await db.DanhSachLichSuNhiemVu.SingleAsync(x => x.MaNhiemVu == taskId);
        Check(history.MaNguoiDung == null && history.HanhDong == HanhDongLichSuNhiemVu.Deleted && history.DuLieuTruoc != null, "History survives user delete with anonymous owner");
        using var snapshot = JsonDocument.Parse(history.DuLieuTruoc!);
        Check(snapshot.RootElement.GetProperty("ma").GetInt32() == taskId &&
            snapshot.RootElement.GetProperty("tieuDe").GetString() == "Việc trước khi xóa" && !snapshot.RootElement.TryGetProperty("matKhauBam", out _),
            "Deletion history keeps the task snapshot without credential data");
        var adminUser = new NguoiDung { Email = "admin@example.test", EmailChuanHoa = "ADMIN@EXAMPLE.TEST", HoTen = "Admin thử",
            MatKhauBam = "test-only-placeholder", VaiTro = VaiTroNguoiDung.Admin, NgayTao = DateTimeOffset.UtcNow, NgayCapNhat = DateTimeOffset.UtcNow };
        db.DanhSachNguoiDung.Add(adminUser); await db.SaveChangesAsync();
        await Delete($"/api/v1/admin/users/{adminUser.Ma}", HttpStatusCode.Conflict);
        await Send(HttpMethod.Patch, $"/api/v1/admin/users/{adminUser.Ma}/deactivate", null, HttpStatusCode.Conflict);
        await Put($"/api/v1/admin/users/{adminUser.Ma}", new { email = adminUser.Email, hoTen = adminUser.HoTen, dangHoatDong = false }, HttpStatusCode.Conflict);
    }
    var competingCreates = await Task.WhenAll(
        client.PostAsJsonAsync("/api/v1/admin/users", new { email = "RACE@example.test", hoTen = "Trùng đồng thời", matKhau = password }),
        client.PostAsJsonAsync("/api/v1/admin/users", new { email = "race@example.test", hoTen = "Trùng đồng thời", matKhau = password }));
    Check(competingCreates.Count(x => x.StatusCode == HttpStatusCode.Created) == 1 &&
        competingCreates.Count(x => x.StatusCode == HttpStatusCode.Conflict) == 1, "Concurrent unique insert returns one 201 and one 409");
    foreach (var response in competingCreates) response.Dispose();
    int[] adminIds;
    await using (var db = new ApplicationDbContext(dbOptions))
    {
        var secondAdmin = new NguoiDung { Email = "admin2@example.test", EmailChuanHoa = "ADMIN2@EXAMPLE.TEST", HoTen = "Admin thử thứ hai",
            VaiTro = VaiTroNguoiDung.Admin, NgayTao = DateTimeOffset.UtcNow, NgayCapNhat = DateTimeOffset.UtcNow };
        secondAdmin.MatKhauBam = new PasswordHasher<NguoiDung>().HashPassword(secondAdmin, Guid.NewGuid().ToString("N"));
        db.DanhSachNguoiDung.Add(secondAdmin); await db.SaveChangesAsync();
        adminIds = await db.DanhSachNguoiDung.Where(x => x.VaiTro == VaiTroNguoiDung.Admin && x.DangHoatDong).Select(x => x.Ma).ToArrayAsync();
    }
    var competingDeletes = await Task.WhenAll(client.DeleteAsync($"/api/v1/admin/users/{adminIds[0]}"), client.DeleteAsync($"/api/v1/admin/users/{adminIds[1]}"));
    Check(competingDeletes.Count(x => x.StatusCode == HttpStatusCode.NoContent) == 1 &&
        competingDeletes.Count(x => x.StatusCode == HttpStatusCode.Conflict) == 1,
        "Concurrent admin deletion retains one active administrator (" + string.Join(",", competingDeletes.Select(x => (int)x.StatusCode)) + ")");
    foreach (var response in competingDeletes) response.Dispose();
    await using (var db = new ApplicationDbContext(dbOptions))
        Check(await db.DanhSachNguoiDung.CountAsync(x => x.VaiTro == VaiTroNguoiDung.Admin && x.DangHoatDong) == 1, "Last active admin remains in PostgreSQL");
    var document = await Get("/swagger/v1/swagger.json");
    Check(document.GetProperty("paths").TryGetProperty("/api/v1/admin/users/{id}", out _), "CRUD appears in OpenAPI");
    Check(document.GetProperty("paths").EnumerateObject().All(path => !path.Name.StartsWith("/api/v1/catalogs/", StringComparison.Ordinal) &&
        !new[] { "/api/v1/admin/schools", "/api/v1/admin/faculties", "/api/v1/admin/cohorts", "/api/v1/admin/student-classes" }
            .Any(retired => path.Name == retired || path.Name.StartsWith(retired + "/", StringComparison.Ordinal))),
        "Retired catalogs are absent from OpenAPI, not merely hidden behind authorization");
    var userSchema = document.GetProperty("components").GetProperty("schemas").GetProperty("NguoiDungResponse").GetProperty("properties");
    Check(userSchema.TryGetProperty("tenTruong", out _) && userSchema.TryGetProperty("tenLop", out _) && !userSchema.TryGetProperty("maLop", out _),
        "Swagger exposes self-declared profile names without the legacy class ID");
    Check(!document.GetRawText().Contains("matKhauBam", StringComparison.OrdinalIgnoreCase), "OpenAPI excludes PasswordHash");
    await app.StopAsync(); await app.DisposeAsync(); app = null;
    app = await StartAsync("Production");
    using var production = Client(app);
    await CheckRetiredCatalogRoutes(production, "Anonymous");
    foreach (var resource in new[] { "users" })
    {
        foreach (var method in new[] { HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Delete })
        {
            var path = "/api/v1/admin/" + resource + (method == HttpMethod.Put || method == HttpMethod.Delete ? "/1" : "");
            using var request = new HttpRequestMessage(method, path) { Content = JsonContent.Create(new { }) };
            using var response = await production.SendAsync(request);
            Check(response.StatusCode == HttpStatusCode.Unauthorized, "No unauthenticated CRUD outside Development: " + method + " " + resource);
        }
    }
    foreach (var action in new[] { "activate", "deactivate" })
    {
        using var blockedActive = await production.PatchAsync("/api/v1/admin/users/1/" + action, null);
        Check(blockedActive.StatusCode == HttpStatusCode.Unauthorized, "Unauthenticated request blocked: " + action);
    }
    Console.WriteLine($"All {checks} PostgreSQL/HTTP checks passed.");
}
finally
{
    if (app != null) { await app.StopAsync(); await app.DisposeAsync(); }
    NpgsqlConnection.ClearAllPools();
    // Chỉ xóa DB tên ngẫu nhiên do chính lượt test tạo, không xóa database được cấu hình sẵn.
    await using var drop = new NpgsqlCommand($"DROP DATABASE \"{databaseName}\" WITH (FORCE)", admin);
    await drop.ExecuteNonQueryAsync();
}
