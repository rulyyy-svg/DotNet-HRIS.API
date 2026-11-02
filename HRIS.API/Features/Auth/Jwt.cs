
using System.Security.Claims;
using System.Text;
using HRIS.API.Common.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace HRIS.API.Features.Auth;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public string Key { get; set; } = "";
    public int ExpiresMinutes { get; set; } = 60;
}

public record LoginResponse(
    string AccessToken,
    DateTime ExpiresAt,
    string TokenType = "Bearer");

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(
        AuthenticatedUser user);
}

public class JwtTokenService(
    IOptions<JwtOptions> options,
    TimeProvider time) : ITokenService
{
    public (string Token, DateTime ExpiresAt) CreateToken(
        AuthenticatedUser user)
    {
        var jwt = options.Value;

        var now = time.GetUtcNow().UtcDateTime;
        var expiresAt = now.AddMinutes(jwt.ExpiresMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.DisplayName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
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
                new Claim("role", role)));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            NotBefore = now,
            IssuedAt = now,
            Expires = expiresAt,
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwt.Key)),
                SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler()
            .CreateToken(descriptor);

        return (token, expiresAt);
    }
}
