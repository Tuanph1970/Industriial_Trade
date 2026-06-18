using IndustryTrade.Modules.IdentityAccess.Application.Users;
using Microsoft.EntityFrameworkCore;

namespace IndustryTrade.Modules.IdentityAccess.Infrastructure.Persistence;

internal sealed class UserAuthorizationProvider(IdentityAccessDbContext db) : IUserAuthorizationProvider
{
    public async Task<UserAuthorization> GetByUserNameAsync(string userName, CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserName == userName && u.IsActive, ct);

        if (user is null)
            return new UserAuthorization(false, [], [], []);

        // Function-scope: union of permissions across the user's roles.
        var rolePermissionSets = await db.Roles.AsNoTracking()
            .Where(r => user.RoleIds.Contains(r.Id))
            .Select(r => r.Permissions)
            .ToListAsync(ct);
        var permissions = rolePermissionSets.SelectMany(p => p).Distinct().ToArray();

        // Data-scope: the user's assigned org unit and all its descendants (the unit's subtree).
        var scopePaths = new List<string>();
        var scopeUnitIds = new List<Guid>();
        if (user.OrgUnitId is { } unitId)
        {
            var path = await db.OrgUnits.AsNoTracking()
                .Where(o => o.Id == unitId)
                .Select(o => o.Path)
                .FirstOrDefaultAsync(ct);

            if (path is not null)
            {
                scopePaths.Add(path);
                scopeUnitIds = await db.OrgUnits.AsNoTracking()
                    .Where(o => o.Path == path || o.Path.StartsWith(path + "."))
                    .Select(o => o.Id)
                    .ToListAsync(ct);
            }
        }

        return new UserAuthorization(true, permissions, scopePaths, scopeUnitIds);
    }
}
