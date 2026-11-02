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
        this IServiceCollection services)
    {
        // Password hashing dan credential validation
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IUserCredentialService, UserCredentialService>();

        // Basic Authentication
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

        return services;
    }
}