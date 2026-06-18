using IndustryTrade.BuildingBlocks.Domain;
using NetTopologySuite.Geometries;

namespace IndustryTrade.Modules.SectorData.Domain.Clusters;

public enum ClusterStatus { Planned = 1, Operating = 2, Suspended = 3 }

/// <summary>
/// A rich sector entity with PostGIS geometry — an industrial cluster (cụm công nghiệp). Demonstrates
/// the geo/master-data pattern (docs/design/03 §4a). Located via a WGS-84 point.
/// </summary>
public sealed class IndustrialCluster : AggregateRoot<Guid>, IAuditable
{
    private static readonly GeometryFactory GeometryFactory =
        NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    private IndustrialCluster() { } // EF

    private IndustrialCluster(Guid id, string code, string name, Guid orgUnitId,
        decimal? areaHa, Point? location, ClusterStatus status) : base(id)
    {
        Code = code;
        Name = name;
        OrgUnitId = orgUnitId;
        AreaHa = areaHa;
        Location = location;
        Status = status;
    }

    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public Guid OrgUnitId { get; private set; }
    public decimal? AreaHa { get; private set; }
    public Point? Location { get; private set; }
    public ClusterStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public string? ModifiedBy { get; private set; }

    public static IndustrialCluster Create(string code, string name, Guid orgUnitId,
        decimal? areaHa, double? longitude, double? latitude, ClusterStatus status)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Cluster code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Cluster name is required.", nameof(name));
        if (orgUnitId == Guid.Empty) throw new ArgumentException("Org unit is required.", nameof(orgUnitId));

        Point? point = longitude is { } lng && latitude is { } lat
            ? GeometryFactory.CreatePoint(new Coordinate(lng, lat))
            : null;

        return new IndustrialCluster(Guid.NewGuid(), code.Trim(), name.Trim(), orgUnitId, areaHa, point, status)
        {
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
