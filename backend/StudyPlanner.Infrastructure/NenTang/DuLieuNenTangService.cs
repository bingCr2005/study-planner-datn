using StudyPlanner.Application.Administration;
using StudyPlanner.Application.NenTang;

namespace StudyPlanner.Infrastructure.NenTang;

public sealed class DuLieuNenTangService(DanhMucJsonStore store)
{
    public Task<IReadOnlyList<HocKyMauResponse>> HocKyAsync(CancellationToken ct) => store.DocAsync<IReadOnlyList<HocKyMauResponse>>(data =>
        data.HocKy.OrderBy(x => x.ThuTu).ThenBy(x => x.Ma).Select(x => Semester(data, x)).ToArray(), ct);
    public Task<PhanTrangResponse<MonHocMauResponse>> MonHocAsync(int? hocKy, string? tuKhoa, int trang, CancellationToken ct)
    {
        if (trang < 1 || trang > int.MaxValue / 20 || hocKy is <= 0 || tuKhoa?.Length > 200)
            throw QuyTacDanhMucMau.Invalid("Bộ lọc hoặc phân trang không hợp lệ.");
        return store.DocAsync(data => {
            var keyword = tuKhoa?.Trim() ?? "";
            var terms = data.HocKy.ToDictionary(x => x.Ma);
            var query = data.MonHoc.Where(x => (!hocKy.HasValue || x.MaHocKyMau == hocKy) &&
                (x.KyHieu.Contains(keyword, StringComparison.OrdinalIgnoreCase) || x.Ten.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(x => terms[x.MaHocKyMau].ThuTu).ThenBy(x => x.KyHieu, StringComparer.Ordinal).ThenBy(x => x.Ma).ToArray();
            return new PhanTrangResponse<MonHocMauResponse>(query.Skip((trang - 1) * 20).Take(20).Select(x => Course(data, x)).ToArray(), trang, 20, query.Length);
        }, ct);
    }
    private static HocKyMauResponse Semester(DanhMucJson data, HocKyJson x) => new(x.Ma, x.Ten, x.ThuTu, x.PhienBan, data.MonHoc.Count(m => m.MaHocKyMau == x.Ma), data.MonHoc.Where(m => m.MaHocKyMau == x.Ma).Sum(m => m.SoTinChi));
    private static MonHocMauResponse Course(DanhMucJson data, MonHocJson x) => new(x.Ma, x.MaHocKyMau, data.HocKy.Single(t => t.Ma == x.MaHocKyMau).Ten, x.KyHieu, x.Ten, x.SoTinChi, x.PhienBan);
    private static QuanTriException Conflict(string message) => new(LoaiLoiNghiepVu.Conflict, "CATALOG_CONFLICT", message);
    private static QuanTriException Missing() => new(LoaiLoiNghiepVu.NotFound, "CATALOG_NOT_FOUND", "Mẫu không còn tồn tại. Hãy tải lại danh mục.");
    private static void Version(int actual, int? expected)
    {
        if (expected is null or < 1) throw QuyTacDanhMucMau.Invalid("Thiếu phiên bản mẫu đang chỉnh sửa.");
        if (actual != expected) throw Conflict("Mẫu đã được thay đổi bởi người khác. Hãy tải lại trước khi chỉnh sửa.");
    }
    public Task<HocKyMauResponse> LuuHocKyAsync(int? id, HocKyMauRequest request, CancellationToken ct)
    {
        QuyTacDanhMucMau.HocKy(request);
        return store.GhiAsync(data => {
            var row = id.HasValue ? data.HocKy.SingleOrDefault(x => x.Ma == id) ?? throw Missing() : null;
            if (row is not null) Version(row.PhienBan, request.PhienBan);
            var name = QuyTacDanhMucMau.Ten(request.Ten);
            if (data.HocKy.Any(x => x.Ma != id && string.Equals(x.Ten, name, StringComparison.OrdinalIgnoreCase))) throw Conflict("Tên học kỳ mẫu đã tồn tại.");
            var saved = new HocKyJson(row?.Ma ?? data.MaHocKyTiepTheo, name, request.ThuTu, row is null ? 1 : checked(row.PhienBan + 1));
            if (row is null) data.MaHocKyTiepTheo = checked(data.MaHocKyTiepTheo + 1); else data.HocKy.Remove(row);
            data.HocKy.Add(saved); return Semester(data, saved);
        }, ct);
    }
    public Task<MonHocMauResponse> LuuMonHocAsync(int? id, MonHocMauRequest request, CancellationToken ct)
    {
        QuyTacDanhMucMau.MonHoc(request);
        return store.GhiAsync(data => {
            var row = id.HasValue ? data.MonHoc.SingleOrDefault(x => x.Ma == id) ?? throw Missing() : null;
            if (row is not null) Version(row.PhienBan, request.PhienBan);
            if (!data.HocKy.Any(x => x.Ma == request.MaHocKyMau)) throw Missing();
            var code = QuyTacDanhMucMau.KyHieu(request.KyHieu);
            if (data.MonHoc.Any(x => x.Ma != id && x.MaHocKyMau == request.MaHocKyMau && x.KyHieu == code)) throw Conflict("Mã môn đã tồn tại trong học kỳ mẫu này.");
            var saved = new MonHocJson(row?.Ma ?? data.MaMonTiepTheo, request.MaHocKyMau, code, QuyTacDanhMucMau.Ten(request.Ten), request.SoTinChi, row is null ? 1 : checked(row.PhienBan + 1));
            if (row is null) data.MaMonTiepTheo = checked(data.MaMonTiepTheo + 1); else data.MonHoc.Remove(row);
            data.MonHoc.Add(saved); return Course(data, saved);
        }, ct);
    }
    public async Task XoaAsync(bool hocKy, int id, int? phienBan, CancellationToken ct)
    {
        await store.GhiAsync(data => {
            if (hocKy)
            {
                var row = data.HocKy.SingleOrDefault(x => x.Ma == id) ?? throw Missing(); Version(row.PhienBan, phienBan);
                if (data.MonHoc.Any(x => x.MaHocKyMau == id)) throw Conflict("Học kỳ mẫu còn môn học. Hãy chuyển hoặc xóa các môn trước.");
                data.HocKy.Remove(row);
            }
            else { var row = data.MonHoc.SingleOrDefault(x => x.Ma == id) ?? throw Missing(); Version(row.PhienBan, phienBan); data.MonHoc.Remove(row); }
            return true;
        }, ct);
    }
}
