using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Web;
using IndustryTrade.Modules.Catalog.Application.AdministrativeUnits;
using IndustryTrade.Modules.Catalog.Application.Classifications;
using IndustryTrade.Modules.Catalog.Domain.AdministrativeUnits;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IndustryTrade.Modules.Catalog.Api.Endpoints;

internal static class ReferenceDataEndpoints
{
    public static void MapCatalogReferenceDataEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var units = endpoints.MapGroup("/api/catalog/administrative-units")
            .WithTags("Catalog — Administrative Units").RequireAuthorization();

        units.MapGet("/", async (ISender sender, int page = 1, int pageSize = 10,
            string? keyword = null, AdministrativeLevel? level = null) =>
            ApiResults.Match(await sender.Send(new GetAdministrativeUnitsQuery(new PageRequest(page, pageSize, keyword), level))));

        units.MapPost("/", async (ISender sender, CreateAdministrativeUnitRequest body) =>
            ApiResults.Match(
                await sender.Send(new CreateAdministrativeUnitCommand(body.Code, body.Name, body.Level, body.ParentId)),
                id => Results.Created($"/api/catalog/administrative-units/{id}", new { id })));

        units.MapPut("/{id:guid}", async (ISender sender, Guid id, UpdateAdministrativeUnitRequest body) =>
            ApiResults.Match(await sender.Send(new UpdateAdministrativeUnitCommand(
                id, body.Name, body.Level, body.ParentId, body.IsActive))));

        units.MapDelete("/{id:guid}", async (ISender sender, Guid id) =>
            ApiResults.Match(await sender.Send(new DeleteAdministrativeUnitCommand(id))));

        var classifications = endpoints.MapGroup("/api/catalog/classifications")
            .WithTags("Catalog — Classifications").RequireAuthorization();

        classifications.MapGet("/", async (ISender sender, int page = 1, int pageSize = 10, string? keyword = null) =>
            ApiResults.Match(await sender.Send(new GetClassificationsQuery(new PageRequest(page, pageSize, keyword)))));

        classifications.MapPost("/", async (ISender sender, CreateClassificationRequest body) =>
            ApiResults.Match(
                await sender.Send(new CreateClassificationCommand(body.Code, body.Name, body.Description, body.Items ?? [])),
                id => Results.Created($"/api/catalog/classifications/{id}", new { id })));

        classifications.MapPut("/{id:guid}", async (ISender sender, Guid id, UpdateClassificationRequest body) =>
            ApiResults.Match(await sender.Send(new UpdateClassificationCommand(
                id, body.Name, body.Description, body.Items ?? []))));

        classifications.MapDelete("/{id:guid}", async (ISender sender, Guid id) =>
            ApiResults.Match(await sender.Send(new DeleteClassificationCommand(id))));
    }
}

public sealed record CreateAdministrativeUnitRequest(string Code, string Name, AdministrativeLevel Level, Guid? ParentId);
public sealed record UpdateAdministrativeUnitRequest(string Name, AdministrativeLevel Level, Guid? ParentId, bool IsActive);

public sealed record CreateClassificationRequest(string Code, string Name, string? Description, ClassificationItemInput[]? Items);
public sealed record UpdateClassificationRequest(string Name, string? Description, ClassificationItemInput[]? Items);
