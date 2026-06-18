using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.BuildingBlocks.Infrastructure.Persistence;
using IndustryTrade.Modules.AuditSystem.Application;
using IndustryTrade.Modules.AuditSystem.Domain;
using Microsoft.EntityFrameworkCore;

namespace IndustryTrade.Modules.AuditSystem.Infrastructure.Persistence;

internal sealed class AuditLogRepository(AuditDbContext db) : IAuditLogRepository
{
    public async Task<IReadOnlyList<AuditLogEntry>> ListAsync(Specification<AuditLogEntry> spec, CancellationToken ct) =>
        await SpecificationEvaluator.Apply(db.Entries.AsNoTracking(), spec).ToListAsync(ct);

    public Task<int> CountAsync(Specification<AuditLogEntry> spec, CancellationToken ct) =>
        SpecificationEvaluator.Apply(db.Entries.AsNoTracking(), spec).CountAsync(ct);
}
