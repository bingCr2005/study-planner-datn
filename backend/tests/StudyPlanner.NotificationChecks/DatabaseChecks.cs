using Microsoft.EntityFrameworkCore;
using Npgsql;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;
using StudyPlanner.Infrastructure.Data;
using StudyPlanner.Infrastructure.HocTap;
using StudyPlanner.Infrastructure.CanhBao;
using StudyPlanner.Application.HocTap;

public static class DatabaseChecks
{
    public static async Task RunAsync()
    {
        var supplied = Environment.GetEnvironmentVariable("STUDYPLANNER_TEST_POSTGRES");
        if (string.IsNullOrWhiteSpace(supplied)) { Console.WriteLine("SKIP notification PostgreSQL checks: isolated localhost test database not configured."); return; }
        var config = new NpgsqlConnectionStringBuilder(supplied);
        if (config.Host is not ("localhost" or "127.0.0.1") || config.Database != "postgres") throw new Exception("Only isolated localhost postgres test admin database is allowed.");
        var name = "studyplanner_notifications_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(config.ConnectionString); await admin.OpenAsync();
        await using (var command = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", admin)) await command.ExecuteNonQueryAsync();
        config.Database = name;
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(config.ConnectionString).Options;
        var clock = new TestClock();
        void Check(bool value, string label) { if (!value) throw new Exception("FAIL DB: " + label); }
        try
        {
            int owner, stranger;
            await using (var db = new ApplicationDbContext(options))
            {
                await db.Database.MigrateAsync();
                NguoiDung User(string email) => new() { Email = email, EmailChuanHoa = email.ToUpperInvariant(), HoTen = email, MatKhauBam = "test-only", NgayTao = clock.GetUtcNow(), NgayCapNhat = clock.GetUtcNow() };
                var a = User("a@notification.test"); var b = User("b@notification.test");
                db.AddRange(a, b); await db.SaveChangesAsync(); owner = a.Ma; stranger = b.Ma;
                var parent = new NhiemVu { MaNguoiDung = owner, TieuDe = "Việc nhắc hạn", HanHoanThanh = clock.GetUtcNow().AddHours(1), NgayTao = clock.GetUtcNow(), NgayCapNhat = clock.GetUtcNow() };
                db.Add(parent);
                await db.SaveChangesAsync();
                db.Add(new NhiemVu { MaNguoiDung = owner, MaNhiemVuCha = parent.Ma, TieuDe = "Bước nhỏ kế thừa hạn", NgayTao = clock.GetUtcNow(), NgayCapNhat = clock.GetUtcNow() });
                await db.SaveChangesAsync();
            }
            async Task<HopThongBaoResponse> List(int user, bool unread = false)
            {
                await using var db = new ApplicationDbContext(options);
                return await new ThongBaoService(db, new EfHocTapStore(db), clock).DanhSachAsync(user, unread, 1, 20, default);
            }
            var parallel = await Task.WhenAll(List(owner), List(owner));
            Check(parallel.All(x => x.TongSo == 1 && x.TongChuaDoc == 1), "Concurrent sync is idempotent under the account lock");
            var item = parallel[0].DanhSach.Single();
            Check(item.HanHieuLuc == clock.GetUtcNow().AddHours(1) && item.TenNhiemVu == "Bước nhỏ kế thừa hạn", "Leaf DTO inherits its parent's deadline");
            Check((await List(stranger)).TongSo == 0, "Owner isolation");
            await using (var db = new ApplicationDbContext(options))
            {
                var service = new ThongBaoService(db, new EfHocTapStore(db), clock);
                try { await service.DocAsync(stranger, item.Ma, item.NgayPhat, default); throw new Exception("Cross-account read succeeded"); }
                catch (HocTapException error) when (error.Status == 404) { }
            }
            await using (var db = new ApplicationDbContext(options)) await new ThongBaoService(db, new EfHocTapStore(db), clock).DocAsync(owner, item.Ma, item.NgayPhat, default);
            Check((await List(owner)).TongChuaDoc == 0 && (await List(owner, true)).TongSo == 0, "Read survives a fresh service/context and unread filter");
            clock.Now = clock.Now.AddHours(2);
            var overdue = await List(owner);
            Check(overdue.TongChuaDoc == 1 && overdue.DanhSach.Single().QuaHan, "Overdue stage reopens unread once");
            await using (var db = new ApplicationDbContext(options)) await new ThongBaoService(db, new EfHocTapStore(db), clock).DocTatCaAsync(owner, parallel[0].DocDenLuc, default);
            Check((await List(owner)).TongChuaDoc == 1, "Stale read-all cutoff cannot mark a newer stage read");
            await using (var db = new ApplicationDbContext(options)) await new ThongBaoService(db, new EfHocTapStore(db), clock).DocTatCaAsync(owner, overdue.DocDenLuc, default);
            Check((await List(owner)).TongChuaDoc == 0, "Read-all persists");
            await using (var db = new ApplicationDbContext(options)) Check(await db.DanhSachNguoiDung.Where(x => x.Ma == owner).Select(x => x.PhienBanLapLich).SingleAsync() == 0, "Inbox writes do not change task/scheduling revision");
            await HttpChecks.RunAsync(options, clock, owner, stranger);
            Console.WriteLine("PASS: notification PostgreSQL concurrency, ownership, persistence, inherited DTO and read-all cutoff checks.");
        }
        finally
        {
            NpgsqlConnection.ClearAllPools();
            await using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{name}\" WITH (FORCE)", admin); await command.ExecuteNonQueryAsync();
        }
    }
    private sealed class TestClock : TimeProvider
    { public DateTimeOffset Now = new(2026, 11, 10, 3, 0, 0, TimeSpan.Zero); public override DateTimeOffset GetUtcNow() => Now; }
}
