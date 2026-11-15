using System.ComponentModel.DataAnnotations;
namespace StudyPlanner.Application.HocTap;

public class BatDauTapTrungRequest
{
    public int? MaNhiemVu { get; init; }
    public int? MaPhienTuHoc { get; init; }
    public string CheDo { get; init; } = "Pomodoro";
    public int MucTieuGiay { get; init; } = 1500;
}
public class ThaoTacTapTrungRequest
{
    [MaxLength(2000)] public string? GhiChu { get; init; }
}
public sealed class NhapTapTrungRequest : BatDauTapTrungRequest
{
    public DateTimeOffset BatDau { get; init; }
    public DateTimeOffset KetThuc { get; init; }
    [MaxLength(2000)] public string? GhiChu { get; init; }
}
