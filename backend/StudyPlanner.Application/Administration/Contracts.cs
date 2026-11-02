using System.ComponentModel.DataAnnotations;
using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Application.Administration;

public sealed record PagedResult<T>(IReadOnlyList<T> DanhSach, int Trang, int KichThuocTrang, int TongSo);

public sealed class ListRequest
{
    [StringLength(200)] public string? TuKhoa { get; init; }
    [Range(1, int.MaxValue)] public int Trang { get; init; } = 1;
    [Range(1, 100)] public int KichThuocTrang { get; init; } = 20;
    public bool? DangHoatDong { get; init; }
}

// Chưa mở sửa VaiTro, hash hoặc thiết lập nhắc hạn qua CRUD quản trị.
public class UserRequest
{
    [Required, StringLength(320)] public string Email { get; init; } = "";
    [Required, StringLength(200)] public string HoTen { get; init; } = "";
    [StringLength(50)] public string? MaSoSinhVien { get; init; }
    [StringLength(200)] public string? TenTruong { get; init; }
    [StringLength(200)] public string? TenLop { get; init; }
    [StringLength(200)] public string? NganhHoc { get; init; }
    public bool DangHoatDong { get; init; } = true;
}

public sealed class CreateUserRequest : UserRequest
{
    [Required, StringLength(128, MinimumLength = 8)] public string MatKhau { get; init; } = "";
}

public sealed class UserActiveRequest
{
    [Required] public bool? DangHoatDong { get; init; }
}


// DTO không có MatKhauBam hoặc navigation để tránh lộ thông tin và vòng JSON.
public sealed record UserResponse(int Ma, string Email, string HoTen, string? MaSoSinhVien,
    string? TenTruong, string? TenLop, string? NganhHoc, UserRole VaiTro, bool DangHoatDong, string MuiGio, int NhacTruocGio,
    DateTimeOffset NgayTao, DateTimeOffset NgayCapNhat);
