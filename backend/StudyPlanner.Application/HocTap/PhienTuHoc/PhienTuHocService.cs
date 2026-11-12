using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Application.HocTap;

public sealed class PhienTuHocService(IHocTapStore store, HocTapDieuPhoi dieuPhoi, QuyTacDatPhien datPhien, TimeProvider clock)
{
    public async Task<HocTapResponse<IReadOnlyList<PhienTuHocResponse>>> DanhSachAsync(int chu,
        DateTimeOffset? tu, DateTimeOffset? den, int? maNhiemVu, CancellationToken ct, int? maLanApDung = null)
    {
        if (tu is DateTimeOffset start && den is DateTimeOffset end)
            QuyTacThoiGian.KiemTraKhoang(start, end);
        else if (maLanApDung is null || tu.HasValue || den.HasValue)
            throw new HocTapException(400, "VALIDATION_ERROR", "Cần nhập đủ khoảng ngày hoặc chọn một lần áp dụng lịch.");
        await using var session = await dieuPhoi.MoAsync(chu, null, false, ct);
        if (maNhiemVu is int id && await store.DemAsync(store.TruyVan<NhiemVu>().Where(t => t.Ma == id && t.MaNguoiDung == chu), ct) == 0)
            throw KhongTimThay();
        var query = store.TruyVan<PhienTuHoc>().Where(p => p.MaNguoiDung == chu && (!maNhiemVu.HasValue || p.MaNhiemVu == maNhiemVu));
        if (maLanApDung is int batch)
        {
            if (await store.DemAsync(store.TruyVan<LanApDungLich>().Where(b => b.Ma == batch && b.MaNguoiDung == chu), ct) == 0)
                throw KhongTimThay();
            query = query.Where(p => p.MaLanApDung == batch);
        }
        if (tu.HasValue && den.HasValue)
            query = query.Where(p => p.BatDauDuKien < den.Value && p.KetThucDuKien > tu.Value);
        var rows = await store.DanhSachAsync(query.OrderBy(p => p.BatDauDuKien).ThenBy(p => p.Ma).Take(5001)
            .Select(p => new { Phien = p, p.NhiemVu.TieuDe, p.NhiemVu.MaHocKy, p.NhiemVu.MaMonHoc }), ct);
        if (rows.Count > 5000) throw new HocTapException(400, "QUERY_LIMIT", "Có quá 5.000 phiên; hãy thu hẹp khoảng ngày.");
        await session.CommitAsync(false, ct);
        return new(session.NguoiDung.PhienBanLapLich, rows.Select(r => PhienTuHocResponse.Tu(r.Phien) with
        { TieuDeNhiemVu = r.TieuDe, MaHocKy = r.MaHocKy, MaMonHoc = r.MaMonHoc }).ToList());
    }

    public async Task<HocTapResponse<PhienTuHocResponse>> ChiTietAsync(int chu, int id, CancellationToken ct)
    {
        await using var session = await dieuPhoi.MoAsync(chu, null, false, ct);
        var row = await TimAsync(chu, id, ct);
        var doan = await DocDoanThucHocAsync(chu, id, ct);
        await session.CommitAsync(false, ct);
        return new(session.NguoiDung.PhienBanLapLich, PhienTuHocResponse.Tu(row, doan));
    }

    public async Task<HocTapResponse<PhienTuHocResponse>> ThemAsync(int chu, long expected, PhienTuHocRequest request, CancellationToken ct)
    {
        KiemTraGhiChu(request);
        await using var session = await dieuPhoi.MoAsync(chu, expected, true, ct);
        var now = clock.GetUtcNow();
        await datPhien.KiemTraAsync(chu, [new(request.MaNhiemVu, request.BatDauDuKien, request.KetThucDuKien)], now, null, ct);
        var row = new PhienTuHoc { MaNguoiDung = chu, MaNhiemVu = request.MaNhiemVu,
            BatDauDuKien = request.BatDauDuKien.ToUniversalTime(), KetThucDuKien = request.KetThucDuKien.ToUniversalTime(),
            GhiChu = TuyChon(request.GhiChu), TrangThai = TrangThaiPhienTuHoc.Planned, NgayTao = now, NgayCapNhat = now };
        store.Them(row);
        await session.CommitAsync(true, ct);
        return new(session.NguoiDung.PhienBanLapLich, PhienTuHocResponse.Tu(row));
    }

    public async Task<HocTapResponse<PhienTuHocResponse>> SuaAsync(int chu, int id, long expected, PhienTuHocRequest request, CancellationToken ct)
    {
        KiemTraGhiChu(request);
        await using var session = await dieuPhoi.MoAsync(chu, expected, true, ct);
        var row = await TimAsync(chu, id, ct);
        var now = clock.GetUtcNow();
        if (request.MaNhiemVu != row.MaNhiemVu)
            throw new HocTapException(409, "SESSION_TASK_IMMUTABLE", "Không đổi nhiệm vụ của phiên đã tạo; hãy hủy rồi đặt phiên mới.");
        var note = TuyChon(request.GhiChu);
        var changed = row.BatDauDuKien != request.BatDauDuKien || row.KetThucDuKien != request.KetThucDuKien || row.GhiChu != note;
        if (changed)
        {
            if (row.TrangThai != TrangThaiPhienTuHoc.Planned || row.BatDauDuKien <= now)
                throw new HocTapException(409, "SESSION_NOT_EDITABLE", "Chỉ dời/sửa phiên Planned chưa bắt đầu.");
            await datPhien.KiemTraAsync(chu, [new(row.MaNhiemVu, request.BatDauDuKien, request.KetThucDuKien)], now, row.Ma, ct);
            row.BatDauDuKien = request.BatDauDuKien.ToUniversalTime(); row.KetThucDuKien = request.KetThucDuKien.ToUniversalTime();
            row.GhiChu = note; row.NgayCapNhat = now;
        }
        await session.CommitAsync(changed, ct);
        return new(session.NguoiDung.PhienBanLapLich, PhienTuHocResponse.Tu(row));
    }

