using IndustryTrade.BuildingBlocks.Application.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace IndustryTrade.BuildingBlocks.Web.Security;

public static class SecurityExtensions
{
    /// <summary>
    /// Wires Keycloak (OIDC) bearer authentication and the current-user resolver.
    /// Function-scope is enforced by the AuthorizationBehavior; endpoints only need to require
    /// an authenticated user. Configuration: <c>Keycloak:Authority</c>, <c>Keycloak:Audience</c>.
    /// </summary>
    public static IServiceCollection AddKeycloakAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var authority = configuration["Keycloak:Authority"];
        var audience = configuration["Keycloak:Audience"];
        var requireHttps = configuration.GetValue("Keycloak:RequireHttpsMetadata", false);

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = audience;
                options.RequireHttpsMetadata = requireHttps;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "preferred_username",
                    RoleClaimType = CurrentUser.PermissionClaim,
                    ValidateAudience = !string.IsNullOrWhiteSpace(audience)
                };
            });

        services.AddAuthorization();
        return services;
    }
}
