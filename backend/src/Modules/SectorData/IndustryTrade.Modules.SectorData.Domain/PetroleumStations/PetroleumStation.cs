using IndustryTrade.BuildingBlocks.Domain;
using NetTopologySuite.Geometries;

namespace IndustryTrade.Modules.SectorData.Domain.PetroleumStations;

public enum StationStatus { Operating = 1, Suspended = 2, Closed = 3 }

/// <summary>A petroleum/fuel retail station (cửa hàng xăng dầu) — rich geo entity (UC-14/24).</summary>
public sealed class PetroleumStation : AggregateRoot<Guid>, IAuditable
{
    private PetroleumStation() { } // EF

    private PetroleumStation(Guid id, string code, string name, Guid orgUnitId,
        string? licenseNo, string? address, Point? location, StationStatus status) : base(id)
    {
        Code = code;
        Name = name;
        OrgUnitId = orgUnitId;
        LicenseNo = licenseNo;
        Address = address;
        Location = location;
        Status = status;
    }

    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public Guid OrgUnitId { get; private set; }
    public string? LicenseNo { get; private set; }
    public string? Address { get; private set; }
    public Point? Location { get; private set; }
    public StationStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public string? ModifiedBy { get; private set; }

    public static PetroleumStation Create(string code, string name, Guid orgUnitId,
        string? licenseNo, string? address, double? longitude, double? latitude, StationStatus status)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Station code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Station name is required.", nameof(name));
        if (orgUnitId == Guid.Empty) throw new ArgumentException("Org unit is required.", nameof(orgUnitId));

        return new PetroleumStation(Guid.NewGuid(), code.Trim(), name.Trim(), orgUnitId,
            licenseNo?.Trim(), address?.Trim(), Geo.Point(longitude, latitude), status)
        {
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Update(string name, string? licenseNo, string? address,
        double? longitude, double? latitude, StationStatus status)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Station name is required.", nameof(name));

        Name = name.Trim();
        LicenseNo = licenseNo?.Trim();
        Address = address?.Trim();
        Location = Geo.Point(longitude, latitude);
        Status = status;
        ModifiedAtUtc = DateTime.UtcNow;
    }
}
