using System.Text.Json;
using System.Text.Json.Serialization;
using StudyPlanner.Application.HocTap;

namespace StudyPlanner.Application.UuTien;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CauHinhUuTien
{
    public int PhienBanSchema { get; init; }
    public string[] TieuChi { get; init; } = [];
}

public static class QuyTacCauHinhUuTien
{
    public static CauHinhUuTien Doc(string json)
    {
        try
        {
            var config = JsonSerializer.Deserialize<CauHinhUuTien>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            KiemTra(config); return config!;
        }
        catch (Exception e) when (e is JsonException or HocTapException)
        { throw new HocTapException(503, "PRIORITY_CONFIG_UNAVAILABLE", "Cấu hình ưu tiên không hợp lệ. Cần quản trị viên kiểm tra dữ liệu."); }
    }
    public static CauHinhUuTien MacDinh() => new() { PhienBanSchema = 1, TieuChi = ["Importance", "Deadline", "InProgress"] };
    public static void KiemTra(CauHinhUuTien? value)
    {
        if (value is null || value.PhienBanSchema != 1 || value.TieuChi is null || value.TieuChi.Length is < 1 or > 3 ||
            !value.TieuChi.Contains("Deadline") || value.TieuChi.Distinct().Count() != value.TieuChi.Length ||
            value.TieuChi.Any(x => x is not ("Deadline" or "Importance" or "InProgress")))
            throw new HocTapException(400, "PRIORITY_CONFIG_INVALID", "Quy tắc phải có hạn hoàn thành; mỗi tiêu chí chỉ được xuất hiện một lần.");
    }
}
