using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Web;
using IndustryTrade.Modules.SectorData.Application;
using IndustryTrade.Modules.SectorData.Application.Clusters;
using IndustryTrade.Modules.SectorData.Application.Observations;
using IndustryTrade.Modules.SectorData.Application.Violations;
using IndustryTrade.Modules.SectorData.Domain.Clusters;
using IndustryTrade.Modules.SectorData.Domain.Violations;
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

        group.MapPost("/{id:guid}/action", async (ISender sender, Guid id, ObservationActionRequest body) =>
            ApiResults.Match(await sender.Send(new ObservationActionCommand(id, body.Action))));
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

        group.MapPut("/{id:guid}", async (ISender sender, Guid id, UpdateClusterRequest body) =>
            ApiResults.Match(await sender.Send(new UpdateClusterCommand(
                id, body.Name, body.AreaHa, body.Latitude, body.Longitude, body.Status))));

        group.MapDelete("/{id:guid}", async (ISender sender, Guid id) =>
            ApiResults.Match(await sender.Send(new DeleteClusterCommand(id))));
    }
}

internal static class ViolationEndpoints
{
    public static void MapViolationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/sector/violations")
            .WithTags("Sector Data — Market Violations")
            .RequireAuthorization();

        group.MapGet("/", async (ISender sender,
            int page = 1, int pageSize = 10, string? keyword = null, ViolationGroup? violationGroup = null) =>
            ApiResults.Match(await sender.Send(
                new GetViolationsQuery(new PageRequest(page, pageSize, keyword), violationGroup))));

        group.MapPost("/", async (ISender sender, CreateViolationRequest body) =>
            ApiResults.Match(
                await sender.Send(new CreateViolationCommand(
                    body.CaseNo, body.Group, body.OrgUnitId, body.BusinessName,
                    body.InspectedOn, body.ViolationContent)),
                id => Results.Created($"/api/sector/violations/{id}", new { id })));

        group.MapPut("/{id:guid}", async (ISender sender, Guid id, UpdateViolationRequest body) =>
            ApiResults.Match(await sender.Send(new UpdateViolationCommand(
                id, body.Group, body.BusinessName, body.InspectedOn, body.ViolationContent,
                body.SanctionContent, body.FineAmount, body.Status))));

        group.MapDelete("/{id:guid}", async (ISender sender, Guid id) =>
            ApiResults.Match(await sender.Send(new DeleteViolationCommand(id))));
    }
}

public sealed record CreateObservationRequest(
    Guid IndicatorId, Guid OrgUnitId, int PeriodYear, int? PeriodMonth,
    decimal? Value, string? ValueText, string? Source);

public sealed record ObservationActionRequest(ObservationAction Action);

public sealed record CreateClusterRequest(
    string Code, string Name, Guid OrgUnitId, decimal? AreaHa,
    double? Latitude, double? Longitude, ClusterStatus Status);

public sealed record CreateViolationRequest(
    string CaseNo, ViolationGroup Group, Guid OrgUnitId, string BusinessName,
    DateOnly InspectedOn, string ViolationContent);

public sealed record UpdateClusterRequest(
    string Name, decimal? AreaHa, double? Latitude, double? Longitude, ClusterStatus Status);

public sealed record UpdateViolationRequest(
    ViolationGroup Group, string BusinessName, DateOnly InspectedOn, string ViolationContent,
    string? SanctionContent, decimal? FineAmount, ViolationStatus Status);
