using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Security;
using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.IdentityAccess.Application.Organizations;

public sealed record GetOrgUnitsQuery(PageRequest Page)
    : IQuery<PagedResult<OrgUnitDto>>, IPermissionAuthorized
{
    public string RequiredPermission => IdentityPermissions.OrgUnitsRead;
}

public sealed class GetOrgUnitsHandler(IOrgUnitRepository repository, ICurrentUser currentUser)
    : IQueryHandler<GetOrgUnitsQuery, PagedResult<OrgUnitDto>>
{
    public async Task<Result<PagedResult<OrgUnitDto>>> Handle(GetOrgUnitsQuery query, CancellationToken ct)
    {
        var page = query.Page;

        // Data-scope: super-admins see everything; everyone else is limited to their org-unit subtree
        // (the unit ids were resolved from the ltree subtree query during the claims transformation).
        var scopeUnitIds = currentUser.IsSuperAdmin ? null : currentUser.DataScopeUnitIds.ToArray();

        var items = await repository.ListAsync(new OrgUnitSearchSpec(page, scopeUnitIds), ct);
        var total = await repository.CountAsync(new OrgUnitSearchSpec(page, scopeUnitIds, forCount: true), ct);

        var dtos = items.Select(OrgUnitDto.FromEntity).ToList();
        return new PagedResult<OrgUnitDto>(dtos, total, page.NormalizedPage, page.NormalizedPageSize);
    }
}
