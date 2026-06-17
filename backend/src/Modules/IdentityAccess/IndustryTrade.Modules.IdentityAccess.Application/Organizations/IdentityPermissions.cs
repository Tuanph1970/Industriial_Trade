namespace IndustryTrade.Modules.IdentityAccess.Application.Organizations;

/// <summary>Function-scope permission codes for this module (map 1:1 to roles).</summary>
public static class IdentityPermissions
{
    public const string OrgUnitsRead = "identity.orgunits.read";
    public const string OrgUnitsManage = "identity.orgunits.manage";
    public const string UsersRead = "identity.users.read";
    public const string UsersManage = "identity.users.manage";
    public const string RolesRead = "identity.roles.read";
    public const string RolesManage = "identity.roles.manage";

    /// <summary>All codes — handy for seeding an "administrator" role.</summary>
    public static readonly string[] All =
    [
        OrgUnitsRead, OrgUnitsManage, UsersRead, UsersManage, RolesRead, RolesManage
    ];
}
