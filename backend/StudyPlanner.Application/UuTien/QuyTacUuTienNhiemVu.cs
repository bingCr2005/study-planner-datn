using StudyPlanner.Application.HocTap;
using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Application.UuTien;

// Cùng sáu nhóm và mốc giờ với todayModel.ts. Admin chỉ phân xử bên trong một nhóm.
public static class QuyTacUuTienNhiemVu
{
    public static int Nhom(DateTimeOffset? han, DateTimeOffset now)
    {
        if (han is null) return 5;
        var hours = (han.Value - now).TotalHours;
        return hours < 0 ? 0 : hours <= 24 ? 1 : hours <= 72 ? 2 : hours <= 168 ? 3 : 4;
    }
    public static IOrderedEnumerable<CongViecLapLich> SapXep(IEnumerable<CongViecLapLich> jobs,
        DateTimeOffset now, CauHinhUuTien config, bool thuHanTruoc = false)
    {
        QuyTacCauHinhUuTien.KiemTra(config);
        var ordered = jobs.OrderBy(j => Nhom(j.HanHieuLuc, now));
        if (thuHanTruoc) ordered = ordered.ThenBy(j => j.HanHieuLuc ?? DateTimeOffset.MaxValue);
        foreach (var criterion in config.TieuChi)
            ordered = criterion switch
            {
                "Importance" => ordered.ThenByDescending(j => j.MucDoUuTien),
                "Deadline" => ordered.ThenBy(j => j.HanHieuLuc ?? DateTimeOffset.MaxValue),
                _ => ordered.ThenByDescending(j => j.TrangThai == TrangThaiNhiemVu.InProgress)
            };
        return ordered.ThenBy(j => j.MaNhiemVu);
    }
    public static string LyDo(CongViecLapLich job, DateTimeOffset now, CauHinhUuTien config)
    {
        string[] groups = ["Đã quá hạn", "Đến hạn trong 24 giờ", "Còn trên 1 đến 3 ngày", "Còn trên 3 đến 7 ngày", "Còn trên 7 ngày", "Chưa có hạn"];
        var reasons = new List<string> { groups[Nhom(job.HanHieuLuc, now)] };
        if (config.TieuChi.Contains("Importance") && job.MucDoUuTien == MucDoUuTienNhiemVu.High) reasons.Add("Mức quan trọng cao");
        if (config.TieuChi.Contains("InProgress") && job.TrangThai == TrangThaiNhiemVu.InProgress) reasons.Add("Đang thực hiện");
        return string.Join(" · ", reasons);
    }
}
