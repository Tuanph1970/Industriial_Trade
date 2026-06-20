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
        // Optional: fetch OIDC metadata from a different (internal) URL than the public issuer, so the
        // API can reach Keycloak over the container network while tokens still carry the public issuer.
        var metadataAddress = configuration["Keycloak:MetadataAddress"];

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = audience;
                options.RequireHttpsMetadata = requireHttps;
                if (!string.IsNullOrWhiteSpace(metadataAddress))
                    options.MetadataAddress = metadataAddress;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "preferred_username",
                    RoleClaimType = CurrentUser.PermissionClaim,
                    ValidateAudience = !string.IsNullOrWhiteSpace(audience),
                    // Tokens are issued by the public Keycloak URL even when metadata is fetched internally.
                    ValidIssuer = authority,
                    ValidateIssuer = !string.IsNullOrWhiteSpace(authority)
                };
            });

        services.AddAuthorization();
        return services;
    }
}
