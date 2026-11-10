using System.ComponentModel.DataAnnotations;

namespace StudyPlanner.Application.HocTap;

public sealed class PhienTuHocRequest
{
    [Range(1, int.MaxValue)] public int MaNhiemVu { get; init; }
    public DateTimeOffset BatDauDuKien { get; init; }
    public DateTimeOffset KetThucDuKien { get; init; }
    [StringLength(2000)] public string? GhiChu { get; init; }
}
