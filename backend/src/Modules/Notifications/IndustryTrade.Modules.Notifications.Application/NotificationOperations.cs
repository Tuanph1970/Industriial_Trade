using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Security;
using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.Notifications.Application;

/// <summary>Resolves the caller's notification audience: super-admin sees all (null), others are
/// filtered to broadcasts + notifications addressed to a permission they hold, within their scope.</summary>
internal static class NotificationAudience
{
    public static (string[]? Permissions, Guid[]? ScopeUnitIds) For(ICurrentUser user) =>
        user.IsSuperAdmin ? (null, null) : (user.Permissions.ToArray(), user.DataScopeUnitIds.ToArray());
}

public sealed record GetNotificationsQuery(PageRequest Page, bool UnreadOnly)
    : IQuery<PagedResult<NotificationDto>>, IPermissionAuthorized
{
    public string RequiredPermission => NotificationsPermissions.Read;
}

public sealed class GetNotificationsHandler(INotificationRepository repository, ICurrentUser currentUser)
    : IQueryHandler<GetNotificationsQuery, PagedResult<NotificationDto>>
{
    public async Task<Result<PagedResult<NotificationDto>>> Handle(GetNotificationsQuery query, CancellationToken ct)
    {
        var page = query.Page;
        var (perms, scope) = NotificationAudience.For(currentUser);
        var items = await repository.ListAsync(new NotificationSearchSpec(page, query.UnreadOnly, perms, scope), ct);
        var total = await repository.CountAsync(new NotificationSearchSpec(page, query.UnreadOnly, perms, scope, forCount: true), ct);
        return new PagedResult<NotificationDto>(items.Select(NotificationDto.FromEntity).ToList(), total,
            page.NormalizedPage, page.NormalizedPageSize);
    }
}

public sealed record GetUnreadCountQuery : IQuery<int>, IPermissionAuthorized
{
    public string RequiredPermission => NotificationsPermissions.Read;
}

public sealed class GetUnreadCountHandler(INotificationRepository repository, ICurrentUser currentUser)
    : IQueryHandler<GetUnreadCountQuery, int>
{
    public async Task<Result<int>> Handle(GetUnreadCountQuery query, CancellationToken ct)
    {
        var (perms, scope) = NotificationAudience.For(currentUser);
        return await repository.CountAsync(
            new NotificationSearchSpec(new PageRequest(), unreadOnly: true, perms, scope, forCount: true), ct);
    }
}

public sealed record MarkNotificationReadCommand(Guid Id) : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => NotificationsPermissions.Read;
}

public sealed class MarkNotificationReadHandler(INotificationRepository repository)
    : ICommandHandler<MarkNotificationReadCommand>
{
    public async Task<Result> Handle(MarkNotificationReadCommand command, CancellationToken ct)
    {
        var notification = await repository.GetByIdAsync(command.Id, ct);
        if (notification is null)
            return Result.Failure(Error.NotFound("Notification"));

        notification.MarkRead();
        await repository.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public sealed record MarkAllNotificationsReadCommand : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => NotificationsPermissions.Read;
}

public sealed class MarkAllNotificationsReadHandler(INotificationRepository repository, ICurrentUser currentUser)
    : ICommandHandler<MarkAllNotificationsReadCommand>
{
    public async Task<Result> Handle(MarkAllNotificationsReadCommand command, CancellationToken ct)
    {
        var (perms, scope) = NotificationAudience.For(currentUser);
        // forCount:true → all matching unread for this audience, unpaged.
        var unread = await repository.ListAsync(
            new NotificationSearchSpec(new PageRequest(), unreadOnly: true, perms, scope, forCount: true), ct);
        foreach (var notification in unread)
            notification.MarkRead();
        await repository.SaveChangesAsync(ct);
        return Result.Success();
    }
}
