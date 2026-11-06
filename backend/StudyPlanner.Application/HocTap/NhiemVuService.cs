using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using StudyPlanner.Application.Administration;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;
namespace StudyPlanner.Application.HocTap;

// All mutations share one transaction: graph + sessions + reminders + history + source revision.
public sealed class NhiemVuService(IHocTapStore store, HocTapDieuPhoi dieuPhoi, TimeProvider clock)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };
    public async Task<HocTapResponse<PhanTrangResponse<NhiemVuResponse>>> DanhSachAsync(int chu, DanhSachNhiemVuRequest q, CancellationToken ct)
    {
        Validate(q);
        if ((q.Loai.HasValue && !Enum.IsDefined(q.Loai.Value)) || (q.MucDoUuTien.HasValue && !Enum.IsDefined(q.MucDoUuTien.Value)) || (q.TrangThai.HasValue && !Enum.IsDefined(q.TrangThai.Value))) throw Invalid("Giá trị enum bộ lọc không hợp lệ.");
        if (!new[] { "all", "today", "upcoming", "inbox", "completed", "overdue", "undated" }.Contains(q.CheDo) ||
            !new[] { "due", "priority", "progress", "created" }.Contains(q.SapXep)) throw Invalid("Bộ lọc/sắp xếp không hợp lệ.");
        if ((long)(q.Trang - 1) * q.KichThuocTrang > int.MaxValue) throw Invalid("Trang quá lớn.");
        if (q.TuHan.HasValue && q.DenHan.HasValue && q.TuHan >= q.DenHan) throw Invalid("Khoảng hạn không hợp lệ.");
        await using var tx = await dieuPhoi.MoAsync(chu, null, false, ct); var now = clock.GetUtcNow();
        await PhamViAsync(chu, q.MaHocKy, q.MaMonHoc, ct);
        var query = store.TruyVan<NhiemVu>().Where(x => x.MaNguoiDung == chu);
        var leaves = q.ChiLa || q.CheDo is "today" or "upcoming" or "completed" or "overdue" or "undated";
        query = leaves ? query.Where(x => !x.DanhSachNhiemVuCon.Any()) : query.Where(x => x.MaNhiemVuCha == null);
        if (q.MaHocKy.HasValue) query = query.Where(x => x.MaHocKy == q.MaHocKy);
        if (q.MaMonHoc.HasValue) query = query.Where(x => x.MaMonHoc == q.MaMonHoc);
        if (q.Loai.HasValue) query = query.Where(x => x.Loai == q.Loai);
        if (q.MucDoUuTien.HasValue) query = query.Where(x => x.MucDoUuTien == q.MucDoUuTien);
        if (q.CheDo == "completed") query = query.Where(x => x.TrangThai == TrangThaiNhiemVu.Completed);
        else if (q.TrangThai.HasValue) query = query.Where(x => x.TrangThai == q.TrangThai);
        else query = q.CheDo == "completed" ? query.Where(x => x.TrangThai == TrangThaiNhiemVu.Completed) : query.Where(x => x.TrangThai != TrangThaiNhiemVu.Completed);
        var keyword = q.TuKhoa?.Trim().ToUpperInvariant();
        if (!string.IsNullOrEmpty(keyword)) query = query.Where(x => x.TieuDe.ToUpper().Contains(keyword) || (x.MoTa != null && x.MoTa.ToUpper().Contains(keyword)));
        if (q.CheDo == "inbox") query = query.Where(x => x.MaHocKy == null && x.MaMonHoc == null);
        if (q.CheDo is "today" or "upcoming")
        {
            var zone = QuyTacThoiGian.MuiGio(tx.NguoiDung.MuiGio);
            var date = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, zone).DateTime);
            query = QuyTacCay.LocHan(query, QuyTacThoiGian.Utc(date, TimeOnly.MinValue, zone),
                QuyTacThoiGian.Utc(date.AddDays(q.CheDo == "today" ? 1 : 7), TimeOnly.MinValue, zone));
        }
        if (q.CheDo == "overdue") query = QuyTacCay.LocHan(query, null, now);
        if (q.CheDo == "undated") query = QuyTacCay.LocHan(query, null, null, true);
        query = QuyTacCay.LocHan(query, q.TuHan, q.DenHan);
        var count = await store.DemAsync(query, ct);
        IOrderedQueryable<NhiemVu> ordered = q.SapXep switch
        {
            "priority" => query.OrderByDescending(x => x.MucDoUuTien == MucDoUuTienNhiemVu.High ? 2 : x.MucDoUuTien == MucDoUuTienNhiemVu.Medium ? 1 : 0),
            "progress" => query.OrderByDescending(x => x.TienDo),
            "created" => query.OrderByDescending(x => x.NgayTao),
            _ => query.OrderBy(QuyTacCay.BieuThucHan)
        };
        var page = await store.DanhSachAsync(ordered.ThenBy(x => x.Ma).Skip((q.Trang - 1) * q.KichThuocTrang).Take(q.KichThuocTrang), ct);
        var cache = new Dictionary<int, List<NhiemVu>>(); var result = new List<NhiemVuResponse>();
        foreach (var n in page)
        {
            var tree = cache.Values.FirstOrDefault(x => x.Any(v => v.Ma == n.Ma)) ?? await store.CayAsync(chu, n.Ma, ct);
            cache.TryAdd(tree[0].Ma, tree); result.Add(Dto(n, tree, now));
        }
        await tx.CommitAsync(false, ct); return new(tx.NguoiDung.PhienBanLapLich, new(result, q.Trang, q.KichThuocTrang, count));
    }
    public async Task<HocTapResponse<NhiemVuResponse>> ChiTietAsync(int chu, int ma, CancellationToken ct)
    {
        await using var tx = await dieuPhoi.MoAsync(chu, null, false, ct); var tree = await store.CayAsync(chu, ma, ct);
        return new(tx.NguoiDung.PhienBanLapLich, Dto(tree.Single(x => x.Ma == ma), tree, clock.GetUtcNow()));
    }
    public async Task<HocTapResponse<IReadOnlyList<NhiemVuResponse>>> CayAsync(int chu, int ma, CancellationToken ct)
    {
        await using var tx = await dieuPhoi.MoAsync(chu, null, false, ct); var tree = await store.CayAsync(chu, ma, ct); var now = clock.GetUtcNow();
        return new(tx.NguoiDung.PhienBanLapLich, Nhanh(tree, ma).Select(x => Dto(x, tree, now)).ToArray());
    }
    public async Task<HocTapResponse<NhiemVuResponse>> ThemAsync(int chu, long version, NhiemVuRequest r, CancellationToken ct)
    {
        KiemTra(r); await using var tx = await dieuPhoi.MoAsync(chu, version, true, ct);
        var scope = await PhamViAsync(chu, r.MaHocKy, r.MaMonHoc, ct); var now = clock.GetUtcNow();
        var n = new NhiemVu { MaNguoiDung = chu, NgayTao = now, NgayCapNhat = now };
        Gan(n, r, scope); store.Them(n); await store.LuuAsync(ct);
        // New timed events may also invalidate existing planned sessions.
        var changed = await PhienBiAnhHuongAsync(chu, [n], [], [n.Ma], now, ct, new HashSet<int>{n.Ma});
        await XuLyPhienAsync(changed, r.HuyPhienLienQuan, false, now, ct);
        GhiLichSu(n, null, Snapshot(n), HanhDongLichSuNhiemVu.Created, "User", now);
        await DongBoNhacAsync(chu, [n], now, ct); await tx.CommitAsync(true, ct);
        return new(tx.NguoiDung.PhienBanLapLich, Dto(n, [n], now));
    }
    public Task<HocTapResponse<object>> ThayDoiAsync(int chu, int ma, long? version, string action, object input, bool preview, CancellationToken ct) => ChayAsync(chu, ma, version, action, input, preview, ct);
    public async Task<HocTapResponse<AnhHuongNhiemVuResponse>> XemTruocAsync(int chu, int ma, XemThayDoiNhiemVuRequest r, CancellationToken ct)
    {
        object input;
        try
        {
            input = r.ThaoTac switch
            {
                "update" => r.NoiDung.Deserialize<NhiemVuRequest>(Json)!, "children" => r.NoiDung.Deserialize<ThemNhiemVuConRequest>(Json)!,
                "progress" or "complete" or "reopen" => r.NoiDung.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null ? new TienDoNhiemVuRequest() : r.NoiDung.Deserialize<TienDoNhiemVuRequest>(Json)!,
                "parent" => r.NoiDung.Deserialize<ChaNhiemVuRequest>(Json)!, "scope" => r.NoiDung.Deserialize<PhamViNhiemVuRequest>(Json)!,
                "delete" => new TienDoNhiemVuRequest(), _ => throw Invalid("Thao tác không hợp lệ.")
            };
        }
        catch (JsonException) { throw Invalid("Nội dung xem trước không hợp lệ."); }
        if (input is null) throw Invalid("Thiếu nội dung xem trước.");
        var result = await ChayAsync(chu, ma, null, r.ThaoTac, input, true, ct);
        return new(result.PhienBan, (AnhHuongNhiemVuResponse)result.DuLieu);
    }
    public async Task<HocTapResponse<PhanTrangResponse<LichSuNhiemVuResponse>>> LichSuAsync(int chu, DanhSachLichSuRequest q, CancellationToken ct)
    {
        Validate(q); if ((long)(q.Trang - 1) * q.KichThuocTrang > int.MaxValue || (q.Tu.HasValue && q.Den.HasValue && q.Tu >= q.Den)) throw Invalid("Khoảng/trang không hợp lệ.");
        await using var tx = await dieuPhoi.MoAsync(chu, null, false, ct);
        var query = store.TruyVan<LichSuNhiemVu>().Where(x => x.MaNguoiDung == chu);
        if (q.MaNhiemVu.HasValue) query = query.Where(x => x.MaNhiemVu == q.MaNhiemVu);
        if (q.HanhDong.HasValue) query = query.Where(x => x.HanhDong == q.HanhDong);
        if (q.Tu.HasValue) query = query.Where(x => x.NgayThayDoi >= q.Tu);
        if (q.Den.HasValue) query = query.Where(x => x.NgayThayDoi < q.Den);
        var count = await store.DemAsync(query, ct);
        var rows = await store.DanhSachAsync(query.OrderByDescending(x => x.NgayThayDoi).ThenByDescending(x => x.Ma).Skip((q.Trang - 1) * q.KichThuocTrang).Take(q.KichThuocTrang)
            .Select(x => new LichSuNhiemVuResponse(x.Ma,x.MaNhiemVu,x.TieuDeNhiemVu,x.HanhDong,x.NguonThayDoi,x.DuLieuTruoc,x.DuLieuSau,x.NgayThayDoi)), ct);
        return new(tx.NguoiDung.PhienBanLapLich, new(rows,q.Trang,q.KichThuocTrang,count));
    }
    private async Task<HocTapResponse<object>> ChayAsync(int chu, int ma, long? version, string action, object input, bool preview, CancellationToken ct)
    {
        await using var tx = await dieuPhoi.MoAsync(chu, version, !preview, ct); var now = clock.GetUtcNow();
        var nodes = await store.CayAsync(chu, ma, ct); var node = nodes.Single(x => x.Ma == ma);
        if (action == "parent" && ((ChaNhiemVuRequest)input).MaNhiemVuCha is int target)
            nodes = nodes.Concat(await store.CayAsync(chu, target, ct)).DistinctBy(x => x.Ma).ToList();
        var before = nodes.ToDictionary(x => x.Ma, Snapshot);
        var beforeSchedule = nodes.ToDictionary(x => x.Ma,x => DauVetLapLich(x,nodes));
        var beforeEvents = nodes.ToDictionary(x => x.Ma,x => (x.BatDauSuKien,x.KetThucSuKien));
        var added = new List<NhiemVu>(); var deleted = new List<NhiemVu>(); bool confirm;
        switch (action)
        {
            case "update":
                var update = (NhiemVuRequest)input; KiemTra(update); confirm = update.HuyPhienLienQuan;
                if (update.MaMonHoc != node.MaMonHoc || (update.MaHocKy.HasValue && update.MaHocKy != node.MaHocKy)) throw new HocTapException(409,"TASK_SCOPE_ACTION_REQUIRED","Đổi học kỳ/môn qua thao tác đổi phạm vi của nhiệm vụ gốc.");
                if (nodes.Any(x => x.MaNhiemVuCha == ma) && update.ThoiLuongDuKienPhut.HasValue) throw new HocTapException(409,"TASK_ESTIMATE_IS_DERIVED","Cha lấy thời lượng từ các lá.");
                Gan(node,update,(node.MaHocKy,node.MaMonHoc)); break;
            case "children":
                var children = (ThemNhiemVuConRequest)input; Validate(children); confirm = children.HuyPhienLienQuan;
                foreach (var r in children.DanhSach)
                {
                    KiemTra(r);
                    if ((r.MaHocKy.HasValue && r.MaHocKy != node.MaHocKy) || (r.MaMonHoc.HasValue && r.MaMonHoc != node.MaMonHoc)) throw Invalid("Con phải cùng học kỳ/môn với cha.");
                    var child = new NhiemVu { Ma = -1 - added.Count, MaNguoiDung = chu, MaNhiemVuCha = ma, NgayTao = now, NgayCapNhat = now };
                    Gan(child,r,(node.MaHocKy,node.MaMonHoc)); added.Add(child); nodes.Add(child);
                }
                node.ThoiLuongDuKienPhut = null; break;
            case "progress": case "complete": case "reopen":
                var progress = (TienDoNhiemVuRequest)input; Validate(progress); confirm = progress.HuyPhienLienQuan;
                if (nodes.Any(x => x.MaNhiemVuCha == ma)) throw new HocTapException(409,"TASK_PROGRESS_IS_DERIVED","Tiến độ cha được tính từ con; hãy cập nhật nhiệm vụ lá.");
                var value = action == "complete" ? 100 : progress.TienDo;
                if (action == "reopen" && value == 100) throw Invalid("Mở lại nhận tiến độ 0–99.");
                node.TienDo = value; node.TrangThai = value == 100 ? TrangThaiNhiemVu.Completed : value == 0 ? TrangThaiNhiemVu.NotStarted : TrangThaiNhiemVu.InProgress;
                node.NgayHoanThanh = value == 100 ? node.NgayHoanThanh ?? now : null; break;
            case "parent":
                var parent = (ChaNhiemVuRequest)input; confirm = parent.HuyPhienLienQuan;
                if (parent.MaNhiemVuCha is int pid)
                {
                    var pn = nodes.Single(x => x.Ma == pid);
                    if (pn.MaHocKy != node.MaHocKy || pn.MaMonHoc != node.MaMonHoc) throw new HocTapException(409,"TASK_SCOPE_CONFLICT","Không chuyển cha khác học kỳ/môn. Đổi phạm vi cả cây trước.");
                }
                node.MaNhiemVuCha = parent.MaNhiemVuCha; break;
            case "scope":
                var scopeRequest = (PhamViNhiemVuRequest)input; confirm = scopeRequest.HuyPhienLienQuan;
                if (node.MaNhiemVuCha.HasValue) throw new HocTapException(409,"TASK_ROOT_REQUIRED","Chỉ đổi phạm vi từ nhiệm vụ gốc.");
                var scope = await PhamViAsync(chu,scopeRequest.MaHocKy,scopeRequest.MaMonHoc,ct);
                foreach (var n in nodes) { n.MaHocKy = scope.HocKy; n.MaMonHoc = scope.MonHoc; } break;
            case "delete":
                confirm = ((TienDoNhiemVuRequest)input).HuyPhienLienQuan; deleted = Nhanh(nodes,ma).ToList(); nodes.RemoveAll(x => deleted.Contains(x)); break;
            default: throw Invalid("Thao tác không hợp lệ.");
        }
        KiemTraRung(nodes); QuyTacCay.TinhLai(nodes,now);
        var changedSchedule = nodes.Where(x => !beforeSchedule.TryGetValue(x.Ma,out var old) || DauVetLapLich(x,nodes) != old).Select(x => x.Ma).ToArray();
        var changedEvents = nodes.Where(x => !beforeEvents.TryGetValue(x.Ma,out var old) || old != (x.BatDauSuKien,x.KetThucSuKien)).Select(x => x.Ma).ToHashSet();
        var affected = await PhienBiAnhHuongAsync(chu,nodes,deleted,changedSchedule,now,ct,changedEvents);
        var deletedIds = deleted.Select(x => x.Ma).ToArray();
        var changedIds = nodes.Where(x => x.Ma > 0 && before.TryGetValue(x.Ma,out var old) && Snapshot(x) != old).Select(x => x.Ma).ToArray();
        var impact = new AnhHuongNhiemVuResponse(deleted.Count,
            await store.DemAsync(store.TruyVan<LichNhac>().Where(x => x.MaNguoiDung == chu && deletedIds.Contains(x.MaNhiemVu)),ct),
            await store.DemAsync(store.TruyVan<ThongBao>().Where(x => x.MaNguoiDung == chu && deletedIds.Contains(x.MaNhiemVu)),ct),
            await store.DemAsync(store.TruyVan<TepDinhKemNhiemVu>().Where(x => x.MaNguoiTaiLen == chu && deletedIds.Contains(x.MaNhiemVu)),ct),
            affected.Select(x => x.Ma).ToArray(),
            await store.DanhSachAsync(store.TruyVan<PhienTuHoc>().Where(x => x.MaNguoiDung == chu && deletedIds.Contains(x.MaNhiemVu)).Select(x => x.Ma),ct), changedIds);
        if (preview) return new(tx.NguoiDung.PhienBanLapLich, impact);
        if (affected.Count > 0 && !confirm) throw new HocTapException(409,"SCHEDULE_CHANGE_CONFIRMATION_REQUIRED","Thao tác cần hủy các phiên dự kiến liên quan. Hãy xem trước và xác nhận.",new { anhHuong = impact });
        await XuLyPhienAsync(affected,confirm,false,now,ct);
        foreach (var n in added) { n.Ma = 0; store.Them(n); }
        foreach (var n in nodes.Where(x => x.Ma > 0 && changedIds.Contains(x.Ma))) n.NgayCapNhat = now;
        foreach (var n in deleted)
        {
            GhiLichSu(n,before[n.Ma],null,HanhDongLichSuNhiemVu.Deleted,"User",now);
            store.Xoa(n);
        }
        await store.LuuAsync(ct);
        foreach (var n in added) GhiLichSu(n,null,Snapshot(n),HanhDongLichSuNhiemVu.Created,"User",now);
        foreach (var n in nodes.Where(x => changedIds.Contains(x.Ma)))
        {
            var old = before[n.Ma]; var after = Snapshot(n);
            var historyAction = n.Ma == ma && action == "complete" ? HanhDongLichSuNhiemVu.Completed : n.Ma == ma && action == "reopen" ? HanhDongLichSuNhiemVu.Reopened : HanhDongLichSuNhiemVu.Updated;
            GhiLichSu(n,old,after,historyAction,n.Ma == ma ? "User" : "System",now);
        }
        var changed = changedIds.Length > 0 || added.Count > 0 || deleted.Count > 0 || affected.Count > 0;
        if (changed) await DongBoNhacAsync(chu,nodes,now,ct);
        await tx.CommitAsync(changed,ct);
        object data = action == "delete" ? impact : action == "children" ? added.Select(x => Dto(x,nodes,now)).ToArray() : Dto(node,nodes,now);
        return new(tx.NguoiDung.PhienBanLapLich,data);
    }
    private async Task<List<PhienTuHoc>> PhienBiAnhHuongAsync(int chu, IReadOnlyList<NhiemVu> nodes, IReadOnlyList<NhiemVu> deleted, IReadOnlyList<int> relevant, DateTimeOffset now, CancellationToken ct, IReadOnlySet<int>? changedEvents = null)
    {
        if (relevant.Count == 0 && deleted.Count == 0 && (changedEvents is null || changedEvents.Count == 0)) return [];
        var ids = relevant.Concat(deleted.Select(x => x.Ma)).Where(x => x > 0).ToArray();
        if (await store.DemAsync(store.TruyVan<DoanThucHoc>().Where(x => x.MaNguoiDung == chu && ids.Contains(x.MaNhiemVu) && x.KetThucThucTe == null),ct) > 0)
            throw new HocTapException(409,"SESSION_IN_PROGRESS","Hãy kết thúc đoạn thực học trước khi thay đổi nhiệm vụ.");
        var sessions = await store.DanhSachAsync(store.TruyVan<PhienTuHoc>().Where(x => x.MaNguoiDung == chu && (x.TrangThai == TrangThaiPhienTuHoc.InProgress || (x.TrangThai == TrangThaiPhienTuHoc.Planned && x.KetThucDuKien > now))).Take(10001),ct);
        if (sessions.Count > 10000) throw Invalid("Quá nhiều phiên để kiểm tra; hãy thu hẹp dữ liệu.");
        var map = nodes.ToDictionary(x => x.Ma); var removed = deleted.Select(x => x.Ma).ToHashSet(); var cancel = new HashSet<PhienTuHoc>();
        foreach (var s in sessions)
        {
            var invalid = removed.Contains(s.MaNhiemVu);
            if (relevant.Contains(s.MaNhiemVu) && map.TryGetValue(s.MaNhiemVu,out var n)) invalid |= n.TienDo == 100 || !n.ThoiLuongDuKienPhut.HasValue || nodes.Any(x => x.MaNhiemVuCha == n.Ma) || (QuyTacCay.HanHieuLuc(n,map).Han is DateTimeOffset due && s.KetThucDuKien > due);
            invalid |= nodes.Any(x => changedEvents is not null && changedEvents.Contains(x.Ma) && x.BatDauSuKien is DateTimeOffset start && x.KetThucSuKien is DateTimeOffset end && start < s.KetThucDuKien && s.BatDauDuKien < end);
            if (invalid)
            {
                if (s.TrangThai == TrangThaiPhienTuHoc.InProgress) throw new HocTapException(409,"SESSION_IN_PROGRESS","Thay đổi ảnh hưởng phiên đang học; hãy kết thúc phiên trước.");
                cancel.Add(s);
            }
        }
        foreach (var n in nodes.Where(x => relevant.Contains(x.Ma) && !nodes.Any(c => c.MaNhiemVuCha == x.Ma) && x.ThoiLuongConLaiPhut.HasValue))
        {
            var pending = sessions.Where(s => s.MaNhiemVu == n.Ma && !cancel.Contains(s)).OrderByDescending(s => s.BatDauDuKien).ToList();
            var booked = pending.Sum(s => (s.KetThucDuKien - (s.BatDauDuKien > now ? s.BatDauDuKien : now)).TotalMinutes);
            foreach (var s in pending)
            {
                if (booked <= n.ThoiLuongConLaiPhut!.Value) break;
                if (s.TrangThai == TrangThaiPhienTuHoc.InProgress) throw new HocTapException(409,"SESSION_IN_PROGRESS","Tiến độ mới làm thừa phiên đang học; hãy kết thúc phiên trước.");
                cancel.Add(s); booked -= (s.KetThucDuKien - (s.BatDauDuKien > now ? s.BatDauDuKien : now)).TotalMinutes;
            }
        }
        return cancel.OrderBy(x => x.Ma).ToList();
    }
    private static Task XuLyPhienAsync(List<PhienTuHoc> sessions,bool confirm,bool preview,DateTimeOffset now,CancellationToken ct)
    {
        if (sessions.Count > 0 && !confirm && !preview) throw new HocTapException(409,"SCHEDULE_CHANGE_CONFIRMATION_REQUIRED","Cần xác nhận hủy phiên dự kiến bị ảnh hưởng.",new { maPhienBiHuy = sessions.Select(x => x.Ma).ToArray() });
        if (!preview) foreach (var s in sessions) { s.TrangThai = TrangThaiPhienTuHoc.Cancelled; s.NgayCapNhat = now; }
        return Task.CompletedTask;
    }
    private async Task DongBoNhacAsync(int chu, IReadOnlyList<NhiemVu> nodes, DateTimeOffset now, CancellationToken ct)
    {
        var ids = nodes.Select(x => x.Ma).ToArray(); var map = nodes.ToDictionary(x => x.Ma);
        var pending = await store.DanhSachAsync(store.TruyVan<LichNhac>().Where(x => x.MaNguoiDung == chu && ids.Contains(x.MaNhiemVu) && (x.TrangThai == TrangThaiLichNhac.Pending || x.TrangThai == TrangThaiLichNhac.Failed)),ct);
        var user = await store.NguoiDungAsync(chu,false,ct);
        foreach (var reminder in pending)
        {
            var node = map[reminder.MaNhiemVu]; var due = QuyTacCay.HanHieuLuc(node,map).Han;
            reminder.NgayCapNhat = now;
            if (node.TienDo == 100 || due is null || due <= now) { reminder.TrangThai = TrangThaiLichNhac.Cancelled; reminder.NgayPhat = null; reminder.LanThuTiepTheoLuc = null; continue; }
            var minutes = checked((node.NhacTruocGio ?? user.NhacTruocGio) * 60);
            reminder.NhacTruocPhut = minutes; reminder.ThoiDiemNhac = due.Value.AddMinutes(-minutes) < now ? now : due.Value.AddMinutes(-minutes);
            reminder.TrangThai = TrangThaiLichNhac.Pending; reminder.SoLanThu = 0; reminder.LanThuTiepTheoLuc = null; reminder.NgayPhat = null;
        }
    }
    private async Task<(int? HocKy,int? MonHoc)> PhamViAsync(int chu,int? hocKy,int? mon,CancellationToken ct)
    {
        if (hocKy is <= 0 || mon is <= 0) throw Invalid("Mã học kỳ/môn phải lớn hơn 0.");
        if (mon.HasValue)
        {
            var course = await store.DauTienAsync(store.TruyVan<MonHoc>().Where(x => x.Ma == mon && x.HocKy.MaNguoiDung == chu),ct) ?? throw NotFound();
            if (hocKy.HasValue && hocKy != course.MaHocKy) throw Invalid("Môn không thuộc học kỳ đã chọn."); hocKy = course.MaHocKy;
        }
        if (hocKy.HasValue && await store.DemAsync(store.TruyVan<HocKy>().Where(x => x.Ma == hocKy && x.MaNguoiDung == chu),ct) == 0) throw NotFound();
        return (hocKy,mon);
    }
    private static void Gan(NhiemVu n,NhiemVuRequest r,(int? HocKy,int? MonHoc) scope)
    {
        n.TieuDe = r.TieuDe.Trim(); n.MoTa = string.IsNullOrWhiteSpace(r.MoTa) ? null : r.MoTa.Trim();
        n.MaHocKy = scope.HocKy; n.MaMonHoc = scope.MonHoc; n.Loai = r.Loai ?? (scope.MonHoc.HasValue ? LoaiNhiemVu.Assignment : LoaiNhiemVu.Personal);
        n.MucDoUuTien = r.MucDoUuTien; n.HanHoanThanh = r.HanHoanThanh?.ToUniversalTime(); n.BatDauSuKien = r.BatDauSuKien?.ToUniversalTime(); n.KetThucSuKien = r.KetThucSuKien?.ToUniversalTime();
        n.ThoiLuongDuKienPhut = r.ThoiLuongDuKienPhut; n.NhacTruocGio = r.NhacTruocGio;
    }
    private static void KiemTra(NhiemVuRequest r)
    {
        Validate(r);
        if (string.IsNullOrWhiteSpace(r.TieuDe) || !Enum.IsDefined(r.MucDoUuTien) || (r.Loai.HasValue && !Enum.IsDefined(r.Loai.Value))) throw Invalid("Tiêu đề/loại/ưu tiên không hợp lệ.");
        if (r.BatDauSuKien.HasValue != r.KetThucSuKien.HasValue || (r.BatDauSuKien.HasValue && r.KetThucSuKien <= r.BatDauSuKien)) throw Invalid("Giờ sự kiện cần đủ hai đầu và kết thúc sau bắt đầu.");
    }
    private static void Validate(object r)
    {
        var errors = new List<ValidationResult>(); if (!Validator.TryValidateObject(r,new ValidationContext(r),errors,true)) throw Invalid(string.Join(" ",errors.Select(x => x.ErrorMessage)));
    }
    private static void KiemTraRung(List<NhiemVu> nodes)
    {
        var map = nodes.ToDictionary(x => x.Ma); var groups = new Dictionary<int,List<NhiemVu>>();
        foreach (var n in nodes)
        {
            var current = n; var seen = new HashSet<int>();
            while (current.MaNhiemVuCha is int id)
            {
                if (!seen.Add(current.Ma)) throw new HocTapException(409,"TASK_CYCLE","Không được tạo vòng trong cây.");
                if (!map.TryGetValue(id,out var p)) throw new HocTapException(409,"TASK_TREE_INVALID","Thiếu nhiệm vụ cha."); current = p;
            }
            if (!groups.TryGetValue(current.Ma,out var group)) groups[current.Ma] = group = []; group.Add(n);
        }
        foreach (var group in groups.Values) QuyTacCay.KiemTra(group);
    }
    private static IReadOnlyList<NhiemVu> Nhanh(IReadOnlyList<NhiemVu> nodes,int id)
    {
        var children = nodes.ToLookup(x => x.MaNhiemVuCha); var result = new List<NhiemVu>();
        void Visit(NhiemVu n) { result.Add(n); foreach (var c in children[n.Ma]) Visit(c); }
        Visit(nodes.Single(x => x.Ma == id)); return result;
    }
    private static NhiemVuResponse Dto(NhiemVu n,IReadOnlyList<NhiemVu> tree,DateTimeOffset now)
    {
        var map = tree.ToDictionary(x => x.Ma); var han = QuyTacCay.HanHieuLuc(n,map); var branch = Nhanh(tree,n.Ma);
        var leaves = branch.Where(x => !tree.Any(c => c.MaNhiemVuCha == x.Ma)).ToArray(); var missing = leaves.Any(x => !x.ThoiLuongDuKienPhut.HasValue);
        var estimate = missing ? (int?)null : checked(leaves.Sum(x => x.ThoiLuongDuKienPhut!.Value));
        var remaining = missing ? (int?)null : checked(leaves.Sum(x => x.ThoiLuongConLaiPhut!.Value));
        return new(n.Ma,n.MaHocKy,n.MaMonHoc,n.MaNhiemVuCha,n.TieuDe,n.MoTa,n.Loai,n.MucDoUuTien,n.HanHoanThanh,han.Han,han.Nguon,n.BatDauSuKien,n.KetThucSuKien,n.TrangThai,n.TienDo,estimate,remaining,missing,branch.Count>1,n.TienDo<100 && han.Han.HasValue && han.Han<now,n.NhacTruocGio,n.NgayHoanThanh,n.NgayTao,n.NgayCapNhat);
    }
    private static string Snapshot(NhiemVu n) => JsonSerializer.Serialize(new { n.TieuDe,n.MoTa,n.MaNhiemVuCha,n.MaHocKy,n.MaMonHoc,n.Loai,n.MucDoUuTien,n.HanHoanThanh,n.BatDauSuKien,n.KetThucSuKien,n.TienDo,n.TrangThai,n.ThoiLuongDuKienPhut,n.NhacTruocGio,n.NgayHoanThanh },Json);
    private static string DauVetLapLich(NhiemVu n,IReadOnlyList<NhiemVu> nodes) => JsonSerializer.Serialize(new
    {
        n.MaNhiemVuCha,n.MaHocKy,n.MaMonHoc,n.TienDo,n.ThoiLuongDuKienPhut,n.BatDauSuKien,n.KetThucSuKien,
        Han=QuyTacCay.HanHieuLuc(n,nodes.ToDictionary(x=>x.Ma)).Han,CoCon=nodes.Any(x=>x.MaNhiemVuCha==n.Ma)
    },Json);
    private void GhiLichSu(NhiemVu n,string? before,string? after,HanhDongLichSuNhiemVu action,string source,DateTimeOffset now) => store.Them(new LichSuNhiemVu { MaNguoiDung=n.MaNguoiDung,MaNhiemVu=n.Ma,TieuDeNhiemVu=n.TieuDe,HanhDong=action,NguonThayDoi=source,DuLieuTruoc=before,DuLieuSau=after,NgayThayDoi=now });
    private static HocTapException Invalid(string message) => new(400,"VALIDATION_ERROR",message);
    private static HocTapException NotFound() => new(404,"RESOURCE_NOT_FOUND","Không tìm thấy dữ liệu.");
}
