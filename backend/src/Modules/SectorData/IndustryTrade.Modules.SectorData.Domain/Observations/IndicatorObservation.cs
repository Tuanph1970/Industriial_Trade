using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.SectorData.Domain.Observations;

public enum ObservationStatus { Draft = 1, Submitted = 2, Approved = 3 }

/// <summary>
/// A single numeric (or textual) statistical value for an indicator, reported by an org unit for a
/// period. This one generic, partition-friendly aggregate backs most sector indicators — new
/// indicators are new Catalog rows, never schema changes (docs/design/03 §4b, 04 §3.3).
/// References Catalog (IndicatorId) and IdentityAccess (OrgUnitId) by id only — no cross-schema FK.
/// </summary>
public sealed class IndicatorObservation : AggregateRoot<Guid>, IAuditable
{
    private IndicatorObservation() { } // EF

    private IndicatorObservation(Guid id, Guid indicatorId, Guid orgUnitId,
        int periodYear, int? periodMonth, decimal? value, string? valueText, string? source) : base(id)
    {
        IndicatorId = indicatorId;
        OrgUnitId = orgUnitId;
        PeriodYear = periodYear;
        PeriodMonth = periodMonth;
        Value = value;
        ValueText = valueText;
        Source = source;
        Status = ObservationStatus.Draft;
    }

    public Guid IndicatorId { get; private set; }
    public Guid OrgUnitId { get; private set; }
    public int PeriodYear { get; private set; }
    public int? PeriodMonth { get; private set; }
    public decimal? Value { get; private set; }
    public string? ValueText { get; private set; }
    public string? Source { get; private set; }
    public ObservationStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public string? ModifiedBy { get; private set; }

    public static IndicatorObservation Create(Guid indicatorId, Guid orgUnitId,
        int periodYear, int? periodMonth, decimal? value, string? valueText, string? source)
    {
        if (indicatorId == Guid.Empty) throw new ArgumentException("Indicator is required.", nameof(indicatorId));
        if (orgUnitId == Guid.Empty) throw new ArgumentException("Org unit is required.", nameof(orgUnitId));
        if (periodYear is < 2000 or > 2100) throw new ArgumentOutOfRangeException(nameof(periodYear));
        if (periodMonth is < 1 or > 12) throw new ArgumentOutOfRangeException(nameof(periodMonth));

        return new IndicatorObservation(Guid.NewGuid(), indicatorId, orgUnitId,
            periodYear, periodMonth, value, valueText?.Trim(), source?.Trim())
        {
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    // Approval workflow (commune submits → specialist/leader approves or returns for correction).
    public void Submit()
    {
        if (Status != ObservationStatus.Draft)
            throw new InvalidOperationException("Only a draft observation can be submitted.");
        Status = ObservationStatus.Submitted;
        Touch();
    }

    public void Approve()
    {
        if (Status != ObservationStatus.Submitted)
            throw new InvalidOperationException("Only a submitted observation can be approved.");
        Status = ObservationStatus.Approved;
        Touch();
    }

    public void ReturnToDraft()
    {
        if (Status != ObservationStatus.Submitted)
            throw new InvalidOperationException("Only a submitted observation can be returned.");
        Status = ObservationStatus.Draft;
        Touch();
    }

    private void Touch() => ModifiedAtUtc = DateTime.UtcNow;
}