    public async Task<HocTapResponse<PhienTuHocResponse>> DoiTrangThaiAsync(int chu, int id, long expected, string action, CancellationToken ct)
    {
        await using var session = await dieuPhoi.MoAsync(chu, expected, true, ct);
        var row = await TimAsync(chu, id, ct);
        var now = clock.GetUtcNow();
        var target = action switch
        {
            "start" => TrangThaiPhienTuHoc.InProgress, "finish" => TrangThaiPhienTuHoc.Completed,
            "miss" => TrangThaiPhienTuHoc.Missed, "cancel" => TrangThaiPhienTuHoc.Cancelled,
            _ => throw new HocTapException(400, "VALIDATION_ERROR", "Thao tác phiên không hợp lệ.")
        };
        var doan = await DocDoanThucHocAsync(chu, id, ct);
        var dangChay = doan.SingleOrDefault(x => x.KetThucThucTe is null);
        var changed = row.TrangThai != target;
        if (changed)
        {
            if (action == "start")
            {
                if (row.TrangThai != TrangThaiPhienTuHoc.Planned || now < row.BatDauDuKien || now >= row.KetThucDuKien)
                    throw TrangThaiSai("Chỉ bắt đầu phiên Planned trong khoảng giờ đã đặt.");
                if (await store.DemAsync(store.TruyVan<PhienTuHoc>().Where(p => p.MaNguoiDung == chu && p.Ma != id && p.TrangThai == TrangThaiPhienTuHoc.InProgress), ct) > 0)
                    throw TrangThaiSai("Đang có phiên khác chưa kết thúc.");
                if (await store.DemAsync(store.TruyVan<NhiemVu>().Where(t => t.MaNguoiDung == chu && t.Ma == row.MaNhiemVu && t.TrangThai == TrangThaiNhiemVu.Completed), ct) > 0)
                    throw TrangThaiSai("Nhiệm vụ đã hoàn thành.");
                if (await store.DemAsync(store.TruyVan<DoanThucHoc>().Where(x => x.MaNguoiDung == chu && x.KetThucThucTe == null), ct) > 0)
                    throw TrangThaiSai("Cần kết thúc đồng hồ của phiên đang chạy trước.");
                var moi = new DoanThucHoc { MaNguoiDung = chu, MaNhiemVu = row.MaNhiemVu, MaPhienTuHoc = id, BatDauThucTe = now };
                store.Them(moi);
                doan.Add(moi);
            }
            else if (action == "finish")
            {
                if (row.TrangThai != TrangThaiPhienTuHoc.InProgress && !(row.TrangThai == TrangThaiPhienTuHoc.Planned && now >= row.KetThucDuKien))
                    throw TrangThaiSai("Chỉ kết thúc phiên đang học hoặc xác nhận phiên Planned đã qua giờ.");
                if (dangChay is not null)
                {
                    // PostgreSQL lưu timestamp theo microsecond và yêu cầu kết thúc sau bắt đầu.
                    var ketThucToiThieu = dangChay.BatDauThucTe.AddTicks(10);
                    dangChay.KetThucThucTe = now < ketThucToiThieu ? ketThucToiThieu : now;
                }
            }
            else if (action == "miss")
            {
                if (row.TrangThai != TrangThaiPhienTuHoc.Planned || now < row.KetThucDuKien)
                    throw TrangThaiSai("Chỉ xác nhận bỏ lỡ phiên Planned đã hết giờ.");
            }
            else if (row.TrangThai != TrangThaiPhienTuHoc.Planned || now >= row.BatDauDuKien)
                throw TrangThaiSai("Chỉ hủy phiên Planned chưa bắt đầu.");
            if (dangChay is not null && (action is "miss" or "cancel"))
                throw TrangThaiSai("Cần đóng đoạn thực học trước khi chuyển trạng thái phiên.");
            row.TrangThai = target; row.NgayCapNhat = now;
        }
        await session.CommitAsync(changed, ct);
        return new(session.NguoiDung.PhienBanLapLich, PhienTuHocResponse.Tu(row, doan));
    }

    private Task<List<DoanThucHoc>> DocDoanThucHocAsync(int chu, int id, CancellationToken ct) =>
        store.DanhSachAsync(store.TruyVan<DoanThucHoc>().Where(x => x.MaNguoiDung == chu && x.MaPhienTuHoc == id), ct);

    private async Task<PhienTuHoc> TimAsync(int chu, int id, CancellationToken ct) =>
        await store.DauTienAsync(store.TruyVan<PhienTuHoc>().Where(p => p.Ma == id && p.MaNguoiDung == chu), ct) ?? throw KhongTimThay();
    private static void KiemTraGhiChu(PhienTuHocRequest request)
    {
        if (request.GhiChu?.Length > 2000) throw new HocTapException(400, "VALIDATION_ERROR", "Ghi chú tối đa 2.000 ký tự.");
    }
    private static string? TuyChon(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static HocTapException KhongTimThay() => new(404, "RESOURCE_NOT_FOUND", "Không tìm thấy phiên học.");
    private static HocTapException TrangThaiSai(string message) => new(409, "SESSION_STATE_CONFLICT", message);
}
