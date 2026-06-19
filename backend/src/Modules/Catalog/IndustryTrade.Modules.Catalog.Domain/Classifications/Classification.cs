using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.Catalog.Domain.Classifications;

/// <summary>One entry of a classification scheme (a code + label, ordered).</summary>
public sealed class ClassificationItem
{
    public string Code { get; init; } = default!;
    public string Name { get; init; } = default!;
    public int SortOrder { get; init; }
}

/// <summary>
/// A classification scheme / code list (danh mục phân loại — docs/design/03 §3) such as goods
/// categories or business types. Owns its ordered <see cref="Items"/>; new schemes are catalog rows,
/// not schema migrations (the recurring-change defense).
/// </summary>
public sealed class Classification : AggregateRoot<Guid>, IAuditable
{
    private readonly List<ClassificationItem> _items = new();

    private Classification() { } // EF

    private Classification(Guid id, string code, string name, string? description) : base(id)
    {
        Code = code;
        Name = name;
        Description = description;
    }

    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyCollection<ClassificationItem> Items => _items.AsReadOnly();

    public DateTime CreatedAtUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public string? ModifiedBy { get; private set; }

    public static Classification Create(string code, string name, string? description,
        IEnumerable<(string Code, string Name, int SortOrder)> items)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Classification code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Classification name is required.", nameof(name));

        var scheme = new Classification(Guid.NewGuid(), code.Trim(), name.Trim(), description?.Trim())
        {
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
        };
        scheme.ReplaceItems(items);
        return scheme;
    }

    public void Update(string name, string? description,
        IEnumerable<(string Code, string Name, int SortOrder)> items)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Classification name is required.", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        ReplaceItems(items);
        ModifiedAtUtc = DateTime.UtcNow;
    }

    private void ReplaceItems(IEnumerable<(string Code, string Name, int SortOrder)> items)
    {
        _items.Clear();
        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Code) || string.IsNullOrWhiteSpace(item.Name)) continue;
            _items.Add(new ClassificationItem { Code = item.Code.Trim(), Name = item.Name.Trim(), SortOrder = item.SortOrder });
        }
    }
}
