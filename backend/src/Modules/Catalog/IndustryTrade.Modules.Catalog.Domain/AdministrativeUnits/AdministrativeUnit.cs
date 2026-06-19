using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.Catalog.Domain.AdministrativeUnits;

/// <summary>Vietnam administrative-division level (2-tier model: province → commune; district kept for legacy data).</summary>
public enum AdministrativeLevel { Province = 1, District = 2, Commune = 3 }

/// <summary>
/// An administrative unit (đơn vị hành chính) — reference data for the province's territory
/// (provinces / communes-wards). Distinct from an org unit: this classifies *where* data belongs,
/// not the department's reporting hierarchy. Optionally nested via <see cref="ParentId"/>.
/// </summary>
public sealed class AdministrativeUnit : AggregateRoot<Guid>, IAuditable
{
    private AdministrativeUnit() { } // EF

    private AdministrativeUnit(Guid id, string code, string name, AdministrativeLevel level, Guid? parentId)
        : base(id)
    {
        Code = code;
        Name = name;
        Level = level;
        ParentId = parentId;
    }

    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public AdministrativeLevel Level { get; private set; }
    public Guid? ParentId { get; private set; }
    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public string? ModifiedBy { get; private set; }

    public static AdministrativeUnit Create(string code, string name, AdministrativeLevel level, Guid? parentId)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Unit code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Unit name is required.", nameof(name));

        return new AdministrativeUnit(Guid.NewGuid(), code.Trim(), name.Trim(), level, parentId)
        {
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public void Update(string name, AdministrativeLevel level, Guid? parentId, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Unit name is required.", nameof(name));

        Name = name.Trim();
        Level = level;
        ParentId = parentId;
        IsActive = isActive;
        ModifiedAtUtc = DateTime.UtcNow;
    }
}
