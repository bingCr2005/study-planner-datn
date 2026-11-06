using System.ComponentModel.DataAnnotations;
using StudyPlanner.Application.Administration;

namespace StudyPlanner.Application.Common;

// Quy tắc tài nguyên dùng chung cho tài khoản và các use case khác;
// không phụ thuộc danh mục trường/khoa/khóa/lớp.
public sealed class QuyTacTaiNguyen(IQuanLyNguoiDungStore store)
{
    public async Task<T> GetAsync<T>(int id, CancellationToken ct) where T : class
    {
        if (id <= 0)
            throw new QuanTriException(LoaiLoiNghiepVu.InvalidInput, "VALIDATION_ERROR", "ID phải lớn hơn 0.");
        return await store.FindAsync<T>(id, ct) ?? throw new QuanTriException(LoaiLoiNghiepVu.NotFound,
            "RESOURCE_NOT_FOUND", "Không tìm thấy " + typeof(T).Name + ".");
    }

    public static void Validate(object request)
    {
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, new ValidationContext(request), results, true))
            throw new QuanTriException(LoaiLoiNghiepVu.InvalidInput, "VALIDATION_ERROR",
                string.Join(" ", results.Select(x => x.ErrorMessage)));
    }

    public static QuanTriException Conflict(string code, string message) =>
        new(LoaiLoiNghiepVu.Conflict, code, message);

    public static PhanTrangResponse<TResult> Map<T, TResult>(PhanTrangResponse<T> page, Func<T, TResult> map) =>
        new(page.DanhSach.Select(map).ToArray(), page.Trang, page.KichThuocTrang, page.TongSo);
}
