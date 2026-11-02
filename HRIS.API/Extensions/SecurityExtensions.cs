
using System.Text;
using HRIS.API.Common.Security;
using HRIS.API.Entities;
using HRIS.API.Features.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HRIS.API.Extensions;

public static class SecurityExtensions
{
    /// <summary>
    /// Modules 10-11: password hashing, JWT Bearer authentication,
    /// roles, policies, fallback policy, and current user.
    /// </summary>
    public static IServiceCollection AddHrisSecurity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Password hashing dan credential validation
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IUserCredentialService, UserCredentialService>();

        // JWT Options: membaca konfigurasi dari appsettings.json dan User Secrets
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(
                options => Encoding.UTF8.GetByteCount(options.Key) >= 32,
                "Jwt:Key must be at least 32 bytes (HS256). Set it with User Secrets.")
            .ValidateOnStart();

        // JWT Token Service
        services.AddSingleton<ITokenService, JwtTokenService>();

        // Authentication: JWT Bearer
        services.AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        // JWT Token Validation
        services.AddOptions<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>(
                (bearer, jwtOptions) =>
                {
                    var jwt = jwtOptions.Value;

                    bearer.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = jwt.Issuer,

                            ValidateAudience = true,
                            ValidAudience = jwt.Audience,

                            ValidateLifetime = true,

                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(jwt.Key)),

                            ClockSkew = TimeSpan.FromMinutes(1)
                        };
                });

        // Authorization Policies
        services.AddAuthorizationBuilder()
            .AddPolicy(
                Policies.HrOnly,
                policy => policy.RequireRole(
                    Roles.Admin,
                    Roles.Hr))
            .AddPolicy(
                Policies.HasEmployeeProfile,
                policy => policy.RequireClaim(
                    HrisClaimTypes.EmployeeId))
            .SetFallbackPolicy(
                new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build());

        // Current User
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }
}
