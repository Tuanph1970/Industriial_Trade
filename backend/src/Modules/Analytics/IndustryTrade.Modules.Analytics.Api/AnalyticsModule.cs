using IndustryTrade.BuildingBlocks.Web;
using IndustryTrade.Modules.Analytics.Application;
using IndustryTrade.Modules.Analytics.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IndustryTrade.Modules.Analytics.Api;

public sealed class AnalyticsModule : IModule
{
    public string Name => "Analytics";

    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAnalyticsInfrastructure(configuration);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetDashboardQuery).Assembly));
        return services;
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/analytics")
            .WithTags("Analytics & Dashboards")
            .RequireAuthorization();

        group.MapGet("/dashboard", async (ISender sender) =>
            ApiResults.Match(await sender.Send(new GetDashboardQuery())));

        group.MapGet("/violations-summary", async (ISender sender) =>
            ApiResults.Match(await sender.Send(new GetViolationsSummaryQuery())));

        group.MapGet("/reporting-summary", async (ISender sender) =>
            ApiResults.Match(await sender.Send(new GetReportingSummaryQuery())));

        group.MapGet("/observations-by-sector", async (ISender sender) =>
            ApiResults.Match(await sender.Send(new GetObservationsBySectorQuery())));

        group.MapGet("/commerce-by-type", async (ISender sender) =>
            ApiResults.Match(await sender.Send(new GetCommerceByTypeQuery())));
    }
}
