using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.BuildingBlocks.Infrastructure.Persistence;
using IndustryTrade.Modules.SectorData.Application.Clusters;
using IndustryTrade.Modules.SectorData.Application.CommerceLocations;
using IndustryTrade.Modules.SectorData.Application.Ecommerce;
using IndustryTrade.Modules.SectorData.Application.Observations;
using IndustryTrade.Modules.SectorData.Application.PetroleumStations;
using IndustryTrade.Modules.SectorData.Application.Violations;
using IndustryTrade.Modules.SectorData.Domain.Clusters;
using IndustryTrade.Modules.SectorData.Domain.CommerceLocations;
using IndustryTrade.Modules.SectorData.Domain.Ecommerce;
using IndustryTrade.Modules.SectorData.Domain.Observations;
using IndustryTrade.Modules.SectorData.Domain.PetroleumStations;
using IndustryTrade.Modules.SectorData.Domain.Violations;
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

internal sealed class ViolationRepository(SectorDataDbContext db) : IViolationRepository
{
    public Task<bool> ExistsByCaseNoAsync(string caseNo, CancellationToken ct) =>
        db.Violations.AnyAsync(x => x.CaseNo == caseNo, ct);

    public async Task<IReadOnlyList<MarketViolationCase>> ListAsync(
        Specification<MarketViolationCase> spec, CancellationToken ct) =>
        await SpecificationEvaluator.Apply(db.Violations.AsQueryable(), spec).ToListAsync(ct);

    public Task<int> CountAsync(Specification<MarketViolationCase> spec, CancellationToken ct) =>
        SpecificationEvaluator.Apply(db.Violations.AsQueryable(), spec).CountAsync(ct);

    public async Task AddAsync(MarketViolationCase violation, CancellationToken ct) =>
        await db.Violations.AddAsync(violation, ct);

    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}

internal sealed class PetrolStationRepository(SectorDataDbContext db) : IPetrolStationRepository
{
    public Task<bool> ExistsByCodeAsync(string code, CancellationToken ct) =>
        db.PetroleumStations.AnyAsync(x => x.Code == code, ct);

    public async Task<IReadOnlyList<PetroleumStation>> ListAsync(Specification<PetroleumStation> spec, CancellationToken ct) =>
        await SpecificationEvaluator.Apply(db.PetroleumStations.AsQueryable(), spec).ToListAsync(ct);

    public Task<int> CountAsync(Specification<PetroleumStation> spec, CancellationToken ct) =>
        SpecificationEvaluator.Apply(db.PetroleumStations.AsQueryable(), spec).CountAsync(ct);

    public async Task AddAsync(PetroleumStation station, CancellationToken ct) =>
        await db.PetroleumStations.AddAsync(station, ct);

    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}

internal sealed class CommerceLocationRepository(SectorDataDbContext db) : ICommerceLocationRepository
{
    public Task<bool> ExistsByCodeAsync(string code, CancellationToken ct) =>
        db.CommerceLocations.AnyAsync(x => x.Code == code, ct);

    public async Task<IReadOnlyList<CommerceLocation>> ListAsync(Specification<CommerceLocation> spec, CancellationToken ct) =>
        await SpecificationEvaluator.Apply(db.CommerceLocations.AsQueryable(), spec).ToListAsync(ct);

    public Task<int> CountAsync(Specification<CommerceLocation> spec, CancellationToken ct) =>
        SpecificationEvaluator.Apply(db.CommerceLocations.AsQueryable(), spec).CountAsync(ct);

    public async Task AddAsync(CommerceLocation location, CancellationToken ct) =>
        await db.CommerceLocations.AddAsync(location, ct);

    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}

internal sealed class EcommerceParticipantRepository(SectorDataDbContext db) : IEcommerceParticipantRepository
{
    public Task<bool> ExistsByTaxCodeAsync(string taxCode, CancellationToken ct) =>
        db.EcommerceParticipants.AnyAsync(x => x.TaxCode == taxCode, ct);

    public async Task<IReadOnlyList<EcommerceParticipant>> ListAsync(Specification<EcommerceParticipant> spec, CancellationToken ct) =>
        await SpecificationEvaluator.Apply(db.EcommerceParticipants.AsQueryable(), spec).ToListAsync(ct);

    public Task<int> CountAsync(Specification<EcommerceParticipant> spec, CancellationToken ct) =>
        SpecificationEvaluator.Apply(db.EcommerceParticipants.AsQueryable(), spec).CountAsync(ct);

    public async Task AddAsync(EcommerceParticipant participant, CancellationToken ct) =>
        await db.EcommerceParticipants.AddAsync(participant, ct);

    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
