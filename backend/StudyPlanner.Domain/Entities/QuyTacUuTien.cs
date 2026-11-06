namespace StudyPlanner.Domain.Entities;

/// <summary>Phiên bản cấu hình quy tắc; không phải điểm ưu tiên đã tính của từng nhiệm vụ.</summary>
public class QuyTacUuTien
{
    public int Ma { get; set; }
    public string Ten { get; set; } = string.Empty;
    public int PhienBan { get; set; }
    public bool DangSuDung { get; set; }
    // jsonb; Application phải kiểm tra schema/trọng số trước khi cho Admin kích hoạt.
    public string CauHinh { get; set; } = "{}";
    public DateTimeOffset NgayTao { get; set; }
}
