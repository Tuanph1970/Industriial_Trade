using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.Catalog.Domain.ReportTemplates;

/// <summary>One row of a report template, binding an indicator to a labelled position.</summary>
public sealed class TemplateLine
{
    public Guid IndicatorId { get; init; }
    public string Label { get; init; } = default!;
    public int RowOrder { get; init; }
}

/// <summary>
/// Structure of a statistical report form per Circular 34/2022 (biểu mẫu báo cáo — docs/design/03 §3).
/// Owns its ordered lines, each bound to a catalog indicator.
/// </summary>
public sealed class ReportTemplate : AggregateRoot<Guid>, IAuditable
{
    private readonly List<TemplateLine> _lines = new();

    private ReportTemplate() { } // EF

    private ReportTemplate(Guid id, string code, string name, string? description) : base(id)
    {
        Code = code;
        Name = name;
        Description = description;
    }

    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyCollection<TemplateLine> Lines => _lines.AsReadOnly();

    public DateTime CreatedAtUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public string? ModifiedBy { get; private set; }

    public static ReportTemplate Create(string code, string name, string? description,
        IEnumerable<(Guid IndicatorId, string Label, int RowOrder)> lines)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Template code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Template name is required.", nameof(name));

        var template = new ReportTemplate(Guid.NewGuid(), code.Trim(), name.Trim(), description?.Trim())
        {
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        foreach (var line in lines)
            template._lines.Add(new TemplateLine { IndicatorId = line.IndicatorId, Label = line.Label.Trim(), RowOrder = line.RowOrder });
        return template;
    }
}
