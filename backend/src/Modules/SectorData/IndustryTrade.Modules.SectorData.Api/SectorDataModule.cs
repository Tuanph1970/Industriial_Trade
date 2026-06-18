using FluentValidation;
using IndustryTrade.BuildingBlocks.Web;
using IndustryTrade.Modules.SectorData.Api.Endpoints;
using IndustryTrade.Modules.SectorData.Application.Observations;
using IndustryTrade.Modules.SectorData.Infrastructure;
using IndustryTrade.Modules.SectorData.Infrastructure.Persistence;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IndustryTrade.Modules.SectorData.Api;

public sealed class SectorDataModule : IModule
{
    public string Name => "SectorData";

    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSectorDataInfrastructure(configuration);

        var applicationAssembly = typeof(CreateObservationCommand).Assembly;
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapObservationEndpoints();
        endpoints.MapClusterEndpoints();
    }

    public async Task ApplyMigrationsAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SectorDataDbContext>();
        await db.Database.MigrateAsync();
    }
}
