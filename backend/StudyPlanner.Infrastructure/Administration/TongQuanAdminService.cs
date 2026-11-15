using System.Data;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Domain.Enums;
using StudyPlanner.Infrastructure.Data;

namespace StudyPlanner.Infrastructure.Administration;

public sealed record SinhVienMoiResponse(int Ma, string HoTen, string Email, bool DangHoatDong, DateTimeOffset NgayTao);
public sealed record TongQuanAdminResponse(int TongSinhVien, int DangMo, int DaKhoa, int MoiTrong7Ngay,
    DateTimeOffset TuNgay, DateTimeOffset DenNgay, IReadOnlyList<SinhVienMoiResponse> SinhVienMoi);

public sealed class TongQuanAdminService(ApplicationDbContext db, TimeProvider clock)
{
    public async Task<TongQuanAdminResponse> LayAsync(CancellationToken ct)
    {
        var now = clock.GetUtcNow(); var from = now.AddDays(-7);
        // One consistent snapshot for the aggregate and newest five, including during account changes.
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        var students = db.DanhSachNguoiDung.AsNoTracking().Where(x => x.VaiTro == VaiTroNguoiDung.Student);
        var stats = await students.GroupBy(x => 1).Select(group => new {
            Total = group.Count(), Open = group.Count(x => x.DangHoatDong), Locked = group.Count(x => !x.DangHoatDong),
            Recent = group.Count(x => x.NgayTao >= from && x.NgayTao <= now)
        }).SingleOrDefaultAsync(ct);
        var latest = await students.OrderByDescending(x => x.NgayTao).ThenByDescending(x => x.Ma).Take(5)
            .Select(x => new SinhVienMoiResponse(x.Ma, x.HoTen, x.Email, x.DangHoatDong, x.NgayTao)).ToListAsync(ct);
        await tx.CommitAsync(ct);
        return new(stats?.Total ?? 0, stats?.Open ?? 0, stats?.Locked ?? 0, stats?.Recent ?? 0, from, now, latest);
    }
}
