using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.BuildingBlocks.Infrastructure.Persistence;
using IndustryTrade.Modules.Integration.Application.Services;
using IndustryTrade.Modules.Integration.Application.Status;
using IndustryTrade.Modules.Integration.Domain.Services;
using IndustryTrade.Modules.Integration.Domain.Status;
using Microsoft.EntityFrameworkCore;

namespace IndustryTrade.Modules.Integration.Infrastructure.Persistence;

internal sealed class DataSharingServiceRepository(IntegrationDbContext db) : IDataSharingServiceRepository
{
    public Task<DataSharingService?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Services.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<bool> ExistsByCodeAsync(string code, CancellationToken ct) =>
        db.Services.AnyAsync(x => x.Code == code, ct);

    public async Task<IReadOnlyList<DataSharingService>> ListAsync(Specification<DataSharingService> spec, CancellationToken ct) =>
        await SpecificationEvaluator.Apply(db.Services.AsQueryable(), spec).ToListAsync(ct);

    public Task<int> CountAsync(Specification<DataSharingService> spec, CancellationToken ct) =>
        SpecificationEvaluator.Apply(db.Services.AsQueryable(), spec).CountAsync(ct);

    public async Task<IReadOnlyList<DataSharingService>> GetPublishedAsync(CancellationToken ct) =>
        await db.Services.AsNoTracking().Where(x => x.Status == ServiceStatus.Published).ToListAsync(ct);

    public async Task AddAsync(DataSharingService service, CancellationToken ct) => await db.Services.AddAsync(service, ct);
    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}

internal sealed class ConnectionStatusStore(IntegrationDbContext db) : IConnectionStatusStore
{
    public async Task RecordAsync(IEnumerable<ConnectionStatusCheck> checks, CancellationToken ct)
    {
        await db.StatusChecks.AddRangeAsync(checks, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ConnectionStatusCheck>> ListAsync(Specification<ConnectionStatusCheck> spec, CancellationToken ct) =>
        await SpecificationEvaluator.Apply(db.StatusChecks.AsNoTracking(), spec).ToListAsync(ct);

    public Task<int> CountAsync(Specification<ConnectionStatusCheck> spec, CancellationToken ct) =>
        SpecificationEvaluator.Apply(db.StatusChecks.AsNoTracking(), spec).CountAsync(ct);
}

internal sealed class ConnectionProbe(IntegrationDbContext db) : IConnectionProbe
{
    public async Task<bool> PingDatabaseAsync(CancellationToken ct)
    {
        try { return await db.Database.CanConnectAsync(ct); }
        catch { return false; }
    }
}
