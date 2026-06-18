using IndustryTrade.Modules.IdentityAccess.Application.Organizations;
using IndustryTrade.Modules.IdentityAccess.Domain.Organizations;
using IndustryTrade.Modules.IdentityAccess.Domain.Roles;
using IndustryTrade.Modules.IdentityAccess.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace IndustryTrade.Modules.IdentityAccess.Infrastructure.Persistence;

/// <summary>Development-only seed data so the Keycloak demo users map to real DB users + data-scope.</summary>
public static class IdentityAccessSeeder
{
    public static async Task SeedAsync(IdentityAccessDbContext db, CancellationToken ct = default)
    {
        if (await db.Users.AnyAsync(ct))
            return;

        var root = OrgUnit.Create("SCT_HY", "Sở Công Thương Hưng Yên", OrgUnitType.Department, parent: null);
        var commune = OrgUnit.Create("X01", "Xã Demo 01", OrgUnitType.Commune, parent: root);
        db.OrgUnits.AddRange(root, commune);

        var adminRole = Role.Create("ADMIN", "Quản trị hệ thống", IdentityPermissions.All);
        var specialistRole = Role.Create("SPECIALIST", "Chuyên viên",
            [
                IdentityPermissions.OrgUnitsRead, IdentityPermissions.OrgUnitsManage, IdentityPermissions.UsersRead,
                "catalog.indicators.read", "catalog.indicators.manage",
                "sector.observations.read", "sector.observations.manage",
                "sector.clusters.read", "sector.clusters.manage",
                "sector.violations.read", "sector.violations.manage"
            ]);
        db.Roles.AddRange(adminRole, specialistRole);

        // Usernames match the Keycloak realm users (deploy/keycloak/realm-industry-trade.json).
        var superadmin = UserAccount.Create("superadmin", "Super Admin", null, root.Id, [adminRole.Id]);
        var chuyenvien = UserAccount.Create("chuyenvien", "Chuyên viên Demo", null, commune.Id, [specialistRole.Id]);
        db.Users.AddRange(superadmin, chuyenvien);

        await db.SaveChangesAsync(ct);
    }
}
