using FluentValidation;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.Catalog.Application.Indicators;
using IndustryTrade.Modules.Catalog.Domain.Classifications;

namespace IndustryTrade.Modules.Catalog.Application.Classifications;

public sealed record ClassificationItemDto(string Code, string Name, int SortOrder);

public sealed record ClassificationDto(
    Guid Id, string Code, string Name, string? Description, IReadOnlyList<ClassificationItemDto> Items, bool IsActive)
{
    public static ClassificationDto FromEntity(Classification c) => new(
        c.Id, c.Code, c.Name, c.Description,
        c.Items.OrderBy(i => i.SortOrder).Select(i => new ClassificationItemDto(i.Code, i.Name, i.SortOrder)).ToList(),
        c.IsActive);
}

public interface IClassificationRepository
{
    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct);
    Task<Classification?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Classification>> ListAsync(Specification<Classification> spec, CancellationToken ct);
    Task<int> CountAsync(Specification<Classification> spec, CancellationToken ct);
    Task AddAsync(Classification scheme, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}

public sealed class ClassificationSearchSpec : Specification<Classification>
{
    public ClassificationSearchSpec(PageRequest request, bool forCount = false)
    {
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim().ToLower();
            Where(c => c.Code.ToLower().Contains(kw) || c.Name.ToLower().Contains(kw));
        }
        if (!forCount)
        {
            ApplyOrderBy(c => c.Code);
            ApplyPaging(request.Skip, request.NormalizedPageSize);
        }
    }
}

public sealed record ClassificationItemInput(string Code, string Name, int SortOrder);

// ── Create ──────────────────────────────────────────────────────────────────
public sealed record CreateClassificationCommand(
    string Code, string Name, string? Description, ClassificationItemInput[] Items)
    : ICommand<Guid>, IPermissionAuthorized
{
    public string RequiredPermission => CatalogPermissions.MasterDataManage;
}

public sealed class CreateClassificationValidator : AbstractValidator<CreateClassificationCommand>
{
    public CreateClassificationValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleForEach(x => x.Items).ChildRules(i =>
        {
            i.RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            i.RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        });
    }
}

public sealed class CreateClassificationHandler(IClassificationRepository repository)
    : ICommandHandler<CreateClassificationCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateClassificationCommand command, CancellationToken ct)
    {
        if (await repository.ExistsByCodeAsync(command.Code, ct))
            return Result.Failure<Guid>(Error.Conflict($"Classification code '{command.Code}' already exists."));

        var scheme = Classification.Create(command.Code, command.Name, command.Description,
            (command.Items ?? []).Select(i => (i.Code, i.Name, i.SortOrder)));
        await repository.AddAsync(scheme, ct);
        await repository.SaveChangesAsync(ct);
        return scheme.Id;
    }
}

// ── List ────────────────────────────────────────────────────────────────────
public sealed record GetClassificationsQuery(PageRequest Page)
    : IQuery<PagedResult<ClassificationDto>>, IPermissionAuthorized
{
    public string RequiredPermission => CatalogPermissions.MasterDataRead;
}

public sealed class GetClassificationsHandler(IClassificationRepository repository)
    : IQueryHandler<GetClassificationsQuery, PagedResult<ClassificationDto>>
{
    public async Task<Result<PagedResult<ClassificationDto>>> Handle(GetClassificationsQuery query, CancellationToken ct)
    {
        var page = query.Page;
        var items = await repository.ListAsync(new ClassificationSearchSpec(page), ct);
        var total = await repository.CountAsync(new ClassificationSearchSpec(page, forCount: true), ct);
        return new PagedResult<ClassificationDto>(items.Select(ClassificationDto.FromEntity).ToList(), total,
            page.NormalizedPage, page.NormalizedPageSize);
    }
}

// ── Update ──────────────────────────────────────────────────────────────────
public sealed record UpdateClassificationCommand(
    Guid Id, string Name, string? Description, ClassificationItemInput[] Items)
    : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => CatalogPermissions.MasterDataManage;
}

public sealed class UpdateClassificationValidator : AbstractValidator<UpdateClassificationCommand>
{
    public UpdateClassificationValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleForEach(x => x.Items).ChildRules(i =>
        {
            i.RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            i.RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        });
    }
}

public sealed class UpdateClassificationHandler(IClassificationRepository repository)
    : ICommandHandler<UpdateClassificationCommand>
{
    public async Task<Result> Handle(UpdateClassificationCommand command, CancellationToken ct)
    {
        var scheme = await repository.GetByIdAsync(command.Id, ct);
        if (scheme is null) return Result.Failure(Error.NotFound("Classification"));

        scheme.Update(command.Name, command.Description,
            (command.Items ?? []).Select(i => (i.Code, i.Name, i.SortOrder)));
        await repository.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ── Delete ──────────────────────────────────────────────────────────────────
public sealed record DeleteClassificationCommand(Guid Id) : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => CatalogPermissions.MasterDataManage;
}

public sealed class DeleteClassificationHandler(IClassificationRepository repository)
    : ICommandHandler<DeleteClassificationCommand>
{
    public async Task<Result> Handle(DeleteClassificationCommand command, CancellationToken ct) =>
        await repository.DeleteAsync(command.Id, ct)
            ? Result.Success() : Result.Failure(Error.NotFound("Classification"));
}
