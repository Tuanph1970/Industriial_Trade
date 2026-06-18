using FluentValidation;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.IdentityAccess.Application.Organizations;
using IndustryTrade.Modules.IdentityAccess.Domain.Roles;

namespace IndustryTrade.Modules.IdentityAccess.Application.Roles;

public sealed record CreateRoleCommand(string Code, string Name, string[] Permissions)
    : ICommand<Guid>, IPermissionAuthorized
{
    public string RequiredPermission => IdentityPermissions.RolesManage;
}

public sealed class CreateRoleValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}

public sealed class CreateRoleHandler(IRoleRepository repository) : ICommandHandler<CreateRoleCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateRoleCommand command, CancellationToken ct)
    {
        var role = Role.Create(command.Code, command.Name, command.Permissions ?? []);
        await repository.AddAsync(role, ct);
        await repository.SaveChangesAsync(ct);
        return role.Id;
    }
}

public sealed record DeleteRoleCommand(Guid Id) : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => IdentityPermissions.RolesManage;
}

public sealed class DeleteRoleHandler(IRoleRepository repository) : ICommandHandler<DeleteRoleCommand>
{
    public async Task<Result> Handle(DeleteRoleCommand command, CancellationToken ct) =>
        await repository.DeleteAsync(command.Id, ct) ? Result.Success() : Result.Failure(Error.NotFound("Role"));
}
