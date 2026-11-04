using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Domain.Entities;

public class LichSuNhiemVu
{
    public int Ma { get; set; }
    // Mã chụp lại: không có navigation/FK tới TaskItem để lịch sử còn sau khi xóa nhiệm vụ.
    public int MaNhiemVu { get; set; }
    public int? MaNguoiDung { get; set; }
    public HanhDongLichSuNhiemVu HanhDong { get; set; }
    // User hoặc System khi có worker; không ghi danh tính/bí mật vào trường này.
    public string NguonThayDoi { get; set; } = "User";
    public string TieuDeNhiemVu { get; set; } = string.Empty;
    // Chuỗi JSON ánh xạ jsonb; chỉ serialize trường nhiệm vụ cho phép, không lưu bí mật.
    public string? DuLieuTruoc { get; set; }
    public string? DuLieuSau { get; set; }
    public DateTimeOffset NgayThayDoi { get; set; }
    public NguoiDung? NguoiDung { get; set; }
}

