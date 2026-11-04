using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using StudyPlanner.Application.Administration;
using StudyPlanner.Application.Common;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.Auth;

public sealed class DangNhapRequest
{
    // Chuỗi để giữ số 0 đầu mã sinh viên. Không gắn EmailAddress vì còn nhận mã sinh viên.
    [Required(ErrorMessage = "Nhập email hoặc mã sinh viên."), StringLength(320)] public string TaiKhoan { get; init; } = "";
    [Required, StringLength(128)] public string MatKhau { get; init; } = "";
}

