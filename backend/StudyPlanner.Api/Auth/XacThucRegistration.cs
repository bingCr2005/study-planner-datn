using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudyPlanner.Application.Auth;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Infrastructure.Data;

namespace StudyPlanner.Api.Auth;

public sealed class JwtTokenIssuer(byte[] key, string issuer, string audience, int minutes) : IAccessTokenIssuer
{
    public string Stamp(NguoiDung user) => Convert.ToBase64String(HMACSHA256.HashData(key,
        Encoding.UTF8.GetBytes($"{user.Ma}:{user.MatKhauBam}")));

    public DangNhapResponse Issue(NguoiDung user)
    {
        var now = DateTimeOffset.UtcNow;
        var expires = now.AddMinutes(minutes);
        var jwt = new JwtSecurityToken(issuer, audience,
            [new Claim("sub", user.Ma.ToString()), new Claim("role", user.VaiTro.ToString()),
             new Claim("pwd", Stamp(user)), new Claim("jti", Guid.NewGuid().ToString("N"))],
            now.UtcDateTime, expires.UtcDateTime,
            new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256));
        return new(new JwtSecurityTokenHandler().WriteToken(jwt), "Bearer", expires,
            new(user.Ma, user.Email, user.HoTen, user.MaSoSinhVien, user.TenTruong, user.TenLop, user.NganhHoc,
                user.VaiTro, user.DangHoatDong, user.MuiGio, user.NhacTruocGio, user.NgayTao, user.NgayCapNhat));
    }
}

public static class XacThucRegistration
{
    public static IServiceCollection AddStudyPlannerXacThuc(this IServiceCollection services,
        IConfiguration config, IHostEnvironment environment)
    {
        // Không có khóa cố định trong source. Dev không cấu hình sẽ dùng khóa ngẫu nhiên theo lần chạy.
        var secret = config["Auth:SigningKey"];
        if (string.IsNullOrEmpty(secret) && !environment.IsDevelopment())
            throw new InvalidOperationException("Cần cấu hình Auth:SigningKey ngoài source.");
        var key = string.IsNullOrEmpty(secret) ? RandomNumberGenerator.GetBytes(64) : Encoding.UTF8.GetBytes(secret);
        if (key.Length < 32) throw new InvalidOperationException("Auth:SigningKey cần ít nhất 32 byte.");
        var issuer = config["Auth:Issuer"] ?? "StudyPlanner";
        var audience = config["Auth:Audience"] ?? "StudyPlanner.Client";
        var minutes = config.GetValue("Auth:AccessTokenMinutes", 30);
        if (minutes is < 1 or > 60) throw new InvalidOperationException("Auth:AccessTokenMinutes từ 1 đến 60.");
        var tokens = new JwtTokenIssuer(key, issuer, audience, minutes);
        services.AddSingleton(tokens);
        services.AddSingleton<IAccessTokenIssuer>(tokens);
        services.AddScoped<XacThucService>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new()
            {
                ValidateIssuer = true, ValidIssuer = issuer, ValidateAudience = true, ValidAudience = audience,
                ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = true, ClockSkew = TimeSpan.Zero, ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                NameClaimType = "sub", RoleClaimType = "role"
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    if (!int.TryParse(context.Principal?.FindFirstValue("sub"), out var id))
                    { context.Fail("Invalid account"); return; }
                    var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
                    var user = await db.DanhSachNguoiDung.AsNoTracking().SingleOrDefaultAsync(x => x.Ma == id, context.HttpContext.RequestAborted);
                    if (user is null || !user.DangHoatDong || context.Principal?.FindFirstValue("pwd") != tokens.Stamp(user))
                    { context.Fail("Invalid account"); return; }
                    // Luôn dùng role hiện tại, không tin quyền cũ trong token.
                    var identity = (ClaimsIdentity)context.Principal!.Identity!;
                    foreach (var claim in identity.FindAll("role").ToArray()) identity.RemoveClaim(claim);
                    identity.AddClaim(new Claim("role", user.VaiTro.ToString()));
                },
                OnChallenge = context =>
                {
                    context.HandleResponse(); context.Response.Headers.WWWAuthenticate = "Bearer";
                    return WriteProblem(context.HttpContext, 401, "UNAUTHORIZED", "Cần đăng nhập hoặc phiên đã hết hiệu lực.");
                },
                OnForbidden = context => WriteProblem(context.HttpContext, 403, "FORBIDDEN", "Bạn không có quyền thực hiện thao tác.")
            };
        });
        services.AddAuthorization(options => options.AddPolicy("Admin", policy => policy.RequireAuthenticatedUser().RequireRole("Admin")));
        services.AddRateLimiter(options =>
        {
            options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
            options.OnRejected = (context, _) => new ValueTask(WriteProblem(context.HttpContext, 429, "RATE_LIMITED", "Thử lại sau một phút."));
        });
        return services;
    }

    public static Task WriteProblem(HttpContext context, int status, string code, string message)
    {
        context.Response.StatusCode = status;
        var problem = new ProblemDetails { Status = status, Title = message, Instance = context.Request.Path };
        problem.Extensions["code"] = code; problem.Extensions["traceId"] = context.TraceIdentifier;
        return context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
    }
}
