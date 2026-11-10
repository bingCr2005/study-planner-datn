using System.ComponentModel.DataAnnotations;

namespace StudyPlanner.Application.HocTap;

public sealed class XemTruocLichRequest
{
    public DateOnly? NgayBatDau { get; init; }
    [Range(1, 31)] public int SoNgay { get; init; } = 14;
    public int? MaHocKy { get; init; }
    public int? MaMonHoc { get; init; }
    public List<int> DanhSachMaNhiemVu { get; init; } = [];
}
