using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.HocTap;
using StudyPlanner.Application.UuTien;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Infrastructure.Data;

namespace StudyPlanner.Infrastructure.UuTien;

public sealed record QuyTacUuTienResponse(int Ma, string Ten, int PhienBan, bool DangSuDung, CauHinhUuTien CauHinh, DateTimeOffset? NgayTao);
public sealed record TrangQuyTacResponse(IReadOnlyList<QuyTacUuTienResponse> DanhSach, int Trang, int KichThuocTrang, int TongSo);

public sealed class QuyTacUuTienService(ApplicationDbContext db, TimeProvider clock)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static QuyTacUuTienResponse Dto(QuyTacUuTien row)
    {
        try
        {
            var config = JsonSerializer.Deserialize<CauHinhUuTien>(row.CauHinh, Json);
            QuyTacCauHinhUuTien.KiemTra(config);
            return new(row.Ma, row.Ten, row.PhienBan, row.DangSuDung, config!, row.NgayTao);
        }
        catch (Exception error) when (error is JsonException or HocTapException)
        { throw new HocTapException(503, "PRIORITY_CONFIG_UNAVAILABLE", "Cấu hình quy tắc đã lưu không hợp lệ. Cần quản trị viên kiểm tra dữ liệu."); }
    }

    public async Task<QuyTacUuTienResponse> DangDungAsync(CancellationToken ct)
    {
        var row = await db.DanhSachQuyTacUuTien.AsNoTracking().SingleOrDefaultAsync(x => x.DangSuDung, ct);
        return row is null ? new(0, "Mặc định", 0, true, QuyTacCauHinhUuTien.MacDinh(), null) : Dto(row);
    }
    public async Task<TrangQuyTacResponse> DanhSachAsync(int trang, int kichThuocTrang, CancellationToken ct)
    {
        if (trang < 1 || kichThuocTrang is < 1 or > 50 || (long)(trang - 1) * kichThuocTrang > int.MaxValue)
            throw new HocTapException(400, "VALIDATION_ERROR", "Phân trang quy tắc không hợp lệ.");
        var query = db.DanhSachQuyTacUuTien.AsNoTracking();
        var total = await query.CountAsync(ct);
        var rows = await query.OrderByDescending(x => x.PhienBan).Skip((trang - 1) * kichThuocTrang).Take(kichThuocTrang).ToListAsync(ct);
        return new(rows.Select(Dto).ToArray(), trang, kichThuocTrang, total);
    }
    private Task KhoaAsync(CancellationToken ct) => db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(193701, 11)", ct);
    public async Task<QuyTacUuTienResponse> TaoAsync(string? ten, CauHinhUuTien? config, CancellationToken ct)
    {
        ten = ten?.Trim();
        if (string.IsNullOrWhiteSpace(ten) || ten.Length > 200) throw new HocTapException(400, "VALIDATION_ERROR", "Tên quy tắc cần từ 1 đến 200 ký tự.");
        QuyTacCauHinhUuTien.KiemTra(config);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await KhoaAsync(ct);
        // Allocate from the existing identity sequence, never reusing a deleted version.
        var highest = Math.Max(await db.DanhSachQuyTacUuTien.MaxAsync(x => (int?)x.PhienBan, ct) ?? 0,
            await db.DanhSachQuyTacUuTien.MaxAsync(x => (int?)x.Ma, ct) ?? 0);
        var next = await db.Database.SqlQueryRaw<long>("SELECT nextval(pg_get_serial_sequence('quy_tac_uu_tien', 'ma')) AS \"Value\"").SingleAsync(ct);
        if (next <= highest)
            next = await db.Database.SqlQuery<long>($"SELECT setval(pg_get_serial_sequence('quy_tac_uu_tien', 'ma'), {(long)highest + 1}, true) AS \"Value\"").SingleAsync(ct);
        if (next > int.MaxValue) throw new HocTapException(409, "PRIORITY_VERSION_LIMIT", "Đã đạt giới hạn phiên bản quy tắc.");
        var row = new QuyTacUuTien { Ma = (int)next, Ten = ten, PhienBan = (int)next, CauHinh = JsonSerializer.Serialize(config, Json), NgayTao = clock.GetUtcNow() };
        db.Add(row); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        return Dto(row);
    }
    public async Task<QuyTacUuTienResponse> ApDungAsync(int id, int? phienBanDangDung, CancellationToken ct)
    {
        if (phienBanDangDung is null or < 0) throw new HocTapException(400, "VALIDATION_ERROR", "Cần phiên bản quy tắc đang dùng để áp dụng.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await KhoaAsync(ct);
        var active = await db.DanhSachQuyTacUuTien.SingleOrDefaultAsync(x => x.DangSuDung, ct);
        if ((active?.PhienBan ?? 0) != phienBanDangDung) throw new HocTapException(409, "PRIORITY_RULE_CHANGED", "Quy tắc đang dùng đã thay đổi. Tải lại trước khi áp dụng.");
        var target = await db.DanhSachQuyTacUuTien.SingleOrDefaultAsync(x => x.Ma == id, ct)
            ?? throw new HocTapException(404, "RESOURCE_NOT_FOUND", "Không tìm thấy quy tắc.");
        _ = Dto(target);
        if (active?.Ma != id)
        {
            if (active is not null) { active.DangSuDung = false; await db.SaveChangesAsync(ct); }
            target.DangSuDung = true; await db.SaveChangesAsync(ct);
        }
        await tx.CommitAsync(ct); return Dto(target);
    }
    public async Task XoaAsync(int id, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await KhoaAsync(ct);
        var row = await db.DanhSachQuyTacUuTien.SingleOrDefaultAsync(x => x.Ma == id, ct)
            ?? throw new HocTapException(404, "RESOURCE_NOT_FOUND", "Không tìm thấy quy tắc.");
        if (row.DangSuDung) throw new HocTapException(409, "PRIORITY_RULE_ACTIVE", "Không thể xóa quy tắc đang dùng. Hãy áp dụng phiên bản khác trước.");
        db.Remove(row); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
    }
}
