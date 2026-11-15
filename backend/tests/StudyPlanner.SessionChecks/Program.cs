using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using StudyPlanner.Api.Administration;
using StudyPlanner.Api.Auth;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;
using StudyPlanner.Infrastructure.Data;

void Check(bool value, string label) { if (!value) throw new Exception("FAIL: " + label); Console.WriteLine("PASS: " + label); }
var hub = new KenhPhienDangNhap();
using (var first = hub.Mo(1))
using (var second = hub.Mo(1))
using (var unrelated = hub.Mo(2))
{
    hub.Phat(1, "account-disabled");
    Check(first.Reader.TryRead(out var a) && a == "account-disabled" && second.Reader.TryRead(out var b) && b == a,
        "Broadcast reaches all tabs for the target account");
    Check(!unrelated.Reader.TryRead(out _), "Other accounts do not receive the event");
    first.Dispose(); hub.Phat(1, "session-invalid");
    Check(first.Reader.Completion.IsCompleted, "Disconnected subscriber is removed and completed");
}
var supplied = Environment.GetEnvironmentVariable("STUDYPLANNER_TEST_POSTGRES");
if (string.IsNullOrWhiteSpace(supplied)) throw new Exception("Configure isolated localhost test PostgreSQL.");
var config = new NpgsqlConnectionStringBuilder(supplied);
if (config.Host is not ("127.0.0.1" or "localhost") || config.Database != "postgres") throw new Exception("Only local test admin database is allowed.");
var database = "studyplanner_sessions_" + Guid.NewGuid().ToString("N");
await using var adminDb = new NpgsqlConnection(config.ConnectionString); await adminDb.OpenAsync();
await using (var command = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", adminDb)) await command.ExecuteNonQueryAsync();
config.Database = database;
var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(config.ConnectionString).Options;
try
{
    NguoiDung User(string email, VaiTroNguoiDung role) => new() { Email = email, EmailChuanHoa = email.ToUpperInvariant(),
        HoTen = email, VaiTro = role, DangHoatDong = true, MatKhauBam = "test-only", NgayTao = DateTimeOffset.UtcNow, NgayCapNhat = DateTimeOffset.UtcNow };
    var administrator = User("admin@sessions.test", VaiTroNguoiDung.Admin);
    var student = User("student@sessions.test", VaiTroNguoiDung.Student);
    var other = User("other@sessions.test", VaiTroNguoiDung.Student);
    await using (var db = new ApplicationDbContext(options)) { await db.Database.MigrateAsync(); db.AddRange(administrator, student, other); await db.SaveChangesAsync(); }
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
    builder.Logging.ClearProviders(); builder.WebHost.UseUrls("http://127.0.0.1:0");
    builder.Configuration["Auth:SigningKey"] = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
    builder.Services.AddScoped(_ => new ApplicationDbContext(options)); builder.Services.AddQuanTri();
    builder.Services.AddStudyPlannerXacThuc(builder.Configuration, builder.Environment);
    await using var app = builder.Build();
    app.UseExceptionHandler(); app.UseAuthentication(); app.UseAuthorization(); app.UseRateLimiter(); app.MapControllers();
    await app.StartAsync();
    try
    {
        var origin = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        var issuer = app.Services.GetRequiredService<JwtTokenIssuer>();
        HttpClient Client(NguoiDung? user)
        {
            var client = new HttpClient { BaseAddress = new Uri(origin), Timeout = TimeSpan.FromSeconds(10) };
            if (user is not null) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", issuer.Issue(user).TokenTruyCap);
            return client;
        }
        using var admin = Client(administrator); using var pupil = Client(student); using var another = Client(other); using var anonymous = Client(null);
        const string endpoint = "/api/v1/users/me/session-events";
        Check((await anonymous.GetAsync(endpoint)).StatusCode == HttpStatusCode.Unauthorized, "SSE requires authenticated JWT");
        using var streams = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        using var stream1 = await pupil.GetAsync(endpoint, HttpCompletionOption.ResponseHeadersRead, streams.Token);
        using var stream2 = await pupil.GetAsync(endpoint, HttpCompletionOption.ResponseHeadersRead, streams.Token);
        using var otherStream = await another.GetAsync(endpoint, HttpCompletionOption.ResponseHeadersRead, streams.Token);
        Check(stream1.IsSuccessStatusCode && stream1.Content.Headers.ContentType?.MediaType == "text/event-stream", "SSE response flushes before any action");
        using var reader1 = new StreamReader(await stream1.Content.ReadAsStreamAsync(streams.Token));
        using var reader2 = new StreamReader(await stream2.Content.ReadAsStreamAsync(streams.Token));
        using var otherReader = new StreamReader(await otherStream.Content.ReadAsStreamAsync(streams.Token));
        async Task<string?> ReadEvent(StreamReader reader)
        {
            string? line;
            while ((line = await reader.ReadLineAsync(streams.Token)) is not null) if (line.StartsWith("event: ")) return line[7..];
            return null;
        }
        var event1 = ReadEvent(reader1); var event2 = ReadEvent(reader2); var otherEvent = ReadEvent(otherReader);
        Check((await pupil.PatchAsync($"/api/v1/admin/users/{other.Ma}/deactivate", null)).StatusCode == HttpStatusCode.Forbidden, "Student cannot disable another account");
        Check((await admin.PatchAsync($"/api/v1/admin/users/{administrator.Ma}/deactivate", null)).StatusCode == HttpStatusCode.Conflict, "Last administrator remains protected");
        var start = DateTimeOffset.UtcNow;
        Check((await admin.PatchAsync($"/api/v1/admin/users/{student.Ma}/deactivate", null)).IsSuccessStatusCode, "Admin deactivation persists successfully");
        Check(await event1.WaitAsync(TimeSpan.FromSeconds(2)) == "account-disabled" && await event2.WaitAsync(TimeSpan.FromSeconds(2)) == "account-disabled", "Idle student tabs receive the lock within two seconds without another request");
        Check(DateTimeOffset.UtcNow - start < TimeSpan.FromSeconds(2), "No polling delay in the normal push path");
        Check(!otherEvent.IsCompleted, "Unrelated logged-in account remains connected");
        await using (var db = new ApplicationDbContext(options)) Check(!(await db.DanhSachNguoiDung.SingleAsync(x => x.Ma == student.Ma)).DangHoatDong, "Database already locked when event is delivered");
        using var rejected = await pupil.GetAsync(endpoint);
        var problem = JsonDocument.Parse(await rejected.Content.ReadAsStringAsync());
        Check(rejected.StatusCode == HttpStatusCode.Unauthorized && problem.RootElement.GetProperty("code").GetString() == "ACCOUNT_DISABLED", "Reconnect after a missed lock identifies the locked account");
        using var rejectedProfile = await pupil.GetAsync("/api/v1/users/me");
        Check(rejectedProfile.StatusCode == HttpStatusCode.Unauthorized && (await rejectedProfile.Content.ReadAsStringAsync()).Contains("ACCOUNT_DISABLED"), "Ordinary API calls also preserve the locked-account reason");
        Check((await another.GetAsync("/api/v1/users/me")).IsSuccessStatusCode, "Another student is not logged out");
        streams.Cancel();
        try { await otherEvent; } catch (OperationCanceledException) { }
        Check((await admin.PatchAsync($"/api/v1/admin/users/{student.Ma}/activate", null)).IsSuccessStatusCode, "Account can still be activated again");
        // Reconnect, then mutate outside this API instance: heartbeat DB check covers missed broadcasts.
        using var resumed = await pupil.GetAsync(endpoint, HttpCompletionOption.ResponseHeadersRead);
        using var resumedReader = new StreamReader(await resumed.Content.ReadAsStreamAsync());
        var missed = ReadExternal(resumedReader);
        await using (var db = new ApplicationDbContext(options)) { var row = await db.DanhSachNguoiDung.SingleAsync(x => x.Ma == student.Ma); row.DangHoatDong = false; await db.SaveChangesAsync(); }
        Check(await missed.WaitAsync(TimeSpan.FromSeconds(18)) == "account-disabled", "Heartbeat catches changes made outside the current API instance");
        async Task<string?> ReadExternal(StreamReader reader)
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(18));
            string? line;
            while ((line = await reader.ReadLineAsync(timeout.Token)) is not null) if (line.StartsWith("event: ")) return line[7..];
            return null;
        }
    }
    finally { await app.StopAsync(); }
}
finally
{
    NpgsqlConnection.ClearAllPools();
    await using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{database}\" WITH (FORCE)", adminDb); await command.ExecuteNonQueryAsync();
}
