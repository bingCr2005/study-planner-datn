using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Application.HocTap;

public sealed record PhienTuHocResponse(int Ma, int MaNhiemVu, int? MaLanApDung,
    DateTimeOffset BatDauDuKien, DateTimeOffset KetThucDuKien, TrangThaiPhienTuHoc TrangThai,
    string? GhiChu, DateTimeOffset NgayTao, DateTimeOffset NgayCapNhat)
{
    public DateTimeOffset? BatDauBamGio { get; init; }
    public long? SoGiayDaHoc { get; init; }
    public string? TieuDeNhiemVu { get; init; }
    public int? MaHocKy { get; init; }
    public int? MaMonHoc { get; init; }

    public static PhienTuHocResponse Tu(PhienTuHoc p) => new(p.Ma, p.MaNhiemVu, p.MaLanApDung,
        p.BatDauDuKien, p.KetThucDuKien, p.TrangThai, p.GhiChu, p.NgayTao, p.NgayCapNhat);

    public static PhienTuHocResponse Tu(PhienTuHoc p, IReadOnlyList<DoanThucHoc> doan) => Tu(p) with
    {
        BatDauBamGio = doan.SingleOrDefault(x => x.KetThucThucTe is null)?.BatDauThucTe,
        SoGiayDaHoc = doan.Count == 0 ? null : doan.Where(x => x.KetThucThucTe.HasValue)
            .Sum(x => (x.KetThucThucTe!.Value - x.BatDauThucTe).Ticks) / TimeSpan.TicksPerSecond
    };
}
