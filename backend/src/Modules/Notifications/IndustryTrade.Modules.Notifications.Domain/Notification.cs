using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.Notifications.Domain;

/// <summary>
/// A notification produced by a domain event arriving through the outbox (docs/design/02 §5).
/// <see cref="Recipient"/> null = broadcast (a shared activity feed); a value targets one user.
/// </summary>
public sealed class Notification : AggregateRoot<Guid>
{
    private Notification() { } // EF

    private Notification(Guid id, string? recipient, string title, string message, string category, string? refId)
        : base(id)
    {
        Recipient = recipient;
        Title = title;
        Message = message;
        Category = category;
        RefId = refId;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public string? Recipient { get; private set; }
    public string Title { get; private set; } = default!;
    public string Message { get; private set; } = default!;
    public string Category { get; private set; } = default!;
    public string? RefId { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public static Notification Create(string title, string message, string category, string? refId = null, string? recipient = null)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(title));
        if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message is required.", nameof(message));
        return new Notification(Guid.NewGuid(), recipient, title.Trim(), message.Trim(), category, refId);
    }

    public void MarkRead() => IsRead = true;
}
