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
