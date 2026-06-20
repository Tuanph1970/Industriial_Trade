namespace IndustryTrade.Modules.IdentityAccess.Application.Users;

/// <summary>
/// Admin operations against the external identity provider (Keycloak). Lets the app manage credentials
/// without owning them — authentication stays delegated to Keycloak (docs/design/02 §5).
/// </summary>
public interface IIdentityProviderAdmin
{
    /// <summary>
    /// Resets the user's password to the configured default as a <b>temporary</b> credential (the user
    /// must change it at next login). Returns the new password on success, or null if it failed
    /// (user not found in the IdP or the IdP was unreachable).
    /// </summary>
    Task<string?> ResetToDefaultPasswordAsync(string userName, CancellationToken ct);
}
