using System.ComponentModel.DataAnnotations;

namespace StudyPlanner.Application.HocTap;

public sealed class ApDungLichRequest
{
    [Required] public string TokenPreview { get; init; } = "";
    [Required, MinLength(1), MaxLength(2000)] public List<ChonPhienTuHocRequest> Phien { get; init; } = [];
}
