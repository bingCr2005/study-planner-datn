using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Domain.Entities;

public class NguoiDung
{
    public int Ma { get; set; }
    public string Email { get; set; } = string.Empty;
    public string EmailChuanHoa { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string MatKhauBam { get; set; } = string.Empty;
    public string? MaSoSinhVien { get; set; }
    // Hồ sơ tự khai của bản cốt lõi; không tạo phân cấp hoặc quyền giao việc.
    public string? TenTruong { get; set; }
    public string? TenLop { get; set; }
    public string? NganhHoc { get; set; }
    public VaiTroNguoiDung VaiTro { get; set; } = VaiTroNguoiDung.Student;
    public bool DangHoatDong { get; set; } = true;
    public string MuiGio { get; set; } = "Asia/Ho_Chi_Minh";
    public int NhacTruocGio { get; set; } = 24;
    public int PhienHocPhut { get; set; } = 25;
    public int NghiGiuaPhienPhut { get; set; } = 5;
    public int GioiHanHocMoiNgayPhut { get; set; } = 180;
    // Tăng trong giao dịch khi dữ liệu ảnh hưởng lịch thay đổi; chưa thay thế khóa/kiểm tra xung đột.
    public long PhienBanLapLich { get; set; }
    public ICollection<HocKy> DanhSachHocKy { get; set; } = new List<HocKy>();
    public ICollection<NhiemVu> DanhSachNhiemVu { get; set; } = new List<NhiemVu>();
    public ICollection<ThongBao> DanhSachThongBao { get; set; } = new List<ThongBao>();
    public ICollection<LichSuNhiemVu> DanhSachLichSuNhiemVu { get; set; } = new List<LichSuNhiemVu>();
    public DateTimeOffset NgayTao { get; set; }
    public DateTimeOffset NgayCapNhat { get; set; }
}

