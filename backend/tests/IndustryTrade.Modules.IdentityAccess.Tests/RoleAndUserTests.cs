using FluentAssertions;
using IndustryTrade.Modules.IdentityAccess.Domain.Roles;
using IndustryTrade.Modules.IdentityAccess.Domain.Users;
using Xunit;

namespace IndustryTrade.Modules.IdentityAccess.Tests;

public class RoleAndUserTests
{
    [Fact]
    public void Role_create_deduplicates_and_trims_permissions()
    {
        var role = Role.Create("ADMIN", "Admin", ["a.read", " a.read ", "b.manage", ""]);
        role.Permissions.Should().BeEquivalentTo("a.read", "b.manage");
        role.IsActive.Should().BeTrue();
    }

    [Fact]
    public void User_create_assigns_unit_and_roles()
    {
        var roleId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        var user = UserAccount.Create("chuyenvien", "Chuyen Vien", "cv@itrade.gov.vn", unitId, [roleId, roleId]);

        user.UserName.Should().Be("chuyenvien");
        user.OrgUnitId.Should().Be(unitId);
        user.RoleIds.Should().ContainSingle().Which.Should().Be(roleId); // deduped
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void User_assign_unit_changes_data_scope_anchor()
    {
        var user = UserAccount.Create("u", null, null, null, []);
        var newUnit = Guid.NewGuid();
        user.AssignUnit(newUnit);
        user.OrgUnitId.Should().Be(newUnit);
    }
}
