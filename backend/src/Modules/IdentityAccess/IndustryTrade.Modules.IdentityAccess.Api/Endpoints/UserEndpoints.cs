using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Web;
using IndustryTrade.Modules.IdentityAccess.Application.Users;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IndustryTrade.Modules.IdentityAccess.Api.Endpoints;

internal static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/identity/users")
            .WithTags("Identity & Access — Users")
            .RequireAuthorization();

        group.MapGet("/", async (ISender sender, int page = 1, int pageSize = 10, string? keyword = null) =>
            ApiResults.Match(await sender.Send(new GetUsersQuery(new PageRequest(page, pageSize, keyword)))));

        group.MapPost("/", async (ISender sender, CreateUserRequest body) =>
            ApiResults.Match(
                await sender.Send(new CreateUserCommand(
                    body.UserName, body.FullName, body.Email, body.OrgUnitId, body.RoleIds ?? [])),
                id => Results.Created($"/api/identity/users/{id}", new { id })));

        group.MapDelete("/{id:guid}", async (ISender sender, Guid id) =>
            ApiResults.Match(await sender.Send(new DeleteUserCommand(id))));
    }
}

public sealed record CreateUserRequest(
    string UserName, string? FullName, string? Email, Guid? OrgUnitId, Guid[]? RoleIds);
