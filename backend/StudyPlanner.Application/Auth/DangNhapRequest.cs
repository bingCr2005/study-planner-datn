using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace StudyPlanner.Application.Auth;

public sealed class DangNhapRequest
{
    // Chuỗi để giữ số 0 đầu mã sinh viên. Không gắn EmailAddress vì còn nhận mã sinh viên.
    [Required(ErrorMessage = "Vui lòng nhập email hoặc MSSV.")]
    [StringLength(320, ErrorMessage = "Tên đăng nhập không được dài quá 320 ký tự.")]
    [TenDangNhapHopLe]
    public string TaiKhoan { get; init; } = "";

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [StringLength(128, ErrorMessage = "Mật khẩu không được dài quá 128 ký tự.")]
    public string MatKhau { get; init; } = "";
}

// Kiểm tra ở cấp thuộc tính để vẫn trả lỗi tài khoản khi mật khẩu cũng sai định dạng.
internal sealed class TenDangNhapHopLeAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // Giữ cách trim tài khoản cũ; tuyệt đối không trim/lọc mật khẩu.
        var account = (value as string)?.Trim() ?? "";
        var message = Regex.IsMatch(account, """['"`‘’“”]""") ? "Tên đăng nhập không được chứa dấu nháy." :
            account.Any(char.IsWhiteSpace) ? "Tên đăng nhập không được chứa khoảng trắng ở giữa." :
            account.Length > 0 && (account.Contains('@')
                ? !Regex.IsMatch(account, @"\A[^\s@]+@[^\s@]+\.[^\s@]+\z")
                : !Regex.IsMatch(account, @"\A[\p{L}\p{N}_]+\z"))
                ? "Vui lòng nhập email đúng định dạng hoặc MSSV chỉ gồm chữ, số và dấu _." : null;
        return message is null ? ValidationResult.Success : new ValidationResult(message, [validationContext.MemberName!]);
    }
}

