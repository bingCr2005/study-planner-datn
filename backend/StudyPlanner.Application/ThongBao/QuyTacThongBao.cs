using StudyPlanner.Application.HocTap;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Application.CanhBao;

public static class QuyTacThongBao
{
    public const string SapHan = "Nhiệm vụ sắp đến hạn", QuaHan = "Nhiệm vụ đã quá hạn";

    // One inbox row per task, as required by the existing unique index. Crossing
    // the deadline replaces its reminder with an unread overdue notification.
    public static IReadOnlyList<Domain.Entities.ThongBao> DongBo(IReadOnlyList<NhiemVu> tasks,
        IReadOnlyList<Domain.Entities.ThongBao> existing, NguoiDung user, DateTimeOffset now)
    {
        var map = tasks.ToDictionary(x => x.Ma);
        var parents = tasks.Where(x => x.MaNhiemVuCha.HasValue).Select(x => x.MaNhiemVuCha!.Value).ToHashSet();
        var inbox = existing.ToDictionary(x => x.MaNhiemVu);
        var added = new List<Domain.Entities.ThongBao>();
        foreach (var task in tasks)
        {
            inbox.TryGetValue(task.Ma, out var row);
            var due = QuyTacCay.HanHieuLuc(task, map).Han;
            var completed = task.TrangThai == TrangThaiNhiemVu.Completed || task.TienDo == 100;
            // Delivered messages remain readable after completion; no further reminder.
            if (completed)
            {
                if (row is not null && row.TrangThaiPhat != TrangThaiPhatThongBao.Delivered) Cancel(row, now);
                continue;
            }
            if (parents.Contains(task.Ma) || due is null)
            { if (row is not null) Cancel(row, now); continue; }
            var reminder = due.Value.AddHours(-(task.NhacTruocGio ?? user.NhacTruocGio));
            var overdue = due.Value < now;
            var marker = overdue ? due.Value : reminder;
            var title = overdue ? QuaHan : SapHan;
            if (now < reminder)
            { if (row is not null) Cancel(row, now); continue; }
            if (row is null)
            {
                row = new() { MaNguoiDung = user.Ma, MaNhiemVu = task.Ma, NgayTao = now };
                added.Add(row);
            }
            if (row.TrangThaiPhat == TrangThaiPhatThongBao.Delivered && row.ThoiDiemNhac == marker && row.TieuDe == title) continue;
            row.TieuDe = title;
            row.NoiDung = overdue ? "Nhiệm vụ đã qua hạn và chưa hoàn thành." : "Nhiệm vụ sắp đến hạn hoàn thành.";
            row.ThoiDiemNhac = marker;
            row.TrangThaiPhat = TrangThaiPhatThongBao.Delivered;
            row.NgayPhat = now; row.NgayDoc = null; row.NgayCapNhat = now;
            row.MaLichNhac = null; row.SoLanThu = 0; row.LanThuTiepTheoLuc = null;
        }
        return added;
    }

    private static void Cancel(Domain.Entities.ThongBao row, DateTimeOffset now)
    {
        if (row.TrangThaiPhat == TrangThaiPhatThongBao.Cancelled) return;
        row.TrangThaiPhat = TrangThaiPhatThongBao.Cancelled;
        row.NgayDoc = null; row.NgayPhat = null; row.LanThuTiepTheoLuc = null; row.NgayCapNhat = now;
    }
}
