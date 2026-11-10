namespace StudyPlanner.Application.HocTap;

public sealed record KetQuaXepLich(IReadOnlyList<DeXuatPhienTuHoc> Phien, IReadOnlyDictionary<int, int> ChuaXep);
