using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.Modules.Reporting.Domain.Submissions;

namespace IndustryTrade.Modules.Reporting.Application.Submissions;

public sealed record SubmissionDto(
    Guid Id, Guid CampaignId, Guid OrgUnitId, string Title, ReportState State, DateTime CreatedAtUtc)
{
    public static SubmissionDto FromEntity(ReportSubmission s) =>
        new(s.Id, s.CampaignId, s.OrgUnitId, s.Title, s.State, s.CreatedAtUtc);
}

public sealed record TransitionDto(ReportState FromState, ReportState ToState, string Action, string? ActorName, DateTime AtUtc, string? Note);

public sealed record SubmissionDetailDto(
    Guid Id, Guid CampaignId, Guid OrgUnitId, string Title, ReportState State,
    DateTime CreatedAtUtc, IReadOnlyList<TransitionDto> History)
{
    public static SubmissionDetailDto FromEntity(ReportSubmission s) => new(
        s.Id, s.CampaignId, s.OrgUnitId, s.Title, s.State, s.CreatedAtUtc,
        s.History.OrderBy(h => h.AtUtc)
            .Select(h => new TransitionDto(h.FromState, h.ToState, h.Action, h.ActorName, h.AtUtc, h.Note))
            .ToList());
}

public interface ISubmissionRepository
{
    Task<ReportSubmission?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<ReportSubmission>> ListAsync(Specification<ReportSubmission> spec, CancellationToken ct);
    Task<int> CountAsync(Specification<ReportSubmission> spec, CancellationToken ct);
    Task AddAsync(ReportSubmission submission, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}

public sealed class SubmissionSearchSpec : Specification<ReportSubmission>
{
    public SubmissionSearchSpec(PageRequest request, Guid[]? scopeUnitIds,
        ReportState? state = null, Guid? campaignId = null, bool forCount = false)
    {
        if (scopeUnitIds is not null)
            Where(s => scopeUnitIds.Contains(s.OrgUnitId));
        if (state is { } st)
            Where(s => s.State == st);
        if (campaignId is { } cid)
            Where(s => s.CampaignId == cid);

        if (!forCount)
        {
            ApplyOrderByDescending(s => s.CreatedAtUtc);
            ApplyPaging(request.Skip, request.NormalizedPageSize);
        }
    }
}
