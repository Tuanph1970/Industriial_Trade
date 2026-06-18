using FluentValidation;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.IdentityAccess.Application.Organizations;

// ── Update ────────────────────────────────────────────────────────────────
public sealed record UpdateOrgUnitCommand(Guid Id, string Name, bool IsActive)
    : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => IdentityPermissions.OrgUnitsManage;
}

public sealed class UpdateOrgUnitValidator : AbstractValidator<UpdateOrgUnitCommand>
{
    public UpdateOrgUnitValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
    }
}

public sealed class UpdateOrgUnitHandler(IOrgUnitRepository repository) : ICommandHandler<UpdateOrgUnitCommand>
{
    public async Task<Result> Handle(UpdateOrgUnitCommand command, CancellationToken ct)
    {
        var unit = await repository.GetByIdAsync(command.Id, ct);
        if (unit is null)
            return Result.Failure(Error.NotFound("Org unit"));

        unit.Update(command.Name, command.IsActive);
        await repository.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ── Delete ────────────────────────────────────────────────────────────────
public sealed record DeleteOrgUnitCommand(Guid Id) : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => IdentityPermissions.OrgUnitsManage;
}

public sealed class DeleteOrgUnitHandler(IOrgUnitRepository repository) : ICommandHandler<DeleteOrgUnitCommand>
{
    public async Task<Result> Handle(DeleteOrgUnitCommand command, CancellationToken ct)
    {
        var unit = await repository.GetByIdAsync(command.Id, ct);
        if (unit is null)
            return Result.Failure(Error.NotFound("Org unit"));

        if (await repository.HasChildrenAsync(command.Id, ct))
            return Result.Failure(Error.Conflict("Cannot delete a unit that has child units."));

        repository.Remove(unit);
        await repository.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ── Detail ────────────────────────────────────────────────────────────────
public sealed record GetOrgUnitByIdQuery(Guid Id) : IQuery<OrgUnitDto>, IPermissionAuthorized
{
    public string RequiredPermission => IdentityPermissions.OrgUnitsRead;
}

public sealed class GetOrgUnitByIdHandler(IOrgUnitRepository repository) : IQueryHandler<GetOrgUnitByIdQuery, OrgUnitDto>
{
    public async Task<Result<OrgUnitDto>> Handle(GetOrgUnitByIdQuery query, CancellationToken ct)
    {
        var unit = await repository.GetByIdAsync(query.Id, ct);
        return unit is null
            ? Result.Failure<OrgUnitDto>(Error.NotFound("Org unit"))
            : OrgUnitDto.FromEntity(unit);
    }
}
