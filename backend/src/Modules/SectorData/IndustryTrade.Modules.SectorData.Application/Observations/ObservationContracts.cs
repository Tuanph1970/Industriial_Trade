using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.Modules.SectorData.Domain.Observations;

namespace IndustryTrade.Modules.SectorData.Application.Observations;

public sealed record ObservationDto(
    Guid Id, Guid IndicatorId, Guid OrgUnitId, int PeriodYear, int? PeriodMonth,
    decimal? Value, string? ValueText, string? Source, ObservationStatus Status)
{
    public static ObservationDto FromEntity(IndicatorObservation o) =>
        new(o.Id, o.IndicatorId, o.OrgUnitId, o.PeriodYear, o.PeriodMonth, o.Value, o.ValueText, o.Source, o.Status);
}

public interface IObservationRepository
{
    Task<IReadOnlyList<IndicatorObservation>> ListAsync(Specification<IndicatorObservation> spec, CancellationToken ct);
    Task<int> CountAsync(Specification<IndicatorObservation> spec, CancellationToken ct);
    Task AddAsync(IndicatorObservation observation, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}

/// <summary>List spec with data-scope: pass <paramref name="scopeUnitIds"/> = null for super-admin
/// (no restriction); a non-null array restricts rows to those org units.</summary>
public sealed class ObservationSearchSpec : Specification<IndicatorObservation>
{
    public ObservationSearchSpec(PageRequest request, Guid[]? scopeUnitIds, int? periodYear = null, bool forCount = false)
    {
        if (scopeUnitIds is not null)
            Where(o => scopeUnitIds.Contains(o.OrgUnitId));
        if (periodYear is { } year)
            Where(o => o.PeriodYear == year);

        if (!forCount)
        {
            ApplyOrderByDescending(o => o.PeriodYear);
            ApplyPaging(request.Skip, request.NormalizedPageSize);
        }
    }
}
