using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;
namespace StudyPlanner.Application.HocTap;

internal static class QuyTacAnhHuongLich
{
    public static async Task<List<PhienTuHoc>> LayPhienAsync(IHocTapStore store, int chu, DateTimeOffset now, CancellationToken ct)
    {
        var rows = await store.DanhSachAsync(store.TruyVan<PhienTuHoc>().Where(x => x.MaNguoiDung == chu &&
            ((x.TrangThai == TrangThaiPhienTuHoc.Planned && x.KetThucDuKien > now) || x.TrangThai == TrangThaiPhienTuHoc.InProgress)).Take(10001), ct);
        if (rows.Count > 10000) throw new HocTapException(409, "SCHEDULE_SOURCE_LIMIT", "Có quá nhiều phiên tương lai để kiểm tra ảnh hưởng an toàn.");
        return rows;
    }

    public static async Task<bool> DangHocAsync(IHocTapStore store, int chu, IReadOnlyList<PhienTuHoc> affected, DateTimeOffset now, CancellationToken ct)
    {
        if (affected.Any(x => x.TrangThai == TrangThaiPhienTuHoc.InProgress || x.BatDauDuKien <= now)) return true;
        var ids = affected.Select(x => x.Ma).ToArray();
        return ids.Length > 0 && await store.DemAsync(store.TruyVan<DoanThucHoc>().Where(x => x.MaNguoiDung == chu &&
            x.KetThucThucTe == null && x.MaPhienTuHoc != null && ids.Contains(x.MaPhienTuHoc.Value)), ct) > 0;
    }

    public static void ApDung(IReadOnlyList<PhienTuHoc> affected, AnhHuongLichResponse impact, bool huy, DateTimeOffset now)
    {
        if (impact.CanKetThucPhienDangHoc)
            throw new HocTapException(409, "SESSION_NEEDS_REVIEW", "Có phiên đang học hoặc đã đến giờ bắt đầu. Hãy xử lý phiên trước khi sửa lịch.", impact);
        if (affected.Count != 0 && !huy)
            throw new HocTapException(409, "SCHEDULE_CHANGE_CONFIRMATION_REQUIRED", "Thay đổi ảnh hưởng phiên tự học. Hãy xem trước và xác nhận hủy các phiên liên quan.", impact);
        foreach (var session in affected)
        { session.TrangThai = TrangThaiPhienTuHoc.Cancelled; session.NgayCapNhat = now; }
    }
}
