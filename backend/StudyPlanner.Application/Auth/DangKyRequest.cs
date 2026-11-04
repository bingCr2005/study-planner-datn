using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using StudyPlanner.Application.Administration;
using StudyPlanner.Application.Common;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.Auth;

public sealed class DangKyRequest
{
    [Required, StringLength(320), EmailAddress] public string Email { get; init; } = "";
    [Required, StringLength(200)] public string HoTen { get; init; } = "";
    [Required, StringLength(128, MinimumLength = 8)] public string MatKhau { get; init; } = "";
    // Bắt buộc ở luồng sinh viên tự đăng ký; không ép hồ sơ tài khoản cũ/Admin thành NOT NULL.
    [Required(ErrorMessage = "Mã sinh viên là bắt buộc."), StringLength(50)] public string MaSoSinhVien { get; init; } = "";
    [Required(ErrorMessage = "Tên trường là bắt buộc."), StringLength(200)] public string TenTruong { get; init; } = "";
    [Required(ErrorMessage = "Tên lớp là bắt buộc."), StringLength(200)] public string TenLop { get; init; } = "";
    [Required(ErrorMessage = "Ngành học là bắt buộc."), StringLength(200)] public string NganhHoc { get; init; } = "";
}

