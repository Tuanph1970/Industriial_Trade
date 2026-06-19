using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.Catalog.Domain.ReportingPeriods;

public enum Periodicity { Monthly = 1, Quarterly = 2, Yearly = 3 }

/// <summary>Definition of a reporting period type (kỳ báo cáo) that campaigns are opened against.</summary>
public sealed class ReportingPeriodDefinition : AggregateRoot<Guid>, IAuditable
{
    private ReportingPeriodDefinition() { } // EF

    private ReportingPeriodDefinition(Guid id, string code, string name, Periodicity periodicity) : base(id)
    {
        Code = code;
        Name = name;
        Periodicity = periodicity;
    }

    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public Periodicity Periodicity { get; private set; }
    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public string? ModifiedBy { get; private set; }

    public static ReportingPeriodDefinition Create(string code, string name, Periodicity periodicity)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Period code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Period name is required.", nameof(name));

        return new ReportingPeriodDefinition(Guid.NewGuid(), code.Trim(), name.Trim(), periodicity)
        {
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Update(string name, Periodicity periodicity)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Period name is required.", nameof(name));

        Name = name.Trim();
        Periodicity = periodicity;
        ModifiedAtUtc = DateTime.UtcNow;
    }
}
