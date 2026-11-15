using StudyPlanner.Domain.Entities;
namespace StudyPlanner.Application.HocTap;

public sealed record TapTrungResponse(int Ma, int? MaNhiemVu, int? MaPhienTuHoc, string TieuDe, string CheDo,
    string TrangThai, int MucTieuGiay, long SoMiliGiayDaHoc, DateTimeOffset BatDau, DateTimeOffset? BatDauDoan,
    DateTimeOffset? KetThuc, string? GhiChu, bool NhapThuCong)
{
    public static TapTrungResponse Tu(PhienTapTrung p) => new(p.Ma, p.MaNhiemVu, p.MaPhienTuHoc, p.TieuDe, p.CheDo,
        p.TrangThai, p.MucTieuGiay, p.SoMiliGiayDaHoc, p.BatDau, p.BatDauDoan, p.KetThuc, p.GhiChu, p.NhapThuCong);
}
public sealed record TongQuanTapTrung(int PomoHomNay, long MiliGiayHomNay, int TongPomo, long TongMiliGiay);
public sealed record TapTrungState(TapTrungResponse? HienTai, IReadOnlyList<TapTrungResponse> LichSu,
    TongQuanTapTrung TongQuan, int Trang, int TongSo, DateTimeOffset ThoiDiemMayChu, string MuiGio, int? MaPhienDangHocCu);
