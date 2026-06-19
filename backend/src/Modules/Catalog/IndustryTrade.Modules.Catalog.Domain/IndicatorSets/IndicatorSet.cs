using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.Catalog.Domain.IndicatorSets;

/// <summary>A named grouping of indicators for a reporting context (bộ chỉ tiêu — docs/design/03 §3).</summary>
public sealed class IndicatorSet : AggregateRoot<Guid>, IAuditable
{
    private IndicatorSet() { } // EF

    private IndicatorSet(Guid id, string code, string name, string? description, Guid[] indicatorIds) : base(id)
    {
        Code = code;
        Name = name;
        Description = description;
        IndicatorIds = indicatorIds;
    }

    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public Guid[] IndicatorIds { get; private set; } = [];
    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public string? ModifiedBy { get; private set; }

    public static IndicatorSet Create(string code, string name, string? description, IEnumerable<Guid> indicatorIds)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Set code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Set name is required.", nameof(name));

        return new IndicatorSet(Guid.NewGuid(), code.Trim(), name.Trim(), description?.Trim(),
            indicatorIds.Distinct().ToArray())
        {
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void UpdateMembers(IEnumerable<Guid> indicatorIds)
    {
        IndicatorIds = indicatorIds.Distinct().ToArray();
        ModifiedAtUtc = DateTime.UtcNow;
    }

    public void Update(string name, string? description, IEnumerable<Guid> indicatorIds)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Set name is required.", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        IndicatorIds = indicatorIds.Distinct().ToArray();
        ModifiedAtUtc = DateTime.UtcNow;
    }
}
