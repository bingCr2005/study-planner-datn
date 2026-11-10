using System.ComponentModel.DataAnnotations;
namespace StudyPlanner.Application.HocTap;
public sealed class TienDoNhiemVuRequest
{
    [Range(0,100)] public int TienDo { get; init; }
    public bool HuyPhienLienQuan { get; init; }
}
