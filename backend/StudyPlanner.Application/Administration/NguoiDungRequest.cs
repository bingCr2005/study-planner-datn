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
    // Mốc được trả về khi admin đọc hồ sơ. Nếu người khác đã đổi hồ sơ hoặc
    // khóa/mở tài khoản sau mốc này, từ chối lưu thay vì ghi đè dữ liệu mới.
    // Nullable để các client cũ chưa gửi trường này vẫn hoạt động như trước.
    public DateTimeOffset? NgayCapNhatMongDoi { get; init; }
}

