using FluentValidation;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.IdentityAccess.Application.Organizations;
using IndustryTrade.Modules.IdentityAccess.Domain.Users;

namespace IndustryTrade.Modules.IdentityAccess.Application.Users;

public sealed record CreateUserCommand(
    string UserName, string? FullName, string? Email, Guid? OrgUnitId, Guid[] RoleIds)
    : ICommand<Guid>, IPermissionAuthorized
{
    public string RequiredPermission => IdentityPermissions.UsersManage;
}

public sealed class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}

public sealed class CreateUserHandler(IUserRepository repository) : ICommandHandler<CreateUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand command, CancellationToken ct)
    {
        if (await repository.ExistsByUserNameAsync(command.UserName, ct))
            return Result.Failure<Guid>(Error.Conflict($"User '{command.UserName}' already exists."));

        var user = UserAccount.Create(
            command.UserName, command.FullName, command.Email, command.OrgUnitId, command.RoleIds ?? []);
        await repository.AddAsync(user, ct);
        await repository.SaveChangesAsync(ct);
        return user.Id;
    }
}

public sealed record DeleteUserCommand(Guid Id) : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => IdentityPermissions.UsersManage;
}

public sealed class DeleteUserHandler(IUserRepository repository) : ICommandHandler<DeleteUserCommand>
{
    public async Task<Result> Handle(DeleteUserCommand command, CancellationToken ct) =>
        await repository.DeleteAsync(command.Id, ct) ? Result.Success() : Result.Failure(Error.NotFound("User"));
}

public sealed record UpdateUserCommand(
    Guid Id, string? FullName, string? Email, Guid? OrgUnitId, Guid[] RoleIds, bool IsActive)
    : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => IdentityPermissions.UsersManage;
}

public sealed class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}

public sealed class UpdateUserHandler(IUserRepository repository) : ICommandHandler<UpdateUserCommand>
{
    public async Task<Result> Handle(UpdateUserCommand command, CancellationToken ct)
    {
        var user = await repository.GetByIdAsync(command.Id, ct);
        if (user is null) return Result.Failure(Error.NotFound("User"));

        user.Update(command.FullName, command.Email, command.OrgUnitId, command.RoleIds ?? [], command.IsActive);
        await repository.SaveChangesAsync(ct);
        return Result.Success();
    }
}
