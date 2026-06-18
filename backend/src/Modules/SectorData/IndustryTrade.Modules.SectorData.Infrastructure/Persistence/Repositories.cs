using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.BuildingBlocks.Infrastructure.Persistence;
using IndustryTrade.Modules.SectorData.Application.Clusters;
using IndustryTrade.Modules.SectorData.Application.Observations;
using IndustryTrade.Modules.SectorData.Domain.Clusters;
using IndustryTrade.Modules.SectorData.Domain.Observations;
using Microsoft.EntityFrameworkCore;

namespace IndustryTrade.Modules.SectorData.Infrastructure.Persistence;

internal sealed class ObservationRepository(SectorDataDbContext db) : IObservationRepository
{
    public async Task<IReadOnlyList<IndicatorObservation>> ListAsync(
        Specification<IndicatorObservation> spec, CancellationToken ct) =>
        await SpecificationEvaluator.Apply(db.Observations.AsQueryable(), spec).ToListAsync(ct);

    public Task<int> CountAsync(Specification<IndicatorObservation> spec, CancellationToken ct) =>
        SpecificationEvaluator.Apply(db.Observations.AsQueryable(), spec).CountAsync(ct);

    public async Task AddAsync(IndicatorObservation observation, CancellationToken ct) =>
        await db.Observations.AddAsync(observation, ct);

    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}

internal sealed class ClusterRepository(SectorDataDbContext db) : IClusterRepository
{
    public Task<bool> ExistsByCodeAsync(string code, CancellationToken ct) =>
        db.Clusters.AnyAsync(x => x.Code == code, ct);

    public async Task<IReadOnlyList<IndustrialCluster>> ListAsync(
        Specification<IndustrialCluster> spec, CancellationToken ct) =>
        await SpecificationEvaluator.Apply(db.Clusters.AsQueryable(), spec).ToListAsync(ct);

    public Task<int> CountAsync(Specification<IndustrialCluster> spec, CancellationToken ct) =>
        SpecificationEvaluator.Apply(db.Clusters.AsQueryable(), spec).CountAsync(ct);

    public async Task AddAsync(IndustrialCluster cluster, CancellationToken ct) =>
        await db.Clusters.AddAsync(cluster, ct);

    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
