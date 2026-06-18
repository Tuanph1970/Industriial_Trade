using FluentValidation;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Security;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.Reporting.Domain.Submissions;

namespace IndustryTrade.Modules.Reporting.Application.Submissions;

public sealed record CreateSubmissionCommand(Guid CampaignId, Guid OrgUnitId, string Title)
    : ICommand<Guid>, IPermissionAuthorized
{
    public string RequiredPermission => ReportingPermissions.Submit;
}

public sealed class CreateSubmissionValidator : AbstractValidator<CreateSubmissionCommand>
{
    public CreateSubmissionValidator()
    {
        RuleFor(x => x.CampaignId).NotEmpty();
        RuleFor(x => x.OrgUnitId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
    }
}

public sealed class CreateSubmissionHandler(ISubmissionRepository repository, ICurrentUser currentUser)
    : ICommandHandler<CreateSubmissionCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateSubmissionCommand command, CancellationToken ct)
    {
        // Data-scope: a commune official may only create for a unit within their scope.
        if (!currentUser.IsSuperAdmin && !currentUser.DataScopeUnitIds.Contains(command.OrgUnitId))
            return Result.Failure<Guid>(Error.Forbidden("Org unit is outside your data scope."));

        var submission = ReportSubmission.Create(command.CampaignId, command.OrgUnitId, command.Title, currentUser.UserName);
        await repository.AddAsync(submission, ct);
        await repository.SaveChangesAsync(ct);
        return submission.Id;
    }
}

/// <summary>The workflow transitions a caller can request on a submission.</summary>
public enum ReportAction { Submit, AcceptForReview, Return, ForwardForApproval, Approve, Reject, Reopen }

/// <summary>
/// One command for every state-machine transition. <see cref="RequiredPermission"/> is computed from
/// the action, so the right role is enforced for each step by the shared AuthorizationBehavior.
/// </summary>
public sealed record ReportActionCommand(Guid SubmissionId, ReportAction Action, string? Note)
    : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => Action switch
    {
        ReportAction.Submit or ReportAction.Reopen => ReportingPermissions.Submit,
        ReportAction.AcceptForReview or ReportAction.Return or ReportAction.ForwardForApproval => ReportingPermissions.Review,
        ReportAction.Approve or ReportAction.Reject => ReportingPermissions.Approve,
        _ => ReportingPermissions.Read
    };
}

public sealed class ReportActionHandler(ISubmissionRepository repository, ICurrentUser currentUser)
    : ICommandHandler<ReportActionCommand>
{
    public async Task<Result> Handle(ReportActionCommand command, CancellationToken ct)
    {
        var submission = await repository.GetByIdAsync(command.SubmissionId, ct);
        if (submission is null)
            return Result.Failure(Error.NotFound("Report submission"));

        if (!currentUser.IsSuperAdmin && !currentUser.DataScopeUnitIds.Contains(submission.OrgUnitId))
            return Result.Failure(Error.Forbidden("Submission is outside your data scope."));

        var actor = currentUser.UserName;
        switch (command.Action)
        {
            case ReportAction.Submit: submission.Submit(actor); break;
            case ReportAction.AcceptForReview: submission.AcceptForReview(actor); break;
            case ReportAction.Return: submission.ReturnToDraft(actor, command.Note); break;
            case ReportAction.ForwardForApproval: submission.ForwardForApproval(actor); break;
            case ReportAction.Approve: submission.Approve(actor); break;
            case ReportAction.Reject: submission.Reject(actor, command.Note); break;
            case ReportAction.Reopen: submission.Reopen(actor); break;
        }

        await repository.SaveChangesAsync(ct);
        return Result.Success();
    }
}
