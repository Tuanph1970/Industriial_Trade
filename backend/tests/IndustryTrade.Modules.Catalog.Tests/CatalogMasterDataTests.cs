using FluentAssertions;
using IndustryTrade.Modules.Catalog.Domain.IndicatorSets;
using IndustryTrade.Modules.Catalog.Domain.ReportingPeriods;
using IndustryTrade.Modules.Catalog.Domain.ReportTemplates;
using Xunit;

namespace IndustryTrade.Modules.Catalog.Tests;

public class CatalogMasterDataTests
{
    [Fact]
    public void IndicatorSet_deduplicates_members()
    {
        var id = Guid.NewGuid();
        var set = IndicatorSet.Create("S1", "Bộ chỉ tiêu CN", null, [id, id, Guid.NewGuid()]);
        set.IndicatorIds.Should().HaveCount(2);
        set.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ReportTemplate_keeps_its_lines()
    {
        var template = ReportTemplate.Create("T1", "Biểu mẫu CN", "mô tả",
        [
            (Guid.NewGuid(), "Sản lượng", 1),
            (Guid.NewGuid(), "Doanh thu", 2),
        ]);
        template.Lines.Should().HaveCount(2);
        template.Lines.Select(l => l.Label).Should().Contain("Doanh thu");
    }

    [Fact]
    public void ReportingPeriod_create_sets_periodicity()
    {
        var p = ReportingPeriodDefinition.Create("M", "Hàng tháng", Periodicity.Monthly);
        p.Periodicity.Should().Be(Periodicity.Monthly);
        p.IsActive.Should().BeTrue();
    }
}
