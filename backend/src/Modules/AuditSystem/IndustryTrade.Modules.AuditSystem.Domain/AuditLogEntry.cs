using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.AuditSystem.Domain;

/// <summary>An append-only audit record of one user action (design G1, Security Level 3).</summary>
public sealed class AuditLogEntry : AggregateRoot<Guid>
{
    private AuditLogEntry() { } // EF

    private AuditLogEntry(Guid id, string? actor, string action, string payload, bool success, string? error)
        : base(id)
    {
        Actor = actor;
        Action = action;
        Payload = payload;
        Success = success;
        Error = error;
        AtUtc = DateTime.UtcNow;
    }

    public string? Actor { get; private set; }
    public string Action { get; private set; } = default!;
    public string Payload { get; private set; } = default!;
    public bool Success { get; private set; }
    public string? Error { get; private set; }
    public DateTime AtUtc { get; private set; }

    public static AuditLogEntry Create(string? actor, string action, string payload, bool success, string? error) =>
        new(Guid.NewGuid(), actor, action, payload, success, error);
}
