namespace StudyPlanner.Domain.Entities;

public class KhoaHoc
{
    public int Ma { get; set; }
    public string Ten { get; set; } = string.Empty;
    public int NamBatDau { get; set; }
    public int NamKetThuc { get; set; }
    public ICollection<LopSinhVien> DanhSachLopSinhVien { get; set; } = new List<LopSinhVien>();
    public DateTimeOffset NgayTao { get; set; }
    public DateTimeOffset NgayCapNhat { get; set; }
}

