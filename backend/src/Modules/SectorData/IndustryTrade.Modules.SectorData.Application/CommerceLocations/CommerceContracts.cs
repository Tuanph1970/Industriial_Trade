using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.Modules.SectorData.Domain.CommerceLocations;

namespace IndustryTrade.Modules.SectorData.Application.CommerceLocations;

public sealed record CommerceLocationDto(
    Guid Id, string Code, string Name, CommerceLocationType Type, Guid OrgUnitId,
    string? Address, double? Latitude, double? Longitude)
{
    public static CommerceLocationDto FromEntity(CommerceLocation c) =>
        new(c.Id, c.Code, c.Name, c.Type, c.OrgUnitId, c.Address, c.Location?.Y, c.Location?.X);
}

public interface ICommerceLocationRepository
{
    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct);
    Task<IReadOnlyList<CommerceLocation>> ListAsync(Specification<CommerceLocation> spec, CancellationToken ct);
    Task<int> CountAsync(Specification<CommerceLocation> spec, CancellationToken ct);
    Task<CommerceLocation?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(CommerceLocation location, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}

public sealed class CommerceLocationSearchSpec : Specification<CommerceLocation>
{
    public CommerceLocationSearchSpec(PageRequest request, Guid[]? scopeUnitIds,
        CommerceLocationType? type = null, bool forCount = false)
    {
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim().ToLower();
            Where(c => c.Code.ToLower().Contains(kw) || c.Name.ToLower().Contains(kw));
        }
        if (type is { } t)
            Where(c => c.Type == t);
        if (scopeUnitIds is not null)
            Where(c => scopeUnitIds.Contains(c.OrgUnitId));

        if (!forCount)
        {
            ApplyOrderBy(c => c.Code);
            ApplyPaging(request.Skip, request.NormalizedPageSize);
        }
    }
}
