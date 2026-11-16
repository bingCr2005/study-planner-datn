using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyPlanner.Application.HocTap;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Infrastructure.Data;

namespace StudyPlanner.Infrastructure.TepDinhKem;

public sealed record TepDinhKemResponse(int Ma, string TenTep, long KichThuocByte, DateTimeOffset NgayTao);
public sealed class TepDinhKemService(ApplicationDbContext db, HocTapDieuPhoi dieuPhoi, KhoTepNhiemVu kho,
    TimeProvider dongHo, Microsoft.Extensions.Logging.ILogger<TepDinhKemService> logger)
{
    private async Task<NhiemVu> NhiemVuAsync(int chu, int ma, CancellationToken ct) =>
        await db.DanhSachNhiemVu.SingleOrDefaultAsync(n => n.Ma == ma && n.MaNguoiDung == chu, ct)
        ?? throw new HocTapException(404, "RESOURCE_NOT_FOUND", "Không tìm thấy nhiệm vụ.");
    private static TepDinhKemResponse Dto(TepDinhKemNhiemVu x) => new(x.Ma, x.TenTep, x.KichThuocByte, x.NgayTao);

    public async Task<HocTapResponse<IReadOnlyList<TepDinhKemResponse>>> DanhSachAsync(int chu, int ma, CancellationToken ct)
    {
        await using var tx = await dieuPhoi.MoAsync(chu, null, false, ct);
        await NhiemVuAsync(chu, ma, ct);
        var rows = await db.DanhSachTepDinhKemNhiemVu.AsNoTracking().Where(x => x.MaNhiemVu == ma && x.MaNguoiTaiLen == chu)
            .OrderBy(x => x.NgayTao).ThenBy(x => x.Ma).ToListAsync(ct);
        await tx.CommitAsync(false, ct);
        return new(tx.NguoiDung.PhienBanLapLich, rows.Select(Dto).ToArray());
    }
    public async Task<HocTapResponse<TepDinhKemResponse>> ThemAsync(int chu, int ma, long phienBan, string ten,
        Stream noiDung, long kichThuoc, CancellationToken ct)
    {
        // Loại đường dẫn do trình duyệt cũ gửi kèm; giữ tên hiển thị, không tin MIME từ client.
        ten = ten.Replace('\\', '/').Split('/').Last().Trim();
        if (ten.Length is 0 or > 255 || ten.Any(char.IsControl) || ten is "." or "..")
            throw new HocTapException(400, "VALIDATION_ERROR", "Tên tệp phải dài từ 1 đến 255 ký tự và không chứa ký tự điều khiển.");
        await using var tx = await dieuPhoi.MoAsync(chu, phienBan, true, ct);
        await NhiemVuAsync(chu, ma, ct);
        if (await db.DanhSachTepDinhKemNhiemVu.CountAsync(x => x.MaNhiemVu == ma, ct) >= 20)
            throw new HocTapException(400, "ATTACHMENT_LIMIT", "Mỗi nhiệm vụ tối đa 20 tệp đính kèm.");
        var khoa = await kho.GhiAsync(noiDung, kichThuoc, ct);
        var tep = new TepDinhKemNhiemVu { MaNhiemVu = ma, MaNguoiTaiLen = chu, TenTep = ten,
            KhoaLuuTru = khoa, LoaiTep = "application/octet-stream", KichThuocByte = kichThuoc, NgayTao = dongHo.GetUtcNow() };
        db.Add(tep);
        // Nếu mất kết nối lúc COMMIT thì kết quả có thể chưa xác định. Giữ tệp để tránh xóa nhầm;
        // lượt dọn kho chỉ xóa tệp mồ côi sau 24 giờ và sau khi đọc lại database thành công.
        await tx.CommitAsync(true, ct);
        return new(tx.NguoiDung.PhienBanLapLich, Dto(tep));
    }
    public async Task<(Stream NoiDung, string Ten)> TaiAsync(int chu, int ma, int tep, CancellationToken ct)
    {
        await using var tx = await dieuPhoi.MoAsync(chu, null, false, ct);
        var row = await TimAsync(chu, ma, tep, ct);
        var stream = kho.Mo(row.KhoaLuuTru);
        try { await tx.CommitAsync(false, ct); return (stream, row.TenTep); }
        catch { await stream.DisposeAsync(); throw; }
    }
    public async Task<long> XoaAsync(int chu, int ma, int tep, long phienBan, CancellationToken ct)
    {
        await using var tx = await dieuPhoi.MoAsync(chu, phienBan, true, ct);
        var row = await TimAsync(chu, ma, tep, ct);
        db.Remove(row);
        await tx.CommitAsync(true, ct);
        try { kho.Xoa(row.KhoaLuuTru); }
        catch (IOException) { logger.LogWarning("Sẽ thử dọn lại tệp đính kèm {MaTep} ở lượt dọn kho.", tep); }
        catch (UnauthorizedAccessException) { logger.LogWarning("Chưa thể dọn nội dung tệp đính kèm {MaTep}.", tep); }
        return tx.NguoiDung.PhienBanLapLich;
    }
    private async Task<TepDinhKemNhiemVu> TimAsync(int chu, int ma, int tep, CancellationToken ct) =>
        await db.DanhSachTepDinhKemNhiemVu.SingleOrDefaultAsync(x => x.Ma == tep && x.MaNhiemVu == ma && x.MaNguoiTaiLen == chu, ct)
        ?? throw new HocTapException(404, "RESOURCE_NOT_FOUND", "Không tìm thấy tệp đính kèm.");
}
