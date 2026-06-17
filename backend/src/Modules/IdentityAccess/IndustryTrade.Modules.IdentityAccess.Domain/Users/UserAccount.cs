using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.IdentityAccess.Domain.Users;

/// <summary>
/// A system user. Authentication is delegated to Keycloak (matched by <see cref="UserName"/> /
/// <see cref="SubjectId"/>); this aggregate is the source of truth for role assignment and the
/// org unit that determines the user's data-scope (docs/design/03 §2).
/// </summary>
public sealed class UserAccount : AggregateRoot<Guid>, IAuditable
{
    private UserAccount() { } // EF

    private UserAccount(Guid id, string userName, string? fullName, string? email, Guid? orgUnitId, Guid[] roleIds)
        : base(id)
    {
        UserName = userName;
        FullName = fullName;
        Email = email;
        OrgUnitId = orgUnitId;
        RoleIds = roleIds;
    }

    public string UserName { get; private set; } = default!;
    public string? FullName { get; private set; }
    public string? Email { get; private set; }

    /// <summary>Keycloak subject id, linked on first login.</summary>
    public string? SubjectId { get; private set; }

    /// <summary>The unit this user belongs to — its subtree is the user's data-scope.</summary>
    public Guid? OrgUnitId { get; private set; }
    public Guid[] RoleIds { get; private set; } = [];
    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public string? ModifiedBy { get; private set; }

    public static UserAccount Create(
        string userName, string? fullName, string? email, Guid? orgUnitId, IEnumerable<Guid> roleIds)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("User name is required.", nameof(userName));

        return new UserAccount(Guid.NewGuid(), userName.Trim(), fullName?.Trim(), email?.Trim(),
            orgUnitId, roleIds.Distinct().ToArray())
        {
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void AssignRoles(IEnumerable<Guid> roleIds)
    {
        RoleIds = roleIds.Distinct().ToArray();
        Touch();
    }

    public void AssignUnit(Guid? orgUnitId)
    {
        OrgUnitId = orgUnitId;
        Touch();
    }

    public void LinkSubject(string subjectId)
    {
        SubjectId = subjectId;
        Touch();
    }

    public void Activate() { IsActive = true; Touch(); }
    public void Deactivate() { IsActive = false; Touch(); }

    private void Touch() => ModifiedAtUtc = DateTime.UtcNow;
}
