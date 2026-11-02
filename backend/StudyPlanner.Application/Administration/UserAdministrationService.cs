using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using StudyPlanner.Application.Common;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Application.Administration;

public sealed class UserAdministrationService(IAdministrationStore store, IUserPasswordHasher hasher, TimeProvider clock)
{
    private readonly ResourceRules resources = new(store);

    public async Task<PagedResult<UserResponse>> ListAsync(ListRequest query, CancellationToken ct)
    {
        ResourceRules.Validate(query);
        var q = query.TuKhoa?.Trim().ToUpperInvariant() ?? "";
        var result = await store.ListAsync<User>(x => (!query.DangHoatDong.HasValue || x.DangHoatDong == query.DangHoatDong) &&
            (q == "" || x.HoTen.ToUpper().Contains(q) || x.EmailChuanHoa.Contains(q) ||
             (x.MaSoSinhVien != null && x.MaSoSinhVien.ToUpper().Contains(q))), query.Trang, query.KichThuocTrang, ct);
        return ResourceRules.Map(result, Dto);
    }
    public async Task<UserResponse> GetAsync(int id, CancellationToken ct) => Dto(await resources.GetAsync<User>(id, ct));

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken ct)
    {
        ResourceRules.Validate(request);
        await using var transaction = await store.BeginTransactionAsync(ct);
        var user = new User { NgayTao = clock.GetUtcNow() };
        await ApplyAsync(user, request, ct);
        user.NgayCapNhat = user.NgayTao;
        // Tạo tài khoản không có nghĩa đăng nhập: chỉ lưu hash theo chuẩn Identity để Auth dùng lại sau.
        user.MatKhauBam = hasher.Hash(user, request.MatKhau);
        store.Add(user); await store.SaveAsync(ct); await transaction.CommitAsync(ct);
        return Dto(user);
    }
    public async Task<UserResponse> UpdateAsync(int id, UserRequest request, CancellationToken ct)
    {
        ResourceRules.Validate(request);
        await using var transaction = await store.BeginTransactionAsync(ct);
        var user = await resources.GetAsync<User>(id, ct);
        if (user.DangHoatDong && !request.DangHoatDong) await ProtectLastAdminAsync(user, ct);
        await ApplyAsync(user, request, ct);
        await store.SaveAsync(ct); await transaction.CommitAsync(ct);
        return Dto(user);
    }
    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        await using var transaction = await store.BeginTransactionAsync(ct);
        var user = await resources.GetAsync<User>(id, ct);
        await ProtectLastAdminAsync(user, ct);
        var tasks = await store.LoadAsync<TaskItem>(x => x.MaNguoiDung == id, ct);
        var taskIds = tasks.Select(x => x.Ma).ToArray();
        if (await store.AnyAsync<TaskItem>(x => x.MaNguoiDung != id && x.MaNhiemVuCha.HasValue &&
            taskIds.Contains(x.MaNhiemVuCha.Value), ct))
            throw ResourceRules.Conflict("INCONSISTENT_TASK_OWNER", "Cây nhiệm vụ có chủ sở hữu không nhất quán; không thể xóa tài khoản.");
        var now = clock.GetUtcNow();
        foreach (var task in tasks)
            store.Add(new TaskHistory
            {
                MaNguoiDung = id, MaNhiemVu = task.Ma, TieuDeNhiemVu = task.TieuDe,
                HanhDong = TaskHistoryAction.Deleted, NgayThayDoi = now,
                // Danh sách cho phép, không serialize navigation User hoặc thông tin xác thực.
                DuLieuTruoc = JsonSerializer.Serialize(new { task.Ma, task.MaMonHoc, task.MaHocKy, task.MaNhiemVuCha,
                    task.BatDauSuKien, task.KetThucSuKien,
                    task.TieuDe, task.MoTa, task.Loai, task.HanHoanThanh, task.MucDoUuTien, task.TrangThai,
                    task.ThoiLuongDuKienPhut, task.TienDo, task.NhacTruocGio, task.NgayHoanThanh }, new JsonSerializerOptions(JsonSerializerDefaults.Web))
            });
        // Lưu lịch sử trước cascade. FK SET NULL giữ lịch sử sau khi xóa user.
        await store.SaveAsync(ct);
        store.Remove(user); await store.SaveAsync(ct); await transaction.CommitAsync(ct);
    }

    public async Task SetActiveAsync(int id, UserActiveRequest request, CancellationToken ct)
    {
        ResourceRules.Validate(request);
        await using var transaction = await store.BeginTransactionAsync(ct);
        var user = await resources.GetAsync<User>(id, ct);
        if (user.DangHoatDong && request.DangHoatDong == false) await ProtectLastAdminAsync(user, ct);
        user.DangHoatDong = request.DangHoatDong!.Value; user.NgayCapNhat = clock.GetUtcNow();
        await store.SaveAsync(ct); await transaction.CommitAsync(ct);
    }

    private async Task ApplyAsync(User user, UserRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim();
        var normalized = email.ToUpperInvariant();
        if (!new EmailAddressAttribute().IsValid(email) || normalized.Length > 320)
            throw new AdministrationException(FailureKind.InvalidInput, "VALIDATION_ERROR", "Email không hợp lệ.");
        if (await store.AnyAsync<User>(x => x.Ma != user.Ma && x.EmailChuanHoa == normalized, ct))
            throw ResourceRules.Conflict("EMAIL_ALREADY_USED", "Email đã được sử dụng.");
        var studentCode = Optional(request.MaSoSinhVien);
        if (studentCode != null && await store.AnyAsync<User>(x => x.Ma != user.Ma && x.MaSoSinhVien == studentCode, ct))
            throw ResourceRules.Conflict("STUDENT_CODE_ALREADY_USED", "Mã sinh viên đã được sử dụng.");
        user.Email = email; user.EmailChuanHoa = normalized; user.HoTen = request.HoTen.Trim();
        user.MaSoSinhVien = studentCode; user.TenTruong = Optional(request.TenTruong);
        user.TenLop = Optional(request.TenLop); user.NganhHoc = Optional(request.NganhHoc);
        user.DangHoatDong = request.DangHoatDong; user.NgayCapNhat = clock.GetUtcNow();
    }
    private async Task ProtectLastAdminAsync(User user, CancellationToken ct)
    {
        if (user.VaiTro == UserRole.Admin && user.DangHoatDong &&
            await store.CountAsync<User>(x => x.VaiTro == UserRole.Admin && x.DangHoatDong, ct) <= 1)
            throw ResourceRules.Conflict("LAST_ACTIVE_ADMIN", "Không được khóa hoặc xóa quản trị viên hoạt động cuối cùng.");
    }
    private static string? Optional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static UserResponse Dto(User x) => new(x.Ma, x.Email, x.HoTen, x.MaSoSinhVien, x.TenTruong,
        x.TenLop, x.NganhHoc, x.VaiTro, x.DangHoatDong, x.MuiGio, x.NhacTruocGio, x.NgayTao, x.NgayCapNhat);
}
