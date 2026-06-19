using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.Modules.Notifications.Domain;
using IndustryTrade.Modules.Reporting.Domain.Submissions;
using MediatR;

namespace IndustryTrade.Modules.Notifications.Application;

/// <summary>
/// Reacts to the Reporting context's <see cref="ReportStateChanged"/> domain event (delivered via the
/// outbox processor) and records a notification. This is the cross-context saga step from
/// docs/design/02 §5 / 03 §5 — kept in-process via MediatR notifications.
/// </summary>
public sealed class ReportStateChangedNotificationHandler(INotificationRepository repository)
    : INotificationHandler<DomainEventNotification<ReportStateChanged>>
{
    public async Task Handle(DomainEventNotification<ReportStateChanged> wrapper, CancellationToken ct)
    {
        var e = wrapper.DomainEvent;
        var notification = Notification.Create(
            title: $"Báo cáo: {e.Action}",
            message: $"Báo cáo {e.SubmissionId} chuyển trạng thái {e.From} → {e.To}.",
            category: "reporting",
            refId: e.SubmissionId.ToString(),
            targetPermission: TargetFor(e.Action),
            orgUnitId: e.OrgUnitId);

        await repository.AddAsync(notification, ct);
        await repository.SaveChangesAsync(ct);
    }

    // Route each transition to the role that should act/be informed next, within the unit's data-scope.
    // Permission codes mirror Reporting's (kept as literals to avoid coupling to that module's Application).
    private static string TargetFor(string action) => action switch
    {
        "Submit" => "reporting.review",              // specialist reviews next
        "ForwardForApproval" => "reporting.approve", // division leader approves next
        _ => "reporting.submit",                     // back to / informs the commune submitter
    };
}
