using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using StudyPlanner.Application.HocTap;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Infrastructure.Data;
namespace StudyPlanner.Infrastructure.HocTap;

public sealed class EfHocTapStore(ApplicationDbContext db) : IHocTapStore
{
    public IQueryable<T> TruyVan<T>() where T : class => db.Set<T>();
    public Task<List<T>> DanhSachAsync<T>(IQueryable<T> query, CancellationToken ct) => query.ToListAsync(ct);
    public Task<T?> DauTienAsync<T>(IQueryable<T> query, CancellationToken ct) => query.FirstOrDefaultAsync(ct);
    public Task<int> DemAsync<T>(IQueryable<T> query, CancellationToken ct) => query.CountAsync(ct);
    public void Them<T>(T entity) where T : class => db.Set<T>().Add(entity);
    public void Xoa<T>(T entity) where T : class => db.Set<T>().Remove(entity);
    public async Task LuuAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
    public async Task<IHocTapTransaction> BatDauAsync(bool ghi, CancellationToken ct) =>
        new GiaoDich(await db.Database.BeginTransactionAsync(ghi ? IsolationLevel.ReadCommitted : IsolationLevel.RepeatableRead, ct));
    public async Task<NguoiDung> NguoiDungAsync(int chu, bool khoa, CancellationToken ct)
    {
        // JWT validation may already have tracked User: reload the locked row, never trust that earlier snapshot.
        var user = khoa
            ? await db.DanhSachNguoiDung.FromSqlInterpolated($"SELECT * FROM nguoi_dung WHERE ma = {chu} FOR UPDATE").SingleOrDefaultAsync(ct)
            : await db.DanhSachNguoiDung.SingleOrDefaultAsync(x => x.Ma == chu, ct);
        if (user is null) throw new HocTapException(404, "RESOURCE_NOT_FOUND", "Không tìm thấy tài khoản hoạt động.");
        await db.Entry(user).ReloadAsync(ct);
        if (!user.DangHoatDong) throw new HocTapException(404, "RESOURCE_NOT_FOUND", "Không tìm thấy tài khoản hoạt động.");
        return user;
    }
    public async Task<List<NhiemVu>> CayAsync(int chu, int ma, CancellationToken ct)
    {
        var node = await db.DanhSachNhiemVu.SingleOrDefaultAsync(x => x.Ma == ma && x.MaNguoiDung == chu, ct)
            ?? throw new HocTapException(404, "RESOURCE_NOT_FOUND", "Không tìm thấy nhiệm vụ.");
        var seen = new HashSet<int>();
        while (node.MaNhiemVuCha is int parent)
        {
            if (!seen.Add(node.Ma) || seen.Count >= QuyTacCay.DoSauToiDa) throw new HocTapException(409, "TASK_TREE_INVALID", "Cây có vòng hoặc vượt độ sâu.");
            node = await db.DanhSachNhiemVu.SingleOrDefaultAsync(x => x.Ma == parent && x.MaNguoiDung == chu, ct)
                ?? throw new HocTapException(409, "TASK_TREE_INVALID", "Cha không cùng chủ hoặc không tồn tại.");
        }
        var root = node.Ma;
        var result = await db.DanhSachNhiemVu.FromSqlInterpolated($"""
            WITH RECURSIVE cay AS (
                SELECT * FROM nhiem_vu WHERE ma = {root} AND ma_nguoi_dung = {chu}
                UNION ALL
                SELECT n.* FROM nhiem_vu n JOIN cay c ON n.ma_nhiem_vu_cha = c.ma WHERE n.ma_nguoi_dung = {chu}
            ) SELECT * FROM cay LIMIT 1001
            """).ToListAsync(ct);
        QuyTacCay.KiemTra(result); return result;
    }
    private sealed class GiaoDich(IDbContextTransaction tx) : IHocTapTransaction
    {
        public Task CommitAsync(CancellationToken ct) => tx.CommitAsync(ct);
        public ValueTask DisposeAsync() => tx.DisposeAsync();
    }
}
