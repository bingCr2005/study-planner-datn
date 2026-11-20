using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Application.HocTap;

public sealed record CongViecLapLich(int MaNhiemVu, string TieuDe, DateTimeOffset? HanHieuLuc,
    MucDoUuTienNhiemVu MucDoUuTien, int SoPhutCanXep)
{
    public TrangThaiNhiemVu TrangThai { get; init; }
}
