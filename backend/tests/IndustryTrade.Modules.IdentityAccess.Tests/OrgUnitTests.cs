using FluentAssertions;
using IndustryTrade.Modules.IdentityAccess.Domain.Organizations;
using Xunit;

namespace IndustryTrade.Modules.IdentityAccess.Tests;

public class OrgUnitTests
{
    [Fact]
    public void Create_root_unit_sets_path_to_its_label_and_raises_event()
    {
        var dept = OrgUnit.Create("SCT-HY", "Sở Công Thương Hưng Yên", OrgUnitType.Department, parent: null);

        dept.Path.Should().Be("SCT_HY");          // non-ltree chars normalized to '_'
        dept.ParentId.Should().BeNull();
        dept.IsActive.Should().BeTrue();
        dept.DomainEvents.Should().ContainSingle(e => e is OrgUnitCreated);
    }

    [Fact]
    public void Create_child_unit_appends_label_to_parent_path()
    {
        var dept = OrgUnit.Create("SCT", "Sở", OrgUnitType.Department, null);
        var commune = OrgUnit.Create("X01", "Xã 01", OrgUnitType.Commune, dept);

        commune.Path.Should().Be("SCT.X01");
        commune.ParentId.Should().Be(dept.Id);
    }

    [Fact]
    public void Deactivate_marks_unit_inactive()
    {
        var dept = OrgUnit.Create("SCT", "Sở", OrgUnitType.Department, null);
        dept.Deactivate();
        dept.IsActive.Should().BeFalse();
    }
}
