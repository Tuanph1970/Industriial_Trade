using FluentValidation;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.IdentityAccess.Domain.Organizations;

namespace IndustryTrade.Modules.IdentityAccess.Application.Organizations;

public sealed record CreateOrgUnitCommand(string Code, string Name, OrgUnitType Type, Guid? ParentId)
    : ICommand<Guid>, IPermissionAuthorized
{
    public string RequiredPermission => IdentityPermissions.OrgUnitsManage;
}

public sealed class CreateOrgUnitValidator : AbstractValidator<CreateOrgUnitCommand>
{
    public CreateOrgUnitValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Type).IsInEnum();
    }
}

public sealed class CreateOrgUnitHandler(IOrgUnitRepository repository)
    : ICommandHandler<CreateOrgUnitCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateOrgUnitCommand command, CancellationToken ct)
    {
        OrgUnit? parent = null;
        if (command.ParentId is { } parentId)
        {
            parent = await repository.GetByIdAsync(parentId, ct);
            if (parent is null)
                return Result.Failure<Guid>(Error.NotFound("Parent org unit"));
        }

        var unit = OrgUnit.Create(command.Code, command.Name, command.Type, parent);
        await repository.AddAsync(unit, ct);
        await repository.SaveChangesAsync(ct);

        return unit.Id;
    }
}
