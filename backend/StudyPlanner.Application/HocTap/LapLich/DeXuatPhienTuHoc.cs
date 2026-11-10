namespace StudyPlanner.Application.HocTap;

public sealed record DeXuatPhienTuHoc(int MaNhiemVu, DateTimeOffset BatDauDuKien,
    DateTimeOffset KetThucDuKien, string TieuDe, string LyDo);
