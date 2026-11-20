namespace StudyPlanner.Domain.Entities;

public class PhienTapTrung
{
    public int Ma { get; set; }
    public int MaNguoiDung { get; set; }
    public int? MaNhiemVu { get; set; }
    public int? MaPhienTuHoc { get; set; }
    public string TieuDe { get; set; } = "Tập trung tự do";
    public string CheDo { get; set; } = "Pomodoro";
    public string TrangThai { get; set; } = "Running";
    public int MucTieuGiay { get; set; } = 1500;
    public long SoMiliGiayDaHoc { get; set; }
    public DateTimeOffset BatDau { get; set; }
    public DateTimeOffset? BatDauDoan { get; set; }
    public DateTimeOffset? KetThuc { get; set; }
    public string? GhiChu { get; set; }
    public bool NhapThuCong { get; set; }
    public string? LichSuSuaThoiGianJson { get; set; }
    public NguoiDung NguoiDung { get; set; } = null!;
    public NhiemVu? NhiemVu { get; set; }
    public PhienTuHoc? PhienTuHoc { get; set; }
}
