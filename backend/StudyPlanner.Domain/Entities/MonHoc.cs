namespace StudyPlanner.Domain.Entities;

public class MonHoc
{
    public int Ma { get; set; }
    public int MaHocKy { get; set; }
    public string? KyHieu { get; set; }
    public string Ten { get; set; } = string.Empty;
    public int? SoTinChi { get; set; }
    public string? GiangVien { get; set; }
    public string? MauSac { get; set; }
    public HocKy HocKy { get; set; } = null!;
    public ICollection<LichHoc> DanhSachLichHoc { get; set; } = new List<LichHoc>();
    public ICollection<NhiemVu> DanhSachNhiemVu { get; set; } = new List<NhiemVu>();
    public DateTimeOffset NgayTao { get; set; }
    public DateTimeOffset NgayCapNhat { get; set; }
}

