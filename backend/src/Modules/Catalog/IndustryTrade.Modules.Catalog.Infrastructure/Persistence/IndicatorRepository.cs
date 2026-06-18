using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.BuildingBlocks.Infrastructure.Persistence;
using IndustryTrade.Modules.Catalog.Application.Indicators;
using IndustryTrade.Modules.Catalog.Domain.Indicators;
using Microsoft.EntityFrameworkCore;

namespace IndustryTrade.Modules.Catalog.Infrastructure.Persistence;

internal sealed class IndicatorRepository(CatalogDbContext db) : IIndicatorRepository
{
    public Task<Indicator?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Indicators.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<bool> ExistsByCodeAsync(string code, CancellationToken ct) =>
        db.Indicators.AnyAsync(x => x.Code == code, ct);

    public async Task<IReadOnlyList<Indicator>> ListAsync(Specification<Indicator> spec, CancellationToken ct) =>
        await SpecificationEvaluator.Apply(db.Indicators.AsQueryable(), spec).ToListAsync(ct);

    public Task<int> CountAsync(Specification<Indicator> spec, CancellationToken ct) =>
        SpecificationEvaluator.Apply(db.Indicators.AsQueryable(), spec).CountAsync(ct);

    public async Task AddAsync(Indicator indicator, CancellationToken ct) => await db.Indicators.AddAsync(indicator, ct);
    public void Remove(Indicator indicator) => db.Indicators.Remove(indicator);
    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
