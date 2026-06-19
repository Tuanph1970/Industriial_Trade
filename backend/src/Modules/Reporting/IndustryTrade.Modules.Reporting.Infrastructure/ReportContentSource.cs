using Dapper;
using IndustryTrade.Modules.Reporting.Application.Submissions;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace IndustryTrade.Modules.Reporting.Infrastructure;

/// <summary>
/// Read-only ACL that assembles report content by joining a Catalog template's lines to the unit's
/// SectorData observations for the campaign's period. Cross-schema read coupling is the accepted
/// trade-off (mirrors Analytics — docs/design/03 §6); Reporting keeps no code reference to those modules.
/// </summary>
internal sealed class ReportContentSource(IConfiguration configuration) : IReportContentSource
{
    private readonly string _connectionString = configuration.GetConnectionString("Postgres")
        ?? throw new InvalidOperationException("Missing ConnectionStrings:Postgres.");

    public async Task<IReadOnlyList<ExtractedReportLine>> ExtractAsync(
        Guid templateId, Guid orgUnitId, int periodYear, int? periodMonth, CancellationToken ct)
    {
        const string sql = """
            select l."IndicatorId"            as IndicatorId,
                   i."Code"                   as IndicatorCode,
                   l."Label"                  as Label,
                   l."RowOrder"               as RowOrder,
                   o."Value"                  as Value,
                   o."ValueText"              as ValueText
            from catalog.report_template_line l
            join catalog.indicator i on i."Id" = l."IndicatorId"
            left join sector.indicator_observation o
                   on o."IndicatorId" = l."IndicatorId"
                  and o."OrgUnitId"   = @orgUnitId
                  and o."PeriodYear"  = @periodYear
                  and (o."PeriodMonth" = @periodMonth or (@periodMonth is null and o."PeriodMonth" is null))
            where l."TemplateId" = @templateId
            order by l."RowOrder";
            """;

        await using var conn = new NpgsqlConnection(_connectionString);
        var rows = await conn.QueryAsync<ExtractedReportLine>(new CommandDefinition(
            sql, new { templateId, orgUnitId, periodYear, periodMonth }, cancellationToken: ct));
        return rows.ToList();
    }
}
