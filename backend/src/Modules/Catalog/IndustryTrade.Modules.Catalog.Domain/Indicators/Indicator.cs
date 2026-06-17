using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.Catalog.Domain.Indicators;

public enum IndicatorDataType { Number = 1, Text = 2, Enumeration = 3 }

public enum IndustrySector { Industry = 1, Energy = 2, Commerce = 3, MarketSurveillance = 4 }

/// <summary>
/// Definition of a statistical indicator per Circular 33/2022/TT-BCT. Versioned with
/// effective/retired dates so the indicator system can evolve across circular revisions without
/// breaking historical data (docs/design/03 §3, 04 §3.2).
/// </summary>
public sealed class Indicator : AggregateRoot<Guid>, IAuditable
{
    private Indicator() { } // EF

    private Indicator(Guid id, string code, string name, string unit,
        IndicatorDataType dataType, IndustrySector sector, DateOnly effectiveFrom) : base(id)
    {
        Code = code;
        Name = name;
        Unit = unit;
        DataType = dataType;
        Sector = sector;
        EffectiveFrom = effectiveFrom;
        Version = 1;
    }

    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string Unit { get; private set; } = default!;
    public IndicatorDataType DataType { get; private set; }
    public IndustrySector Sector { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? RetiredAt { get; private set; }
    public int Version { get; private set; }
    public bool IsActive => RetiredAt is null;

    public DateTime CreatedAtUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public string? ModifiedBy { get; private set; }

    public static Indicator Create(string code, string name, string unit,
        IndicatorDataType dataType, IndustrySector sector, DateOnly effectiveFrom)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Indicator code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Indicator name is required.", nameof(name));

        return new Indicator(Guid.NewGuid(), code.Trim(), name.Trim(), (unit ?? string.Empty).Trim(),
            dataType, sector, effectiveFrom)
        {
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Update(string name, string unit, IndicatorDataType dataType, IndustrySector sector)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Indicator name is required.", nameof(name));
        Name = name.Trim();
        Unit = (unit ?? string.Empty).Trim();
        DataType = dataType;
        Sector = sector;
        Version++;
        ModifiedAtUtc = DateTime.UtcNow;
    }

    public void Retire(DateOnly retiredAt)
    {
        RetiredAt = retiredAt;
        ModifiedAtUtc = DateTime.UtcNow;
    }
}
