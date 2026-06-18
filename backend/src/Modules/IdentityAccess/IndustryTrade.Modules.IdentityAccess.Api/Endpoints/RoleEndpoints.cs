using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Web;
using IndustryTrade.Modules.IdentityAccess.Application.Roles;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IndustryTrade.Modules.IdentityAccess.Api.Endpoints;

internal static class RoleEndpoints
{
    public static void MapRoleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/identity/roles")
            .WithTags("Identity & Access — Roles")
            .RequireAuthorization();

        group.MapGet("/", async (ISender sender, int page = 1, int pageSize = 10, string? keyword = null) =>
            ApiResults.Match(await sender.Send(new GetRolesQuery(new PageRequest(page, pageSize, keyword)))));

        group.MapPost("/", async (ISender sender, CreateRoleRequest body) =>
            ApiResults.Match(
                await sender.Send(new CreateRoleCommand(body.Code, body.Name, body.Permissions ?? [])),
                id => Results.Created($"/api/identity/roles/{id}", new { id })));

        group.MapDelete("/{id:guid}", async (ISender sender, Guid id) =>
            ApiResults.Match(await sender.Send(new DeleteRoleCommand(id))));
    }
}

public sealed record CreateRoleRequest(string Code, string Name, string[]? Permissions);
