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
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;
using StudyPlanner.Application.HocTap;
using StudyPlanner.Application.UuTien;
using StudyPlanner.Infrastructure.Data;
using StudyPlanner.Infrastructure.UuTien;

public static class IntegrationChecks
{
    public static async Task RunAsync()
    {
        var supplied = Environment.GetEnvironmentVariable("STUDYPLANNER_TEST_POSTGRES");
        if (string.IsNullOrWhiteSpace(supplied)) { Console.WriteLine("SKIP priority PostgreSQL/HTTP checks: isolated localhost test database not configured."); return; }
        var connection = new NpgsqlConnectionStringBuilder(supplied);
        if (connection.Host is not ("127.0.0.1" or "localhost") || connection.Database != "postgres") throw new Exception("Only isolated localhost postgres test admin database is allowed.");
        var name = "studyplanner_priority_" + Guid.NewGuid().ToString("N");
        await using var adminConnection = new NpgsqlConnection(connection.ConnectionString); await adminConnection.OpenAsync();
        await using (var command = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", adminConnection)) await command.ExecuteNonQueryAsync();
        connection.Database = name;
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(connection.ConnectionString).Options;
        void Check(bool value, string label) { if (!value) throw new Exception("FAIL: " + label); }
        try
        {
            int adminId, studentId;
            await using (var db = new ApplicationDbContext(options))
            {
                await db.Database.MigrateAsync();
                NguoiDung User(string email, VaiTroNguoiDung role) => new() { Email = email, EmailChuanHoa = email.ToUpperInvariant(), HoTen = email, VaiTro = role, MatKhauBam = "test-only", NgayTao = DateTimeOffset.UtcNow, NgayCapNhat = DateTimeOffset.UtcNow };
                var admin = User("admin@priority.test", VaiTroNguoiDung.Admin); var student = User("student@priority.test", VaiTroNguoiDung.Student);
                db.AddRange(admin, student); await db.SaveChangesAsync(); adminId = admin.Ma; studentId = student.Ma;
            }
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
            builder.Logging.ClearProviders(); builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Configuration["Auth:SigningKey"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            builder.Services.AddScoped(_ => new ApplicationDbContext(options));
            builder.Services.AddQuanTri(); builder.Services.AddStudyPlannerXacThuc(builder.Configuration, builder.Environment);
            builder.Services.AddScoped<QuyTacUuTienService>();
            builder.Services.AddControllers().AddApplicationPart(typeof(StudyPlanner.Api.Controllers.Administration.QuyTacUuTienController).Assembly);
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
                const string route = "/api/v1/admin/priority-rules";
                Check((await anonymous.GetAsync(route)).StatusCode == HttpStatusCode.Unauthorized, "Admin list requires JWT");
                Check((await studentClient.GetAsync(route)).StatusCode == HttpStatusCode.Forbidden, "Student cannot read admin history");
                Check((await studentClient.PostAsJsonAsync(route, new { ten = "Attempt", cauHinh = QuyTacCauHinhUuTien.MacDinh() })).StatusCode == HttpStatusCode.Forbidden, "Student cannot write config");
                var initial = await studentClient.GetFromJsonAsync<QuyTacUuTienResponse>("/api/v1/priority-rules/active");
                Check(initial?.PhienBan == 0 && initial.CauHinh.TieuChi[0] == "Deadline", "Built-in fallback before activation");
                Check((await client.PostAsJsonAsync(route, new { ten = " ", cauHinh = QuyTacCauHinhUuTien.MacDinh() })).StatusCode == HttpStatusCode.BadRequest, "Blank name rejected");
                Check((await client.PostAsJsonAsync(route, new { ten = "Bad", cauHinh = new { phienBanSchema = 1, tieuChi = new[] { "Importance" } } })).StatusCode == HttpStatusCode.BadRequest, "Missing deadline rejected");
                Check((await client.PostAsJsonAsync(route, new { ten = "Bad", cauHinh = new { phienBanSchema = 1, tieuChi = new[] { "Deadline", "script" } } })).StatusCode == HttpStatusCode.BadRequest, "Unknown criterion rejected");
                async Task<QuyTacUuTienResponse> Create(string title, string[] order)
                {
                    var response = await client.PostAsJsonAsync(route, new { ten = title, cauHinh = new { phienBanSchema = 1, tieuChi = order } });
                    Check(response.StatusCode == HttpStatusCode.Created, "Create returns 201");
                    return (await response.Content.ReadFromJsonAsync<QuyTacUuTienResponse>())!;
                }
                var first = await Create("Theo hạn", ["Deadline", "Importance", "InProgress"]);
                var second = await Create("Theo quan trọng", ["Importance", "Deadline"]);
                Check(!first.DangSuDung && !second.DangSuDung && second.PhienBan > first.PhienBan, "Saving new versions never activates implicitly");
                var response = await client.PostAsJsonAsync($"{route}/{first.Ma}/activate", new { phienBanDangDung = 0 });
                Check(response.IsSuccessStatusCode, "Activate first version");
                var active = await studentClient.GetFromJsonAsync<QuyTacUuTienResponse>("/api/v1/priority-rules/active");
                Check(active?.Ma == first.Ma, "Student reads active configuration");
                Check((await client.DeleteAsync($"{route}/{first.Ma}")).StatusCode == HttpStatusCode.Conflict, "Active version cannot be deleted");
                Check((await client.PostAsJsonAsync($"{route}/{second.Ma}/activate", new { phienBanDangDung = 0 })).StatusCode == HttpStatusCode.Conflict, "Stale activation cannot overwrite a newer decision");
                var third = await Create("Theo trạng thái", ["InProgress", "Deadline"]);
                var concurrent = await Task.WhenAll(
                    client.PostAsJsonAsync($"{route}/{second.Ma}/activate", new { phienBanDangDung = first.PhienBan }),
                    client.PostAsJsonAsync($"{route}/{third.Ma}/activate", new { phienBanDangDung = first.PhienBan }));
                Check(concurrent.Count(x => x.IsSuccessStatusCode) == 1 && concurrent.Count(x => x.StatusCode == HttpStatusCode.Conflict) == 1, "Concurrent admins cannot both overwrite the active snapshot");
                await using (var db = new ApplicationDbContext(options))
                {
                    Check(await db.DanhSachQuyTacUuTien.CountAsync(x => x.DangSuDung) == 1, "Exactly one active row");
                    Check(await db.DanhSachNguoiDung.AllAsync(x => x.PhienBanLapLich == 0), "No student task/scheduling revision changes");
                }
                var after = await studentClient.GetFromJsonAsync<QuyTacUuTienResponse>("/api/v1/priority-rules/active");
                Check(after?.Ma == second.Ma || after?.Ma == third.Ma, "Active survives new request/service context");
                Check((await client.DeleteAsync($"{route}/{first.Ma}")).StatusCode == HttpStatusCode.NoContent, "Inactive version can be deleted");
                var draft = await Create("Bản bỏ", ["Deadline"]); await client.DeleteAsync($"{route}/{draft.Ma}");
                var next = await Create("Phiên bản tiếp", ["Deadline"]);
                Check(next.PhienBan > draft.PhienBan, "Deleted highest version is never reused");
                var page = await client.GetFromJsonAsync<TrangQuyTacResponse>(route + "?trang=1&kichThuocTrang=2");
                Check(page?.DanhSach.Count == 2 && page.TongSo >= 3, "History pagination");
                Check((await client.GetAsync(route + "?trang=0")).StatusCode == HttpStatusCode.BadRequest, "Invalid pagination rejected");
                Check((await anonymous.GetAsync("/api/v1/priority-rules/active")).StatusCode == HttpStatusCode.Unauthorized, "Active rule requires authentication");
                Console.WriteLine("PASS: priority PostgreSQL/HTTP JWT roles, validation, immutable drafts, activation, global concurrency, persistence, version allocation, safe deletion and pagination.");
            }
            finally { await app.StopAsync(); }
        }
        finally
        {
            NpgsqlConnection.ClearAllPools();
            await using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{name}\" WITH (FORCE)", adminConnection); await command.ExecuteNonQueryAsync();
        }
    }
}
