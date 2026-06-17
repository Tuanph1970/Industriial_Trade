using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.Catalog.Domain.Indicators;

namespace IndustryTrade.Modules.Catalog.Application.Indicators;

public sealed record GetIndicatorsQuery(PageRequest Page, IndustrySector? Sector)
    : IQuery<PagedResult<IndicatorDto>>, IPermissionAuthorized
{
    public string RequiredPermission => CatalogPermissions.IndicatorsRead;
}

public sealed class GetIndicatorsHandler(IIndicatorRepository repository)
    : IQueryHandler<GetIndicatorsQuery, PagedResult<IndicatorDto>>
{
    public async Task<Result<PagedResult<IndicatorDto>>> Handle(GetIndicatorsQuery query, CancellationToken ct)
    {
        var page = query.Page;
        var items = await repository.ListAsync(new IndicatorSearchSpec(page, query.Sector), ct);
        var total = await repository.CountAsync(new IndicatorSearchSpec(page, query.Sector, forCount: true), ct);
        return new PagedResult<IndicatorDto>(items.Select(IndicatorDto.FromEntity).ToList(), total,
            page.NormalizedPage, page.NormalizedPageSize);
    }
}
