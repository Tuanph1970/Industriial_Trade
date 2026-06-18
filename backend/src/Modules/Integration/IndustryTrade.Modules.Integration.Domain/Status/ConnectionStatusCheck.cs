using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.Integration.Domain.Status;

/// <summary>
/// One recorded connection-status check (design F2). Level 1 = system component (e.g. database),
/// Level 2 = a registered data-sharing service. History is retained for ≥ 3 months.
/// </summary>
public sealed class ConnectionStatusCheck : AggregateRoot<Guid>
{
    private ConnectionStatusCheck() { } // EF

    private ConnectionStatusCheck(Guid id, string component, int level, bool healthy, string? detail) : base(id)
    {
        Component = component;
        Level = level;
        Healthy = healthy;
        Detail = detail;
        CheckedAtUtc = DateTime.UtcNow;
    }

    public string Component { get; private set; } = default!;
    public int Level { get; private set; }
    public bool Healthy { get; private set; }
    public string? Detail { get; private set; }
    public DateTime CheckedAtUtc { get; private set; }

    public static ConnectionStatusCheck Record(string component, int level, bool healthy, string? detail = null) =>
        new(Guid.NewGuid(), component, level, healthy, detail);
}
