namespace IndustryTrade.Modules.IdentityAccess.Application.Users;

/// <summary>Effective authorization for a principal, resolved from the IdentityAccess store.</summary>
public sealed record UserAuthorization(
    bool Found,
    IReadOnlyCollection<string> Permissions,
    IReadOnlyCollection<string> DataScopePaths,
    IReadOnlyCollection<Guid> DataScopeUnitIds);

/// <summary>
/// Resolves a user's function-scope permissions (from their roles) and data-scope paths (from their
/// assigned org unit's subtree). Used by the host's claims transformation so authorization is driven
/// by the database, not by static token attributes (docs/design/03 §2).
/// </summary>
public interface IUserAuthorizationProvider
{
    Task<UserAuthorization> GetByUserNameAsync(string userName, CancellationToken ct);
}
