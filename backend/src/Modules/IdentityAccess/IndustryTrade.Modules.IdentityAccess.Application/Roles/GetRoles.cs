using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.IdentityAccess.Application.Organizations;

namespace IndustryTrade.Modules.IdentityAccess.Application.Roles;

public sealed record GetRolesQuery(PageRequest Page) : IQuery<PagedResult<RoleDto>>, IPermissionAuthorized
{
    public string RequiredPermission => IdentityPermissions.RolesRead;
}

public sealed class GetRolesHandler(IRoleRepository repository)
    : IQueryHandler<GetRolesQuery, PagedResult<RoleDto>>
{
    public async Task<Result<PagedResult<RoleDto>>> Handle(GetRolesQuery query, CancellationToken ct)
    {
        var page = query.Page;
        var items = await repository.ListAsync(new RoleSearchSpec(page), ct);
        var total = await repository.CountAsync(new RoleSearchSpec(page, forCount: true), ct);
        return new PagedResult<RoleDto>(items.Select(RoleDto.FromEntity).ToList(), total,
            page.NormalizedPage, page.NormalizedPageSize);
    }
}
