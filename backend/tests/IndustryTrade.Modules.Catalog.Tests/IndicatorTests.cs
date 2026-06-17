using FluentAssertions;
using IndustryTrade.Modules.Catalog.Domain.Indicators;
using Xunit;

namespace IndustryTrade.Modules.Catalog.Tests;

public class IndicatorTests
{
    [Fact]
    public void Create_sets_version_1_and_active()
    {
        var ind = Indicator.Create("CN01", "Sản lượng điện", "kWh",
            IndicatorDataType.Number, IndustrySector.Energy, new DateOnly(2026, 1, 1));

        ind.Version.Should().Be(1);
        ind.IsActive.Should().BeTrue();
        ind.RetiredAt.Should().BeNull();
    }

    [Fact]
    public void Update_bumps_version()
    {
        var ind = Indicator.Create("CN01", "X", "kWh", IndicatorDataType.Number, IndustrySector.Energy, new(2026, 1, 1));
        ind.Update("Sản lượng điện", "MWh", IndicatorDataType.Number, IndustrySector.Energy);
        ind.Version.Should().Be(2);
        ind.Unit.Should().Be("MWh");
    }

    [Fact]
    public void Retire_marks_inactive()
    {
        var ind = Indicator.Create("CN01", "X", "kWh", IndicatorDataType.Number, IndustrySector.Energy, new(2026, 1, 1));
        ind.Retire(new DateOnly(2026, 12, 31));
        ind.IsActive.Should().BeFalse();
    }
}
