using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using StudyPlanner.Application.Administration;
using StudyPlanner.Application.Common;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.Auth;


public sealed class LoginRequest
{
    [Required(ErrorMessage = "Nhập email."), StringLength(320)] public string Email { get; init; } = "";
    [Required, StringLength(128)] public string MatKhau { get; init; } = "";
}

public sealed record LoginResponse(string TokenTruyCap, string LoaiToken, DateTimeOffset HetHanLuc, UserResponse NguoiDung);
public interface IAccessTokenIssuer { LoginResponse Issue(User user); }
public sealed class AuthenticationFailureException : Exception;

public sealed class AuthService(IAdministrationStore store, IUserPasswordHasher hasher,
    UserAdministrationService users, IAccessTokenIssuer tokens)
{
    // Tài khoản không tồn tại vẫn xác minh hash để giảm khác biệt thời gian đăng nhập.
    private static string? dummyHash;



    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        ResourceRules.Validate(request);
        var identifier = request.Email.Trim();
        var normalizedEmail = identifier.ToUpperInvariant();
        var matches = await store.LoadAsync<User>(x => x.EmailChuanHoa == normalizedEmail, ct);
        var user = matches.Count == 1 ? matches[0] : null;
        // Dummy được cấp hash hợp lệ theo cùng thư viện, không bao giờ lưu DB.
        var candidate = user ?? new User { MatKhauBam = LazyInitializer.EnsureInitialized(ref dummyHash,
            () => hasher.Hash(new User(), Guid.NewGuid().ToString("N"))) };
        var valid = hasher.Verify(candidate, request.MatKhau);
        if (!valid || user is null || !user.DangHoatDong) throw new AuthenticationFailureException();
        return tokens.Issue(user);
    }

    public Task<UserResponse> MeAsync(int id, CancellationToken ct) => users.GetAsync(id, ct);

}
