using FluentValidation;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.Catalog.Application.Indicators;
using IndustryTrade.Modules.Catalog.Domain.IndicatorSets;

namespace IndustryTrade.Modules.Catalog.Application.IndicatorSets;

public sealed record IndicatorSetDto(Guid Id, string Code, string Name, string? Description, Guid[] IndicatorIds, bool IsActive)
{
    public static IndicatorSetDto FromEntity(IndicatorSet s) =>
        new(s.Id, s.Code, s.Name, s.Description, s.IndicatorIds, s.IsActive);
}

public interface IIndicatorSetRepository
{
    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct);
    Task<IReadOnlyList<IndicatorSet>> ListAsync(Specification<IndicatorSet> spec, CancellationToken ct);
    Task<int> CountAsync(Specification<IndicatorSet> spec, CancellationToken ct);
    Task AddAsync(IndicatorSet set, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}

public sealed class IndicatorSetSearchSpec : Specification<IndicatorSet>
{
    public IndicatorSetSearchSpec(PageRequest request, bool forCount = false)
    {
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim().ToLower();
            Where(s => s.Code.ToLower().Contains(kw) || s.Name.ToLower().Contains(kw));
        }
        if (!forCount)
        {
            ApplyOrderBy(s => s.Code);
            ApplyPaging(request.Skip, request.NormalizedPageSize);
        }
    }
}

public sealed record CreateIndicatorSetCommand(string Code, string Name, string? Description, Guid[] IndicatorIds)
    : ICommand<Guid>, IPermissionAuthorized
{
    public string RequiredPermission => CatalogPermissions.MasterDataManage;
}

public sealed class CreateIndicatorSetValidator : AbstractValidator<CreateIndicatorSetCommand>
{
    public CreateIndicatorSetValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
    }
}

public sealed class CreateIndicatorSetHandler(IIndicatorSetRepository repository)
    : ICommandHandler<CreateIndicatorSetCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateIndicatorSetCommand command, CancellationToken ct)
    {
        if (await repository.ExistsByCodeAsync(command.Code, ct))
            return Result.Failure<Guid>(Error.Conflict($"Indicator set code '{command.Code}' already exists."));

        var set = IndicatorSet.Create(command.Code, command.Name, command.Description, command.IndicatorIds ?? []);
        await repository.AddAsync(set, ct);
        await repository.SaveChangesAsync(ct);
        return set.Id;
    }
}

public sealed record GetIndicatorSetsQuery(PageRequest Page) : IQuery<PagedResult<IndicatorSetDto>>, IPermissionAuthorized
{
    public string RequiredPermission => CatalogPermissions.MasterDataRead;
}

public sealed class GetIndicatorSetsHandler(IIndicatorSetRepository repository)
    : IQueryHandler<GetIndicatorSetsQuery, PagedResult<IndicatorSetDto>>
{
    public async Task<Result<PagedResult<IndicatorSetDto>>> Handle(GetIndicatorSetsQuery query, CancellationToken ct)
    {
        var page = query.Page;
        var items = await repository.ListAsync(new IndicatorSetSearchSpec(page), ct);
        var total = await repository.CountAsync(new IndicatorSetSearchSpec(page, forCount: true), ct);
        return new PagedResult<IndicatorSetDto>(items.Select(IndicatorSetDto.FromEntity).ToList(), total,
            page.NormalizedPage, page.NormalizedPageSize);
    }
}
