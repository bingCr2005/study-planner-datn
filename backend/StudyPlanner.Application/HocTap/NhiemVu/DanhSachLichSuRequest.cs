using System.ComponentModel.DataAnnotations;
using StudyPlanner.Domain.Enums;
namespace StudyPlanner.Application.HocTap;
public sealed class DanhSachLichSuRequest
{
    public int? MaNhiemVu { get; init; }
    public HanhDongLichSuNhiemVu? HanhDong { get; init; }
    public DateTimeOffset? Tu { get; init; }
    public DateTimeOffset? Den { get; init; }
    [Range(1,int.MaxValue)] public int Trang { get; init; } = 1;
    [Range(1,100)] public int KichThuocTrang { get; init; } = 20;
}
