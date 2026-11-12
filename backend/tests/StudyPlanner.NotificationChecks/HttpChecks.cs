using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StudyPlanner.Api.Administration;
using StudyPlanner.Api.Auth;
using StudyPlanner.Api.HocTap;
using StudyPlanner.Infrastructure.Data;
using StudyPlanner.Infrastructure.CanhBao;

public static class HttpChecks
{
    public static async Task RunAsync(DbContextOptions<ApplicationDbContext> options, TimeProvider clock, int owner, int stranger)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
        builder.Logging.ClearProviders(); builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Configuration["Auth:SigningKey"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        builder.Services.AddScoped(_ => new ApplicationDbContext(options));
        builder.Services.AddQuanTri(); builder.Services.AddHocTap();
        builder.Services.AddStudyPlannerXacThuc(builder.Configuration, builder.Environment);
        builder.Services.AddSingleton(clock); builder.Services.AddScoped<ThongBaoService>();
        builder.Services.AddControllers().AddApplicationPart(typeof(StudyPlanner.Api.Controllers.ThongBaoController).Assembly);
        await using var app = builder.Build(); app.UseExceptionHandler(); app.UseAuthentication(); app.UseAuthorization(); app.UseRateLimiter(); app.MapControllers();
        await app.StartAsync();
        try
        {
            var origin = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            using var client = new HttpClient { BaseAddress = new(origin) };
            void Check(bool value, string label) { if (!value) throw new Exception("FAIL HTTP: " + label); }
            Check((await client.GetAsync("/api/v1/notifications")).StatusCode == HttpStatusCode.Unauthorized, "Anonymous access rejected");
            await using var db = new ApplicationDbContext(options);
            var issuer = app.Services.GetRequiredService<JwtTokenIssuer>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", issuer.Issue(await db.DanhSachNguoiDung.SingleAsync(x => x.Ma == owner)).TokenTruyCap);
            var response = await client.GetAsync("/api/v1/notifications?trang=1&kichThuocTrang=20");
            Check(response.IsSuccessStatusCode, "JWT owner can list notifications");
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            Check(json.GetProperty("tongSo").GetInt32() == 1 && json.GetProperty("muiGio").GetString() == "Asia/Ho_Chi_Minh", "Vietnamese DTO contract");
            var row = json.GetProperty("danhSach")[0]; var id = row.GetProperty("ma").GetInt32(); var delivered = row.GetProperty("ngayPhat").GetDateTimeOffset();
            Check((await client.GetAsync("/api/v1/notifications?trang=0")).StatusCode == HttpStatusCode.BadRequest, "Invalid page rejected");
            Check((await client.PatchAsJsonAsync($"/api/v1/notifications/{id}/read", new { })).StatusCode == HttpStatusCode.BadRequest, "Missing delivery snapshot rejected");
            Check((await client.PatchAsJsonAsync($"/api/v1/notifications/{id}/read", new { ngayPhat = delivered.AddSeconds(-1) })).StatusCode == HttpStatusCode.Conflict, "Stale delivery snapshot rejected");
            Check((await client.PatchAsJsonAsync($"/api/v1/notifications/{id}/read", new { ngayPhat = delivered })).StatusCode == HttpStatusCode.NoContent, "Read is idempotent 204");
            Check((await client.PostAsJsonAsync("/api/v1/notifications/read-all", new { docDenLuc = clock.GetUtcNow().AddMinutes(1) })).StatusCode == HttpStatusCode.BadRequest, "Future read-all cutoff rejected");
            Check((await client.PostAsJsonAsync("/api/v1/notifications/read-all", new { docDenLuc = json.GetProperty("docDenLuc").GetDateTimeOffset() })).StatusCode == HttpStatusCode.NoContent, "Read-all accepts server cutoff");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", issuer.Issue(await db.DanhSachNguoiDung.SingleAsync(x => x.Ma == stranger)).TokenTruyCap);
            Check((await client.PatchAsJsonAsync($"/api/v1/notifications/{id}/read", new { ngayPhat = delivered })).StatusCode == HttpStatusCode.NotFound, "Other JWT owner cannot read notification");
            Console.WriteLine("PASS: notification HTTP/JWT ownership, DTO, validation, stale snapshot, read and read-all endpoints.");
        }
        finally { await app.StopAsync(); }
    }
}
