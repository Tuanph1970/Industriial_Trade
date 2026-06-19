using Dapper;
using IndustryTrade.Modules.Analytics.Application;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace IndustryTrade.Modules.Analytics.Infrastructure;

/// <summary>
/// Read-only aggregate queries across the operational schemas (sector/reporting/catalog) via Dapper.
/// <c>@units::uuid[] is null</c> means "no data-scope restriction" (super-admin); otherwise rows are
/// limited to the caller's accessible org units. Read coupling to other contexts' tables is an
/// accepted analytics trade-off (could be replaced by materialized views — docs/design/03 §6, 04 §4).
/// </summary>
internal sealed class AnalyticsQueries(IConfiguration configuration) : IAnalyticsQueries
{
    private readonly string _connectionString = configuration.GetConnectionString("Postgres")
        ?? throw new InvalidOperationException("Missing ConnectionStrings:Postgres.");

    private const string ScopeFilter = "(@units::uuid[] is null or \"OrgUnitId\" = any(@units))";

    public async Task<DashboardDto> GetDashboardAsync(Guid[]? scopeUnitIds, CancellationToken ct)
    {
        var sql = $"""
            select
              (select count(*) from sector.industrial_cluster   where {ScopeFilter}) as Clusters,
              (select count(*) from sector.petroleum_station     where {ScopeFilter}) as PetrolStations,
              (select count(*) from sector.commerce_location     where {ScopeFilter}) as CommerceLocations,
              (select count(*) from sector.ecommerce_participant where {ScopeFilter}) as EcommerceParticipants,
              (select count(*) from sector.market_violation_case where {ScopeFilter}) as Violations,
              (select count(*) from sector.indicator_observation where {ScopeFilter}) as Observations,
              (select count(*) from catalog.indicator)                                as Indicators,
              (select count(*) from reporting.campaign)                               as Campaigns,
              (select count(*) from reporting.report_submission  where {ScopeFilter}) as Submissions,
              (select count(*) from reporting.report_submission  where "State" = 4 and {ScopeFilter}) as PendingApproval;
            """;
        await using var conn = new NpgsqlConnection(_connectionString);
        return await conn.QuerySingleAsync<DashboardDto>(new CommandDefinition(sql, new { units = scopeUnitIds }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<ViolationSummaryRow>> GetViolationsSummaryAsync(Guid[]? scopeUnitIds, CancellationToken ct)
    {
        var sql = $"""
            select "Group" as "Group", "Status" as "Status", count(*) as "Count",
                   coalesce(sum("FineAmount"), 0) as "TotalFine"
            from sector.market_violation_case
            where {ScopeFilter}
            group by "Group", "Status"
            order by "Group", "Status";
            """;
        await using var conn = new NpgsqlConnection(_connectionString);
        var rows = await conn.QueryAsync<ViolationSummaryRow>(new CommandDefinition(sql, new { units = scopeUnitIds }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<StateCount>> GetReportingSummaryAsync(Guid[]? scopeUnitIds, CancellationToken ct)
    {
        var sql = $"""
            select "State" as "State", count(*) as "Count"
            from reporting.report_submission
            where {ScopeFilter}
            group by "State"
            order by "State";
            """;
        await using var conn = new NpgsqlConnection(_connectionString);
        var rows = await conn.QueryAsync<StateCount>(new CommandDefinition(sql, new { units = scopeUnitIds }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<SectorObservationRow>> GetObservationsBySectorAsync(Guid[]? scopeUnitIds, CancellationToken ct)
    {
        var sql = $"""
            select i."Sector" as "Sector", count(*) as "Count", coalesce(sum(o."Value"), 0) as "TotalValue"
            from sector.indicator_observation o
            join catalog.indicator i on i."Id" = o."IndicatorId"
            where {ScopeFilter}
            group by i."Sector"
            order by i."Sector";
            """;
        await using var conn = new NpgsqlConnection(_connectionString);
        var rows = await conn.QueryAsync<SectorObservationRow>(new CommandDefinition(sql, new { units = scopeUnitIds }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<CommerceTypeRow>> GetCommerceByTypeAsync(Guid[]? scopeUnitIds, CancellationToken ct)
    {
        var sql = $"""
            select "Type" as "Type", count(*) as "Count"
            from sector.commerce_location
            where {ScopeFilter}
            group by "Type"
            order by "Type";
            """;
        await using var conn = new NpgsqlConnection(_connectionString);
        var rows = await conn.QueryAsync<CommerceTypeRow>(new CommandDefinition(sql, new { units = scopeUnitIds }, cancellationToken: ct));
        return rows.ToList();
    }
}
