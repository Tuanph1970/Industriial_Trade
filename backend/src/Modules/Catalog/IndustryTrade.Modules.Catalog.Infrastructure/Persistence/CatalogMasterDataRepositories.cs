using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.BuildingBlocks.Infrastructure.Persistence;
using IndustryTrade.Modules.Catalog.Application.IndicatorSets;
using IndustryTrade.Modules.Catalog.Application.ReportingPeriods;
using IndustryTrade.Modules.Catalog.Application.ReportTemplates;
using IndustryTrade.Modules.Catalog.Domain.IndicatorSets;
using IndustryTrade.Modules.Catalog.Domain.ReportingPeriods;
using IndustryTrade.Modules.Catalog.Domain.ReportTemplates;
using Microsoft.EntityFrameworkCore;

namespace IndustryTrade.Modules.Catalog.Infrastructure.Persistence;

internal sealed class IndicatorSetRepository(CatalogDbContext db) : IIndicatorSetRepository
{
    public Task<bool> ExistsByCodeAsync(string code, CancellationToken ct) => db.IndicatorSets.AnyAsync(x => x.Code == code, ct);
    public async Task<IReadOnlyList<IndicatorSet>> ListAsync(Specification<IndicatorSet> spec, CancellationToken ct) =>
        await SpecificationEvaluator.Apply(db.IndicatorSets.AsQueryable(), spec).ToListAsync(ct);
    public Task<int> CountAsync(Specification<IndicatorSet> spec, CancellationToken ct) =>
        SpecificationEvaluator.Apply(db.IndicatorSets.AsQueryable(), spec).CountAsync(ct);
    public Task<IndicatorSet?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.IndicatorSets.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddAsync(IndicatorSet set, CancellationToken ct) => await db.IndicatorSets.AddAsync(set, ct);
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct) =>
        await db.IndicatorSets.Where(x => x.Id == id).ExecuteDeleteAsync(ct) > 0;
    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}

internal sealed class ReportTemplateRepository(CatalogDbContext db) : IReportTemplateRepository
{
    public Task<bool> ExistsByCodeAsync(string code, CancellationToken ct) => db.ReportTemplates.AnyAsync(x => x.Code == code, ct);
    public async Task<IReadOnlyList<ReportTemplate>> ListAsync(Specification<ReportTemplate> spec, CancellationToken ct) =>
        await SpecificationEvaluator.Apply(db.ReportTemplates.AsQueryable(), spec).ToListAsync(ct);
    public Task<int> CountAsync(Specification<ReportTemplate> spec, CancellationToken ct) =>
        SpecificationEvaluator.Apply(db.ReportTemplates.AsQueryable(), spec).CountAsync(ct);
    public Task<ReportTemplate?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.ReportTemplates.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddAsync(ReportTemplate template, CancellationToken ct) => await db.ReportTemplates.AddAsync(template, ct);
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct) =>
        await db.ReportTemplates.Where(x => x.Id == id).ExecuteDeleteAsync(ct) > 0;
    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}

internal sealed class ReportingPeriodRepository(CatalogDbContext db) : IReportingPeriodRepository
{
    public Task<bool> ExistsByCodeAsync(string code, CancellationToken ct) => db.ReportingPeriods.AnyAsync(x => x.Code == code, ct);
    public async Task<IReadOnlyList<ReportingPeriodDefinition>> ListAsync(Specification<ReportingPeriodDefinition> spec, CancellationToken ct) =>
        await SpecificationEvaluator.Apply(db.ReportingPeriods.AsQueryable(), spec).ToListAsync(ct);
    public Task<int> CountAsync(Specification<ReportingPeriodDefinition> spec, CancellationToken ct) =>
        SpecificationEvaluator.Apply(db.ReportingPeriods.AsQueryable(), spec).CountAsync(ct);
    public Task<ReportingPeriodDefinition?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.ReportingPeriods.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddAsync(ReportingPeriodDefinition period, CancellationToken ct) => await db.ReportingPeriods.AddAsync(period, ct);
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct) =>
        await db.ReportingPeriods.Where(x => x.Id == id).ExecuteDeleteAsync(ct) > 0;
    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
