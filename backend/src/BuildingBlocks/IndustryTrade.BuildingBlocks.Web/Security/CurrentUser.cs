using System.Security.Claims;
using IndustryTrade.BuildingBlocks.Application.Security;
using Microsoft.AspNetCore.Http;

namespace IndustryTrade.BuildingBlocks.Web.Security;

/// <summary>
/// Resolves <see cref="ICurrentUser"/> from the JWT claims placed on HttpContext by Keycloak.
/// Claim contract (configured via Keycloak protocol mappers, see deploy/keycloak):
///   • "permission" (multivalued) → function-scope permission codes
///   • "scope_path"  (multivalued) → data-scope org-unit path prefixes
///   • the permission "super-admin" grants unrestricted access.
/// </summary>
public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public const string PermissionClaim = "permission";
    public const string ScopePathClaim = "scope_path";
    public const string SuperAdminPermission = "super-admin";

    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
    public string? UserId => Principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Principal?.FindFirstValue("sub");
    public string? UserName => Principal?.FindFirstValue("preferred_username");

    public bool IsSuperAdmin => Permissions.Contains(SuperAdminPermission);

    public IReadOnlySet<string> Permissions =>
        Principal?.FindAll(PermissionClaim).Select(c => c.Value).ToHashSet() ?? new HashSet<string>();

    public IReadOnlyCollection<string> DataScopePaths =>
        Principal?.FindAll(ScopePathClaim).Select(c => c.Value).ToArray() ?? [];

    public bool HasPermission(string permission) =>
        IsSuperAdmin || Permissions.Contains(permission);
}
