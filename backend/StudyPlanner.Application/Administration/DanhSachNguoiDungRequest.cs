using System.ComponentModel.DataAnnotations;
using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Application.Administration;

public sealed class DanhSachNguoiDungRequest
{
    [StringLength(200)] public string? TuKhoa { get; init; }
    [Range(1, int.MaxValue)] public int Trang { get; init; } = 1;
    [Range(1, 100)] public int KichThuocTrang { get; init; } = 20;
    public bool? DangHoatDong { get; init; }
    [EnumDataType(typeof(VaiTroNguoiDung))] public VaiTroNguoiDung? VaiTro { get; init; }
    public DateTimeOffset? TaoTu { get; init; }
    public DateTimeOffset? TaoDen { get; init; }
}

