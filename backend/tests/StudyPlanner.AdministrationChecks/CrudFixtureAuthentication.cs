using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

// Chỉ host kiểm thử CRUD dùng actor giả; AuthChecks kiểm thử JWT thật riêng.
public sealed class CrudFixtureAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger, UrlEncoder encoder, IHostEnvironment environment)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync() => Task.FromResult(
        environment.IsDevelopment()
            ? AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(
                new ClaimsIdentity([new Claim(ClaimTypes.Role, "Admin")], Scheme.Name)), Scheme.Name))
            : AuthenticateResult.NoResult());
}
