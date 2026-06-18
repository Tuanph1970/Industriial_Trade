using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Web;
using IndustryTrade.Modules.IdentityAccess.Application.Organizations;
using IndustryTrade.Modules.IdentityAccess.Domain.Organizations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IndustryTrade.Modules.IdentityAccess.Api.Endpoints;

internal static class OrgUnitEndpoints
{
    public static void MapOrgUnitEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/identity/org-units")
            .WithTags("Identity & Access — Org Units")
            .RequireAuthorization(); // authenticated; per-permission checks run in the AuthorizationBehavior

        group.MapGet("/", async (ISender sender, int page = 1, int pageSize = 10, string? keyword = null) =>
        {
            var result = await sender.Send(new GetOrgUnitsQuery(new PageRequest(page, pageSize, keyword)));
            return ApiResults.Match(result);
        })
        .WithName("GetOrgUnits");

        group.MapPost("/", async (ISender sender, CreateOrgUnitRequest body) =>
        {
            var result = await sender.Send(
                new CreateOrgUnitCommand(body.Code, body.Name, body.Type, body.ParentId));
            return ApiResults.Match(result, id => Results.Created($"/api/identity/org-units/{id}", new { id }));
        })
        .WithName("CreateOrgUnit");

        group.MapGet("/{id:guid}", async (ISender sender, Guid id) =>
            ApiResults.Match(await sender.Send(new GetOrgUnitByIdQuery(id))));

        group.MapPut("/{id:guid}", async (ISender sender, Guid id, UpdateOrgUnitRequest body) =>
            ApiResults.Match(await sender.Send(new UpdateOrgUnitCommand(id, body.Name, body.IsActive))));

        group.MapDelete("/{id:guid}", async (ISender sender, Guid id) =>
            ApiResults.Match(await sender.Send(new DeleteOrgUnitCommand(id))));
    }
}

public sealed record CreateOrgUnitRequest(string Code, string Name, OrgUnitType Type, Guid? ParentId);
public sealed record UpdateOrgUnitRequest(string Name, bool IsActive);
