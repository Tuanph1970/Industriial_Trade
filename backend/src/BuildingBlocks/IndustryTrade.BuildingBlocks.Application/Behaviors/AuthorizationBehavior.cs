using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Security;
using MediatR;

namespace IndustryTrade.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Enforces function-scope authorization for any request implementing <see cref="IPermissionAuthorized"/>.
/// Data-scope (row filtering) is applied inside query handlers/specifications via <see cref="ICurrentUser"/>.
/// </summary>
public sealed class AuthorizationBehavior<TRequest, TResponse>(ICurrentUser currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is IPermissionAuthorized authorized)
        {
            if (!currentUser.IsAuthenticated)
                throw new ForbiddenAccessException("Authentication required.");

            if (!currentUser.HasPermission(authorized.RequiredPermission))
                throw new ForbiddenAccessException(
                    $"Missing permission '{authorized.RequiredPermission}'.");
        }

        return next();
    }
}
