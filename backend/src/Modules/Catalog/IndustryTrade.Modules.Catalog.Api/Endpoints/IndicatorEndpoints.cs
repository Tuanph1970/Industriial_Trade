using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Web;
using IndustryTrade.Modules.Catalog.Application.Indicators;
using IndustryTrade.Modules.Catalog.Domain.Indicators;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IndustryTrade.Modules.Catalog.Api.Endpoints;

internal static class IndicatorEndpoints
{
    public static void MapIndicatorEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/catalog/indicators")
            .WithTags("Catalog — Indicators")
            .RequireAuthorization();

        group.MapGet("/", async (ISender sender,
            int page = 1, int pageSize = 10, string? keyword = null, IndustrySector? sector = null) =>
            ApiResults.Match(await sender.Send(
                new GetIndicatorsQuery(new PageRequest(page, pageSize, keyword), sector))));

        group.MapPost("/", async (ISender sender, CreateIndicatorRequest body) =>
            ApiResults.Match(
                await sender.Send(new CreateIndicatorCommand(
                    body.Code, body.Name, body.Unit, body.DataType, body.Sector, body.EffectiveFrom)),
                id => Results.Created($"/api/catalog/indicators/{id}", new { id })));

        group.MapGet("/{id:guid}", async (ISender sender, Guid id) =>
            ApiResults.Match(await sender.Send(new GetIndicatorByIdQuery(id))));

        group.MapPut("/{id:guid}", async (ISender sender, Guid id, UpdateIndicatorRequest body) =>
            ApiResults.Match(await sender.Send(new UpdateIndicatorCommand(
                id, body.Name, body.Unit, body.DataType, body.Sector))));

        group.MapDelete("/{id:guid}", async (ISender sender, Guid id) =>
            ApiResults.Match(await sender.Send(new DeleteIndicatorCommand(id))));
    }
}

public sealed record CreateIndicatorRequest(
    string Code, string Name, string Unit,
    IndicatorDataType DataType, IndustrySector Sector, DateOnly EffectiveFrom);

public sealed record UpdateIndicatorRequest(
    string Name, string Unit, IndicatorDataType DataType, IndustrySector Sector);
