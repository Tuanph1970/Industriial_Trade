namespace IndustryTrade.BuildingBlocks.Application.Security;

/// <summary>
/// The authenticated principal, resolved from the request (Keycloak JWT). Handlers depend on this
/// abstraction — not on HttpContext — so they stay web-agnostic and testable.
/// Carries both authorization dimensions (docs/design/02 §5):
///   • <see cref="Permissions"/> = function-scope, • <see cref="DataScopePaths"/> = data-scope.
/// </summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    string? UserId { get; }
    string? UserName { get; }

    /// <summary>True for the super administrator — bypasses function- and data-scope checks.</summary>
    bool IsSuperAdmin { get; }

    /// <summary>Function-scope: permission codes granted to the principal.</summary>
    IReadOnlySet<string> Permissions { get; }

    /// <summary>Data-scope: accessible org-unit path prefixes (a unit and all its descendants).</summary>
    IReadOnlyCollection<string> DataScopePaths { get; }

    /// <summary>Data-scope as concrete org-unit ids (the user's unit + descendants), for contexts
    /// that reference units by id rather than by path (e.g. Sector Data).</summary>
    IReadOnlyCollection<Guid> DataScopeUnitIds { get; }

    bool HasPermission(string permission);
}
