using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.Modules.SectorData.Domain.Clusters;

namespace IndustryTrade.Modules.SectorData.Application.Clusters;

public sealed record ClusterDto(
    Guid Id, string Code, string Name, Guid OrgUnitId,
    decimal? AreaHa, double? Latitude, double? Longitude, ClusterStatus Status)
{
    public static ClusterDto FromEntity(IndustrialCluster c) =>
        new(c.Id, c.Code, c.Name, c.OrgUnitId, c.AreaHa, c.Location?.Y, c.Location?.X, c.Status);
}

public interface IClusterRepository
{
    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct);
    Task<IReadOnlyList<IndustrialCluster>> ListAsync(Specification<IndustrialCluster> spec, CancellationToken ct);
    Task<int> CountAsync(Specification<IndustrialCluster> spec, CancellationToken ct);
    Task<IndustrialCluster?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(IndustrialCluster cluster, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}

public sealed class ClusterSearchSpec : Specification<IndustrialCluster>
{
    public ClusterSearchSpec(PageRequest request, Guid[]? scopeUnitIds, bool forCount = false)
    {
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim().ToLower();
            Where(c => c.Code.ToLower().Contains(kw) || c.Name.ToLower().Contains(kw));
        }
        if (scopeUnitIds is not null)
            Where(c => scopeUnitIds.Contains(c.OrgUnitId));

        if (!forCount)
        {
            ApplyOrderBy(c => c.Code);
            ApplyPaging(request.Skip, request.NormalizedPageSize);
        }
    }
}
