using FluentValidation;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Security;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.Reporting.Application.Campaigns;
using IndustryTrade.Modules.Reporting.Domain.Submissions;

namespace IndustryTrade.Modules.Reporting.Application.Submissions;

/// <summary>A report line assembled from a template indicator + the matching observation value.</summary>
public sealed record ExtractedReportLine(
    Guid IndicatorId, string IndicatorCode, string Label, int RowOrder, decimal? Value, string? ValueText);

/// <summary>
/// ACL over Catalog (report templates) and SectorData (observations): given a template and a unit/period,
/// assemble the report's content lines. Implemented as a read-only cross-schema query so Reporting
/// keeps no compile-time coupling to those contexts (same trade-off as Analytics — docs/design/03 §6).
/// </summary>
public interface IReportContentSource
{
    Task<IReadOnlyList<ExtractedReportLine>> ExtractAsync(
        Guid templateId, Guid orgUnitId, int periodYear, int? periodMonth, CancellationToken ct);
}

/// <summary>
/// Auto-extracts a submission's content from a Catalog report template + the unit's observations for
/// the campaign's period, then binds it to the submission (UC: auto-extract reports from observations).
/// </summary>
public sealed record ExtractSubmissionContentCommand(Guid SubmissionId, Guid TemplateId)
    : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => ReportingPermissions.Submit;
}

public sealed class ExtractSubmissionContentValidator : AbstractValidator<ExtractSubmissionContentCommand>
{
    public ExtractSubmissionContentValidator()
    {
        RuleFor(x => x.SubmissionId).NotEmpty();
        RuleFor(x => x.TemplateId).NotEmpty();
    }
}

public sealed class ExtractSubmissionContentHandler(
    ISubmissionRepository submissions, ICampaignRepository campaigns, IReportContentSource source, ICurrentUser currentUser)
    : ICommandHandler<ExtractSubmissionContentCommand>
{
    public async Task<Result> Handle(ExtractSubmissionContentCommand command, CancellationToken ct)
    {
        var submission = await submissions.GetByIdAsync(command.SubmissionId, ct);
        if (submission is null)
            return Result.Failure(Error.NotFound("Report submission"));

        if (!currentUser.IsSuperAdmin && !currentUser.DataScopeUnitIds.Contains(submission.OrgUnitId))
            return Result.Failure(Error.Forbidden("Submission is outside your data scope."));

        var campaign = await campaigns.GetByIdAsync(submission.CampaignId, ct);
        if (campaign is null)
            return Result.Failure(Error.NotFound("Campaign"));

        var extracted = await source.ExtractAsync(
            command.TemplateId, submission.OrgUnitId, campaign.PeriodYear, campaign.PeriodMonth, ct);

        submission.SetContent(command.TemplateId, extracted.Select(e => new ReportLine
        {
            IndicatorId = e.IndicatorId, IndicatorCode = e.IndicatorCode, Label = e.Label,
            RowOrder = e.RowOrder, Value = e.Value, ValueText = e.ValueText,
        }));
        await submissions.SaveChangesAsync(ct);
        return Result.Success();
    }
}
