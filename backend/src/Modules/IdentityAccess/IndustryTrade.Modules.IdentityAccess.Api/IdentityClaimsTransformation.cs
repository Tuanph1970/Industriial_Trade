using System.Security.Claims;
using IndustryTrade.BuildingBlocks.Web.Security;
using IndustryTrade.Modules.IdentityAccess.Application.Users;
using Microsoft.AspNetCore.Authentication;

namespace IndustryTrade.Modules.IdentityAccess.Api;

/// <summary>
/// After Keycloak authenticates the request, enrich the principal from the IdentityAccess store:
/// the user's role permissions (function-scope) and their org-unit path (data-scope). This makes the
/// database — not static token attributes — the source of truth for authorization (docs/design/03 §2).
/// Idempotent: safe to run multiple times per request.
/// </summary>
internal sealed class IdentityClaimsTransformation(IUserAuthorizationProvider provider) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
            return principal;

        var userName = principal.FindFirst("preferred_username")?.Value;
        if (string.IsNullOrWhiteSpace(userName))
            return principal;

        var auth = await provider.GetByUserNameAsync(userName, CancellationToken.None);
        if (!auth.Found)
            return principal;

        AddMissing(identity, CurrentUser.PermissionClaim, auth.Permissions);
        AddMissing(identity, CurrentUser.ScopePathClaim, auth.DataScopePaths);
        return principal;
    }

    private static void AddMissing(ClaimsIdentity identity, string claimType, IEnumerable<string> values)
    {
        var existing = identity.FindAll(claimType).Select(c => c.Value).ToHashSet();
        foreach (var value in values)
            if (existing.Add(value))
                identity.AddClaim(new Claim(claimType, value));
    }
}
