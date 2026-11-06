using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Application.HocTap;

public sealed record PhienTuHocResponse(int Ma, int MaNhiemVu, int? MaLanApDung,
    DateTimeOffset BatDauDuKien, DateTimeOffset KetThucDuKien, TrangThaiPhienTuHoc TrangThai,
    string? GhiChu, DateTimeOffset NgayTao, DateTimeOffset NgayCapNhat)
{
    public static PhienTuHocResponse Tu(PhienTuHoc p) => new(p.Ma, p.MaNhiemVu, p.MaLanApDung,
        p.BatDauDuKien, p.KetThucDuKien, p.TrangThai, p.GhiChu, p.NgayTao, p.NgayCapNhat);
}
