using System.Text.RegularExpressions;
using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.IdentityAccess.Domain.Organizations;

public enum OrgUnitType
{
    Department = 1, // Sở
    Division = 2,   // Phòng
    Commune = 3     // Xã/Phường
}

/// <summary>
/// The organizational unit tree (Cơ quan, đơn vị). Multi-level parent/child — the legacy system
/// could not model this, which was an explicit defect. <see cref="Path"/> is a materialized
/// ltree-style path used for fast subtree / data-scope queries (docs/design/04 §3.1).
/// </summary>
public sealed partial class OrgUnit : AggregateRoot<Guid>, IAuditable
{
    private OrgUnit() { } // EF Core

    private OrgUnit(Guid id, string code, string name, OrgUnitType type, Guid? parentId, string path)
        : base(id)
    {
        Code = code;
        Name = name;
        Type = type;
        ParentId = parentId;
        Path = path;
    }

    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public OrgUnitType Type { get; private set; }
    public Guid? ParentId { get; private set; }
    public string Path { get; private set; } = default!;
    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public string? ModifiedBy { get; private set; }

    public static OrgUnit Create(string code, string name, OrgUnitType type, OrgUnit? parent)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Org unit code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Org unit name is required.", nameof(name));

        var id = Guid.NewGuid();
        var label = ToLabel(code);
        var path = parent is null ? label : $"{parent.Path}.{label}";

        var unit = new OrgUnit(id, code.Trim(), name.Trim(), type, parent?.Id, path)
        {
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        unit.Raise(new OrgUnitCreated(id, unit.Code, unit.Name, parent?.Id));
        return unit;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Org unit name is required.", nameof(name));
        Name = name.Trim();
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    private void Touch() => ModifiedAtUtc = DateTime.UtcNow;

    // ltree labels allow only [A-Za-z0-9_]; normalize the code into a safe label.
    private static string ToLabel(string code) => LabelRegex().Replace(code.Trim(), "_");

    [GeneratedRegex("[^A-Za-z0-9_]")]
    private static partial Regex LabelRegex();
}
