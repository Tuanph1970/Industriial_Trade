using FluentValidation;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.Catalog.Application.Indicators;
using IndustryTrade.Modules.Catalog.Domain.ReportTemplates;

namespace IndustryTrade.Modules.Catalog.Application.ReportTemplates;

public sealed record TemplateLineDto(Guid IndicatorId, string Label, int RowOrder);

public sealed record ReportTemplateDto(
    Guid Id, string Code, string Name, string? Description, IReadOnlyList<TemplateLineDto> Lines, bool IsActive)
{
    public static ReportTemplateDto FromEntity(ReportTemplate t) => new(
        t.Id, t.Code, t.Name, t.Description,
        t.Lines.OrderBy(l => l.RowOrder).Select(l => new TemplateLineDto(l.IndicatorId, l.Label, l.RowOrder)).ToList(),
        t.IsActive);
}

public interface IReportTemplateRepository
{
    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct);
    Task<IReadOnlyList<ReportTemplate>> ListAsync(Specification<ReportTemplate> spec, CancellationToken ct);
    Task<int> CountAsync(Specification<ReportTemplate> spec, CancellationToken ct);
    Task AddAsync(ReportTemplate template, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}

public sealed class ReportTemplateSearchSpec : Specification<ReportTemplate>
{
    public ReportTemplateSearchSpec(PageRequest request, bool forCount = false)
    {
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim().ToLower();
            Where(t => t.Code.ToLower().Contains(kw) || t.Name.ToLower().Contains(kw));
        }
        if (!forCount)
        {
            ApplyOrderBy(t => t.Code);
            ApplyPaging(request.Skip, request.NormalizedPageSize);
        }
    }
}

public sealed record TemplateLineInput(Guid IndicatorId, string Label, int RowOrder);

public sealed record CreateReportTemplateCommand(string Code, string Name, string? Description, TemplateLineInput[] Lines)
    : ICommand<Guid>, IPermissionAuthorized
{
    public string RequiredPermission => CatalogPermissions.MasterDataManage;
}

public sealed class CreateReportTemplateValidator : AbstractValidator<CreateReportTemplateCommand>
{
    public CreateReportTemplateValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleForEach(x => x.Lines).ChildRules(l =>
        {
            l.RuleFor(x => x.IndicatorId).NotEmpty();
            l.RuleFor(x => x.Label).NotEmpty().MaximumLength(250);
        });
    }
}

public sealed class CreateReportTemplateHandler(IReportTemplateRepository repository)
    : ICommandHandler<CreateReportTemplateCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateReportTemplateCommand command, CancellationToken ct)
    {
        if (await repository.ExistsByCodeAsync(command.Code, ct))
            return Result.Failure<Guid>(Error.Conflict($"Template code '{command.Code}' already exists."));

        var template = ReportTemplate.Create(command.Code, command.Name, command.Description,
            (command.Lines ?? []).Select(l => (l.IndicatorId, l.Label, l.RowOrder)));
        await repository.AddAsync(template, ct);
        await repository.SaveChangesAsync(ct);
        return template.Id;
    }
}

public sealed record GetReportTemplatesQuery(PageRequest Page) : IQuery<PagedResult<ReportTemplateDto>>, IPermissionAuthorized
{
    public string RequiredPermission => CatalogPermissions.MasterDataRead;
}

public sealed class GetReportTemplatesHandler(IReportTemplateRepository repository)
    : IQueryHandler<GetReportTemplatesQuery, PagedResult<ReportTemplateDto>>
{
    public async Task<Result<PagedResult<ReportTemplateDto>>> Handle(GetReportTemplatesQuery query, CancellationToken ct)
    {
        var page = query.Page;
        var items = await repository.ListAsync(new ReportTemplateSearchSpec(page), ct);
        var total = await repository.CountAsync(new ReportTemplateSearchSpec(page, forCount: true), ct);
        return new PagedResult<ReportTemplateDto>(items.Select(ReportTemplateDto.FromEntity).ToList(), total,
            page.NormalizedPage, page.NormalizedPageSize);
    }
}

public sealed record DeleteReportTemplateCommand(Guid Id) : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => CatalogPermissions.MasterDataManage;
}

public sealed class DeleteReportTemplateHandler(IReportTemplateRepository repository) : ICommandHandler<DeleteReportTemplateCommand>
{
    public async Task<Result> Handle(DeleteReportTemplateCommand command, CancellationToken ct) =>
        await repository.DeleteAsync(command.Id, ct) ? Result.Success() : Result.Failure(Error.NotFound("Report template"));
}
