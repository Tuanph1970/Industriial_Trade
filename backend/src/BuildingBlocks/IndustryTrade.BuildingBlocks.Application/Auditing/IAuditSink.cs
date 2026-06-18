namespace IndustryTrade.BuildingBlocks.Application.Auditing;

/// <summary>One audited action (a command execution). Persisted by the AuditSystem context.</summary>
public sealed record AuditEntry(string? Actor, string Action, string Payload, bool Success, string? Error);

/// <summary>
/// Append-only audit log sink. Implemented by the AuditSystem context; written to by the
/// <c>AuditBehavior</c> for every command (design G1 — record all user actions, incl. deletes).
/// </summary>
public interface IAuditSink
{
    Task WriteAsync(AuditEntry entry, CancellationToken ct);
}
