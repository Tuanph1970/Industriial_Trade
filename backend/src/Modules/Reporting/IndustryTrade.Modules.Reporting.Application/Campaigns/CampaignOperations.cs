using FluentValidation;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.Reporting.Domain.Campaigns;

namespace IndustryTrade.Modules.Reporting.Application.Campaigns;

public sealed record CampaignDto(
    Guid Id, string Code, string Name, int PeriodYear, int? PeriodMonth, DateOnly? Deadline, CampaignStatus Status)
{
    public static CampaignDto FromEntity(ReportingCampaign c) =>
        new(c.Id, c.Code, c.Name, c.PeriodYear, c.PeriodMonth, c.Deadline, c.Status);
}

public interface ICampaignRepository
{
    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct);
    Task<ReportingCampaign?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<ReportingCampaign>> ListAsync(Specification<ReportingCampaign> spec, CancellationToken ct);
    Task<int> CountAsync(Specification<ReportingCampaign> spec, CancellationToken ct);
    Task AddAsync(ReportingCampaign campaign, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}

public sealed class CampaignSearchSpec : Specification<ReportingCampaign>
{
    public CampaignSearchSpec(PageRequest request, bool forCount = false)
    {
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim().ToLower();
            Where(c => c.Code.ToLower().Contains(kw) || c.Name.ToLower().Contains(kw));
        }
        if (!forCount)
        {
            ApplyOrderByDescending(c => c.PeriodYear);
            ApplyPaging(request.Skip, request.NormalizedPageSize);
        }
    }
}

public sealed record CreateCampaignCommand(string Code, string Name, int PeriodYear, int? PeriodMonth, DateOnly? Deadline)
    : ICommand<Guid>, IPermissionAuthorized
{
    public string RequiredPermission => ReportingPermissions.CampaignsManage;
}

public sealed class CreateCampaignValidator : AbstractValidator<CreateCampaignCommand>
{
    public CreateCampaignValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.PeriodYear).InclusiveBetween(2000, 2100);
    }
}

public sealed class CreateCampaignHandler(ICampaignRepository repository) : ICommandHandler<CreateCampaignCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCampaignCommand command, CancellationToken ct)
    {
        if (await repository.ExistsByCodeAsync(command.Code, ct))
            return Result.Failure<Guid>(Error.Conflict($"Campaign code '{command.Code}' already exists."));

        var campaign = ReportingCampaign.Create(command.Code, command.Name, command.PeriodYear, command.PeriodMonth, command.Deadline);
        await repository.AddAsync(campaign, ct);
        await repository.SaveChangesAsync(ct);
        return campaign.Id;
    }
}

public sealed record GetCampaignsQuery(PageRequest Page) : IQuery<PagedResult<CampaignDto>>, IPermissionAuthorized
{
    public string RequiredPermission => ReportingPermissions.Read;
}

public sealed class GetCampaignsHandler(ICampaignRepository repository)
    : IQueryHandler<GetCampaignsQuery, PagedResult<CampaignDto>>
{
    public async Task<Result<PagedResult<CampaignDto>>> Handle(GetCampaignsQuery query, CancellationToken ct)
    {
        var page = query.Page;
        var items = await repository.ListAsync(new CampaignSearchSpec(page), ct);
        var total = await repository.CountAsync(new CampaignSearchSpec(page, forCount: true), ct);
        return new PagedResult<CampaignDto>(items.Select(CampaignDto.FromEntity).ToList(), total,
            page.NormalizedPage, page.NormalizedPageSize);
    }
}
