using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace IndustryTrade.Modules.SectorData.Domain;

/// <summary>Builds WGS-84 (SRID 4326) points for the sector's geo entities.</summary>
internal static class Geo
{
    private static readonly GeometryFactory Factory =
        NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    public static Point? Point(double? longitude, double? latitude) =>
        longitude is { } lng && latitude is { } lat
            ? Factory.CreatePoint(new Coordinate(lng, lat))
            : null;
}
