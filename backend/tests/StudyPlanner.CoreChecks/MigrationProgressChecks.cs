using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;
using StudyPlanner.Infrastructure.Data;

namespace StudyPlanner.CoreChecks;

/// <summary>Kiểm tra nâng cấp trên dữ liệu cũ, gồm các cây nhiệm vụ đã tồn tại trước khi đổi quy tắc.</summary>
public static class MigrationProgressChecks
{
    private const string Before = "20261112100228_ThemDongHoTapTrung";
    private const string After = "20261113010000_TongHopTienDoNhiemVu";

    public static async Task RunAsync(string supplied)
    {
        var config = new NpgsqlConnectionStringBuilder(supplied);
        if (config.Host is not ("localhost" or "127.0.0.1") || config.Database != "postgres")
            throw new InvalidOperationException("Migration checks require an isolated localhost postgres admin database.");
        var count = 0;
        void Check(bool ok, string label) { if (!ok) throw new Exception("FAIL progress migration: " + label); count++; }

        async Task InDatabase(Func<ApplicationDbContext, Task> run)
        {
            var name = "studyplanner_progress_migration_" + Guid.NewGuid().ToString("N");
            await using var admin = new NpgsqlConnection(config.ConnectionString); await admin.OpenAsync();
            await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", admin)) await create.ExecuteNonQueryAsync();
            var local = new NpgsqlConnectionStringBuilder(config.ConnectionString) { Database = name };
            try
            {
                var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(local.ConnectionString).Options;
                await using var db = new ApplicationDbContext(options);
                await db.GetService<IMigrator>().MigrateAsync(Before);
                await run(db);
            }
            finally
            {
                NpgsqlConnection.ClearAllPools();
                await using var drop = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{name}\" WITH (FORCE)", admin); await drop.ExecuteNonQueryAsync();
            }
        }

