using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.BuildingBlocks.Infrastructure.Persistence;
using IndustryTrade.Modules.Catalog.Application.AdministrativeUnits;
using IndustryTrade.Modules.Catalog.Application.Classifications;
using IndustryTrade.Modules.Catalog.Domain.AdministrativeUnits;
using IndustryTrade.Modules.Catalog.Domain.Classifications;
using Microsoft.EntityFrameworkCore;

namespace IndustryTrade.Modules.Catalog.Infrastructure.Persistence;

internal sealed class AdministrativeUnitRepository(CatalogDbContext db) : IAdministrativeUnitRepository
{
    public Task<bool> ExistsByCodeAsync(string code, CancellationToken ct) =>
        db.AdministrativeUnits.AnyAsync(x => x.Code == code, ct);
    public Task<AdministrativeUnit?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.AdministrativeUnits.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task<IReadOnlyList<AdministrativeUnit>> ListAsync(Specification<AdministrativeUnit> spec, CancellationToken ct) =>
        await SpecificationEvaluator.Apply(db.AdministrativeUnits.AsQueryable(), spec).ToListAsync(ct);
    public Task<int> CountAsync(Specification<AdministrativeUnit> spec, CancellationToken ct) =>
        SpecificationEvaluator.Apply(db.AdministrativeUnits.AsQueryable(), spec).CountAsync(ct);
    public async Task AddAsync(AdministrativeUnit unit, CancellationToken ct) => await db.AdministrativeUnits.AddAsync(unit, ct);
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct) =>
        await db.AdministrativeUnits.Where(x => x.Id == id).ExecuteDeleteAsync(ct) > 0;
    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}

internal sealed class ClassificationRepository(CatalogDbContext db) : IClassificationRepository
{
    public Task<bool> ExistsByCodeAsync(string code, CancellationToken ct) =>
        db.Classifications.AnyAsync(x => x.Code == code, ct);
    public Task<Classification?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Classifications.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task<IReadOnlyList<Classification>> ListAsync(Specification<Classification> spec, CancellationToken ct) =>
        await SpecificationEvaluator.Apply(db.Classifications.AsQueryable(), spec).ToListAsync(ct);
    public Task<int> CountAsync(Specification<Classification> spec, CancellationToken ct) =>
        SpecificationEvaluator.Apply(db.Classifications.AsQueryable(), spec).CountAsync(ct);
    public async Task AddAsync(Classification scheme, CancellationToken ct) => await db.Classifications.AddAsync(scheme, ct);
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct) =>
        await db.Classifications.Where(x => x.Id == id).ExecuteDeleteAsync(ct) > 0;
    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
