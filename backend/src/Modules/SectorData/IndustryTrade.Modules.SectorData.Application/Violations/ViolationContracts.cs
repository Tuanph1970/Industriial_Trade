using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.Modules.SectorData.Domain.Violations;

namespace IndustryTrade.Modules.SectorData.Application.Violations;

public sealed record ViolationDto(
    Guid Id, string CaseNo, ViolationGroup Group, Guid OrgUnitId, string BusinessName,
    DateOnly InspectedOn, string ViolationContent, string? SanctionContent, decimal? FineAmount,
    ViolationStatus Status)
{
    public static ViolationDto FromEntity(MarketViolationCase c) =>
        new(c.Id, c.CaseNo, c.Group, c.OrgUnitId, c.BusinessName, c.InspectedOn,
            c.ViolationContent, c.SanctionContent, c.FineAmount, c.Status);
}

public interface IViolationRepository
{
    Task<bool> ExistsByCaseNoAsync(string caseNo, CancellationToken ct);
    Task<IReadOnlyList<MarketViolationCase>> ListAsync(Specification<MarketViolationCase> spec, CancellationToken ct);
    Task<int> CountAsync(Specification<MarketViolationCase> spec, CancellationToken ct);
    Task<MarketViolationCase?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(MarketViolationCase violation, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}

public sealed class ViolationSearchSpec : Specification<MarketViolationCase>
{
    public ViolationSearchSpec(PageRequest request, Guid[]? scopeUnitIds, ViolationGroup? group = null, bool forCount = false)
    {
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim().ToLower();
            Where(c => c.CaseNo.ToLower().Contains(kw) || c.BusinessName.ToLower().Contains(kw));
        }
        if (group is { } g)
            Where(c => c.Group == g);
        if (scopeUnitIds is not null)
            Where(c => scopeUnitIds.Contains(c.OrgUnitId));

        if (!forCount)
        {
            ApplyOrderByDescending(c => c.InspectedOn);
            ApplyPaging(request.Skip, request.NormalizedPageSize);
        }
    }
}
