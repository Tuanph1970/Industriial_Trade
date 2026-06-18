using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.Modules.SectorData.Domain.PetroleumStations;

namespace IndustryTrade.Modules.SectorData.Application.PetroleumStations;

public sealed record PetrolStationDto(
    Guid Id, string Code, string Name, Guid OrgUnitId, string? LicenseNo, string? Address,
    double? Latitude, double? Longitude, StationStatus Status)
{
    public static PetrolStationDto FromEntity(PetroleumStation s) =>
        new(s.Id, s.Code, s.Name, s.OrgUnitId, s.LicenseNo, s.Address, s.Location?.Y, s.Location?.X, s.Status);
}

public interface IPetrolStationRepository
{
    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct);
    Task<IReadOnlyList<PetroleumStation>> ListAsync(Specification<PetroleumStation> spec, CancellationToken ct);
    Task<int> CountAsync(Specification<PetroleumStation> spec, CancellationToken ct);
    Task AddAsync(PetroleumStation station, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}

public sealed class PetrolStationSearchSpec : Specification<PetroleumStation>
{
    public PetrolStationSearchSpec(PageRequest request, Guid[]? scopeUnitIds, bool forCount = false)
    {
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim().ToLower();
            Where(s => s.Code.ToLower().Contains(kw) || s.Name.ToLower().Contains(kw));
        }
        if (scopeUnitIds is not null)
            Where(s => scopeUnitIds.Contains(s.OrgUnitId));

        if (!forCount)
        {
            ApplyOrderBy(s => s.Code);
            ApplyPaging(request.Skip, request.NormalizedPageSize);
        }
    }
}
