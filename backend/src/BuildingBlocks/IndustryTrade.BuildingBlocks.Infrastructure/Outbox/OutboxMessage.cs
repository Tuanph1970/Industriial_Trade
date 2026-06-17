namespace IndustryTrade.BuildingBlocks.Infrastructure.Outbox;

/// <summary>
/// Transactional outbox row. Domain events are persisted here in the same transaction as the
/// aggregate change; the Worker drains them to RabbitMQ (no lost events). See docs/design/02 §5, 04 §3.7.
/// Each module owns its own outbox table in its schema.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTime OccurredOnUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
}
