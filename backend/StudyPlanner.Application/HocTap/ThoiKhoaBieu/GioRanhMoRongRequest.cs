using System.Text.Json.Serialization;
namespace StudyPlanner.Application.HocTap;

public sealed class NhieuKhungGioRanhRequest
{
    [JsonRequired] public List<int> ThuTrongTuan { get; init; } = [];
    [JsonRequired] public TimeOnly GioBatDau { get; init; }
    [JsonRequired] public TimeOnly GioKetThuc { get; init; }
    [JsonRequired] public DateOnly NgayBatDau { get; init; }
    [JsonRequired] public DateOnly NgayKetThuc { get; init; }
    public bool DangSuDung { get; init; } = true;
}
public sealed class NgoaiLeGioRanhRequest
{
    public DateOnly Ngay { get; init; }
    public bool BoQua { get; init; }
    public TimeOnly? GioBatDau { get; init; }
    public TimeOnly? GioKetThuc { get; init; }
    public bool HuyPhienLienQuan { get; init; }
    public bool Xoa { get; init; }
}
public sealed record NgoaiLeGioRanhResponse(int MaKhungGioRanh, DateOnly Ngay, bool BoQua, TimeOnly? GioBatDau, TimeOnly? GioKetThuc);
