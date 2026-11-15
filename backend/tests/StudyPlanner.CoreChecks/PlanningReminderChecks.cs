using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using StudyPlanner.Api.Auth;
using StudyPlanner.Api.Administration;
using StudyPlanner.Api.Controllers;
using StudyPlanner.Api.HocTap;
using StudyPlanner.Application.Auth;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Infrastructure.Data;

namespace StudyPlanner.CoreChecks;

public static class PlanningReminderChecks
{
    public static async Task RunAsync(string supplied)
    {
        var connection = new NpgsqlConnectionStringBuilder(supplied);
        if (connection.Host is not ("127.0.0.1" or "localhost") || connection.Database != "postgres") throw new Exception("Use isolated localhost postgres.");
        var database = "studyplanner_planning_reminders_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(connection.ConnectionString); await admin.OpenAsync();
        await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", admin)) await create.ExecuteNonQueryAsync();
        connection.Database = database;
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(connection.ConnectionString).Options;
        var clock = new KiemThuClock(new DateTimeOffset(2026, 11, 15, 12, 0, 0, TimeSpan.Zero));
        WebApplication? app = null; var checks = 0;
        void Check(bool ok, string label) { if (!ok) throw new Exception("FAIL planning reminder: " + label); checks++; }
        try
        {
            int owner, other;
            await using (var db = new ApplicationDbContext(options)) {
                await db.Database.MigrateAsync();
                NguoiDung User(string email) => new() { Email = email, EmailChuanHoa = email.ToUpperInvariant(), HoTen = email, MatKhauBam = "fixture", MuiGio = "Asia/Ho_Chi_Minh" };
                var a = User("availability@test.invalid"); var b = User("other-availability@test.invalid"); db.AddRange(a, b); await db.SaveChangesAsync(); owner = a.Ma; other = b.Ma;
            }
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
            builder.Logging.ClearProviders(); builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Configuration["Auth:SigningKey"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseNpgsql(connection.ConnectionString));
            builder.Services.AddQuanTri(); builder.Services.AddHocTap(); builder.Services.AddStudyPlannerXacThuc(builder.Configuration, builder.Environment);
            builder.Services.AddScoped<StudyPlanner.Infrastructure.CanhBao.ThongBaoService>();
            builder.Services.AddSingleton<TimeProvider>(clock); builder.Services.AddControllers().AddApplicationPart(typeof(KhungGioRanhController).Assembly);
            app = builder.Build(); app.UseExceptionHandler(); app.UseAuthentication(); app.UseAuthorization(); app.UseRateLimiter(); app.MapControllers(); await app.StartAsync();
            var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            using var client = new HttpClient { BaseAddress = new(address) }; using var foreign = new HttpClient { BaseAddress = new(address) }; using var anonymous = new HttpClient { BaseAddress = new(address) };
            await using (var db = new ApplicationDbContext(options)) {
                var issuer = app.Services.GetRequiredService<JwtTokenIssuer>();
                client.DefaultRequestHeaders.Authorization = new("Bearer", issuer.Issue(await db.DanhSachNguoiDung.FindAsync(owner) ?? throw new Exception()).TokenTruyCap);
                foreign.DefaultRequestHeaders.Authorization = new("Bearer", issuer.Issue(await db.DanhSachNguoiDung.FindAsync(other) ?? throw new Exception()).TokenTruyCap);
            }
            long version = 0;
            async Task<JsonElement> Send(HttpClient c, string route, string method = "GET", object? body = null, int expected = 200, long? baseline = null) {
                using var request = new HttpRequestMessage(new HttpMethod(method), "/api/v1/" + route);
                if (body is not null) request.Content = JsonContent.Create(body);
                if (method != "GET" && !route.EndsWith("change-preview")) request.Headers.TryAddWithoutValidation("If-Match", $"\"ht-{baseline ?? version}\"");
                using var response = await c.SendAsync(request); var raw = await response.Content.ReadAsStringAsync();
                Check((int)response.StatusCode == expected, method + " " + route + ": " + raw);
                if (response.Headers.ETag is { } etag && c == client) version = long.Parse(etag.Tag[4..^1]);
                if (string.IsNullOrEmpty(raw)) return default;
                var payload = JsonDocument.Parse(raw).RootElement;
                return payload.TryGetProperty(expected < 400 ? "duLieu" : "code", out var value) ? value.Clone() : payload.Clone();
            }
            await Send(anonymous, "planning-reminders", expected: 401);
            var settings = await Send(client, "planning-reminders/settings");
            Check(!settings.GetProperty("dangBat").GetBoolean(), "planning reminders are opt in");
            Check((await Send(client, "planning-reminders")).GetProperty("tongSo").GetInt32() == 0, "disabled reminders never materialize");
            await Send(client, "planning-reminders/settings", "PUT", new { dangBat = true });
            await Send(client, "planning-reminders/settings", "PUT", new { dangBat = false }, 409, 0);
            await Send(client, "availability-windows/batch", "POST", new { thuTrongTuan = new[] { 1, 2, 3, 4, 5 }, gioBatDau = "20:00:00", gioKetThuc = "23:00:00", ngayBatDau = "2026-11-16", ngayKetThuc = "2026-11-20" }, 201);
            var before = version;
            await Task.WhenAll(Send(client, "planning-reminders"), Send(client, "planning-reminders"));
            var inbox = await Send(client, "planning-reminders");
            Check(inbox.GetProperty("tongSo").GetInt32() == 2 && inbox.GetProperty("tongChuaDoc").GetInt32() == 2, "weekly and grouped expiry reminders are created once under concurrent inbox reads");
            Check(version == before, "reading and materializing reminders does not invalidate scheduling previews");
            var rows = inbox.GetProperty("danhSach").EnumerateArray().ToArray();
            Check(rows.Count(row => row.GetProperty("loai").GetString() == "AvailabilityExpiring") == 1 && rows.Any(row => row.GetProperty("noiDung").GetString()!.Contains("5 mẫu")), "five weekday templates sharing expiry generate a single useful reminder");
            Check(rows.All(row => new[] { "/tasks?tab=planning", "/tasks?tab=free" }.Contains(row.GetProperty("duongDan").GetString())), "reminder navigation targets are internal allowlisted routes");
            await using (var db = new ApplicationDbContext(options)) Check(await db.DanhSachNhiemVu.CountAsync() == 0 && await db.DanhSachPhienTuHoc.CountAsync() == 0, "reminders do not create synthetic tasks or study sessions");
            var item = rows[0]; var id = item.GetProperty("ma").GetInt32();
            await Send(foreign, $"planning-reminders/{id}/read", "PATCH", new { ngayPhat = item.GetProperty("ngayPhat").GetString() }, 404);
            await Send(client, $"planning-reminders/{id}/read", "PATCH", new { ngayPhat = "2000-01-01T00:00:00Z" }, 409);
            await Send(client, $"planning-reminders/{id}/read", "PATCH", new { ngayPhat = item.GetProperty("ngayPhat").GetString() }, 204);
            Check((await Send(client, "planning-reminders?chuaDoc=true")).GetProperty("tongChuaDoc").GetInt32() == 1, "read marker applies only to owned unchanged item");
            clock.Advance(TimeSpan.FromDays(1));
            inbox = await Send(client, "planning-reminders");
            Check(inbox.GetProperty("tongSo").GetInt32() == 3, "a new local week creates one review, retaining old reminder history");
            await Send(client, "planning-reminders/read-all", "POST", new { docDenLuc = inbox.GetProperty("docDenLuc").GetString() }, 204);
            Check((await Send(client, "planning-reminders")).GetProperty("tongChuaDoc").GetInt32() == 0, "read all honors the loaded snapshot cutoff");
            Check((await Send(foreign, "planning-reminders")).GetProperty("tongSo").GetInt32() == 0, "another account cannot read reminder history");
            await Send(client, "planning-reminders/settings", "PUT", new { dangBat = false });
            clock.Advance(TimeSpan.FromDays(7));
            Check((await Send(client, "planning-reminders")).GetProperty("tongSo").GetInt32() == 3, "disabling stops future reminder materialization and retains read history");
            // Cấu hình chung dùng lại cột nhắc hạn và hàng tùy chọn kế hoạch, không tạo bản sao dữ liệu.
            await Send(anonymous, "notifications/settings", expected: 401);
            var combined = await Send(client, "notifications/settings");
            Check(combined.GetProperty("nhacTruocGio").GetInt32() == 24 && !combined.GetProperty("nhacKeHoach").GetBoolean(), "combined settings return existing defaults");
            var settingsVersion = version;
            await Send(client, "notifications/settings", "PUT", new { nhacTruocGio = 0, nhacKeHoach = true });
            Check(version == settingsVersion + 1, "saving both settings advances the version once");
            Check((await Send(client, "planning-reminders/settings")).GetProperty("dangBat").GetBoolean(), "combined settings reuse the existing planning preference");
            Check((await Send(foreign, "notifications/settings")).GetProperty("nhacTruocGio").GetInt32() == 24, "settings stay isolated by account");
            var unchangedVersion = version;
            await Send(client, "notifications/settings", "PUT", new { nhacTruocGio = 0, nhacKeHoach = true });
            Check(version == unchangedVersion, "saving unchanged preferences is a no-op");
            await Send(client, "notifications/settings", "PUT", new { nhacTruocGio = 12, nhacKeHoach = false }, 409, settingsVersion);
            await Send(client, "notifications/settings", "PUT", new { nhacTruocGio = -1, nhacKeHoach = false }, 400);
            await Send(client, "notifications/settings", "PUT", new { nhacTruocGio = 8761, nhacKeHoach = false }, 400);
            await Send(client, "notifications/settings", "PUT", new { nhacTruocGio = 24 }, 400);
            await using (var db = new ApplicationDbContext(options)) Check(await db.DanhSachNguoiDung.Where(x => x.Ma == owner).Select(x => x.NhacTruocGio).SingleAsync() == 0 &&
                await db.DanhSachCauHinhNhacKeHoach.CountAsync(x => x.MaNguoiDung == owner && x.DangBat) == 1, "zero reminder hours persist and rejected saves do not partially alter planning preferences");
            Console.WriteLine($"PASS: {checks} planning reminder/settings HTTP/PostgreSQL assertions; opt-in, grouping, ownership, read markers and shared notification preferences.");

        }
        finally {
            if (app is not null) { await app.StopAsync(); await app.DisposeAsync(); }
            NpgsqlConnection.ClearAllPools(); await using var drop = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{database}\" WITH (FORCE)", admin); await drop.ExecuteNonQueryAsync();
        }
    }
}
