using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.Modules.Notifications.Domain;

namespace IndustryTrade.Modules.Notifications.Application;

public static class NotificationsPermissions
{
    public const string Read = "notifications.read";
}

public sealed record NotificationDto(
    Guid Id, string Title, string Message, string Category, string? RefId, bool IsRead, DateTime CreatedAtUtc)
{
    public static NotificationDto FromEntity(Notification n) =>
        new(n.Id, n.Title, n.Message, n.Category, n.RefId, n.IsRead, n.CreatedAtUtc);
}

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Notification>> ListAsync(Specification<Notification> spec, CancellationToken ct);
    Task<int> CountAsync(Specification<Notification> spec, CancellationToken ct);
    Task<IReadOnlyList<Notification>> ListUnreadAsync(CancellationToken ct);
    Task AddAsync(Notification notification, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}

public sealed class NotificationSearchSpec : Specification<Notification>
{
    public NotificationSearchSpec(PageRequest request, bool unreadOnly, bool forCount = false)
    {
        if (unreadOnly)
            Where(n => !n.IsRead);

        if (!forCount)
        {
            ApplyOrderByDescending(n => n.CreatedAtUtc);
            ApplyPaging(request.Skip, request.NormalizedPageSize);
        }
    }
}
