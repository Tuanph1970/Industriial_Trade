using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.Notifications.Domain;

/// <summary>
/// A notification produced by a domain event arriving through the outbox (docs/design/02 §5).
/// Addressed by audience rather than a single username: <see cref="TargetPermission"/> null =
/// broadcast (everyone); a value routes it to whoever holds that permission, and (when
/// <see cref="OrgUnitId"/> is set) within their data-scope. Read-time filtering uses the caller's own
/// claims, so no cross-context user lookup is needed.
/// </summary>
public sealed class Notification : AggregateRoot<Guid>
{
    private Notification() { } // EF

    private Notification(Guid id, string title, string message, string category, string? refId,
        string? targetPermission, Guid? orgUnitId) : base(id)
    {
        Title = title;
        Message = message;
        Category = category;
        RefId = refId;
        TargetPermission = targetPermission;
        OrgUnitId = orgUnitId;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public string Title { get; private set; } = default!;
    public string Message { get; private set; } = default!;
    public string Category { get; private set; } = default!;
    public string? RefId { get; private set; }
    /// <summary>The permission a recipient must hold to see this; null = broadcast to everyone.</summary>
    public string? TargetPermission { get; private set; }
    /// <summary>When set, only recipients whose data-scope covers this unit see it.</summary>
    public Guid? OrgUnitId { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public static Notification Create(string title, string message, string category,
        string? refId = null, string? targetPermission = null, Guid? orgUnitId = null)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(title));
        if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message is required.", nameof(message));
        return new Notification(Guid.NewGuid(), title.Trim(), message.Trim(), category, refId, targetPermission, orgUnitId);
    }

    public void MarkRead() => IsRead = true;
}
