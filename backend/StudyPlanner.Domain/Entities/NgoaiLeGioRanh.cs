namespace StudyPlanner.Domain.Entities;

public sealed class NgoaiLeGioRanh
{
    public int Ma { get; set; }
    public int MaNguoiDung { get; set; }
    public int MaKhungGioRanh { get; set; }
    public DateOnly Ngay { get; set; }
    public bool BoQua { get; set; }
    public TimeOnly? GioBatDau { get; set; }
    public TimeOnly? GioKetThuc { get; set; }
    public DateTimeOffset NgayTao { get; set; }
    public DateTimeOffset NgayCapNhat { get; set; }
    public KhungGioRanh KhungGioRanh { get; set; } = null!;
}
