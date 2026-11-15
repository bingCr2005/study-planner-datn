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

public static class AvailabilityUpgradeChecks
{
    public static async Task RunAsync(string supplied)
    {
        var connection = new NpgsqlConnectionStringBuilder(supplied);
        if (connection.Host is not ("127.0.0.1" or "localhost") || connection.Database != "postgres") throw new Exception("Use isolated localhost postgres.");
        var database = "studyplanner_availability_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(connection.ConnectionString); await admin.OpenAsync();
        await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", admin)) await create.ExecuteNonQueryAsync();
        connection.Database = database;
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(connection.ConnectionString).Options;
        var clock = new KiemThuClock(new DateTimeOffset(2026, 11, 15, 12, 0, 0, TimeSpan.Zero));
        WebApplication? app = null; var checks = 0;
        void Check(bool ok, string label) { if (!ok) throw new Exception("FAIL availability upgrade: " + label); checks++; }
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
            object Batch(int[] days) => new { thuTrongTuan = days, gioBatDau = "19:00:00", gioKetThuc = "21:00:00", ngayBatDau = "2026-11-16", ngayKetThuc = "2026-11-27", dangSuDung = true };
            await Send(anonymous, "availability-windows/batch", "POST", Batch([1]), 401);
            await Send(client, "availability-windows");
            await Send(client, "availability-windows/batch", "POST", Batch([1, 1]), 400);
            await Send(client, "availability-windows/batch", "POST", new { thuTrongTuan = new[] { 1 }, gioKetThuc = "21:00:00", ngayBatDau = "2026-11-16", ngayKetThuc = "2026-11-27" }, 400);
            Check(version == 0, "invalid batch never increments revision");
            var created = await Send(client, "availability-windows/batch", "POST", Batch([1, 2, 3, 4, 5]), 201);
            Check(created.GetArrayLength() == 5 && version == 1, "five weekdays created once in one revision");
            var monday = created[0].GetProperty("ma").GetInt32(); var tuesday = created[1].GetProperty("ma").GetInt32();
            await Send(client, "availability-windows/batch", "POST", Batch([1, 6]), 409);
            Check((await Send(client, "availability-windows")).GetArrayLength() == 5, "partly duplicate batch writes no extra Saturday");
            string Calendar() => "calendar?tu=2026-11-16T00:00:00%2B07:00&den=2026-11-28T00:00:00%2B07:00";
            var calendar = await Send(client, Calendar()); Check(calendar.GetProperty("suKien").GetArrayLength() == 10, "two weeks expand without new database rows");
            var skip = new { ngay = "2026-11-16", boQua = true };
            var unchanged = version;
            await Send(client, $"availability-windows/{monday}/exceptions/change-preview", "POST", skip);
            Check(version == unchanged, "exception preview is read only");
            await Send(foreign, $"availability-windows/{monday}/exceptions", expected: 404);
            await Send(client, $"availability-windows/{monday}/exceptions/2026-11-17", "PUT", skip, 400);
            await Send(client, $"availability-windows/{monday}/exceptions/2026-11-16", "PUT", skip);
            Check((await Send(client, Calendar())).GetProperty("suKien").GetArrayLength() == 9, "one skipped Monday does not remove following Monday");
            var custom = new { ngay = "2026-11-16", boQua = false, gioBatDau = "21:00:00", gioKetThuc = "22:00:00" };
            await Send(client, $"availability-windows/{monday}/exceptions/2026-11-16", "PUT", custom);
            // Truy vấn ngoài khung gốc 19–21 giờ vẫn phải tìm thấy giờ thay thế của ngày đó.
            var shifted = await Send(client, "calendar?tu=2026-11-16T21:00:00%2B07:00&den=2026-11-16T23:00:00%2B07:00");
            Check(shifted.GetProperty("suKien").GetArrayLength() == 1, "replacement outside template hours is expanded independently");
            await Send(client, $"availability-windows/{monday}/exceptions/2026-11-16", "DELETE", expected: 204);
            var task = await Send(client, "tasks", "POST", new { tieuDe = "Ngoại lệ không bỏ quên phiên", thoiLuongDuKienPhut = 60 }, 201);
            var taskId = task.GetProperty("ma").GetInt32();
            var planned = await Send(client, "study-sessions", "POST", new { maNhiemVu = taskId, batDauDuKien = "2026-11-16T20:00:00+07:00", ketThucDuKien = "2026-11-16T20:25:00+07:00" }, 201);
            var sessionId = planned.GetProperty("ma").GetInt32();
            var impact = await Send(client, $"availability-windows/{monday}/exceptions/change-preview", "POST", skip);
            Check(impact.GetProperty("phienBiAnhHuong").EnumerateArray().Any(x => x.GetInt32() == sessionId), "exception preview identifies invalidated session");
            await Send(client, $"availability-windows/{monday}/exceptions/2026-11-16", "PUT", skip, 409);
            Check((await Send(client, $"availability-windows/{monday}/exceptions")).GetArrayLength() == 0, "missing confirmation leaves exception absent");
            await Send(client, $"availability-windows/{monday}/exceptions/2026-11-16", "PUT", new { boQua = true, huyPhienLienQuan = true });
            Check((await Send(client, $"study-sessions/{sessionId}")).GetProperty("trangThai").GetString() == "Cancelled", "confirmed exception cancels affected future session atomically");
            var active = await Send(client, "study-sessions", "POST", new { maNhiemVu = taskId, batDauDuKien = "2026-11-17T19:00:00+07:00", ketThucDuKien = "2026-11-17T19:25:00+07:00" }, 201);
            await Send(client, $"study-sessions/{active.GetProperty("ma").GetInt32()}/start", "POST", new { });
            await Send(client, $"availability-windows/{tuesday}/exceptions/2026-11-17", "PUT", new { boQua = true, huyPhienLienQuan = true }, 409);
            Check((await Send(client, $"availability-windows/{tuesday}/exceptions")).GetArrayLength() == 0, "running session cannot be canceled through an availability exception");
            await Send(client, $"availability-windows/{monday}/exceptions/2026-11-23", "PUT", skip, 409, 0);
            Console.WriteLine($"PASS: {checks} availability upgrade HTTP/PostgreSQL assertions; batch, recurrence, exceptions, ownership, confirmation and active-session protection.");
        }
        finally {
            if (app is not null) { await app.StopAsync(); await app.DisposeAsync(); }
            NpgsqlConnection.ClearAllPools(); await using var drop = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{database}\" WITH (FORCE)", admin); await drop.ExecuteNonQueryAsync();
        }
    }
}
