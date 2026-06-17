namespace IndustryTrade.BuildingBlocks.Domain;

/// <summary>A fact that happened in the domain. Published via the outbox after commit.</summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOnUtc { get; }
}

public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}
