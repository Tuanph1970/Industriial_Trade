using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.IdentityAccess.Application.Organizations;

public sealed record GetOrgUnitsQuery(PageRequest Page) : IQuery<PagedResult<OrgUnitDto>>;

public sealed class GetOrgUnitsHandler(IOrgUnitRepository repository)
    : IQueryHandler<GetOrgUnitsQuery, PagedResult<OrgUnitDto>>
{
    public async Task<Result<PagedResult<OrgUnitDto>>> Handle(GetOrgUnitsQuery query, CancellationToken ct)
    {
        var page = query.Page;
        var items = await repository.ListAsync(new OrgUnitSearchSpec(page), ct);
        var total = await repository.CountAsync(new OrgUnitSearchSpec(page, forCount: true), ct);

        var dtos = items.Select(OrgUnitDto.FromEntity).ToList();
        return new PagedResult<OrgUnitDto>(dtos, total, page.NormalizedPage, page.NormalizedPageSize);
    }
}
