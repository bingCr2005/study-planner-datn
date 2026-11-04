namespace StudyPlanner.Domain.Entities;

public class Khoa
{
    public int Ma { get; set; }
    public int MaTruong { get; set; }
    public string KyHieu { get; set; } = string.Empty;
    public string Ten { get; set; } = string.Empty;
    public DateTimeOffset NgayTao { get; set; }
    public DateTimeOffset NgayCapNhat { get; set; }
    // null! chỉ bỏ cảnh báo nullable; phải tải/gán navigation trước khi đọc School.Ten.
    public Truong Truong { get; set; } = null!;
    public ICollection<LopSinhVien> DanhSachLopSinhVien { get; set; } = new List<LopSinhVien>();
}
