using IndustryTrade.BuildingBlocks.Application.Auditing;
using IndustryTrade.Modules.AuditSystem.Domain;
using IndustryTrade.Modules.AuditSystem.Infrastructure.Persistence;

namespace IndustryTrade.Modules.AuditSystem.Infrastructure;

/// <summary>Persists audit entries to the <c>audit</c> schema. Implements the BuildingBlocks sink
/// the AuditBehavior writes to.</summary>
internal sealed class AuditSink(AuditDbContext db) : IAuditSink
{
    public async Task WriteAsync(AuditEntry entry, CancellationToken ct)
    {
        db.Entries.Add(AuditLogEntry.Create(entry.Actor, entry.Action, entry.Payload, entry.Success, entry.Error));
        await db.SaveChangesAsync(ct);
    }
}
