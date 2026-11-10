using System.ComponentModel.DataAnnotations;
namespace StudyPlanner.Application.HocTap;

public sealed class XemAnhHuongLichRequest
{
    [Required] public string Loai { get; init; } = "";
    public int? Ma { get; init; }
    public int? MaMonHoc { get; init; }
    public bool Xoa { get; init; }
    public LichHocRequest? LichHoc { get; init; }
    public KhungGioRanhRequest? KhungGioRanh { get; init; }
}
