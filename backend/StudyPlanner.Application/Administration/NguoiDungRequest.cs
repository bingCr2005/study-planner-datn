using System.ComponentModel.DataAnnotations;
using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Application.Administration;

// Chưa mở sửa VaiTro, hash hoặc thiết lập nhắc hạn qua CRUD quản trị.
public class NguoiDungRequest
{
    [Required, StringLength(320)] public string Email { get; init; } = "";
    [Required, StringLength(200)] public string HoTen { get; init; } = "";
    [StringLength(50)] public string? MaSoSinhVien { get; init; }
    [StringLength(200)] public string? TenTruong { get; init; }
    [StringLength(200)] public string? TenLop { get; init; }
    [StringLength(200)] public string? NganhHoc { get; init; }
    public bool DangHoatDong { get; init; } = true;
}

