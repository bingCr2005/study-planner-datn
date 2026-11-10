using System.ComponentModel.DataAnnotations;
namespace StudyPlanner.Application.HocTap;
public sealed class ThemNhiemVuConRequest
{
    [Required, MinLength(1), MaxLength(100)] public List<NhiemVuRequest> DanhSach { get; init; } = [];
    public bool HuyPhienLienQuan { get; init; }
}
