
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using HRIS.API.Common.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace HRIS.API.Features.Auth;

public class BasicAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IUserCredentialService credentialService)
    : AuthenticationHandler<AuthenticationSchemeOptions>(
        options, logger, encoder)
{
    public const string SchemeName = "Basic";

    protected override async Task<AuthenticateResult>
        HandleAuthenticateAsync()
    {
        // 1. Periksa Authorization header
        if (!Request.Headers.TryGetValue(
            "Authorization", out var headerValue))
        {
            return AuthenticateResult.NoResult();
        }

        if (!AuthenticationHeaderValue.TryParse(
                headerValue.ToString(), out var header) ||
            !SchemeName.Equals(
                header.Scheme,
                StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrEmpty(header.Parameter))
        {
            return AuthenticateResult.NoResult();
        }

        // 2. Decode Base64 email:password
        string email;
        string password;

        try
        {
            var raw = Encoding.UTF8.GetString(
                Convert.FromBase64String(header.Parameter));

            var separator = raw.IndexOf(':');

            if (separator < 0)
            {
                return AuthenticateResult.Fail(
                    "Invalid Basic credentials format.");
            }

            email = raw[..separator];
            password = raw[(separator + 1)..];
        }
        catch (FormatException)
        {
            return AuthenticateResult.Fail(
                "Invalid Base64 in the Authorization header.");
        }

        // 3. Validasi credential melalui database service
        var user = await credentialService.ValidateAsync(
            email,
            password,
            Context.RequestAborted);

        if (user is null)
        {
            return AuthenticateResult.Fail(
                "Invalid email or password.");
        }

        // 4. Buat claims
        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new(
                ClaimTypes.Email,
                user.Email),

            new(
                ClaimTypes.Name,
                user.DisplayName)
        };

        if (user.EmployeeId is { } employeeId)
        {
            claims.Add(
                new Claim(
                    HrisClaimTypes.EmployeeId,
                    employeeId.ToString()));
        }

        claims.AddRange(
            user.Roles.Select(role =>
                new Claim(ClaimTypes.Role, role)));

        // 5. Buat ClaimsPrincipal
        var identity = new ClaimsIdentity(
            claims,
            Scheme.Name);

        var principal = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(
            principal,
            Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    // 6. Response ketika authentication gagal
    protected override Task HandleChallengeAsync(
        AuthenticationProperties properties)
    {
        Response.Headers.WWWAuthenticate =
            $"{SchemeName} realm=\"HRIS.API\", charset=\"UTF-8\"";

        return base.HandleChallengeAsync(properties);
    }
}
