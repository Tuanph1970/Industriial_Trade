using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Security;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.SectorData.Domain.Violations;

namespace IndustryTrade.Modules.SectorData.Application.Violations;

public sealed record GetViolationsQuery(PageRequest Page, ViolationGroup? Group)
    : IQuery<PagedResult<ViolationDto>>, IPermissionAuthorized
{
    public string RequiredPermission => SectorPermissions.ViolationsRead;
}

public sealed class GetViolationsHandler(IViolationRepository repository, ICurrentUser currentUser)
    : IQueryHandler<GetViolationsQuery, PagedResult<ViolationDto>>
{
    public async Task<Result<PagedResult<ViolationDto>>> Handle(GetViolationsQuery query, CancellationToken ct)
    {
        var page = query.Page;
        var scope = currentUser.IsSuperAdmin ? null : currentUser.DataScopeUnitIds.ToArray();

        var items = await repository.ListAsync(new ViolationSearchSpec(page, scope, query.Group), ct);
        var total = await repository.CountAsync(new ViolationSearchSpec(page, scope, query.Group, forCount: true), ct);
        return new PagedResult<ViolationDto>(items.Select(ViolationDto.FromEntity).ToList(), total,
            page.NormalizedPage, page.NormalizedPageSize);
    }
}
