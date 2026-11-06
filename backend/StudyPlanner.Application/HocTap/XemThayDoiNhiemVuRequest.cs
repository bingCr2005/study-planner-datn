using System.ComponentModel.DataAnnotations;
using System.Text.Json;
namespace StudyPlanner.Application.HocTap;
public sealed class XemThayDoiNhiemVuRequest
{
    [Required] public string ThaoTac { get; init; } = "update";
    public JsonElement NoiDung { get; init; }
}
