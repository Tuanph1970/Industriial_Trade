namespace IndustryTrade.BuildingBlocks.Domain;

/// <summary>Non-generic access to an entity's domain events (used by the outbox interceptor).</summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}

/// <summary>Base entity with identity and a domain-event buffer.</summary>
public abstract class Entity<TId> : IHasDomainEvents
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    protected Entity(TId id) => Id = id;

    // Required by EF Core materialization.
    protected Entity() => Id = default!;

    public TId Id { get; protected set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}

/// <summary>Marker for aggregate roots — the only entities a repository may load/save directly.</summary>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    protected AggregateRoot(TId id) : base(id) { }
    protected AggregateRoot() { }
}

/// <summary>Audit columns applied to auditable aggregates by the persistence layer.</summary>
public interface IAuditable
{
    DateTime CreatedAtUtc { get; }
    string? CreatedBy { get; }
    DateTime? ModifiedAtUtc { get; }
    string? ModifiedBy { get; }
}
