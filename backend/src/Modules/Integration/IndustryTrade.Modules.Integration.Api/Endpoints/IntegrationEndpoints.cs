using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Web;
using IndustryTrade.Modules.Integration.Application.Services;
using IndustryTrade.Modules.Integration.Application.Status;
using IndustryTrade.Modules.Integration.Domain.Services;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IndustryTrade.Modules.Integration.Api.Endpoints;

internal static class IntegrationEndpoints
{
    public static void MapIntegrationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var services = endpoints.MapGroup("/api/integration/services")
            .WithTags("Integration — Data-sharing Services")
            .RequireAuthorization();

        services.MapGet("/", async (ISender sender, int page = 1, int pageSize = 10, string? keyword = null) =>
            ApiResults.Match(await sender.Send(new GetServicesQuery(new PageRequest(page, pageSize, keyword)))));

        services.MapPost("/", async (ISender sender, CreateServiceRequest body) =>
            ApiResults.Match(
                await sender.Send(new CreateServiceCommand(
                    body.Code, body.Name, body.Direction, body.EndpointUrl, body.Description)),
                id => Results.Created($"/api/integration/services/{id}", new { id })));

        services.MapPost("/{id:guid}/status", async (ISender sender, Guid id, ChangeServiceStatusRequest body) =>
            ApiResults.Match(await sender.Send(new ChangeServiceStatusCommand(id, body.Action))));

        var status = endpoints.MapGroup("/api/integration/connection-status")
            .WithTags("Integration — Connection Status")
            .RequireAuthorization();

        status.MapGet("/", async (ISender sender) =>
            ApiResults.Match(await sender.Send(new GetConnectionStatusQuery())));

        status.MapGet("/history", async (ISender sender, int page = 1, int pageSize = 20) =>
            ApiResults.Match(await sender.Send(new GetConnectionStatusHistoryQuery(new PageRequest(page, pageSize)))));
    }
}

public sealed record CreateServiceRequest(
    string Code, string Name, ServiceDirection Direction, string? EndpointUrl, string? Description);

public sealed record ChangeServiceStatusRequest(ServiceLifecycleAction Action);
