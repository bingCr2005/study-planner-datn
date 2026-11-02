using Microsoft.AspNetCore.Identity;
using StudyPlanner.Application.Administration;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Administration;

public sealed class IdentityUserPasswordHasher : IUserPasswordHasher
{
    private readonly PasswordHasher<User> hasher = new();
    public string Hash(User user, string password) => hasher.HashPassword(user, password);
    public bool Verify(User user, string password) =>
        hasher.VerifyHashedPassword(user, user.MatKhauBam, password) != PasswordVerificationResult.Failed;
}
