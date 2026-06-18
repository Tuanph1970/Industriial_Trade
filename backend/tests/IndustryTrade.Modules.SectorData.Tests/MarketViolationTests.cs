using FluentAssertions;
using IndustryTrade.Modules.SectorData.Domain.Violations;
using Xunit;

namespace IndustryTrade.Modules.SectorData.Tests;

public class MarketViolationTests
{
    private static MarketViolationCase NewCase() => MarketViolationCase.Create(
        "QLTT-2026-001", ViolationGroup.ProhibitedAndCounterfeit, Guid.NewGuid(),
        "Hộ kinh doanh A", new DateOnly(2026, 3, 10), "Bán hàng giả nhãn hiệu");

    [Fact]
    public void Create_starts_reported()
    {
        var c = NewCase();
        c.Status.Should().Be(ViolationStatus.Reported);
        c.SanctionContent.Should().BeNull();
    }

    [Fact]
    public void Resolve_requires_sanction_and_sets_status()
    {
        var c = NewCase();
        c.StartHandling();
        c.Status.Should().Be(ViolationStatus.UnderHandling);

        c.Resolve("Phạt tiền và tịch thu hàng hoá", 15_000_000m);
        c.Status.Should().Be(ViolationStatus.Resolved);
        c.FineAmount.Should().Be(15_000_000m);
    }

    [Fact]
    public void Resolve_without_sanction_throws()
    {
        var c = NewCase();
        var act = () => c.Resolve("  ", null);
        act.Should().Throw<ArgumentException>();
    }
}
