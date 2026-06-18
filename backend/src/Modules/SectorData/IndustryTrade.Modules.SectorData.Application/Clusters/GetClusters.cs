using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Security;
using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.SectorData.Application.Clusters;

public sealed record GetClustersQuery(PageRequest Page)
    : IQuery<PagedResult<ClusterDto>>, IPermissionAuthorized
{
    public string RequiredPermission => SectorPermissions.ClustersRead;
}

public sealed class GetClustersHandler(IClusterRepository repository, ICurrentUser currentUser)
    : IQueryHandler<GetClustersQuery, PagedResult<ClusterDto>>
{
    public async Task<Result<PagedResult<ClusterDto>>> Handle(GetClustersQuery query, CancellationToken ct)
    {
        var page = query.Page;
        var scope = currentUser.IsSuperAdmin ? null : currentUser.DataScopeUnitIds.ToArray();

        var items = await repository.ListAsync(new ClusterSearchSpec(page, scope), ct);
        var total = await repository.CountAsync(new ClusterSearchSpec(page, scope, forCount: true), ct);
        return new PagedResult<ClusterDto>(items.Select(ClusterDto.FromEntity).ToList(), total,
            page.NormalizedPage, page.NormalizedPageSize);
    }
}
