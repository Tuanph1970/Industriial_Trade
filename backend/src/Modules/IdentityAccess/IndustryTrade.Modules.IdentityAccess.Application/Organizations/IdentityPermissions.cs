namespace IndustryTrade.Modules.IdentityAccess.Application.Organizations;

/// <summary>Function-scope permission codes for this module (map 1:1 to Keycloak roles).</summary>
public static class IdentityPermissions
{
    public const string OrgUnitsRead = "identity.orgunits.read";
    public const string OrgUnitsManage = "identity.orgunits.manage";
}
