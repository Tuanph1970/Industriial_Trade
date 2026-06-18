using FluentValidation;
using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Web;
using IndustryTrade.Modules.Integration.Api.Endpoints;
using IndustryTrade.Modules.Integration.Application.Services;
using IndustryTrade.Modules.Integration.Infrastructure;
using IndustryTrade.Modules.Integration.Infrastructure.Persistence;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IndustryTrade.Modules.Integration.Api;

public sealed class IntegrationModule : IModule
{
    public string Name => "Integration";

    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddIntegrationInfrastructure(configuration);

        var applicationAssembly = typeof(CreateServiceCommand).Assembly;
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) => endpoints.MapIntegrationEndpoints();

    public async Task ApplyMigrationsAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IntegrationDbContext>();
        await db.Database.MigrateAsync();
    }
}
