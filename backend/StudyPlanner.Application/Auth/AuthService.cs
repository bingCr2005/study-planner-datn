using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using StudyPlanner.Application.Administration;
using StudyPlanner.Application.Common;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.Auth;

public sealed class RegisterRequest
{
    [Required, StringLength(320), EmailAddress] public string Email { get; init; } = "";
    [Required, StringLength(200)] public string HoTen { get; init; } = "";
    [Required, StringLength(128, MinimumLength = 8)] public string MatKhau { get; init; } = "";
    // Bắt buộc ở luồng sinh viên tự đăng ký; không ép hồ sơ tài khoản cũ/Admin thành NOT NULL.
    [Required(ErrorMessage = "Mã sinh viên là bắt buộc."), StringLength(50)] public string MaSoSinhVien { get; init; } = "";
    [Required(ErrorMessage = "Tên trường là bắt buộc."), StringLength(200)] public string TenTruong { get; init; } = "";
    [Required(ErrorMessage = "Tên lớp là bắt buộc."), StringLength(200)] public string TenLop { get; init; } = "";
    [Required(ErrorMessage = "Ngành học là bắt buộc."), StringLength(200)] public string NganhHoc { get; init; } = "";
}
public sealed class LoginRequest
{
    // Chuỗi để giữ số 0 đầu mã sinh viên. Không gắn EmailAddress vì còn nhận mã sinh viên.
    [Required(ErrorMessage = "Nhập email hoặc mã sinh viên."), StringLength(320)] public string TaiKhoan { get; init; } = "";
    [Required, StringLength(128)] public string MatKhau { get; init; } = "";
}
public sealed class ChangePasswordRequest
{
    [Required, StringLength(128)] public string MatKhauHienTai { get; init; } = "";
    [Required, StringLength(128, MinimumLength = 8)] public string MatKhauMoi { get; init; } = "";
}
public sealed record LoginResponse(string TokenTruyCap, string LoaiToken, DateTimeOffset HetHanLuc, UserResponse NguoiDung);
public sealed record ChangePasswordResponse(string ThongBao);
public interface IAccessTokenIssuer { LoginResponse Issue(User user); }
public sealed class AuthenticationFailureException : Exception;

public sealed class AuthService(IAdministrationStore store, IUserPasswordHasher hasher,
    UserAdministrationService users, IAccessTokenIssuer tokens, TimeProvider clock)
{
    // Tài khoản không tồn tại vẫn xác minh hash để giảm khác biệt thời gian đăng nhập.
    private static string? dummyHash;

    public Task<UserResponse> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        // Kiểm tra cả tại Application khi được gọi ngoài HTTP; tránh mất validation khi ánh xạ DTO.
        ResourceRules.Validate(request);
        return users.CreateAsync(new CreateUserRequest { Email = request.Email, HoTen = request.HoTen,
            MatKhau = request.MatKhau, MaSoSinhVien = request.MaSoSinhVien, TenTruong = request.TenTruong,
            TenLop = request.TenLop, NganhHoc = request.NganhHoc, DangHoatDong = true }, ct);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        ResourceRules.Validate(request);
        var identifier = request.TaiKhoan.Trim();
        var normalizedEmail = identifier.ToUpperInvariant();
        var matches = await store.LoadAsync<User>(x => x.EmailChuanHoa == normalizedEmail ||
            x.MaSoSinhVien == identifier, ct);
        // Email không phân biệt hoa/thường; MSSV so đúng chuỗi đã trim theo unique hiện tại.
        // Nếu dữ liệu cũ có MSSV trùng email của tài khoản khác, từ chối thay vì chọn nhầm danh tính.
        var user = matches.Count == 1 ? matches[0] : null;
        // Dummy được cấp hash hợp lệ theo cùng thư viện, không bao giờ lưu DB.
        var candidate = user ?? new User { MatKhauBam = LazyInitializer.EnsureInitialized(ref dummyHash,
            () => hasher.Hash(new User(), Guid.NewGuid().ToString("N"))) };
        var valid = hasher.Verify(candidate, request.MatKhau);
        if (!valid || user is null || !user.DangHoatDong) throw new AuthenticationFailureException();
        return tokens.Issue(user);
    }

    public Task<UserResponse> MeAsync(int id, CancellationToken ct) => users.GetAsync(id, ct);

    public async Task<UserResponse> UpdateAsync(int id, JsonElement patch, CancellationToken ct)
    {
        if (patch.ValueKind != JsonValueKind.Object) throw InvalidProfile();
        await using var transaction = await store.BeginTransactionAsync(ct);
        var user = await store.FindAsync<User>(id, ct);
        if (user is null || !user.DangHoatDong) throw new AuthenticationFailureException();
        var fullName = user.HoTen; var studentCode = user.MaSoSinhVien; var major = user.NganhHoc;
        var schoolName = user.TenTruong; var className = user.TenLop;
        foreach (var field in patch.EnumerateObject())
        {
            if (field.Value.ValueKind is not (JsonValueKind.String or JsonValueKind.Null)) throw InvalidProfile();
            var value = field.Value.GetString()?.Trim();
            switch (field.Name)
            {
                case "hoTen": fullName = value ?? ""; break;
                case "maSoSinhVien": studentCode = string.IsNullOrWhiteSpace(value) ? null : value; break;
                case "tenTruong": schoolName = string.IsNullOrWhiteSpace(value) ? null : value; break;
                case "tenLop": className = string.IsNullOrWhiteSpace(value) ? null : value; break;
                case "nganhHoc": major = string.IsNullOrWhiteSpace(value) ? null : value; break;
                default: throw InvalidProfile();
            }
        }
        ResourceRules.Validate(new UserRequest { Email = user.Email, HoTen = fullName,
            MaSoSinhVien = studentCode, TenTruong = schoolName, TenLop = className,
            NganhHoc = major, DangHoatDong = user.DangHoatDong });
        if (studentCode != null && await store.AnyAsync<User>(x => x.Ma != id && x.MaSoSinhVien == studentCode, ct))
            throw ResourceRules.Conflict("STUDENT_CODE_ALREADY_USED", "Mã sinh viên đã được sử dụng.");
        user.HoTen = fullName; user.MaSoSinhVien = studentCode; user.NganhHoc = major;
        user.TenTruong = schoolName; user.TenLop = className;
        user.NgayCapNhat = clock.GetUtcNow(); await store.SaveAsync(ct); await transaction.CommitAsync(ct);
        return await users.GetAsync(id, ct);
    }
    private static AdministrationException InvalidProfile() => new(FailureKind.InvalidInput, "VALIDATION_ERROR",
        "Chỉ sửa hoTen, maSoSinhVien, tenTruong, tenLop và nganhHoc với giá trị hợp lệ.");

    public async Task ChangePasswordAsync(int id, ChangePasswordRequest request, CancellationToken ct)
    {
        await using var transaction = await store.BeginTransactionAsync(ct);
        var user = await store.FindAsync<User>(id, ct);
        if (user is null || !user.DangHoatDong || !hasher.Verify(user, request.MatKhauHienTai))
            throw new AuthenticationFailureException();
        user.MatKhauBam = hasher.Hash(user, request.MatKhauMoi);
        user.NgayCapNhat = clock.GetUtcNow();
        await store.SaveAsync(ct); await transaction.CommitAsync(ct);
    }
}
