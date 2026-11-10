using System.ComponentModel.DataAnnotations;
using StudyPlanner.Domain.Enums;
namespace StudyPlanner.Application.HocTap;
public sealed class NhiemVuRequest
{
    [Required, StringLength(200)] public string TieuDe { get; init; } = "";
    [StringLength(20000)] public string? MoTa { get; init; }
    public int? MaHocKy { get; init; }
    public int? MaMonHoc { get; init; }
    public LoaiNhiemVu? Loai { get; init; }
    public MucDoUuTienNhiemVu MucDoUuTien { get; init; } = MucDoUuTienNhiemVu.Medium;
    public DateTimeOffset? HanHoanThanh { get; init; }
    public DateTimeOffset? BatDauSuKien { get; init; }
    public DateTimeOffset? KetThucSuKien { get; init; }
    [Range(1,100000)] public int? ThoiLuongDuKienPhut { get; init; }
    [Range(0,8760)] public int? NhacTruocGio { get; init; }
    public bool HuyPhienLienQuan { get; init; }
}
