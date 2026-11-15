using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using StudyPlanner.Api.Administration;
using StudyPlanner.Api.Auth;
using StudyPlanner.Application.Administration;
using StudyPlanner.Application.NenTang;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;
using StudyPlanner.Infrastructure.Data;
using StudyPlanner.Infrastructure.NenTang;

void Check(bool value, string name) { if (!value) throw new Exception("FAIL: " + name); Console.WriteLine("PASS: " + name); }
void Reject(Action action) { try { action(); throw new Exception("Expected invalid input"); } catch (QuanTriException e) { Check(e.LoaiLoi == LoaiLoiNghiepVu.InvalidInput, "Invalid catalog input rejected"); } }
Check(QuyTacDanhMucMau.KyHieu(" cs_01 ") == "CS_01", "Trim and normalize codes");
Reject(() => QuyTacDanhMucMau.HocKy(new(" ", 1, null)));
Reject(() => QuyTacDanhMucMau.HocKy(new("Học kỳ", 0, null)));
Reject(() => QuyTacDanhMucMau.MonHoc(new(1, "CODE", "Tên", 0, null)));
Reject(() => QuyTacDanhMucMau.MonHoc(new(1, "CODE", "Tên", 31, null)));
Reject(() => QuyTacDanhMucMau.MonHoc(new(1, "<code>", "Tên", 3, null)));
await JsonChecks.RunAsync();
var seedPath = Path.Combine(AppContext.BaseDirectory, "Administration/Data/danh-muc-mau.seed.json");
var filePath = Path.Combine(Path.GetTempPath(), "studyplanner-catalog-http-" + Guid.NewGuid().ToString("N"), "catalog.json");
var jsonStore = new DanhMucJsonStore(filePath, seedPath);
var supplied = Environment.GetEnvironmentVariable("STUDYPLANNER_TEST_POSTGRES");
if (string.IsNullOrWhiteSpace(supplied)) { Console.WriteLine("SKIP: set isolated localhost STUDYPLANNER_TEST_POSTGRES for database/HTTP checks."); return; }
var adminConfig = new NpgsqlConnectionStringBuilder(supplied);
if (adminConfig.Host is not ("127.0.0.1" or "localhost") || adminConfig.Database != "postgres") throw new Exception("Only isolated localhost postgres admin database is allowed");
var dbName = "studyplanner_catalog_" + Guid.NewGuid().ToString("N");
await using var connection = new NpgsqlConnection(adminConfig.ConnectionString); await connection.OpenAsync();
await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{dbName}\"", connection)) await create.ExecuteNonQueryAsync();
adminConfig.Database = dbName;
var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(adminConfig.ConnectionString).Options;
try
{
    int adminId, studentId;
    await using (var db = new ApplicationDbContext(options))
    {
        await db.Database.MigrateAsync();
        Check((await new DuLieuNenTangService(jsonStore).HocKyAsync(default)).Count == 2, "First file use initializes two sample semesters from the seed");
        Check(!db.Database.HasPendingModelChanges(), "Migration snapshot matches current model");
        var admin = new NguoiDung { Email = "admin@catalog.test", EmailChuanHoa = "ADMIN@CATALOG.TEST", HoTen = "Admin", VaiTro = VaiTroNguoiDung.Admin, MatKhauBam = "test-only", NgayTao = DateTimeOffset.UtcNow, NgayCapNhat = DateTimeOffset.UtcNow };
        var student = new NguoiDung { Email = "student@catalog.test", EmailChuanHoa = "STUDENT@CATALOG.TEST", HoTen = "Student", VaiTro = VaiTroNguoiDung.Student, MatKhauBam = "test-only", NgayTao = DateTimeOffset.UtcNow, NgayCapNhat = DateTimeOffset.UtcNow };
        db.AddRange(admin, student); await db.SaveChangesAsync(); adminId = admin.Ma; studentId = student.Ma;
    }
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
    builder.Logging.ClearProviders(); builder.WebHost.UseUrls("http://127.0.0.1:0");
    builder.Configuration["Auth:SigningKey"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    builder.Services.AddScoped(_ => new ApplicationDbContext(options));
    builder.Services.AddQuanTri(); builder.Services.AddStudyPlannerXacThuc(builder.Configuration, builder.Environment);
    builder.Services.AddScoped<DuLieuNenTangService>();
    builder.Services.AddSingleton(jsonStore);
    builder.Services.AddControllers().AddApplicationPart(typeof(StudyPlanner.Api.Controllers.Administration.DuLieuNenTangController).Assembly);
    await using var app = builder.Build(); app.UseExceptionHandler(); app.UseAuthentication(); app.UseAuthorization(); app.UseRateLimiter(); app.MapControllers();
    await app.StartAsync();
    try
    {
        var origin = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        using var client = new HttpClient { BaseAddress = new(origin) };
        using var studentClient = new HttpClient { BaseAddress = new(origin) };
        using var anonymous = new HttpClient { BaseAddress = new(origin) };
        await using (var db = new ApplicationDbContext(options))
        {
            var issuer = app.Services.GetRequiredService<JwtTokenIssuer>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", issuer.Issue(await db.DanhSachNguoiDung.SingleAsync(x => x.Ma == adminId)).TokenTruyCap);
            studentClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", issuer.Issue(await db.DanhSachNguoiDung.SingleAsync(x => x.Ma == studentId)).TokenTruyCap);
        }
        const string route = "/api/v1/admin/catalog";
        Check((await anonymous.GetAsync(route + "/semesters")).StatusCode == HttpStatusCode.Unauthorized, "Anonymous blocked");
        Check((await studentClient.GetAsync(route + "/courses")).StatusCode == HttpStatusCode.Forbidden, "Student cannot read admin catalog");
        Check((await studentClient.PostAsJsonAsync(route + "/semesters", new { ten = "Test", thuTu = 3 })).StatusCode == HttpStatusCode.Forbidden, "Student cannot write admin catalog");
        const string suggestions = "/api/v1/catalog/courses";
        Check((await anonymous.GetAsync(suggestions)).StatusCode == HttpStatusCode.Unauthorized, "Course suggestions require authentication");
        var studentSuggestions = (await studentClient.GetFromJsonAsync<PhanTrangResponse<StudyPlanner.Api.Controllers.GoiYMonHocResponse>>(suggestions))!;
        Check(studentSuggestions.TongSo == 12 && studentSuggestions.DanhSach.Count == 12, "Students can read templates across all sample semesters");
        var filteredSuggestions = (await studentClient.GetFromJsonAsync<PhanTrangResponse<StudyPlanner.Api.Controllers.GoiYMonHocResponse>>(suggestions + "?tuKhoa=CSDL01"))!;
        Check(filteredSuggestions.TongSo == 1 && filteredSuggestions.DanhSach[0].SoTinChi == 3, "Suggestion search returns the template code, name and credits");
        Check((await studentClient.PostAsJsonAsync(suggestions, new { ten = "Cannot write", soTinChi = 1 })).StatusCode == HttpStatusCode.MethodNotAllowed, "Suggestion route exposes no write action");
        Check((await studentClient.GetAsync(suggestions + "?trang=0")).StatusCode == HttpStatusCode.BadRequest, "Suggestion pagination validated");
        var terms = (await client.GetFromJsonAsync<HocKyMauResponse[]>(route + "/semesters"))!;
        Check(terms[0].SoMonHoc == 6 && terms[0].TongTinChi == 16, "Semester summaries aggregate actual courses");
        Check((await client.PostAsJsonAsync(route + "/semesters", new { ten = " học kỳ 1 ", thuTu = 1 })).StatusCode == HttpStatusCode.Conflict, "Normalized duplicate semester rejected");
        var created = await client.PostAsJsonAsync(route + "/semesters", new { ten = " Kỳ kiểm thử ", thuTu = 3 });
        Check(created.StatusCode == HttpStatusCode.Created, "Create semester returns 201");
        var term = (await created.Content.ReadFromJsonAsync<HocKyMauResponse>())!;
        Check(term.Ma > 2 && term.Ten == "Kỳ kiểm thử", "Seed identity sequence and trimming");
        var saved = await client.PostAsJsonAsync(route + "/courses", new { maHocKyMau = term.Ma, kyHieu = " test01 ", ten = " Môn thử ", soTinChi = 4 });
        Check(saved.StatusCode == HttpStatusCode.Created, "Create course returns 201");
        var course = (await saved.Content.ReadFromJsonAsync<MonHocMauResponse>())!;
        Check(course.Ma > 12 && course.KyHieu == "TEST01", "Course identity sequence and normalized code");
        Check((await client.PostAsJsonAsync(route + "/courses", new { maHocKyMau = term.Ma, kyHieu = "test01", ten = "Trùng", soTinChi = 3 })).StatusCode == HttpStatusCode.Conflict, "Duplicate code within semester rejected");
        Check((await client.PostAsJsonAsync(route + "/courses", new { maHocKyMau = term.Ma, kyHieu = "ZERO", ten = "Sai", soTinChi = 0 })).StatusCode == HttpStatusCode.BadRequest, "Zero credits rejected over HTTP");
        Check((await client.PostAsJsonAsync(route + "/courses", new { maHocKyMau = 99999, kyHieu = "GHOST", ten = "Sai", soTinChi = 3 })).StatusCode == HttpStatusCode.NotFound, "Missing semester cannot accept courses");
        Check((await client.DeleteAsync($"{route}/semesters/{term.Ma}?phienBan={term.PhienBan}")).StatusCode == HttpStatusCode.Conflict, "Nonempty semester deletion blocked");
        Check((await client.PutAsJsonAsync($"{route}/courses/{course.Ma}", new { maHocKyMau = term.Ma, kyHieu = course.KyHieu, ten = "Đổi", soTinChi = 5 })).StatusCode == HttpStatusCode.BadRequest, "Missing edit version rejected");
        var concurrent = await Task.WhenAll(Enumerable.Range(0, 2).Select(index => client.PutAsJsonAsync($"{route}/courses/{course.Ma}", new { maHocKyMau = term.Ma, kyHieu = course.KyHieu, ten = "Cập nhật " + index, soTinChi = 5, phienBan = 1 })));
        Check(concurrent.Count(x => x.IsSuccessStatusCode) == 1 && concurrent.Count(x => x.StatusCode == HttpStatusCode.Conflict) == 1, "Concurrent admins cannot overwrite the same version");
        var page = (await client.GetFromJsonAsync<PhanTrangResponse<MonHocMauResponse>>($"{route}/courses?maHocKyMau={term.Ma}&tuKhoa=TEST01"))!;
        Check(page.TongSo == 1 && page.DanhSach.Single().SoTinChi == 5, "Filter/search and saved state persist across requests");
        Check((await client.DeleteAsync($"{route}/courses/{course.Ma}?phienBan=1")).StatusCode == HttpStatusCode.Conflict, "Stale deletion rejected");
        Check((await client.GetAsync(route + "/courses?trang=0")).StatusCode == HttpStatusCode.BadRequest, "Invalid pagination rejected");
        for (var index = 0; index < 21; index++)
            Check((await client.PostAsJsonAsync(route + "/courses", new { maHocKyMau = term.Ma, kyHieu = "PAGE" + index, ten = "Phân trang " + index, soTinChi = 3 })).IsSuccessStatusCode, "Create paginated row");
        var second = (await client.GetFromJsonAsync<PhanTrangResponse<MonHocMauResponse>>($"{route}/courses?maHocKyMau={term.Ma}&trang=2"))!;
        Check(second.TongSo == 22 && second.DanhSach.Count == 2, "Second page uses full filtered total");
        var moreSuggestions = (await studentClient.GetFromJsonAsync<PhanTrangResponse<StudyPlanner.Api.Controllers.GoiYMonHocResponse>>(suggestions + "?tuKhoa=PAGE&trang=2"))!;
        Check(moreSuggestions.TongSo == 21 && moreSuggestions.DanhSach.Count == 1, "Student suggestions support searching beyond the first page");
        foreach (var row in (await client.GetFromJsonAsync<PhanTrangResponse<MonHocMauResponse>>($"{route}/courses?maHocKyMau={term.Ma}"))!.DanhSach.Concat(second.DanhSach))
            Check((await client.DeleteAsync($"{route}/courses/{row.Ma}?phienBan={row.PhienBan}")).StatusCode == HttpStatusCode.NoContent, "Delete course returns 204");
        Check((await client.DeleteAsync($"{route}/semesters/{term.Ma}?phienBan=1")).StatusCode == HttpStatusCode.NoContent, "Empty semester can be deleted");
        await using var verify = new ApplicationDbContext(options);
        Check((await new DuLieuNenTangService(new DanhMucJsonStore(filePath, seedPath)).MonHocAsync(null, null, 1, default)).TongSo == 12 && !await verify.DanhSachHocKy.AnyAsync() && !await verify.DanhSachMonHoc.AnyAsync() && await verify.DanhSachNguoiDung.AllAsync(x => x.PhienBanLapLich == 0), "JSON catalog survives new instances and never mutates student academic data");
        Check(verify.Model.GetEntityTypes().All(x => x.GetTableName() is not ("hoc_ky_mau" or "mon_hoc_mau")), "Current model creates no catalog tables");
        await TransferChecks.RunAsync(adminConfig.ConnectionString, seedPath);
    }
    finally { await app.StopAsync(); }
}
finally
{
    NpgsqlConnection.ClearAllPools();
    await using var drop = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{dbName}\" WITH (FORCE)", connection); await drop.ExecuteNonQueryAsync();
    if (Directory.Exists(Path.GetDirectoryName(filePath))) Directory.Delete(Path.GetDirectoryName(filePath)!, true);
}
