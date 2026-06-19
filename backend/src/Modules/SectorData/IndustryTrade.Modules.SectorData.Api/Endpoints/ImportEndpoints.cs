using IndustryTrade.BuildingBlocks.Web;
using IndustryTrade.Modules.SectorData.Application.Import;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IndustryTrade.Modules.SectorData.Api.Endpoints;

internal static class ImportEndpoints
{
    public static void MapSectorImportEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/sector/import")
            .WithTags("Sector Data — Batch Import")
            .RequireAuthorization();

        // Parse an uploaded .xlsx/.xml/.csv into header-keyed rows for client-side preview.
        group.MapPost("/parse", async (ISender sender, IFormFile file) =>
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return ApiResults.Match(await sender.Send(new ParseImportFileCommand(ms.ToArray(), file.FileName)));
        }).DisableAntiforgery();

        // Typed bulk-create endpoints (one per entity); items arrive already code→id resolved.
        var obs = endpoints.MapGroup("/api/sector/observations").RequireAuthorization();
        obs.MapPost("/import", async (ISender sender, BulkObservationsRequest body) =>
            ApiResults.Match(await sender.Send(new BulkCreateObservationsCommand(body.Items ?? []))));

        var clusters = endpoints.MapGroup("/api/sector/clusters").RequireAuthorization();
        clusters.MapPost("/import", async (ISender sender, BulkClustersRequest body) =>
            ApiResults.Match(await sender.Send(new BulkCreateClustersCommand(body.Items ?? []))));

        var petrol = endpoints.MapGroup("/api/sector/petrol-stations").RequireAuthorization();
        petrol.MapPost("/import", async (ISender sender, BulkPetrolStationsRequest body) =>
            ApiResults.Match(await sender.Send(new BulkCreatePetrolStationsCommand(body.Items ?? []))));

        var commerce = endpoints.MapGroup("/api/sector/commerce-locations").RequireAuthorization();
        commerce.MapPost("/import", async (ISender sender, BulkCommerceLocationsRequest body) =>
            ApiResults.Match(await sender.Send(new BulkCreateCommerceLocationsCommand(body.Items ?? []))));

        var ecommerce = endpoints.MapGroup("/api/sector/ecommerce-participants").RequireAuthorization();
        ecommerce.MapPost("/import", async (ISender sender, BulkEcommerceRequest body) =>
            ApiResults.Match(await sender.Send(new BulkCreateEcommerceCommand(body.Items ?? []))));

        var violations = endpoints.MapGroup("/api/sector/violations").RequireAuthorization();
        violations.MapPost("/import", async (ISender sender, BulkViolationsRequest body) =>
            ApiResults.Match(await sender.Send(new BulkCreateViolationsCommand(body.Items ?? []))));
    }
}

public sealed record BulkObservationsRequest(List<ObservationImportItem>? Items);
public sealed record BulkClustersRequest(List<ClusterImportItem>? Items);
public sealed record BulkPetrolStationsRequest(List<PetrolStationImportItem>? Items);
public sealed record BulkCommerceLocationsRequest(List<CommerceLocationImportItem>? Items);
public sealed record BulkEcommerceRequest(List<EcommerceImportItem>? Items);
public sealed record BulkViolationsRequest(List<ViolationImportItem>? Items);
