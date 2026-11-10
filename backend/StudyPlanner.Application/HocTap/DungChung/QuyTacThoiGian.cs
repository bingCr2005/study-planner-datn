using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.HocTap;

public static class QuyTacThoiGian
{
    public static TimeZoneInfo MuiGio(string ma)
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById(ma); }
        catch (Exception e) when (e is TimeZoneNotFoundException or InvalidTimeZoneException)
        { throw new HocTapException(400, "INVALID_TIME_ZONE", "Múi giờ tài khoản không hợp lệ."); }
    }

    public static DateTimeOffset Utc(DateOnly ngay, TimeOnly gio, TimeZoneInfo muiGio)
    {
        var local = ngay.ToDateTime(gio, DateTimeKind.Unspecified);
        if (muiGio.IsInvalidTime(local) || muiGio.IsAmbiguousTime(local))
            throw new HocTapException(409, "LOCAL_TIME_AMBIGUOUS", "Giờ địa phương không tồn tại hoặc có hai cách quy đổi. Hãy điều chỉnh mẫu lịch.", new { ngay, gio, muiGio = muiGio.Id });
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(local, muiGio), TimeSpan.Zero);
    }

    public static bool Giao(KhoangThoiGian a, KhoangThoiGian b) => a.BatDau < b.KetThuc && b.BatDau < a.KetThuc;

    public static List<KhoangThoiGian> HopNhat(IEnumerable<KhoangThoiGian> nguon)
    {
        var ketQua = new List<KhoangThoiGian>();
        foreach (var k in nguon.Where(x => x.KetThuc > x.BatDau).OrderBy(x => x.BatDau).ThenBy(x => x.KetThuc))
        {
            if (ketQua.Count == 0 || k.BatDau > ketQua[^1].KetThuc) ketQua.Add(k);
            else if (k.KetThuc > ketQua[^1].KetThuc) ketQua[^1] = ketQua[^1] with { KetThuc = k.KetThuc };
        }
        return ketQua;
    }

    public static List<KhoangThoiGian> Tru(IEnumerable<KhoangThoiGian> nguon, IEnumerable<KhoangThoiGian> ban)
    {
        var busy = HopNhat(ban);
        var ketQua = new List<KhoangThoiGian>();
        foreach (var k in HopNhat(nguon))
        {
            var dau = k.BatDau;
            foreach (var b in busy)
            {
                if (b.KetThuc <= dau) continue;
                if (b.BatDau >= k.KetThuc) break;
                if (b.BatDau > dau) ketQua.Add(new(dau, b.BatDau));
                if (b.KetThuc > dau) dau = b.KetThuc;
                if (dau >= k.KetThuc) break;
            }
            if (dau < k.KetThuc) ketQua.Add(new(dau, k.KetThuc));
        }
        return ketQua;
    }

    public static bool CoNgay(DateOnly tu, DateOnly den, int thu)
    {
        var thuDau = ((int)tu.DayOfWeek + 6) % 7 + 1;
        return tu.DayNumber + (thu - thuDau + 7) % 7 <= den.DayNumber;
    }

    public static List<KhoangThoiGian> SinhLichHoc(LichHoc lich, HocKy hocKy, TimeZoneInfo muiGio, DateTimeOffset tu, DateTimeOffset den)
    {
        var batDau = lich.NgayApDungTu ?? hocKy.NgayBatDau;
        var ketThuc = lich.NgayApDungDen ?? hocKy.NgayKetThuc;
        if (batDau < hocKy.NgayBatDau) batDau = hocKy.NgayBatDau;
        if (ketThuc > hocKy.NgayKetThuc) ketThuc = hocKy.NgayKetThuc;
        return Sinh(lich.ThuTrongTuan, lich.GioBatDau, lich.GioKetThuc, batDau, ketThuc, muiGio, tu, den);
    }

    public static List<KhoangThoiGian> SinhGioRanh(KhungGioRanh khung, TimeZoneInfo muiGio, DateTimeOffset tu, DateTimeOffset den) =>
        khung.DangSuDung ? Sinh(khung.ThuTrongTuan, khung.GioBatDau, khung.GioKetThuc, khung.NgayBatDau, khung.NgayKetThuc, muiGio, tu, den) : [];

    private static List<KhoangThoiGian> Sinh(int thu, TimeOnly gioTu, TimeOnly gioDen, DateOnly ngayTu, DateOnly ngayDen,
        TimeZoneInfo muiGio, DateTimeOffset tu, DateTimeOffset den)
    {
        if (tu >= den || ngayTu > ngayDen) return [];
        var localTu = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(tu, muiGio).DateTime);
        var localDen = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(den, muiGio).DateTime);
        if (ngayTu < localTu) ngayTu = localTu;
        if (ngayDen > localDen) ngayDen = localDen;
        if (ngayTu > ngayDen) return [];
        var thuDau = ((int)ngayTu.DayOfWeek + 6) % 7 + 1;
        var offset = (thu - thuDau + 7) % 7;
        var ketQua = new List<KhoangThoiGian>();
        for (var soNgay = ngayTu.DayNumber + offset; soNgay <= ngayDen.DayNumber; soNgay += 7)
        {
            var ngay = DateOnly.FromDayNumber(soNgay);
            var k = new KhoangThoiGian(Utc(ngay, gioTu, muiGio), Utc(ngay, gioDen, muiGio));
            if (k.BatDau < den && k.KetThuc > tu) ketQua.Add(k);
            if (ketQua.Count > 10000) throw new HocTapException(400, "RECURRENCE_LIMIT", "Mẫu lịch vượt 10.000 lần xuất hiện. Hãy thu hẹp khoảng ngày hiệu lực.");
        }
        return ketQua;
    }

    public static void KiemTraKhoang(DateTimeOffset tu, DateTimeOffset den, int toiDaNgay = 93)
    {
        if (tu >= den || den - tu > TimeSpan.FromDays(toiDaNgay))
            throw new HocTapException(400, "INVALID_DATE_RANGE", $"Khoảng truy vấn phải có kết thúc sau bắt đầu và không vượt {toiDaNgay} ngày.");
    }
}
