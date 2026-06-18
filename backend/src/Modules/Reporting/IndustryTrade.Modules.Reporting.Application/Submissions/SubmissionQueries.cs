using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Security;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.Reporting.Domain.Submissions;

namespace IndustryTrade.Modules.Reporting.Application.Submissions;

public sealed record GetSubmissionsQuery(PageRequest Page, ReportState? State, Guid? CampaignId)
    : IQuery<PagedResult<SubmissionDto>>, IPermissionAuthorized
{
    public string RequiredPermission => ReportingPermissions.Read;
}

public sealed class GetSubmissionsHandler(ISubmissionRepository repository, ICurrentUser currentUser)
    : IQueryHandler<GetSubmissionsQuery, PagedResult<SubmissionDto>>
{
    public async Task<Result<PagedResult<SubmissionDto>>> Handle(GetSubmissionsQuery query, CancellationToken ct)
    {
        var page = query.Page;
        var scope = currentUser.IsSuperAdmin ? null : currentUser.DataScopeUnitIds.ToArray();
        var items = await repository.ListAsync(new SubmissionSearchSpec(page, scope, query.State, query.CampaignId), ct);
        var total = await repository.CountAsync(new SubmissionSearchSpec(page, scope, query.State, query.CampaignId, forCount: true), ct);
        return new PagedResult<SubmissionDto>(items.Select(SubmissionDto.FromEntity).ToList(), total,
            page.NormalizedPage, page.NormalizedPageSize);
    }
}

public sealed record GetSubmissionDetailQuery(Guid Id) : IQuery<SubmissionDetailDto>, IPermissionAuthorized
{
    public string RequiredPermission => ReportingPermissions.Read;
}

public sealed class GetSubmissionDetailHandler(ISubmissionRepository repository, ICurrentUser currentUser)
    : IQueryHandler<GetSubmissionDetailQuery, SubmissionDetailDto>
{
    public async Task<Result<SubmissionDetailDto>> Handle(GetSubmissionDetailQuery query, CancellationToken ct)
    {
        var submission = await repository.GetByIdAsync(query.Id, ct);
        if (submission is null)
            return Result.Failure<SubmissionDetailDto>(Error.NotFound("Report submission"));

        if (!currentUser.IsSuperAdmin && !currentUser.DataScopeUnitIds.Contains(submission.OrgUnitId))
            return Result.Failure<SubmissionDetailDto>(Error.Forbidden("Submission is outside your data scope."));

        return SubmissionDetailDto.FromEntity(submission);
    }
}
