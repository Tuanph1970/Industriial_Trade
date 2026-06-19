using FluentValidation;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.Catalog.Application.Indicators;
using IndustryTrade.Modules.Catalog.Domain.AdministrativeUnits;

namespace IndustryTrade.Modules.Catalog.Application.AdministrativeUnits;

public sealed record AdministrativeUnitDto(
    Guid Id, string Code, string Name, AdministrativeLevel Level, Guid? ParentId, bool IsActive)
{
    public static AdministrativeUnitDto FromEntity(AdministrativeUnit u) =>
        new(u.Id, u.Code, u.Name, u.Level, u.ParentId, u.IsActive);
}

public interface IAdministrativeUnitRepository
{
    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct);
    Task<AdministrativeUnit?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<AdministrativeUnit>> ListAsync(Specification<AdministrativeUnit> spec, CancellationToken ct);
    Task<int> CountAsync(Specification<AdministrativeUnit> spec, CancellationToken ct);
    Task AddAsync(AdministrativeUnit unit, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}

public sealed class AdministrativeUnitSearchSpec : Specification<AdministrativeUnit>
{
    public AdministrativeUnitSearchSpec(PageRequest request, AdministrativeLevel? level = null, bool forCount = false)
    {
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim().ToLower();
            Where(u => u.Code.ToLower().Contains(kw) || u.Name.ToLower().Contains(kw));
        }
        if (level is { } l)
            Where(u => u.Level == l);

        if (!forCount)
        {
            ApplyOrderBy(u => u.Code);
            ApplyPaging(request.Skip, request.NormalizedPageSize);
        }
    }
}

// ── Create ──────────────────────────────────────────────────────────────────
public sealed record CreateAdministrativeUnitCommand(
    string Code, string Name, AdministrativeLevel Level, Guid? ParentId)
    : ICommand<Guid>, IPermissionAuthorized
{
    public string RequiredPermission => CatalogPermissions.MasterDataManage;
}

public sealed class CreateAdministrativeUnitValidator : AbstractValidator<CreateAdministrativeUnitCommand>
{
    public CreateAdministrativeUnitValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Level).IsInEnum();
    }
}

public sealed class CreateAdministrativeUnitHandler(IAdministrativeUnitRepository repository)
    : ICommandHandler<CreateAdministrativeUnitCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateAdministrativeUnitCommand command, CancellationToken ct)
    {
        if (await repository.ExistsByCodeAsync(command.Code, ct))
            return Result.Failure<Guid>(Error.Conflict($"Administrative-unit code '{command.Code}' already exists."));

        var unit = AdministrativeUnit.Create(command.Code, command.Name, command.Level, command.ParentId);
        await repository.AddAsync(unit, ct);
        await repository.SaveChangesAsync(ct);
        return unit.Id;
    }
}

// ── List ────────────────────────────────────────────────────────────────────
public sealed record GetAdministrativeUnitsQuery(PageRequest Page, AdministrativeLevel? Level)
    : IQuery<PagedResult<AdministrativeUnitDto>>, IPermissionAuthorized
{
    public string RequiredPermission => CatalogPermissions.MasterDataRead;
}

public sealed class GetAdministrativeUnitsHandler(IAdministrativeUnitRepository repository)
    : IQueryHandler<GetAdministrativeUnitsQuery, PagedResult<AdministrativeUnitDto>>
{
    public async Task<Result<PagedResult<AdministrativeUnitDto>>> Handle(GetAdministrativeUnitsQuery query, CancellationToken ct)
    {
        var page = query.Page;
        var items = await repository.ListAsync(new AdministrativeUnitSearchSpec(page, query.Level), ct);
        var total = await repository.CountAsync(new AdministrativeUnitSearchSpec(page, query.Level, forCount: true), ct);
        return new PagedResult<AdministrativeUnitDto>(items.Select(AdministrativeUnitDto.FromEntity).ToList(), total,
            page.NormalizedPage, page.NormalizedPageSize);
    }
}

// ── Update ──────────────────────────────────────────────────────────────────
public sealed record UpdateAdministrativeUnitCommand(
    Guid Id, string Name, AdministrativeLevel Level, Guid? ParentId, bool IsActive)
    : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => CatalogPermissions.MasterDataManage;
}

public sealed class UpdateAdministrativeUnitValidator : AbstractValidator<UpdateAdministrativeUnitCommand>
{
    public UpdateAdministrativeUnitValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Level).IsInEnum();
    }
}

public sealed class UpdateAdministrativeUnitHandler(IAdministrativeUnitRepository repository)
    : ICommandHandler<UpdateAdministrativeUnitCommand>
{
    public async Task<Result> Handle(UpdateAdministrativeUnitCommand command, CancellationToken ct)
    {
        var unit = await repository.GetByIdAsync(command.Id, ct);
        if (unit is null) return Result.Failure(Error.NotFound("Administrative unit"));

        unit.Update(command.Name, command.Level, command.ParentId, command.IsActive);
        await repository.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ── Delete ──────────────────────────────────────────────────────────────────
public sealed record DeleteAdministrativeUnitCommand(Guid Id) : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => CatalogPermissions.MasterDataManage;
}

public sealed class DeleteAdministrativeUnitHandler(IAdministrativeUnitRepository repository)
    : ICommandHandler<DeleteAdministrativeUnitCommand>
{
    public async Task<Result> Handle(DeleteAdministrativeUnitCommand command, CancellationToken ct) =>
        await repository.DeleteAsync(command.Id, ct)
            ? Result.Success() : Result.Failure(Error.NotFound("Administrative unit"));
}
