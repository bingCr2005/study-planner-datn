using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace StudyPlanner.Application.HocTap;

public sealed class KhungGioRanhRequest
{
    [JsonRequired, Range(1, 7)] public int ThuTrongTuan { get; init; }
    [JsonRequired] public TimeOnly GioBatDau { get; init; }
    [JsonRequired] public TimeOnly GioKetThuc { get; init; }
    [JsonRequired] public DateOnly NgayBatDau { get; init; }
    [JsonRequired] public DateOnly NgayKetThuc { get; init; }
    public bool DangSuDung { get; init; } = true;
    public bool HuyPhienLienQuan { get; init; }
}
