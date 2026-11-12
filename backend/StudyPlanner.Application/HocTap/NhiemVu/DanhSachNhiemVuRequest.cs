using System.ComponentModel.DataAnnotations;
using StudyPlanner.Domain.Enums;
namespace StudyPlanner.Application.HocTap;
public sealed class DanhSachNhiemVuRequest
{
    [Range(1,int.MaxValue)] public int Trang { get; init; } = 1;
    [Range(1,100)] public int KichThuocTrang { get; init; } = 20;
    [StringLength(200)] public string? TuKhoa { get; init; }
    public int? MaHocKy { get; init; }
    public int? MaMonHoc { get; init; }
    public LoaiNhiemVu? Loai { get; init; }
    public MucDoUuTienNhiemVu? MucDoUuTien { get; init; }
    public TrangThaiNhiemVu? TrangThai { get; init; }
    public string CheDo { get; init; } = "all";
    public bool ChiLa { get; init; }
    public string SapXep { get; init; } = "due";
    public DateTimeOffset? TuHan { get; init; }
    public DateTimeOffset? DenHan { get; init; }
    public string? KhoangHoanThanh { get; init; }
    public DateOnly? NgayHoanThanh { get; init; }
}
