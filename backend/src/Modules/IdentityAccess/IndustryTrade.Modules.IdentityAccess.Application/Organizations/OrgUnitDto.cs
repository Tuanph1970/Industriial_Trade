using IndustryTrade.Modules.IdentityAccess.Domain.Organizations;

namespace IndustryTrade.Modules.IdentityAccess.Application.Organizations;

public sealed record OrgUnitDto(
    Guid Id,
    string Code,
    string Name,
    OrgUnitType Type,
    Guid? ParentId,
    string Path,
    bool IsActive)
{
    public static OrgUnitDto FromEntity(OrgUnit u) =>
        new(u.Id, u.Code, u.Name, u.Type, u.ParentId, u.Path, u.IsActive);
}
