using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Web;
using IndustryTrade.Modules.SectorData.Application;
using IndustryTrade.Modules.SectorData.Application.CommerceLocations;
using IndustryTrade.Modules.SectorData.Application.Ecommerce;
using IndustryTrade.Modules.SectorData.Application.PetroleumStations;
using IndustryTrade.Modules.SectorData.Domain.CommerceLocations;
using IndustryTrade.Modules.SectorData.Domain.PetroleumStations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IndustryTrade.Modules.SectorData.Api.Endpoints;

internal static class PetrolStationEndpoints
{
    public static void MapPetrolStationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/sector/petrol-stations")
            .WithTags("Sector Data — Petroleum Stations")
            .RequireAuthorization();

        group.MapGet("/", async (ISender sender, int page = 1, int pageSize = 10, string? keyword = null) =>
            ApiResults.Match(await sender.Send(new GetPetrolStationsQuery(new PageRequest(page, pageSize, keyword)))));

        group.MapPost("/", async (ISender sender, CreatePetrolStationRequest body) =>
            ApiResults.Match(
                await sender.Send(new CreatePetrolStationCommand(
                    body.Code, body.Name, body.OrgUnitId, body.LicenseNo, body.Address,
                    body.Latitude, body.Longitude, body.Status)),
                id => Results.Created($"/api/sector/petrol-stations/{id}", new { id })));

        group.MapPut("/{id:guid}", async (ISender sender, Guid id, UpdatePetrolStationRequest body) =>
            ApiResults.Match(await sender.Send(new UpdatePetrolStationCommand(
                id, body.Name, body.LicenseNo, body.Address, body.Latitude, body.Longitude, body.Status))));

        group.MapDelete("/{id:guid}", async (ISender sender, Guid id) =>
            ApiResults.Match(await sender.Send(new DeletePetrolStationCommand(id))));
    }
}

internal static class CommerceLocationEndpoints
{
    public static void MapCommerceLocationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/sector/commerce-locations")
            .WithTags("Sector Data — Commerce Locations")
            .RequireAuthorization();

        group.MapGet("/", async (ISender sender,
            int page = 1, int pageSize = 10, string? keyword = null, CommerceLocationType? type = null) =>
            ApiResults.Match(await sender.Send(
                new GetCommerceLocationsQuery(new PageRequest(page, pageSize, keyword), type))));

        group.MapPost("/", async (ISender sender, CreateCommerceLocationRequest body) =>
            ApiResults.Match(
                await sender.Send(new CreateCommerceLocationCommand(
                    body.Code, body.Name, body.Type, body.OrgUnitId, body.Address, body.Latitude, body.Longitude)),
                id => Results.Created($"/api/sector/commerce-locations/{id}", new { id })));

        group.MapPut("/{id:guid}", async (ISender sender, Guid id, UpdateCommerceLocationRequest body) =>
            ApiResults.Match(await sender.Send(new UpdateCommerceLocationCommand(
                id, body.Name, body.Type, body.Address, body.Latitude, body.Longitude))));

        group.MapDelete("/{id:guid}", async (ISender sender, Guid id) =>
            ApiResults.Match(await sender.Send(new DeleteCommerceLocationCommand(id))));
    }
}

internal static class EcommerceEndpoints
{
    public static void MapEcommerceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/sector/ecommerce-participants")
            .WithTags("Sector Data — E-commerce Participants")
            .RequireAuthorization();

        group.MapGet("/", async (ISender sender, int page = 1, int pageSize = 10, string? keyword = null) =>
            ApiResults.Match(await sender.Send(new GetEcommerceParticipantsQuery(new PageRequest(page, pageSize, keyword)))));

        group.MapPost("/", async (ISender sender, CreateEcommerceRequest body) =>
            ApiResults.Match(
                await sender.Send(new CreateEcommerceParticipantCommand(
                    body.TaxCode, body.BusinessName, body.OrgUnitId, body.Platforms ?? [], body.MainGoods)),
                id => Results.Created($"/api/sector/ecommerce-participants/{id}", new { id })));

        group.MapPut("/{id:guid}", async (ISender sender, Guid id, UpdateEcommerceRequest body) =>
            ApiResults.Match(await sender.Send(new UpdateEcommerceCommand(
                id, body.BusinessName, body.Platforms ?? [], body.MainGoods))));

        group.MapDelete("/{id:guid}", async (ISender sender, Guid id) =>
            ApiResults.Match(await sender.Send(new DeleteEcommerceCommand(id))));
    }
}

public sealed record CreatePetrolStationRequest(
    string Code, string Name, Guid OrgUnitId, string? LicenseNo, string? Address,
    double? Latitude, double? Longitude, StationStatus Status);

public sealed record CreateCommerceLocationRequest(
    string Code, string Name, CommerceLocationType Type, Guid OrgUnitId,
    string? Address, double? Latitude, double? Longitude);

public sealed record CreateEcommerceRequest(
    string TaxCode, string BusinessName, Guid OrgUnitId, string[]? Platforms, string? MainGoods);

public sealed record UpdatePetrolStationRequest(
    string Name, string? LicenseNo, string? Address,
    double? Latitude, double? Longitude, StationStatus Status);

public sealed record UpdateCommerceLocationRequest(
    string Name, CommerceLocationType Type, string? Address, double? Latitude, double? Longitude);

public sealed record UpdateEcommerceRequest(
    string BusinessName, string[]? Platforms, string? MainGoods);
