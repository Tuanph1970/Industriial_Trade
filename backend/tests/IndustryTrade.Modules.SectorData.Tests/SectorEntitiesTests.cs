using FluentAssertions;
using IndustryTrade.Modules.SectorData.Domain.CommerceLocations;
using IndustryTrade.Modules.SectorData.Domain.Ecommerce;
using IndustryTrade.Modules.SectorData.Domain.PetroleumStations;
using Xunit;

namespace IndustryTrade.Modules.SectorData.Tests;

public class SectorEntitiesTests
{
    [Fact]
    public void PetrolStation_builds_point_from_coordinates()
    {
        var s = PetroleumStation.Create("XD01", "CHXD số 1", Guid.NewGuid(), "GP-123", "QL5",
            longitude: 106.06, latitude: 20.64, StationStatus.Operating);
        s.Location!.X.Should().Be(106.06);
        s.Location.SRID.Should().Be(4326);
    }

    [Fact]
    public void CommerceLocation_requires_code()
    {
        var act = () => CommerceLocation.Create(" ", "Chợ A", CommerceLocationType.Market,
            Guid.NewGuid(), null, null, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Ecommerce_deduplicates_and_trims_platforms()
    {
        var e = EcommerceParticipant.Create("0101234567", "Công ty A", Guid.NewGuid(),
            ["Shopee", " Shopee ", "Lazada", ""], "Nông sản");
        e.Platforms.Should().BeEquivalentTo("Shopee", "Lazada");
    }
}
