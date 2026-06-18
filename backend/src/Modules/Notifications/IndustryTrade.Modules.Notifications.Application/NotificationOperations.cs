using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.Notifications.Application;

public sealed record GetNotificationsQuery(PageRequest Page, bool UnreadOnly)
    : IQuery<PagedResult<NotificationDto>>, IPermissionAuthorized
{
    public string RequiredPermission => NotificationsPermissions.Read;
}

public sealed class GetNotificationsHandler(INotificationRepository repository)
    : IQueryHandler<GetNotificationsQuery, PagedResult<NotificationDto>>
{
    public async Task<Result<PagedResult<NotificationDto>>> Handle(GetNotificationsQuery query, CancellationToken ct)
    {
        var page = query.Page;
        var items = await repository.ListAsync(new NotificationSearchSpec(page, query.UnreadOnly), ct);
        var total = await repository.CountAsync(new NotificationSearchSpec(page, query.UnreadOnly, forCount: true), ct);
        return new PagedResult<NotificationDto>(items.Select(NotificationDto.FromEntity).ToList(), total,
            page.NormalizedPage, page.NormalizedPageSize);
    }
}

public sealed record GetUnreadCountQuery : IQuery<int>, IPermissionAuthorized
{
    public string RequiredPermission => NotificationsPermissions.Read;
}

public sealed class GetUnreadCountHandler(INotificationRepository repository)
    : IQueryHandler<GetUnreadCountQuery, int>
{
    public async Task<Result<int>> Handle(GetUnreadCountQuery query, CancellationToken ct) =>
        await repository.CountAsync(new NotificationSearchSpec(new PageRequest(), unreadOnly: true, forCount: true), ct);
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

public sealed class MarkAllNotificationsReadHandler(INotificationRepository repository)
    : ICommandHandler<MarkAllNotificationsReadCommand>
{
    public async Task<Result> Handle(MarkAllNotificationsReadCommand command, CancellationToken ct)
    {
        foreach (var notification in await repository.ListUnreadAsync(ct))
            notification.MarkRead();
        await repository.SaveChangesAsync(ct);
        return Result.Success();
    }
}
