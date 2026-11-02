
using System.Text;
using HRIS.API.Common.Security;
using HRIS.API.Entities;
using HRIS.API.Features.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace HRIS.API.Extensions;

public static class SecurityExtensions
{
    public static IServiceCollection AddHrisSecurity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Password hashing dan credential validation
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IUserCredentialService, UserCredentialService>();

        // Basic Authentication tetap dipertahankan
        services.AddAuthentication(BasicAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions,
                BasicAuthenticationHandler>(
                BasicAuthenticationHandler.SchemeName,
                options => { });

        // Authorization Policies
        services.AddAuthorizationBuilder()
            .AddPolicy(
                Policies.HrOnly,
                policy => policy.RequireRole(Roles.Admin, Roles.Hr))
            .AddPolicy(
                Policies.HasEmployeeProfile,
                policy => policy.RequireClaim(HrisClaimTypes.EmployeeId))
            .SetFallbackPolicy(
                new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build());

        // Current User
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        // JWT Options
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(
                options => Encoding.UTF8.GetByteCount(options.Key) >= 32,
                "Jwt:Key must be at least 32 bytes (HS256). Set it with User Secrets.")
            .ValidateOnStart();

        // JWT Token Service
        services.AddSingleton<ITokenService, JwtTokenService>();

        return services;
    }
}
