using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.BuildingBlocks.Infrastructure.Persistence;
using IndustryTrade.Modules.IdentityAccess.Application.Roles;
using IndustryTrade.Modules.IdentityAccess.Application.Users;
using IndustryTrade.Modules.IdentityAccess.Domain.Roles;
using IndustryTrade.Modules.IdentityAccess.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace IndustryTrade.Modules.IdentityAccess.Infrastructure.Persistence;

internal sealed class RoleRepository(IdentityAccessDbContext db) : IRoleRepository
{
    public Task<Role?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Roles.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Role>> ListAsync(Specification<Role> spec, CancellationToken ct) =>
        await SpecificationEvaluator.Apply(db.Roles.AsQueryable(), spec).ToListAsync(ct);

    public Task<int> CountAsync(Specification<Role> spec, CancellationToken ct) =>
        SpecificationEvaluator.Apply(db.Roles.AsQueryable(), spec).CountAsync(ct);

    public async Task AddAsync(Role role, CancellationToken ct) => await db.Roles.AddAsync(role, ct);
    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}

internal sealed class UserRepository(IdentityAccessDbContext db) : IUserRepository
{
    public Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<UserAccount?> GetByUserNameAsync(string userName, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(x => x.UserName == userName, ct);

    public Task<bool> ExistsByUserNameAsync(string userName, CancellationToken ct) =>
        db.Users.AnyAsync(x => x.UserName == userName, ct);

    public async Task<IReadOnlyList<UserAccount>> ListAsync(Specification<UserAccount> spec, CancellationToken ct) =>
        await SpecificationEvaluator.Apply(db.Users.AsQueryable(), spec).ToListAsync(ct);

    public Task<int> CountAsync(Specification<UserAccount> spec, CancellationToken ct) =>
        SpecificationEvaluator.Apply(db.Users.AsQueryable(), spec).CountAsync(ct);

    public async Task AddAsync(UserAccount user, CancellationToken ct) => await db.Users.AddAsync(user, ct);
    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
