using FluentValidation;
using IndustryTrade.BuildingBlocks.Web;
using IndustryTrade.Modules.IdentityAccess.Api.Endpoints;
using IndustryTrade.Modules.IdentityAccess.Application.Organizations;
using IndustryTrade.Modules.IdentityAccess.Infrastructure;
using IndustryTrade.Modules.IdentityAccess.Infrastructure.Persistence;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IndustryTrade.Modules.IdentityAccess.Api;

/// <summary>
/// Reference module wiring. Every other bounded context follows this exact shape
/// (Domain → Application → Infrastructure → Api implementing IModule). See docs/design/02 §2.
/// </summary>
public sealed class IdentityAccessModule : IModule
{
    public string Name => "IdentityAccess";

    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityAccessInfrastructure(configuration);

        // Handlers + validators for this module's Application assembly.
        var applicationAssembly = typeof(CreateOrgUnitCommand).Assembly;
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) => endpoints.MapOrgUnitEndpoints();

    public async Task ApplyMigrationsAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityAccessDbContext>();
        await db.Database.MigrateAsync();
    }
}
