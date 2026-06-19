using FluentValidation;
using IndustryTrade.BuildingBlocks.Web;
using IndustryTrade.Modules.Catalog.Api.Endpoints;
using IndustryTrade.Modules.Catalog.Application.Indicators;
using IndustryTrade.Modules.Catalog.Infrastructure;
using IndustryTrade.Modules.Catalog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IndustryTrade.Modules.Catalog.Api;

public sealed class CatalogModule : IModule
{
    public string Name => "Catalog";

    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddCatalogInfrastructure(configuration);

        var applicationAssembly = typeof(CreateIndicatorCommand).Assembly;
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapIndicatorEndpoints();
        endpoints.MapCatalogMasterDataEndpoints();
        endpoints.MapCatalogReferenceDataEndpoints();
    }

    public async Task ApplyMigrationsAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await db.Database.MigrateAsync();
    }
}
