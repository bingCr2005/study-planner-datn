using System.Text.RegularExpressions;
using StudyPlanner.Application.Administration;

namespace StudyPlanner.Application.NenTang;

public sealed record HocKyMauRequest(string Ten, int ThuTu, int? PhienBan);
public sealed record MonHocMauRequest(int MaHocKyMau, string KyHieu, string Ten, int SoTinChi, int? PhienBan);
public sealed record HocKyMauResponse(int Ma, string Ten, int ThuTu, int PhienBan, int SoMonHoc, int TongTinChi);
public sealed record MonHocMauResponse(int Ma, int MaHocKyMau, string TenHocKy, string KyHieu, string Ten, int SoTinChi, int PhienBan);

public static class QuyTacDanhMucMau
{
    public static string Ten(string? value)
    {
        var name = value?.Trim() ?? "";
        if (name.Length is < 1 or > 200) throw Invalid("Tên phải có từ 1 đến 200 ký tự.");
        return name;
    }
    public static string KyHieu(string? value)
    {
        var code = value?.Trim().ToUpperInvariant() ?? "";
        if (!Regex.IsMatch(code, @"\A[A-Z0-9][A-Z0-9._-]{0,29}\z"))
            throw Invalid("Mã môn gồm 1–30 chữ cái không dấu, số, dấu chấm, gạch ngang hoặc gạch dưới.");
        return code;
    }
    public static void HocKy(HocKyMauRequest request)
    {
        Ten(request.Ten);
        if (request.ThuTu is < 1 or > 100) throw Invalid("Thứ tự học kỳ phải từ 1 đến 100.");
    }
    public static void MonHoc(MonHocMauRequest request)
    {
        Ten(request.Ten); KyHieu(request.KyHieu);
        if (request.MaHocKyMau < 1) throw Invalid("Chọn học kỳ mẫu hợp lệ.");
        if (request.SoTinChi is < 1 or > 30) throw Invalid("Số tín chỉ phải từ 1 đến 30.");
    }
    public static QuanTriException Invalid(string message) => new(LoaiLoiNghiepVu.InvalidInput, "CATALOG_INVALID", message);
}
