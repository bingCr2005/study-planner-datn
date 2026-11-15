using StudyPlanner.Domain.Entities;
namespace StudyPlanner.Application.HocTap;

public static class QuyTacGioRanh
{
    public static List<KhoangThoiGian> Sinh(IEnumerable<KhungGioRanh> templates, IEnumerable<NgoaiLeGioRanh> exceptions,
        TimeZoneInfo zone, DateTimeOffset from, DateTimeOffset to)
    {
        var byTemplate = exceptions.ToLookup(x => x.MaKhungGioRanh);
        var rows = new List<KhoangThoiGian>();
        foreach (var template in templates.Where(t => t.DangSuDung))
        {
            var changes = byTemplate[template.Ma].ToDictionary(x => x.Ngay);
            foreach (var range in QuyTacThoiGian.SinhGioRanh(template, zone, from, to))
            {
                var date = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(range.BatDau, zone).DateTime);
                if (!changes.ContainsKey(date)) rows.Add(range);
            }
            foreach (var change in changes.Values.Where(e => !e.BoQua && e.Ngay >= template.NgayBatDau && e.Ngay <= template.NgayKetThuc))
            {
                // Giờ thay thế có thể nằm ngoài khung gốc, nên phải sinh độc lập để không bỏ sót khi truy vấn.
                var start = QuyTacThoiGian.Utc(change.Ngay, change.GioBatDau!.Value, zone);
                var end = QuyTacThoiGian.Utc(change.Ngay, change.GioKetThuc!.Value, zone);
                if (start < to && end > from) rows.Add(new(start, end));
            }
        }
        return QuyTacThoiGian.HopNhat(rows).ToList();
    }
}
