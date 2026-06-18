using FluentAssertions;
using IndustryTrade.Modules.SectorData.Domain.Clusters;
using IndustryTrade.Modules.SectorData.Domain.Observations;
using Xunit;

namespace IndustryTrade.Modules.SectorData.Tests;

public class SectorDataTests
{
    [Fact]
    public void Observation_starts_in_draft_and_can_be_submitted()
    {
        var obs = IndicatorObservation.Create(Guid.NewGuid(), Guid.NewGuid(), 2026, 6, 1234.5m, null, "manual");
        obs.Status.Should().Be(ObservationStatus.Draft);
        obs.Submit();
        obs.Status.Should().Be(ObservationStatus.Submitted);
    }

    [Fact]
    public void Observation_rejects_invalid_month()
    {
        var act = () => IndicatorObservation.Create(Guid.NewGuid(), Guid.NewGuid(), 2026, 13, 1m, null, null);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Cluster_builds_wgs84_point_from_lng_lat()
    {
        var cluster = IndustrialCluster.Create("CCN01", "Cụm CN 01", Guid.NewGuid(),
            areaHa: 25.5m, longitude: 106.05, latitude: 20.65, ClusterStatus.Operating);

        cluster.Location.Should().NotBeNull();
        cluster.Location!.X.Should().Be(106.05);  // longitude
        cluster.Location.Y.Should().Be(20.65);    // latitude
        cluster.Location.SRID.Should().Be(4326);
    }

    [Fact]
    public void Cluster_without_coordinates_has_no_location()
    {
        var cluster = IndustrialCluster.Create("CCN02", "Cụm CN 02", Guid.NewGuid(),
            null, null, null, ClusterStatus.Planned);
        cluster.Location.Should().BeNull();
    }
}
