namespace StudyPlanner.Domain.Entities;

/// <summary>Thông tin tệp. Nội dung nhị phân nằm ở kho riêng, không gửi đường dẫn máy chủ cho trình duyệt.</summary>
public class TepDinhKemNhiemVu
{
    public int Ma { get; set; }
    public int MaNhiemVu { get; set; }
    public int MaNguoiTaiLen { get; set; }
    public string TenTep { get; set; } = string.Empty;
    // Khóa nội bộ do máy chủ cấp; API tải xuống kiểm tra chủ sở hữu trước khi mở kho.
    public string KhoaLuuTru { get; set; } = string.Empty;
    public string LoaiTep { get; set; } = string.Empty;
    public long KichThuocByte { get; set; }
    public DateTimeOffset NgayTao { get; set; }
    public NhiemVu NhiemVu { get; set; } = null!;
    public NguoiDung NguoiTaiLen { get; set; } = null!;
}