        static NguoiDung User(string email, long revision) => new() {
            Email = email, EmailChuanHoa = email.ToUpperInvariant(), HoTen = "Migration fixture", MatKhauBam = "fixture-hash",
            PhienBanLapLich = revision, NgayTao = DateTimeOffset.UtcNow, NgayCapNhat = DateTimeOffset.UtcNow,
        };
        async Task SeedUser(ApplicationDbContext db, NguoiDung user)
        {
            // Cố ý tạo dữ liệu ở cấu trúc cũ, khi các cột cấu hình học của bản nâng cấp chưa tồn tại.
            await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO nguoi_dung (email,email_chuan_hoa,ho_ten,mat_khau_bam,phien_ban_lap_lich,ngay_tao,ngay_cap_nhat) VALUES ({user.Email},{user.EmailChuanHoa},{user.HoTen},{user.MatKhauBam},{user.PhienBanLapLich},{user.NgayTao},{user.NgayCapNhat})");
            user.Ma = await db.DanhSachNguoiDung.Where(u => u.Email == user.Email).Select(u => u.Ma).SingleAsync();
        }
        var oldDate = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
        NhiemVu Task(int owner, string title, int progress = 0, NhiemVu? parent = null, string? checklist = null) => new() {
            MaNguoiDung = owner, TieuDe = title, NhiemVuCha = parent, TienDo = progress, ChecklistJson = checklist,
            TrangThai = progress == 100 ? TrangThaiNhiemVu.Completed : progress == 0 ? TrangThaiNhiemVu.NotStarted : TrangThaiNhiemVu.InProgress,
            NgayHoanThanh = progress == 100 ? oldDate : null, NgayTao = oldDate, NgayCapNhat = oldDate,
        };
        async Task SeedTask(ApplicationDbContext db, NhiemVu task)
        {
            // EF hiện đọc/ghi cả cột ghim. Tạo và đọc dữ liệu thử theo đúng cấu trúc trước migration.
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO nhiem_vu (ma_nguoi_dung, ma_nhiem_vu_cha, tieu_de, checklist, loai, muc_do_uu_tien,
                    tien_do, trang_thai, ngay_hoan_thanh, ngay_xoa, ma_nhom_xoa, ngay_tao, ngay_cap_nhat)
                VALUES ({task.MaNguoiDung}, {task.NhiemVuCha?.Ma}, {task.TieuDe},
                    CAST({task.ChecklistJson} AS jsonb), {task.Loai.ToString()}, {task.MucDoUuTien.ToString()},
                    {task.TienDo}, {task.TrangThai.ToString()}, {task.NgayHoanThanh}, {task.NgayXoa}, {task.MaNhomXoa}, {task.NgayTao}, {task.NgayCapNhat})
                """);
            task.Ma = await db.DanhSachNhiemVu.IgnoreQueryFilters()
                .Where(n => n.MaNguoiDung == task.MaNguoiDung && n.TieuDe == task.TieuDe).Select(n => n.Ma).SingleAsync();
        }
        const string fullChecklist = """[{"tieuDe":"Đọc ví dụ","daXong":true},{"tieuDe":"Làm bài","daXong":true}]""";
        const string halfChecklist = """[{"tieuDe":"Một","daXong":true},{"tieuDe":"Hai","daXong":false}]""";

        await InDatabase(async db =>
        {
            var owner = User("migration@example.test", 7); var other = User("other-migration@example.test", 11);
            await SeedUser(db, owner); await SeedUser(db, other);
            var a = Task(owner.Ma, "Đồ án", 100);
            var b = Task(owner.Ma, "Slide", 100, a, fullChecklist);
            var c = Task(owner.Ma, "Nội dung slide", 0, b);
            var d = Task(owner.Ma, "Báo cáo", 100, a);
            var floor = Task(owner.Ma, "Không làm tròn thành 100", 100);
            var almost = Task(owner.Ma, "99 phần trăm", 99, floor);
            var done = Task(owner.Ma, "Đã xong", 100, floor);
            var completed = Task(owner.Ma, "Cha được hoàn thành từ con");
            var completedChild = Task(owner.Ma, "Con đã xong", 100, completed);
            var manualLeaf = Task(owner.Ma, "Lá nhập tay", 25);
            var checklistLeaf = Task(owner.Ma, "Lá checklist", 0, checklist: halfChecklist);
            var untouched = Task(other.Ma, "Tài khoản khác", 40);
            foreach (var task in new[] { a, b, c, d, floor, almost, done, completed, completedChild, manualLeaf, checklistLeaf, untouched })
                await SeedTask(db, task);
            var trash = Task(owner.Ma, "Nhóm trong thùng rác", 100, checklist: fullChecklist);
            trash.NgayXoa = oldDate; await SeedTask(db, trash);
            await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE nhiem_vu SET ma_nhom_xoa = ma WHERE ma = {trash.Ma}");
            var trashChild = Task(owner.Ma, "Con chưa xong trong thùng rác", 0, trash);
            trashChild.NgayXoa = oldDate; trashChild.MaNhomXoa = trash.Ma;
            await SeedTask(db, trashChild);
            // Kiểm tra migration lịch sử bằng SQL để không phụ thuộc entity đã được gỡ.
            await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO lich_nhac(ma_nguoi_dung,ma_nhiem_vu,thoi_diem_nhac,noi_dung,moc_nhac,trang_thai,so_lan_thu,ngay_tao,ngay_cap_nhat) VALUES ({owner.Ma},{completed.Ma},{DateTimeOffset.UtcNow.AddDays(1)},'Pending fixture','Deadline','Pending',0,now(),now())");
            var pending = new ThongBao { MaNguoiDung = owner.Ma, MaNhiemVu = completed.Ma, ThoiDiemNhac = DateTimeOffset.UtcNow.AddDays(1), NoiDung = "Pending fixture" };
            var delivered = new ThongBao { MaNguoiDung = owner.Ma, MaNhiemVu = floor.Ma, ThoiDiemNhac = oldDate,
                TrangThaiPhat = TrangThaiPhatThongBao.Delivered, NgayPhat = oldDate, NoiDung = "Delivered fixture" };
            db.AddRange(pending, delivered); await db.SaveChangesAsync(); db.ChangeTracker.Clear();

            await db.GetService<IMigrator>().MigrateAsync(After);
            var all = await db.DanhSachNhiemVu.IgnoreQueryFilters().Select(n => new NhiemVu {
                Ma = n.Ma, MaNguoiDung = n.MaNguoiDung, MaNhiemVuCha = n.MaNhiemVuCha, TieuDe = n.TieuDe,
                ChecklistJson = n.ChecklistJson, TienDo = n.TienDo, TrangThai = n.TrangThai,
                NgayHoanThanh = n.NgayHoanThanh, NgayXoa = n.NgayXoa, MaNhomXoa = n.MaNhomXoa,
            }).ToListAsync();
            NhiemVu Read(int id) => all.Single(n => n.Ma == id);
            var preserved = all.Single(n => n.MaNhiemVuCha == b.Ma && n.TieuDe == "Công việc chung");
            Check(JsonDocument.Parse(preserved.ChecklistJson!).RootElement.GetArrayLength() == 2 && preserved.TienDo == 100, "parent checklist preserved verbatim in a real completed child");
            Check(preserved.MaNguoiDung == owner.Ma && preserved.NgayHoanThanh == oldDate && preserved.NgayXoa == null, "preserved child ownership and completed date");
            Check(Read(b.Ma).ChecklistJson == null && Read(b.Ma).TienDo == 50, "parent own checklist no longer competes with children");
            Check(Read(a.Ma).TienDo == 75 && Read(a.Ma).TrangThai == TrangThaiNhiemVu.InProgress && Read(a.Ma).NgayHoanThanh == null, "nested progress rolls upward and reopens the whole chain");
            Check(Read(floor.Ma).TienDo == 99 && Read(floor.Ma).NgayHoanThanh == null, "incomplete descendant never rounds parent to Completed");
            Check(Read(completed.Ma).TienDo == 100 && Read(completed.Ma).NgayHoanThanh != null, "all completed direct children complete their parent");
            Check(Read(manualLeaf.Ma).TienDo == 25 && Read(untouched.Ma).TienDo == 40, "manual leaves retain progress");
            Check(Read(checklistLeaf.Ma).TienDo == 50, "leaf checklist source is normalized");
            var preservedTrash = all.Single(n => n.MaNhiemVuCha == trash.Ma && n.TieuDe == "Công việc chung");
            Check(preservedTrash.NgayXoa == oldDate && preservedTrash.MaNhomXoa == trash.Ma && preservedTrash.TienDo == 100, "trashed checklist child remains within its deleted group");
            Check(Read(trash.Ma).TienDo == 50 && Read(trash.Ma).ChecklistJson == null, "deleted tree normalized separately");
            var history = await db.DanhSachLichSuNhiemVu.Where(h => h.MaNhiemVu == a.Ma || h.MaNhiemVu == b.Ma || h.MaNhiemVu == preserved.Ma).ToListAsync();
            Check(history.All(h => h.NguonThayDoi == "System") && history.Any(h => h.MaNhiemVu == preserved.Ma && h.HanhDong == HanhDongLichSuNhiemVu.Created), "system history records the preserved child");
            var reopened = history.Single(h => h.MaNhiemVu == b.Ma);
            Check(reopened.HanhDong == HanhDongLichSuNhiemVu.Reopened && JsonDocument.Parse(reopened.DuLieuTruoc!).RootElement.GetProperty("tienDo").GetInt32() == 100 &&
                JsonDocument.Parse(reopened.DuLieuTruoc!).RootElement.GetProperty("checklist").GetArrayLength() == 2 &&
                JsonDocument.Parse(reopened.DuLieuSau!).RootElement.GetProperty("tienDo").GetInt32() == 50, "camelCase before/after history retains original progress and checklist");
            Check(await db.DanhSachNguoiDung.Where(u => u.Ma == owner.Ma).Select(u => u.PhienBanLapLich).SingleAsync() == 8 &&
                await db.DanhSachNguoiDung.Where(u => u.Ma == other.Ma).Select(u => u.PhienBanLapLich).SingleAsync() == 11, "only affected owners advance their revision once");
            Check(await db.Database.SqlQueryRaw<int>("SELECT count(*)::integer AS \"Value\" FROM lich_nhac WHERE trang_thai='Cancelled'").SingleAsync() == 1, "newly completed task reminder cancelled");
            Check((await db.DanhSachThongBao.IgnoreQueryFilters().SingleAsync(r => r.Ma == pending.Ma)).TrangThaiPhat == TrangThaiPhatThongBao.Cancelled &&
                (await db.DanhSachThongBao.IgnoreQueryFilters().SingleAsync(r => r.Ma == delivered.Ma)).TrangThaiPhat == TrangThaiPhatThongBao.Delivered, "pending delivery cancelled while delivered history preserved");
            var taskCount = all.Count; var historyCount = await db.DanhSachLichSuNhiemVu.CountAsync();
            await db.GetService<IMigrator>().MigrateAsync(After);
            Check(await db.DanhSachNhiemVu.IgnoreQueryFilters().CountAsync() == taskCount && await db.DanhSachLichSuNhiemVu.CountAsync() == historyCount, "migration history prevents duplicate preservation");
            try { await db.GetService<IMigrator>().MigrateAsync(Before); throw new Exception("Expected downgrade rejection"); }
            catch (PostgresException e) when (e.MessageText.Contains("TASK_PROGRESS_MIGRATION_NO_AUTOMATIC_DOWNGRADE")) { Check(true, "downgrade explicitly refuses to delete preserved work"); }
            Check((await db.Database.GetAppliedMigrationsAsync()).Contains(After) && await db.DanhSachNhiemVu.IgnoreQueryFilters().CountAsync() == taskCount, "failed downgrade leaves data and migration history intact");
            // Tạo dữ liệu theo tên bảng cũ rồi nâng cấp để kiểm tra đổi tên không làm mất lịch sử học.
            const string beforeRename = "20261114092625_CaiTienLapKeHoach";
            await db.GetService<IMigrator>().MigrateAsync(beforeRename);
            await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO doan_thuc_hoc(ma_nguoi_dung,ma_nhiem_vu,bat_dau_thuc_te,ket_thuc_thuc_te) VALUES ({owner.Ma},{manualLeaf.Ma},{oldDate},{oldDate.AddMinutes(20)}),({owner.Ma},{manualLeaf.Ma},{oldDate.AddMinutes(30)},NULL)");
            await db.GetService<IMigrator>().MigrateAsync("20261114122238_DoiTenRangBuocKhongNullKhoangHoc");
            var intervals = await db.DanhSachKhoangThoiGianHoc.OrderBy(x => x.Ma).ToListAsync();
            Check(intervals.Count == 2 && intervals[0].MaNhiemVu == manualLeaf.Ma && intervals[0].BatDauThucTe == oldDate &&
                intervals[0].KetThucThucTe == oldDate.AddMinutes(20) && intervals[1].KetThucThucTe == null,
                "rename preserves closed and running intervals, IDs and task links");
            var intervalIds = intervals.Select(x => x.Ma).ToArray();
            var constraintNames = await db.Database.SqlQueryRaw<string>("SELECT conname AS \"Value\" FROM pg_constraint WHERE conrelid='khoang_thoi_gian_hoc'::regclass").ToListAsync();
            Check(constraintNames.Count(x => x.StartsWith("PK_") || x.StartsWith("FK_") || x.StartsWith("CK_")) == 6 &&
                constraintNames.All(x => x.Contains("khoang_thoi_gian_hoc") && !x.Contains("doan_thuc_hoc")),
                "all primary, foreign and check constraints use the new name");
            var indexNames = await db.Database.SqlQueryRaw<string>("SELECT indexname AS \"Value\" FROM pg_indexes WHERE tablename='khoang_thoi_gian_hoc'").ToListAsync();
            Check(indexNames.Count == 5 && indexNames.All(x => x.Contains("khoang_thoi_gian_hoc")), "all indexes use the new name, including primary key");
            Check(await db.Database.SqlQueryRaw<string>("SELECT pg_get_serial_sequence('khoang_thoi_gian_hoc','ma') AS \"Value\"").SingleAsync() == "public.khoang_thoi_gian_hoc_ma_seq", "identity sequence renamed with its current value intact");
            await db.GetService<IMigrator>().MigrateAsync(beforeRename);
            Check(await db.Database.SqlQueryRaw<int>("SELECT count(*)::integer AS \"Value\" FROM doan_thuc_hoc").SingleAsync() == 2, "downgrade restores original table without deleting intervals");
            await db.Database.MigrateAsync();
            db.ChangeTracker.Clear();
            Check(!await db.DanhSachNhiemVu.IgnoreQueryFilters().AnyAsync(n => n.DaGhim), "existing tasks are unpinned after upgrading");
            Check((await db.DanhSachKhoangThoiGianHoc.OrderBy(x => x.Ma).Select(x => x.Ma).ToArrayAsync()).SequenceEqual(intervalIds), "upgrade again retains original interval identities");
            var later = new KhoangThoiGianHoc { MaNguoiDung = owner.Ma, MaNhiemVu = manualLeaf.Ma, BatDauThucTe = oldDate.AddDays(1), KetThucThucTe = oldDate.AddDays(1).AddMinutes(10) };
            db.Add(later); await db.SaveChangesAsync();
            Check(later.Ma > intervalIds.Max(), "new interval identity continues after rename and rollback");
            var upgradedUser = await db.DanhSachNguoiDung.SingleAsync(u => u.Ma == owner.Ma);
            Check(upgradedUser.PhienHocPhut == 25 && upgradedUser.NghiGiuaPhienPhut == 5 && upgradedUser.GioiHanHocMoiNgayPhut == 180,
                "later planning migration backfills shared settings without changing task work");
            Check(await db.DanhSachNgoaiLeGioRanh.CountAsync() == 0 && await db.DanhSachNhacKeHoach.CountAsync() == 0 &&
                await db.DanhSachCauHinhNhacKeHoach.CountAsync() == 0, "later planning migration creates empty exception/reminder stores, reminders opt in");
        });

        await InDatabase(async db =>
        {
            var owner = User("migration-bounds@example.test", 0); await SeedUser(db, owner);
            var root = Task(owner.Ma, "1000 nút trước khi bảo toàn", 100, checklist: fullChecklist); await SeedTask(db, root);
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO nhiem_vu (ma_nguoi_dung, ma_nhiem_vu_cha, tieu_de, loai, muc_do_uu_tien, tien_do, trang_thai, ngay_tao, ngay_cap_nhat)
                SELECT {owner.Ma}, {root.Ma}, 'Con ' || i, 'Personal', 'Medium', 0, 'NotStarted', {oldDate}, {oldDate}
                FROM generate_series(0, 998) AS i
                """);
            try { await db.GetService<IMigrator>().MigrateAsync(After); throw new Exception("Expected tree size rejection"); }
            catch (PostgresException e) when (e.MessageText.Contains("TASK_PROGRESS_MIGRATION_TREE_LIMIT")) { Check(true, "prospective preserved child respects 1000-node limit"); }
            Check(!(await db.Database.GetAppliedMigrationsAsync()).Contains(After), "failed preflight does not mark migration applied");
            Check(await db.DanhSachNhiemVu.CountAsync() == 1000 && await db.DanhSachNhiemVu.AnyAsync(n => n.Ma == root.Ma && n.ChecklistJson != null) &&
                await db.DanhSachLichSuNhiemVu.CountAsync() == 0 && await db.DanhSachNguoiDung.Select(u => u.PhienBanLapLich).SingleAsync() == 0,
                "failed preflight leaves checklist, progress, history and revision untouched");
        });
        Console.WriteLine($"PASS: {count} legacy progress migration assertions; active/deleted trees, preservation, rollup, revision, reminders and downgrade protection.");
    }
}
