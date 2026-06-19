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
    Task AddAsync(Notification notification, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}

/// <summary>
/// Lists notifications visible to the caller. Audience rule: broadcasts (no target) are visible to all;
/// targeted ones require the caller to hold <c>TargetPermission</c> and (when set) have the
/// notification's <c>OrgUnitId</c> in their data-scope. Pass <paramref name="audiencePermissions"/> =
/// null for an unrestricted view (super-admin).
/// </summary>
public sealed class NotificationSearchSpec : Specification<Notification>
{
    public NotificationSearchSpec(PageRequest request, bool unreadOnly,
        string[]? audiencePermissions = null, Guid[]? scopeUnitIds = null, bool forCount = false)
    {
        if (audiencePermissions is not null)
        {
            if (scopeUnitIds is null)
                Where(n => n.TargetPermission == null || audiencePermissions.Contains(n.TargetPermission));
            else
                Where(n => n.TargetPermission == null
                    || (audiencePermissions.Contains(n.TargetPermission)
                        && (n.OrgUnitId == null || scopeUnitIds.Contains(n.OrgUnitId.Value))));
        }

        if (unreadOnly)
            Where(n => !n.IsRead);

        if (!forCount)
        {
            ApplyOrderByDescending(n => n.CreatedAtUtc);
            ApplyPaging(request.Skip, request.NormalizedPageSize);
        }
    }
}
