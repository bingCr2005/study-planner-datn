namespace StudyPlanner.Application.HocTap;

public sealed record TongHopTapTrung(int SoPhien, int SoPomodoro, long TongMiliGiay);
public sealed record TapTrungTheoNgay(DateOnly Ngay, int SoPhien, int SoPomodoro, long TongMiliGiay);
public sealed record TapTrungTheoNhiemVu(int? MaNhiemVu, string TieuDe, int SoPhien, int SoPomodoro, long TongMiliGiay);
public sealed record ThongKeTapTrungResponse(DateOnly TuNgay, DateOnly DenNgay, string MuiGio,
    TongHopTapTrung TongHop, IReadOnlyList<TapTrungTheoNgay> TheoNgay, IReadOnlyList<TapTrungTheoNhiemVu> TheoNhiemVu);
