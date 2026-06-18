using IndustryTrade.BuildingBlocks.Domain;
using NetTopologySuite.Geometries;

namespace IndustryTrade.Modules.SectorData.Domain.CommerceLocations;

public enum CommerceLocationType { Market = 1, Supermarket = 2, Mall = 3, ConvenienceStore = 4 }

/// <summary>A commerce location — market / supermarket / mall / convenience store (UC-13/23).</summary>
public sealed class CommerceLocation : AggregateRoot<Guid>, IAuditable
{
    private CommerceLocation() { } // EF

    private CommerceLocation(Guid id, string code, string name, CommerceLocationType type,
        Guid orgUnitId, string? address, Point? location) : base(id)
    {
        Code = code;
        Name = name;
        Type = type;
        OrgUnitId = orgUnitId;
        Address = address;
        Location = location;
    }

    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public CommerceLocationType Type { get; private set; }
    public Guid OrgUnitId { get; private set; }
    public string? Address { get; private set; }
    public Point? Location { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public string? ModifiedBy { get; private set; }

    public static CommerceLocation Create(string code, string name, CommerceLocationType type,
        Guid orgUnitId, string? address, double? longitude, double? latitude)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Location code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Location name is required.", nameof(name));
        if (orgUnitId == Guid.Empty) throw new ArgumentException("Org unit is required.", nameof(orgUnitId));

        return new CommerceLocation(Guid.NewGuid(), code.Trim(), name.Trim(), type, orgUnitId,
            address?.Trim(), Geo.Point(longitude, latitude))
        {
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
