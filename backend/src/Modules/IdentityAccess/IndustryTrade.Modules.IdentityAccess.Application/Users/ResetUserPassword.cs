using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.IdentityAccess.Application.Organizations;

namespace IndustryTrade.Modules.IdentityAccess.Application.Users;

/// <summary>
/// Resets a user's Keycloak password to the configured default (temporary — they must change it at
/// next login). Returns the new password so an admin can hand it over. Audited like any command.
/// </summary>
public sealed record ResetUserPasswordCommand(Guid Id) : ICommand<string>, IPermissionAuthorized
{
    public string RequiredPermission => IdentityPermissions.UsersManage;
}

public sealed class ResetUserPasswordHandler(IUserRepository repository, IIdentityProviderAdmin identityProvider)
    : ICommandHandler<ResetUserPasswordCommand, string>
{
    public async Task<Result<string>> Handle(ResetUserPasswordCommand command, CancellationToken ct)
    {
        var user = await repository.GetByIdAsync(command.Id, ct);
        if (user is null)
            return Result.Failure<string>(Error.NotFound("User"));

        var newPassword = await identityProvider.ResetToDefaultPasswordAsync(user.UserName, ct);
        return newPassword is null
            ? Result.Failure<string>(Error.Validation("Không đặt lại được mật khẩu (kiểm tra kết nối Keycloak)."))
            : Result.Success(newPassword);
    }
}
