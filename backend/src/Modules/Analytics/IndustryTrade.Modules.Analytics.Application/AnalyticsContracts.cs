namespace IndustryTrade.Modules.Analytics.Application;

public static class AnalyticsPermissions
{
    public const string Read = "analytics.read";
}

/// <summary>Leadership at-a-glance counts across the sector + reporting domains.</summary>
public sealed record DashboardDto(
    long Clusters, long PetrolStations, long CommerceLocations, long EcommerceParticipants,
    long Violations, long Observations, long Indicators, long Campaigns, long Submissions, long PendingApproval);

public sealed record ViolationSummaryRow(int Group, int Status, long Count, decimal TotalFine);
public sealed record StateCount(int State, long Count);
/// <summary>Statistical roll-up of observations by indicator sector (Circular-34 aggregate).</summary>
public sealed record SectorObservationRow(int Sector, long Count, decimal TotalValue);
public sealed record CommerceTypeRow(int Type, long Count);

/// <summary>
/// Read-only analytics over the operational schemas (sector, reporting, catalog). This is the CQRS
/// read side (docs/design/03 §6); in production these can be backed by materialized views refreshed
/// on domain events. <paramref name="scopeUnitIds"/> null = no data-scope restriction (super-admin).
/// </summary>
public interface IAnalyticsQueries
{
    Task<DashboardDto> GetDashboardAsync(Guid[]? scopeUnitIds, CancellationToken ct);
    Task<IReadOnlyList<ViolationSummaryRow>> GetViolationsSummaryAsync(Guid[]? scopeUnitIds, CancellationToken ct);
    Task<IReadOnlyList<StateCount>> GetReportingSummaryAsync(Guid[]? scopeUnitIds, CancellationToken ct);
    Task<IReadOnlyList<SectorObservationRow>> GetObservationsBySectorAsync(Guid[]? scopeUnitIds, CancellationToken ct);
    Task<IReadOnlyList<CommerceTypeRow>> GetCommerceByTypeAsync(Guid[]? scopeUnitIds, CancellationToken ct);
}
