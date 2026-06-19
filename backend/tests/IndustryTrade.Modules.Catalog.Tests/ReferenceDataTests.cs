using FluentAssertions;
using IndustryTrade.Modules.Catalog.Domain.AdministrativeUnits;
using IndustryTrade.Modules.Catalog.Domain.Classifications;
using Xunit;

namespace IndustryTrade.Modules.Catalog.Tests;

public class ReferenceDataTests
{
    [Fact]
    public void AdministrativeUnit_create_then_update_changes_fields()
    {
        var province = AdministrativeUnit.Create("HY", "Hưng Yên", AdministrativeLevel.Province, null);
        province.IsActive.Should().BeTrue();
        province.ParentId.Should().BeNull();

        var parent = Guid.NewGuid();
        province.Update("Hưng Yên (mới)", AdministrativeLevel.Commune, parent, isActive: false);

        province.Name.Should().Be("Hưng Yên (mới)");
        province.Level.Should().Be(AdministrativeLevel.Commune);
        province.ParentId.Should().Be(parent);
        province.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Classification_keeps_items_and_skips_blank_ones()
    {
        var scheme = Classification.Create("LH", "Loại hình KD", "mô tả",
        [
            ("01", "Hộ kinh doanh", 1),
            ("02", "Doanh nghiệp", 2),
            ("", "  ", 3),   // blank → skipped
        ]);

        scheme.Items.Should().HaveCount(2);
        scheme.Items.Select(i => i.Code).Should().BeEquivalentTo(["01", "02"]);
        scheme.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Classification_update_replaces_items()
    {
        var scheme = Classification.Create("LH", "Loại hình", null, [("01", "A", 1)]);
        scheme.Update("Loại hình KD", "desc", [("02", "B", 1), ("03", "C", 2)]);

        scheme.Name.Should().Be("Loại hình KD");
        scheme.Items.Should().HaveCount(2);
        scheme.Items.Select(i => i.Code).Should().NotContain("01");
    }
}
