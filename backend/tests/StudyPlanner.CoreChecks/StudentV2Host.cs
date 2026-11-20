using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using StudyPlanner.Api.Academic;
using StudyPlanner.Api.Administration;
using StudyPlanner.Api.Auth;
using StudyPlanner.Api.CanhBao;
using StudyPlanner.Api.Controllers;
using StudyPlanner.Api.HocTap;
using StudyPlanner.Application.Auth;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;
using StudyPlanner.Infrastructure.Data;
using StudyPlanner.Infrastructure.NenTang;
using StudyPlanner.Infrastructure.TepDinhKem;
using StudyPlanner.Infrastructure.UuTien;

namespace StudyPlanner.CoreChecks;

// A disposable localhost host using the real controllers/services and migrations.
// These diagnostic/seed routes exist only in this test executable, never the API app.
public static class StudentV2Host
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    public static async Task RunAsync()
    {
        var config = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("STUDYPLANNER_TEST_POSTGRES"));
        if (config.Host is not ("127.0.0.1" or "localhost") || config.Database != "postgres")
            throw new InvalidOperationException("Student V2 host requires an isolated localhost postgres admin database.");
        var output = Path.GetFullPath(Environment.GetEnvironmentVariable("STUDENT_V2_OUTPUT") ?? "backend/obj/student-v2");
        Directory.CreateDirectory(output);
        var database = "studyplanner_student_v2_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(config.ConnectionString); await admin.OpenAsync();
        await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", admin)) await create.ExecuteNonQueryAsync();
        config.Database = database;
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development", ContentRootPath = Path.GetFullPath("backend/StudyPlanner.Api") });
        builder.Logging.ClearProviders(); builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Configuration["Auth:SigningKey"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseNpgsql(config.ConnectionString));
        builder.Services.AddQuanTri(); builder.Services.AddAcademic(); builder.Services.AddHocTap(); builder.Services.AddThongBao();
        builder.Services.AddScoped<QuyTacUuTienService>();
        builder.Services.AddScoped<DuLieuNenTangService>();
        builder.Services.AddSingleton(new DanhMucJsonStore(Path.Combine(output, "catalog.json"), Path.GetFullPath("backend/StudyPlanner.Api/Administration/Data/danh-muc-mau.seed.json")));
        builder.Services.AddSingleton(new KhoTepNhiemVu(Path.Combine(output, "attachments")));
        builder.Services.AddScoped<TepDinhKemService>();
        builder.Services.AddStudyPlannerXacThuc(builder.Configuration, builder.Environment);
        builder.Services.AddControllers().AddApplicationPart(typeof(LapLichController).Assembly);
        await using var app = builder.Build();
        app.UseExceptionHandler(); app.UseAuthentication(); app.UseAuthorization(); app.UseRateLimiter(); app.MapControllers();
        app.MapPost("/__test/fixture/{label}", async (string label, XacThucService auth, ApplicationDbContext db, JwtTokenIssuer issuer) =>
        {
            var suffix = Guid.NewGuid().ToString("N");
            var created = await auth.DangKyAsync(new DangKyRequest { Email = suffix + "@example.test", HoTen = "[LPTEST-V2] " + label,
                MatKhau = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24)), MaSoSinhVien = "QA" + suffix,
                TenTruong = "Test isolated", TenLop = "QA", NganhHoc = "QA" }, default);
            var user = await db.DanhSachNguoiDung.SingleAsync(u => u.Ma == created.Ma);
            if (label == "admin") { user.VaiTro = VaiTroNguoiDung.Admin; await db.SaveChangesAsync(); }
            return issuer.Issue(user);
        });
        app.MapGet("/__test/state/{owner:int}", async (int owner, ApplicationDbContext db) => new
        {
            tasks = await db.DanhSachNhiemVu.IgnoreQueryFilters().Where(t => t.MaNguoiDung == owner)
                .Select(t => new { t.Ma, t.TieuDe, t.MaHocKy, t.MaMonHoc, t.MaNhiemVuCha, t.TienDo, t.NgayXoa }).ToListAsync(),
            sessions = await db.Set<PhienTuHoc>().IgnoreQueryFilters().Where(p => p.MaNguoiDung == owner)
                .Select(p => new { p.Ma, p.MaNhiemVu, p.MaLanApDung, p.BatDauDuKien, p.KetThucDuKien, trangThai = p.TrangThai.ToString() }).ToListAsync(),
            focus = await db.Set<PhienTapTrung>().Where(p => p.MaNguoiDung == owner)
                .Select(p => new { p.Ma, p.MaNhiemVu, p.TrangThai, p.SoMiliGiayDaHoc }).ToListAsync(),
            batches = await db.Set<LanApDungLich>().CountAsync(p => p.MaNguoiDung == owner),
            history = await db.Set<LichSuNhiemVu>().CountAsync(p => p.MaNguoiDung == owner)
        });
        app.MapGet("/__test/health", () => new { now = DateTimeOffset.UtcNow, database, timezone = "Asia/Ho_Chi_Minh", algorithm = 4 });
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(); await db.Database.MigrateAsync();
            await File.WriteAllTextAsync(Path.Combine(output, "schema.json"), JsonSerializer.Serialize(new { database, migrations = await db.Database.GetAppliedMigrationsAsync() }, Json));
        }
        try
        {
            await app.StartAsync();
            var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            await File.WriteAllTextAsync(Path.Combine(output, "host.json"), JsonSerializer.Serialize(new { address, database, now = DateTimeOffset.UtcNow }, Json));
            Console.WriteLine("Student V2 test host ready; diagnostic endpoints are isolated and test-only.");
            // A stop file allows the orchestrator to run finally/DB cleanup without killing Kestrel.
            while (!File.Exists(Path.Combine(output, "stop"))) await Task.Delay(250);
        }
        finally
        {
            await app.StopAsync(); NpgsqlConnection.ClearAllPools();
            await using var drop = new NpgsqlCommand($"DROP DATABASE \"{database}\" WITH (FORCE)", admin); await drop.ExecuteNonQueryAsync();
            await File.WriteAllTextAsync(Path.Combine(output, "cleanup.json"), JsonSerializer.Serialize(new { database, dropped = true, time = DateTimeOffset.UtcNow }, Json));
        }
    }

    public static async Task AuditAsync()
    {
        var settings = new ConfigurationBuilder().AddJsonFile(Path.GetFullPath("backend/StudyPlanner.Api/appsettings.json"))
            .AddJsonFile(Path.GetFullPath("backend/StudyPlanner.Api/appsettings.Development.json")).Build();
        await using var connection = new NpgsqlConnection(settings.GetConnectionString("DefaultConnection")); await connection.OpenAsync();
        await using var tx = await connection.BeginTransactionAsync(IsolationLevel.RepeatableRead);
        await using (var readOnly = new NpgsqlCommand("SET TRANSACTION READ ONLY", connection, tx)) await readOnly.ExecuteNonQueryAsync();
        async Task<object?> Scalar(string sql) { await using var cmd = new NpgsqlCommand(sql, connection, tx); return await cmd.ExecuteScalarAsync(); }
        var result = new {
            time = DateTimeOffset.UtcNow, mode = "RepeatableRead READ ONLY", serverVersion = connection.PostgreSqlVersion.ToString(),
            tables = await Scalar("SELECT count(*) FROM information_schema.tables WHERE table_schema='public' AND table_type='BASE TABLE'"),
            migration = await Scalar("SELECT \"MigrationId\" FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\" DESC LIMIT 1"),
            priority = await Scalar("SELECT json_build_object('version',phien_ban,'config',cau_hinh)::text FROM quy_tac_uu_tien WHERE dang_su_dung LIMIT 1")
        };
        await tx.RollbackAsync();
        Console.WriteLine(JsonSerializer.Serialize(result, Json));
    }
}
