namespace StudyPlanner.Application.HocTap;

public sealed record ApDungLichResponse(int MaLanApDung, bool DaTonTai, IReadOnlyList<PhienTuHocResponse> Phien);
