using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Net.Http.Json;
using System.Text.Json;
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
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;
using StudyPlanner.Infrastructure.Administration;
using StudyPlanner.Infrastructure.Data;

var supplied = Environment.GetEnvironmentVariable("STUDYPLANNER_TEST_POSTGRES");
if (string.IsNullOrWhiteSpace(supplied)) { Console.WriteLine("SKIP overview PostgreSQL/HTTP checks: isolated localhost test database not configured."); return; }
var config = new NpgsqlConnectionStringBuilder(supplied);
if (config.Host is not ("localhost" or "127.0.0.1") || config.Database != "postgres") throw new Exception("Only isolated localhost postgres admin database is allowed.");
var name = "studyplanner_overview_" + Guid.NewGuid().ToString("N");
await using var adminConnection = new NpgsqlConnection(config.ConnectionString); await adminConnection.OpenAsync();
await using (var command = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", adminConnection)) await command.ExecuteNonQueryAsync();
config.Database = name;
var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(config.ConnectionString).Options;
var clock = new TestClock();
void Check(bool condition, string label) { if (!condition) throw new Exception("FAIL: " + label); }
try
{
    int adminId, studentId;
    var ids = new List<int>();
    await using (var db = new ApplicationDbContext(options))
    {
        await db.Database.MigrateAsync();
        var empty = await new TongQuanAdminService(db, clock).LayAsync(default);
        Check(empty.TongSinhVien == 0 && empty.DangMo == 0 && empty.DaKhoa == 0 && empty.MoiTrong7Ngay == 0 && empty.SinhVienMoi.Count == 0, "Empty statistics are real zeros");
        NguoiDung User(string email, VaiTroNguoiDung role, DateTimeOffset created, bool active = true) => new() { Email = email, EmailChuanHoa = email.ToUpperInvariant(), HoTen = email, VaiTro = role, DangHoatDong = active, MatKhauBam = "test-only", NgayTao = created, NgayCapNhat = clock.GetUtcNow().AddDays(1) };
        var admin = User("admin@overview.test", VaiTroNguoiDung.Admin, clock.GetUtcNow());
        db.AddRange(admin, User("admin2@overview.test", VaiTroNguoiDung.Admin, clock.GetUtcNow().AddHours(1)));
        await db.SaveChangesAsync(); adminId = admin.Ma;
        var from = clock.GetUtcNow().AddDays(-7);
        for (var i = 0; i < 34; i++)
        {
            var created = i < 7 ? clock.GetUtcNow().AddHours(-(i == 4 ? 3 : i)) : i == 7 ? from : i == 8 ? from.AddMilliseconds(-1) : from.AddDays(-i);
            var user = User($"student-{i}@overview.test", VaiTroNguoiDung.Student, created, i % 3 != 0);
            db.Add(user); await db.SaveChangesAsync(); ids.Add(user.Ma);
        }
        studentId = ids[1];
    }
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
    builder.Logging.ClearProviders(); builder.WebHost.UseUrls("http://127.0.0.1:0");
    builder.Configuration["Auth:SigningKey"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    builder.Services.AddScoped(_ => new ApplicationDbContext(options));
    builder.Services.AddQuanTri(); builder.Services.AddStudyPlannerXacThuc(builder.Configuration, builder.Environment);
    builder.Services.AddSingleton<TimeProvider>(clock); builder.Services.AddScoped<TongQuanAdminService>();
    builder.Services.AddControllers().AddApplicationPart(typeof(StudyPlanner.Api.Controllers.Administration.TongQuanAdminController).Assembly);
    await using var app = builder.Build(); app.UseExceptionHandler(); app.UseAuthentication(); app.UseAuthorization(); app.UseRateLimiter(); app.MapControllers();
    await app.StartAsync();
    try
    {
        var origin = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        using var client = new HttpClient { BaseAddress = new(origin) }; using var studentClient = new HttpClient { BaseAddress = new(origin) }; using var anonymous = new HttpClient { BaseAddress = new(origin) };
        await using (var db = new ApplicationDbContext(options))
        {
            var issuer = app.Services.GetRequiredService<JwtTokenIssuer>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", issuer.Issue(await db.DanhSachNguoiDung.SingleAsync(x => x.Ma == adminId)).TokenTruyCap);
            studentClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", issuer.Issue(await db.DanhSachNguoiDung.SingleAsync(x => x.Ma == studentId)).TokenTruyCap);
        }
        Check((await anonymous.GetAsync("/api/v1/admin/overview")).StatusCode == HttpStatusCode.Unauthorized, "Anonymous rejected");
        Check((await studentClient.GetAsync("/api/v1/admin/overview")).StatusCode == HttpStatusCode.Forbidden, "Student rejected");
        var summary = (await client.GetFromJsonAsync<TongQuanAdminResponse>("/api/v1/admin/overview"))!;
        Check(summary.TongSinhVien == 34 && summary.DangMo == 22 && summary.DaKhoa == 12 && summary.MoiTrong7Ngay == 8, "Aggregates count all students, exclude admins and use the exact inclusive seven-day boundary");
        Check(summary.SinhVienMoi.Select(x => x.Ma).SequenceEqual(new[] { ids[0], ids[1], ids[2], ids[4], ids[3] }), "Newest five sort by creation then ID descending");
        var dates = "&TaoTu=" + Uri.EscapeDataString(summary.TuNgay.ToString("O")) + "&TaoDen=" + Uri.EscapeDataString(summary.DenNgay.ToString("O"));
        async Task<int> Total(string query)
        {
            var result = await client.GetFromJsonAsync<JsonElement>("/api/v1/admin/users?KichThuocTrang=5&" + query);
            return result.GetProperty("tongSo").GetInt32();
        }
        Check(await Total("VaiTro=Student") == 34, "Total metric drill-down matches across pagination");
        Check(await Total("VaiTro=Student&DangHoatDong=true") == 22 && await Total("VaiTro=Student&DangHoatDong=false") == 12, "Status metric drill-down matches");
        Check(await Total("VaiTro=Student" + dates) == 8, "Recent metric drill-down matches the exact server cutoff");
        Check(await Total("VaiTro=Admin") == 2, "Existing account list still supports admins");
        Check((await client.GetAsync("/api/v1/admin/users?VaiTro=Owner")).StatusCode == HttpStatusCode.BadRequest, "Unknown role rejected");
        Check((await client.GetAsync("/api/v1/admin/users?TaoTu=2026-11-13T00:00:00Z&TaoDen=2026-11-12T00:00:00Z")).StatusCode == HttpStatusCode.BadRequest, "Reversed date range rejected");
        Check((await client.GetAsync("/api/v1/admin/users?TaoTu=bad")).StatusCode == HttpStatusCode.BadRequest, "Malformed date rejected");
        var text = await client.GetStringAsync("/api/v1/admin/overview");
        Check(!text.Contains("matKhau", StringComparison.OrdinalIgnoreCase) && !text.Contains("test-only"), "No credential fields in overview");
        await using (var db = new ApplicationDbContext(options))
        {
            var user = await db.DanhSachNguoiDung.SingleAsync(x => x.Ma == ids[1]); user.DangHoatDong = false; await db.SaveChangesAsync();
        }
        var reloaded = (await client.GetFromJsonAsync<TongQuanAdminResponse>("/api/v1/admin/overview"))!;
        Check(reloaded.DangMo == 21 && reloaded.DaKhoa == 13 && !reloaded.SinhVienMoi.Single(x => x.Ma == ids[1]).DangHoatDong, "Reload reflects account state changes");
        Console.WriteLine("PASS: overview PostgreSQL/HTTP JWT access, aggregate counts beyond page size, empty state, seven-day boundary, latest-five ordering, safe DTO, reload and matching role/date/status drill-down filters.");
    }
    finally { await app.StopAsync(); }
}
finally
{
    NpgsqlConnection.ClearAllPools();
    await using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{name}\" WITH (FORCE)", adminConnection); await command.ExecuteNonQueryAsync();
}
sealed class TestClock : TimeProvider { public override DateTimeOffset GetUtcNow() => new(2026, 11, 12, 3, 0, 0, TimeSpan.Zero); }
