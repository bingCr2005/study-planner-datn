using Microsoft.AspNetCore.Identity;
using StudyPlanner.Application.Administration;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Administration;

public sealed class IdentityNguoiDungPasswordHasher : INguoiDungPasswordHasher
{
    private readonly PasswordHasher<NguoiDung> hasher = new();
    public string Hash(NguoiDung user, string password) => hasher.HashPassword(user, password);
    public bool Verify(NguoiDung user, string password) =>
        hasher.VerifyHashedPassword(user, user.MatKhauBam, password) != PasswordVerificationResult.Failed;
}
