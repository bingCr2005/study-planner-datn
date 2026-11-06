using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Application.HocTap;

/// <summary>Cùng một bộ kiểm tra cho apply và phiên nhập tay/dời; phải gọi trong giao dịch đã khóa tài khoản.</summary>
public sealed class QuyTacDatPhien(IHocTapStore store, ThoiKhoaBieuService lich)
{
    public async Task KiemTraAsync(int chu, IReadOnlyList<ChonPhienTuHocRequest> phien,
        DateTimeOffset now, int? boQuaMaPhien, CancellationToken ct)
    {
        if (phien.Count is < 1 or > 2000) throw Loi("Cần từ 1 đến 2.000 phiên.");
        if (phien.Distinct().Count() != phien.Count) throw Loi("Danh sách có phiên lặp.");
        var ids = phien.Select(p => p.MaNhiemVu).Distinct().ToArray();
        var tasks = await store.DanhSachAsync(store.TruyVan<NhiemVu>().Where(t => t.MaNguoiDung == chu && ids.Contains(t.Ma)), ct);
        if (tasks.Count != ids.Length) throw new HocTapException(404, "RESOURCE_NOT_FOUND", "Không tìm thấy nhiệm vụ.");
        var parents = await store.DanhSachAsync(store.TruyVan<NhiemVu>().Where(t => t.MaNguoiDung == chu &&
            t.MaNhiemVuCha.HasValue && ids.Contains(t.MaNhiemVuCha.Value)).Select(t => t.MaNhiemVuCha!.Value).Distinct(), ct);
        if (parents.Count > 0) throw XungDot("TASK_NOT_LEAF", "Chỉ đặt phiên cho nhiệm vụ lá.");
        if (tasks.Any(t => t.TrangThai == TrangThaiNhiemVu.Completed || !t.ThoiLuongDuKienPhut.HasValue))
            throw XungDot("TASK_NOT_SCHEDULABLE", "Nhiệm vụ phải chưa hoàn thành và có ước lượng.");
        var semesterIds = tasks.Where(t => t.MaHocKy.HasValue).Select(t => t.MaHocKy!.Value).Distinct().ToArray();
        if (await store.DemAsync(store.TruyVan<HocKy>().Where(s => s.MaNguoiDung == chu && semesterIds.Contains(s.Ma) && s.DaLuuTru), ct) > 0)
            throw XungDot("SEMESTER_ARCHIVED", "Không đặt phiên mới vào học kỳ lưu trữ.");
        var dictionary = tasks.ToDictionary(t => t.Ma);
        await NapToTienAsync(store, chu, dictionary, ct);
        var min = phien.Min(p => p.BatDauDuKien);
        var max = phien.Max(p => p.KetThucDuKien);
        if (max - min > TimeSpan.FromDays(32)) throw Loi("Các phiên trong một lần ghi không được trải quá 31 ngày địa phương.");
        var source = await lich.LayNguonAsync(chu, min.AddMinutes(-10), max.AddMinutes(10), ct);
        var free = QuyTacThoiGian.HopNhat(source.GioRanh).ToList();
        var busy = source.LichHoc.Concat(source.SuKien).ToList();
        var restFrom = min.AddMinutes(-10); var restTo = max.AddMinutes(10);
        var existingQuery = store.TruyVan<PhienTuHoc>().Where(p => p.MaNguoiDung == chu &&
            p.Ma != boQuaMaPhien && p.TrangThai != TrangThaiPhienTuHoc.Cancelled && p.KetThucDuKien > now &&
            (ids.Contains(p.MaNhiemVu) || p.BatDauDuKien < restTo && p.KetThucDuKien > restFrom));
        var existing = await store.DanhSachAsync(existingQuery.Take(10001), ct);
        if (existing.Count > 10000) throw new HocTapException(400, "QUERY_LIMIT", "Dữ liệu phiên vượt giới hạn kiểm tra; hãy thu hẹp hoặc xử lý lịch cũ.");
        if (await store.DemAsync(store.TruyVan<PhienTuHoc>().Where(p => p.MaNguoiDung == chu &&
            p.TrangThai == TrangThaiPhienTuHoc.InProgress && p.KetThucDuKien <= now), ct) > 0)
            throw XungDot("SESSION_NEEDS_REVIEW", "Có phiên đang học đã quá giờ; hãy kết thúc phiên trước khi đặt lịch mới.");
        var slots = phien.OrderBy(p => p.BatDauDuKien).ThenBy(p => p.MaNhiemVu).ToList();
        foreach (var slot in slots)
        {
            if (slot.MaNhiemVu <= 0 || slot.BatDauDuKien < now || slot.KetThucDuKien <= slot.BatDauDuKien ||
                slot.BatDauDuKien.UtcTicks % TimeSpan.TicksPerMinute != 0 || slot.KetThucDuKien.UtcTicks % TimeSpan.TicksPerMinute != 0)
                throw Loi("Phiên phải ở tương lai, có giờ kết thúc sau bắt đầu và đặt theo phút nguyên.");
            var duration = (slot.KetThucDuKien - slot.BatDauDuKien).TotalMinutes;
            if (duration > QuyTacLapLich.PhienToiDaPhut) throw Loi("Mỗi phiên tối đa 50 phút.");
            var task = dictionary[slot.MaNhiemVu];
            var due = QuyTacCay.HanHieuLuc(task, dictionary).Han;
            if (due.HasValue && slot.KetThucDuKien > due.Value)
                throw XungDot("TASK_DEADLINE_CONFLICT", "Phiên vượt quá hạn hiệu lực của nhiệm vụ.");
            var range = new KhoangThoiGian(slot.BatDauDuKien, slot.KetThucDuKien);
            if (!free.Any(g => g.BatDau <= range.BatDau && g.KetThuc >= range.KetThuc))
                throw XungDot("OUTSIDE_AVAILABILITY", "Phiên không nằm trọn trong giờ rảnh đang bật.");
            if (busy.Any(b => QuyTacThoiGian.Giao(range, b)))
                throw XungDot("SCHEDULE_CONFLICT", "Phiên trùng lịch học hoặc sự kiện.");
            var withBreak = new KhoangThoiGian(range.BatDau.AddMinutes(-10), range.KetThuc.AddMinutes(10));
            if (existing.Any(p => QuyTacThoiGian.Giao(withBreak, new(p.BatDauDuKien, p.KetThucDuKien))))
                throw XungDot("SCHEDULE_CONFLICT", "Phiên trùng lịch đã đặt hoặc thiếu 10 phút nghỉ.");
        }
        for (var i = 1; i < slots.Count; i++)
            if (slots[i].BatDauDuKien < slots[i - 1].KetThucDuKien.AddMinutes(10))
                throw XungDot("SCHEDULE_CONFLICT", "Các phiên yêu cầu trùng nhau hoặc thiếu 10 phút nghỉ.");
        foreach (var group in slots.GroupBy(s => s.MaNhiemVu))
        {
            var task = dictionary[group.Key];
            var reserved = existing.Where(p => p.MaNhiemVu == task.Ma &&
                p.TrangThai is TrangThaiPhienTuHoc.Planned or TrangThaiPhienTuHoc.InProgress)
                .Sum(p => Math.Max(0, (p.KetThucDuKien - (p.BatDauDuKien < now ? now : p.BatDauDuKien)).TotalMinutes));
            var available = Math.Max(0, task.ThoiLuongConLaiPhut!.Value - reserved);
            var list = group.OrderBy(s => s.BatDauDuKien).ToList();
            var minutes = list.Sum(p => (p.KetThucDuKien - p.BatDauDuKien).TotalMinutes);
            if (minutes > available) throw XungDot("TASK_MINUTES_EXCEEDED", "Thời lượng đặt mới vượt phần việc còn chưa được dành chỗ.");
            for (var i = 0; i < list.Count; i++)
            {
                var duration = (list[i].KetThucDuKien - list[i].BatDauDuKien).TotalMinutes;
                if (duration < 25 && (i != list.Count - 1 || minutes != available))
                    throw Loi("Phiên dưới 25 phút chỉ được dùng để xếp hết phần cuối còn lại của nhiệm vụ.");
            }
        }
    }

    public static async Task NapToTienAsync(IHocTapStore store, int chu, Dictionary<int, NhiemVu> map, CancellationToken ct)
    {
        for (var depth = 0; depth <= 10; depth++)
        {
            var ids = map.Values.Where(t => t.MaNhiemVuCha.HasValue && !map.ContainsKey(t.MaNhiemVuCha.Value))
                .Select(t => t.MaNhiemVuCha!.Value).Distinct().ToArray();
            if (ids.Length == 0) return;
            if (depth == 10) throw XungDot("TREE_LIMIT", "Cây vượt giới hạn 10 cấp.");
            var rows = await store.DanhSachAsync(store.TruyVan<NhiemVu>().Where(t => t.MaNguoiDung == chu && ids.Contains(t.Ma)), ct);
            if (rows.Count != ids.Length) throw XungDot("INVALID_TASK_TREE", "Cây có liên kết cha không hợp lệ.");
            foreach (var row in rows) map[row.Ma] = row;
        }
    }

    private static HocTapException Loi(string message) => new(400, "VALIDATION_ERROR", message);
    private static HocTapException XungDot(string code, string message) => new(409, code, message);
}
