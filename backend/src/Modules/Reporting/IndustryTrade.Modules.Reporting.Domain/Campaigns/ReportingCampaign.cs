using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.Reporting.Domain.Campaigns;

public enum CampaignStatus { Open = 1, Closed = 2 }

/// <summary>
/// A reporting period opened by a specialist (kỳ báo cáo). Base units submit reports against it
/// (docs/design/03 §5). Closing a campaign stops new submissions.
/// </summary>
public sealed class ReportingCampaign : AggregateRoot<Guid>, IAuditable
{
    private ReportingCampaign() { } // EF

    private ReportingCampaign(Guid id, string code, string name, int periodYear, int? periodMonth, DateOnly? deadline)
        : base(id)
    {
        Code = code;
        Name = name;
        PeriodYear = periodYear;
        PeriodMonth = periodMonth;
        Deadline = deadline;
        Status = CampaignStatus.Open;
    }

    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public int PeriodYear { get; private set; }
    public int? PeriodMonth { get; private set; }
    public DateOnly? Deadline { get; private set; }
    public CampaignStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public string? ModifiedBy { get; private set; }

    public static ReportingCampaign Create(string code, string name, int periodYear, int? periodMonth, DateOnly? deadline)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Campaign code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Campaign name is required.", nameof(name));
        if (periodYear is < 2000 or > 2100) throw new ArgumentOutOfRangeException(nameof(periodYear));
        if (periodMonth is < 1 or > 12) throw new ArgumentOutOfRangeException(nameof(periodMonth));

        return new ReportingCampaign(Guid.NewGuid(), code.Trim(), name.Trim(), periodYear, periodMonth, deadline)
        {
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Close()
    {
        Status = CampaignStatus.Closed;
        ModifiedAtUtc = DateTime.UtcNow;
    }
}
