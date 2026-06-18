using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Web;
using IndustryTrade.Modules.SectorData.Application.Clusters;
using IndustryTrade.Modules.SectorData.Application.Observations;
using IndustryTrade.Modules.SectorData.Domain.Clusters;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IndustryTrade.Modules.SectorData.Api.Endpoints;

internal static class ObservationEndpoints
{
    public static void MapObservationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/sector/observations")
            .WithTags("Sector Data — Observations")
            .RequireAuthorization();

        group.MapGet("/", async (ISender sender, int page = 1, int pageSize = 10, int? periodYear = null) =>
            ApiResults.Match(await sender.Send(new GetObservationsQuery(new PageRequest(page, pageSize), periodYear))));

        group.MapPost("/", async (ISender sender, CreateObservationRequest body) =>
            ApiResults.Match(
                await sender.Send(new CreateObservationCommand(
                    body.IndicatorId, body.OrgUnitId, body.PeriodYear, body.PeriodMonth,
                    body.Value, body.ValueText, body.Source)),
                id => Results.Created($"/api/sector/observations/{id}", new { id })));
    }
}

internal static class ClusterEndpoints
{
    public static void MapClusterEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/sector/clusters")
            .WithTags("Sector Data — Industrial Clusters")
            .RequireAuthorization();

        group.MapGet("/", async (ISender sender, int page = 1, int pageSize = 10, string? keyword = null) =>
            ApiResults.Match(await sender.Send(new GetClustersQuery(new PageRequest(page, pageSize, keyword)))));

        group.MapPost("/", async (ISender sender, CreateClusterRequest body) =>
            ApiResults.Match(
                await sender.Send(new CreateClusterCommand(
                    body.Code, body.Name, body.OrgUnitId, body.AreaHa, body.Latitude, body.Longitude, body.Status)),
                id => Results.Created($"/api/sector/clusters/{id}", new { id })));
    }
}

public sealed record CreateObservationRequest(
    Guid IndicatorId, Guid OrgUnitId, int PeriodYear, int? PeriodMonth,
    decimal? Value, string? ValueText, string? Source);

public sealed record CreateClusterRequest(
    string Code, string Name, Guid OrgUnitId, decimal? AreaHa,
    double? Latitude, double? Longitude, ClusterStatus Status);
