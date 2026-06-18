using FluentValidation;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.Catalog.Application.Indicators;
using IndustryTrade.Modules.Catalog.Domain.ReportingPeriods;

namespace IndustryTrade.Modules.Catalog.Application.ReportingPeriods;

public sealed record ReportingPeriodDto(Guid Id, string Code, string Name, Periodicity Periodicity, bool IsActive)
{
    public static ReportingPeriodDto FromEntity(ReportingPeriodDefinition p) =>
        new(p.Id, p.Code, p.Name, p.Periodicity, p.IsActive);
}

public interface IReportingPeriodRepository
{
    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct);
    Task<IReadOnlyList<ReportingPeriodDefinition>> ListAsync(Specification<ReportingPeriodDefinition> spec, CancellationToken ct);
    Task<int> CountAsync(Specification<ReportingPeriodDefinition> spec, CancellationToken ct);
    Task AddAsync(ReportingPeriodDefinition period, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}

public sealed class ReportingPeriodSearchSpec : Specification<ReportingPeriodDefinition>
{
    public ReportingPeriodSearchSpec(PageRequest request, bool forCount = false)
    {
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim().ToLower();
            Where(p => p.Code.ToLower().Contains(kw) || p.Name.ToLower().Contains(kw));
        }
        if (!forCount)
        {
            ApplyOrderBy(p => p.Code);
            ApplyPaging(request.Skip, request.NormalizedPageSize);
        }
    }
}

public sealed record CreateReportingPeriodCommand(string Code, string Name, Periodicity Periodicity)
    : ICommand<Guid>, IPermissionAuthorized
{
    public string RequiredPermission => CatalogPermissions.MasterDataManage;
}

public sealed class CreateReportingPeriodValidator : AbstractValidator<CreateReportingPeriodCommand>
{
    public CreateReportingPeriodValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Periodicity).IsInEnum();
    }
}

public sealed class CreateReportingPeriodHandler(IReportingPeriodRepository repository)
    : ICommandHandler<CreateReportingPeriodCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateReportingPeriodCommand command, CancellationToken ct)
    {
        if (await repository.ExistsByCodeAsync(command.Code, ct))
            return Result.Failure<Guid>(Error.Conflict($"Period code '{command.Code}' already exists."));

        var period = ReportingPeriodDefinition.Create(command.Code, command.Name, command.Periodicity);
        await repository.AddAsync(period, ct);
        await repository.SaveChangesAsync(ct);
        return period.Id;
    }
}

public sealed record GetReportingPeriodsQuery(PageRequest Page) : IQuery<PagedResult<ReportingPeriodDto>>, IPermissionAuthorized
{
    public string RequiredPermission => CatalogPermissions.MasterDataRead;
}

public sealed class GetReportingPeriodsHandler(IReportingPeriodRepository repository)
    : IQueryHandler<GetReportingPeriodsQuery, PagedResult<ReportingPeriodDto>>
{
    public async Task<Result<PagedResult<ReportingPeriodDto>>> Handle(GetReportingPeriodsQuery query, CancellationToken ct)
    {
        var page = query.Page;
        var items = await repository.ListAsync(new ReportingPeriodSearchSpec(page), ct);
        var total = await repository.CountAsync(new ReportingPeriodSearchSpec(page, forCount: true), ct);
        return new PagedResult<ReportingPeriodDto>(items.Select(ReportingPeriodDto.FromEntity).ToList(), total,
            page.NormalizedPage, page.NormalizedPageSize);
    }
}
