namespace StudyPlanner.Domain.Entities;

public class LopSinhVien
{
    public int Ma { get; set; }
    public int MaKhoa { get; set; }
    public int MaKhoaHoc { get; set; }
    public string Ten { get; set; } = string.Empty;
    public Khoa Khoa { get; set; } = null!;
    public KhoaHoc KhoaHoc { get; set; } = null!;
    public ICollection<NguoiDung> DanhSachNguoiDung { get; set; } = new List<NguoiDung>();
    public DateTimeOffset NgayTao { get; set; }
    public DateTimeOffset NgayCapNhat { get; set; }
}

