namespace IndustryTrade.Modules.Reporting.Domain.Submissions;

/// <summary>
/// One content line of a report: an indicator (from the bound Catalog template) with the value
/// auto-extracted from the unit's SectorData observation for the campaign's period. Value is null
/// when no matching observation exists yet.
/// </summary>
public sealed class ReportLine
{
    public Guid IndicatorId { get; init; }
    public string IndicatorCode { get; init; } = default!;
    public string Label { get; init; } = default!;
    public int RowOrder { get; init; }
    public decimal? Value { get; init; }
    public string? ValueText { get; init; }
}
