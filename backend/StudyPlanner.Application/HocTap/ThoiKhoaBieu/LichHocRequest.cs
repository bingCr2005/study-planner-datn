using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace StudyPlanner.Application.HocTap;

public sealed class LichHocRequest
{
    [JsonRequired, Range(1, 7)] public int ThuTrongTuan { get; init; }
    [JsonRequired] public TimeOnly GioBatDau { get; init; }
    [JsonRequired] public TimeOnly GioKetThuc { get; init; }
    [StringLength(200)] public string? PhongHoc { get; init; }
    public DateOnly? NgayApDungTu { get; init; }
    public DateOnly? NgayApDungDen { get; init; }
    public bool XacNhanTrungLich { get; init; }
    public bool HuyPhienLienQuan { get; init; }
}
