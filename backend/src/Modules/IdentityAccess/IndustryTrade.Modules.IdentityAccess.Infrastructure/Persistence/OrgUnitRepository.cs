using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.BuildingBlocks.Infrastructure.Persistence;
using IndustryTrade.Modules.IdentityAccess.Application.Organizations;
using IndustryTrade.Modules.IdentityAccess.Domain.Organizations;
using Microsoft.EntityFrameworkCore;

namespace IndustryTrade.Modules.IdentityAccess.Infrastructure.Persistence;

internal sealed class OrgUnitRepository(IdentityAccessDbContext db) : IOrgUnitRepository
{
    public Task<OrgUnit?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.OrgUnits.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<OrgUnit>> ListAsync(Specification<OrgUnit> spec, CancellationToken ct) =>
        await SpecificationEvaluator.Apply(db.OrgUnits.AsQueryable(), spec).ToListAsync(ct);

    public Task<int> CountAsync(Specification<OrgUnit> spec, CancellationToken ct) =>
        SpecificationEvaluator.Apply(db.OrgUnits.AsQueryable(), spec).CountAsync(ct);

    public async Task AddAsync(OrgUnit unit, CancellationToken ct) =>
        await db.OrgUnits.AddAsync(unit, ct);

    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
