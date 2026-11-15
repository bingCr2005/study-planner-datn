using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using StudyPlanner.Api.Administration;
using StudyPlanner.Api.Auth;
using StudyPlanner.Api.Controllers;
using StudyPlanner.Api.HocTap;
using StudyPlanner.Application.HocTap;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;
using StudyPlanner.Infrastructure.Data;
using StudyPlanner.Infrastructure.HocTap;

namespace StudyPlanner.CoreChecks;

public static class PlanningUpgradeChecks
{
    public static void Run()
    {
        var count = 0;
        void Check(bool ok, string message) { if (!ok) throw new Exception("FAIL configured planning: " + message); count++; }
        var zone = QuyTacThoiGian.MuiGio("Asia/Ho_Chi_Minh");
        var now = new DateTimeOffset(2026, 11, 14, 8, 0, 0, TimeSpan.Zero);
        var day = new DateOnly(2026, 11, 14);
        var start = QuyTacThoiGian.Utc(day, new(18, 0), zone);
        var config = new CauHinhHocResponse(20, 3, 40);
        var jobs = new[] { new CongViecLapLich(1, "Đồ án", null, MucDoUuTienNhiemVu.High, 100) };
        var free = new[] { new KhoangThoiGian(start, start.AddHours(4)), new KhoangThoiGian(start.AddDays(1), start.AddDays(1).AddHours(4)) };
        var plan = QuyTacLapLich.Xep(jobs, free, now, config, zone);
        Check(plan.Phien.Count == 4 && plan.ChuaXep[1] == 20, "daily cap limits each day to forty study minutes");
        Check(plan.Phien.All(p => (p.KetThucDuKien - p.BatDauDuKien).TotalMinutes == 20), "preferred session length reaches the scheduler");
        Check(plan.Phien[1].BatDauDuKien == plan.Phien[0].KetThucDuKien.AddMinutes(3), "configured rest is used between sessions");
        var reserved = QuyTacLapLich.Xep(jobs, free, now, config, zone, new Dictionary<DateOnly, double> { [day] = 25 });
        Check(reserved.Phien.Where(p => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(p.BatDauDuKien, zone).DateTime) == day)
            .Sum(p => (p.KetThucDuKien - p.BatDauDuKien).TotalMinutes) == 15, "existing reservations consume the daily budget");
        var shortJob = new[] { jobs[0] with { SoPhutCanXep = 25 } };
        var tail = QuyTacLapLich.Xep(shortJob, free, now, config, zone);
        Check(tail.Phien.Count == 2 && (tail.Phien[1].KetThucDuKien - tail.Phien[1].BatDauDuKien).TotalMinutes == 5, "only a final remainder may be under fifteen minutes");
        var midnight = QuyTacThoiGian.Utc(day.AddDays(1), TimeOnly.MinValue, zone);
        var overnight = QuyTacLapLich.Xep(jobs, [new(midnight.AddHours(-1), midnight.AddHours(1))], now, config, zone);
        Check(overnight.Phien.Count == 4, "one continuous free interval is split at local midnight for daily budgets");
        var totals = QuyTacGioiHanNgay.Tong([new(midnight.AddMinutes(-15), midnight.AddMinutes(15))], zone);
        Check(totals[day] == 15 && totals[day.AddDays(1)] == 15, "a crossing-midnight reservation counts its actual minutes in both local days");
        foreach (var invalid in new[] { new CauHinhHocResponse(14, 5, 180), new(51, 5, 180), new(25, 0, 180), new(25, 31, 180), new(25, 5, 20) })
        {
            try { invalid.KiemTra(); throw new Exception("Expected configuration validation"); }
            catch (HocTapException e) { Check(e.Code == "VALIDATION_ERROR", "invalid configuration is rejected"); }
        }
        Console.WriteLine($"PASS: {count} configured planning/day-budget assertions");
    }

    public static async Task RunAsync(string supplied)
    {
        var config = new NpgsqlConnectionStringBuilder(supplied);
        if (config.Host is not ("localhost" or "127.0.0.1") || config.Database != "postgres")
            throw new InvalidOperationException("Planning upgrade checks require an isolated localhost postgres admin database.");
        var database = "studyplanner_planning_upgrade_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(config.ConnectionString); await admin.OpenAsync();
        await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", admin)) await create.ExecuteNonQueryAsync();
        config.Database = database;
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(config.ConnectionString).Options;
        WebApplication? app = null;
        var count = 0;
        void Check(bool ok, string message) { if (!ok) throw new Exception("FAIL planning HTTP: " + message); count++; }
        var clock = new KiemThuClock(new DateTimeOffset(2026, 11, 14, 8, 0, 0, TimeSpan.Zero));
        try
        {
            NguoiDung owner, stranger; NhiemVu task;
            await using (var db = new ApplicationDbContext(options))
            {
                await db.Database.MigrateAsync();
                NguoiDung User(string email) => new() { Email = email, EmailChuanHoa = email.ToUpperInvariant(), HoTen = "Planning fixture", MatKhauBam = "unused-fixture",
                    MuiGio = "Asia/Ho_Chi_Minh", DangHoatDong = true, NgayTao = clock.GetUtcNow(), NgayCapNhat = clock.GetUtcNow() };
                owner = User("planning-upgrade@example.test"); stranger = User("planning-stranger@example.test");
                db.AddRange(owner, stranger); await db.SaveChangesAsync();
                task = new() { MaNguoiDung = owner.Ma, TieuDe = "Đồ án cần xếp lại", ThoiLuongDuKienPhut = 80 };
                db.Add(task);
                db.Add(new KhungGioRanh { MaNguoiDung = owner.Ma, ThuTrongTuan = 6, GioBatDau = new(18, 0), GioKetThuc = new(22, 0),
                    NgayBatDau = new(2026, 11, 14), NgayKetThuc = new(2026, 12, 31), DangSuDung = true });
                await db.SaveChangesAsync();
            }
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
            builder.Logging.ClearProviders(); builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Configuration["Auth:SigningKey"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseNpgsql(config.ConnectionString));
            builder.Services.AddQuanTri(); builder.Services.AddHocTap(); builder.Services.AddStudyPlannerXacThuc(builder.Configuration, builder.Environment);
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IHocTapStore>(sp => new LoiSauLuuStore(new EfHocTapStore(sp.GetRequiredService<ApplicationDbContext>()), sp.GetRequiredService<IHttpContextAccessor>()));
            builder.Services.AddControllers().AddApplicationPart(typeof(CauHinhHocController).Assembly);
            builder.Services.AddSingleton<TimeProvider>(clock);
            app = builder.Build(); app.UseExceptionHandler(); app.UseAuthentication(); app.UseAuthorization(); app.UseRateLimiter(); app.MapControllers(); await app.StartAsync();
            var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            using var client = new HttpClient { BaseAddress = new(address) };
            using var other = new HttpClient { BaseAddress = new(address) };
            using var anonymous = new HttpClient { BaseAddress = new(address) };
            var issuer = app.Services.GetRequiredService<JwtTokenIssuer>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", issuer.Issue(owner).TokenTruyCap);
            other.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", issuer.Issue(stranger).TokenTruyCap);
            long revision = 0;
            async Task<JsonElement> Send(HttpClient who, string method, string path, object? body = null, int expected = 200, string? key = null, int? failSave = null, long? explicitVersion = null)
            {
                using var request = new HttpRequestMessage(new HttpMethod(method), "/api/v1/" + path);
                if (body is not null) request.Content = JsonContent.Create(body);
                if (method is not "GET") request.Headers.TryAddWithoutValidation("If-Match", $"\"ht-{explicitVersion ?? revision}\"");
                if (key is not null) request.Headers.TryAddWithoutValidation("Idempotency-Key", key);
                if (failSave.HasValue) request.Headers.TryAddWithoutValidation("X-CoreChecks-Fail-Save", failSave.ToString());
                using var response = await who.SendAsync(request); var text = await response.Content.ReadAsStringAsync();
                Check((int)response.StatusCode == expected, path + ": " + (int)response.StatusCode + "; " + text);
                if (!response.IsSuccessStatusCode) return default;
                var result = JsonDocument.Parse(text).RootElement.Clone();
                if (who == client) revision = result.GetProperty("phienBan").GetInt64();
                Check(response.Headers.ETag is not null, "response includes ETag");
                return result.GetProperty("duLieu");
            }
            object Settings(int study = 20, int rest = 3, int daily = 40) => new { phienHocPhut = study, nghiGiuaPhienPhut = rest, gioiHanHocMoiNgayPhut = daily };
            object Preview(bool update = false) => new { ngayBatDau = "2026-11-14", soNgay = 1, danhSachMaNhiemVu = new[] { task.Ma }, capNhatKeHoach = update };
            object Apply(JsonElement preview) => new { tokenPreview = preview.GetProperty("tokenPreview").GetString(), phien = preview.GetProperty("phien").EnumerateArray().Select(row => new {
                maNhiemVu = row.GetProperty("maNhiemVu").GetInt32(), batDauDuKien = row.GetProperty("batDauDuKien").GetString(), ketThucDuKien = row.GetProperty("ketThucDuKien").GetString() }).ToArray() };
            await Send(anonymous, "GET", "study-settings", expected: 401);
            var defaults = await Send(client, "GET", "study-settings");
            Check(defaults.GetProperty("phienHocPhut").GetInt32() == 25 && defaults.GetProperty("nghiGiuaPhienPhut").GetInt32() == 5, "shared settings have server defaults");
            await Send(client, "PUT", "study-settings", Settings(14), expected: 400);
            await Send(client, "PUT", "study-settings", Settings(), explicitVersion: 999, expected: 409);
            var saved = await Send(client, "PUT", "study-settings", Settings());
            var settingsRevision = revision;
            await Send(client, "PUT", "study-settings", Settings());
            Check(revision == settingsRevision, "unchanged settings do not bump revision");
            var foreignSettings = await Send(other, "GET", "study-settings");
            Check(foreignSettings.GetProperty("phienHocPhut").GetInt32() == 25, "settings are scoped to account");
            var preview = await Send(client, "POST", "planning/preview", Preview());
            Check(preview.GetProperty("phien").GetArrayLength() == 2 && preview.GetProperty("tongPhutDeXuat").GetInt32() == 40 && preview.GetProperty("tongPhutChuaXep").GetInt32() == 40,
                "preview uses preferred length and cap, returning unscheduled work");
            Check(preview.GetProperty("cauHinhHoc").GetProperty("nghiGiuaPhienPhut").GetInt32() == 3 && preview.GetProperty("maPhienThayThe").GetArrayLength() == 0,
                "preview discloses effective configuration and no replacements by default");
            await Send(client, "POST", "planning/apply", Apply(preview), key: "initial-plan");
            int[] initialIds;
            await using (var db = new ApplicationDbContext(options)) initialIds = await db.DanhSachPhienTuHoc.Where(p => p.MaNguoiDung == owner.Ma && p.TrangThai == TrangThaiPhienTuHoc.Planned).Select(p => p.Ma).ToArrayAsync();
            var replan = await Send(client, "POST", "planning/preview", Preview(true));
            Check(replan.GetProperty("maPhienThayThe").GetArrayLength() == 2 && replan.GetProperty("tongPhutDeXuat").GetInt32() == 40, "replan releases exactly selected future sessions while calculating");
            var beforeRevision = revision;
            await Send(client, "POST", "planning/apply", Apply(replan), expected: 500, key: "rollback-replan", failSave: 1);
            await using (var db = new ApplicationDbContext(options))
            {
                Check(await db.DanhSachPhienTuHoc.CountAsync(p => initialIds.Contains(p.Ma) && p.TrangThai == TrangThaiPhienTuHoc.Planned) == 2, "failed replan rolls persisted cancellations back");
                Check(await db.DanhSachNguoiDung.Where(u => u.Ma == owner.Ma).Select(u => u.PhienBanLapLich).SingleAsync() == beforeRevision, "failed replan retains revision");
            }
            var applied = await Send(client, "POST", "planning/apply", Apply(replan), key: "replace-plan");
            var repeated = await Send(client, "POST", "planning/apply", Apply(replan), key: "replace-plan");
            Check(applied.GetProperty("maLanApDung").GetInt32() == repeated.GetProperty("maLanApDung").GetInt32(), "retry reuses the successful replacement batch");
            await using (var db = new ApplicationDbContext(options))
            {
                Check(await db.DanhSachPhienTuHoc.CountAsync(p => p.MaNguoiDung == owner.Ma && p.TrangThai == TrangThaiPhienTuHoc.Planned) == 2 &&
                    await db.DanhSachPhienTuHoc.CountAsync(p => initialIds.Contains(p.Ma) && p.TrangThai == TrangThaiPhienTuHoc.Cancelled) == 2, "successful replan cancels old sessions and does not duplicate active reservations");
            }
            var stale = await Send(client, "POST", "planning/preview", Preview(true));
            await Send(client, "PUT", "study-settings", Settings(20, 4, 40));
            await Send(client, "POST", "planning/apply", Apply(stale), expected: 409, key: "stale-settings");
            await Send(other, "POST", "planning/apply", Apply(stale), expected: 404, key: "foreign-preview");
            Console.WriteLine($"PASS: {count} shared study settings/replanning HTTP/PostgreSQL assertions");
        }
        finally
        {
            if (app is not null) { await app.StopAsync(); await app.DisposeAsync(); }
            NpgsqlConnection.ClearAllPools();
            await using var drop = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{database}\" WITH (FORCE)", admin); await drop.ExecuteNonQueryAsync();
        }
    }
}
