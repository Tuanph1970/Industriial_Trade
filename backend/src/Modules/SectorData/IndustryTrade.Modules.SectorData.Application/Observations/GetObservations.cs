using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Security;
using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.SectorData.Application.Observations;

public sealed record GetObservationsQuery(PageRequest Page, int? PeriodYear)
    : IQuery<PagedResult<ObservationDto>>, IPermissionAuthorized
{
    public string RequiredPermission => SectorPermissions.ObservationsRead;
}

public sealed class GetObservationsHandler(IObservationRepository repository, ICurrentUser currentUser)
    : IQueryHandler<GetObservationsQuery, PagedResult<ObservationDto>>
{
    public async Task<Result<PagedResult<ObservationDto>>> Handle(GetObservationsQuery query, CancellationToken ct)
    {
        var page = query.Page;
        var scope = currentUser.IsSuperAdmin ? null : currentUser.DataScopeUnitIds.ToArray();

        var items = await repository.ListAsync(new ObservationSearchSpec(page, scope, query.PeriodYear), ct);
        var total = await repository.CountAsync(new ObservationSearchSpec(page, scope, query.PeriodYear, forCount: true), ct);
        return new PagedResult<ObservationDto>(items.Select(ObservationDto.FromEntity).ToList(), total,
            page.NormalizedPage, page.NormalizedPageSize);
    }
}
