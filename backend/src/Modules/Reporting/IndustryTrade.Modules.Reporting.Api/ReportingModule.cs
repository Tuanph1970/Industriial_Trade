using FluentValidation;
using IndustryTrade.BuildingBlocks.Web;
using IndustryTrade.Modules.Reporting.Api.Endpoints;
using IndustryTrade.Modules.Reporting.Application.Campaigns;
using IndustryTrade.Modules.Reporting.Infrastructure;
using IndustryTrade.Modules.Reporting.Infrastructure.Persistence;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IndustryTrade.Modules.Reporting.Api;

public sealed class ReportingModule : IModule
{
    public string Name => "Reporting";

    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddReportingInfrastructure(configuration);

        var applicationAssembly = typeof(CreateCampaignCommand).Assembly;
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapCampaignEndpoints();
        endpoints.MapSubmissionEndpoints();
    }

    public async Task ApplyMigrationsAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ReportingDbContext>();
        await db.Database.MigrateAsync();
    }
}
