using FluentValidation;
using IndustryTrade.BuildingBlocks.Web;
using IndustryTrade.Modules.IdentityAccess.Api.Endpoints;
using IndustryTrade.Modules.IdentityAccess.Application.Organizations;
using IndustryTrade.Modules.IdentityAccess.Infrastructure;
using IndustryTrade.Modules.IdentityAccess.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

        var applicationAssembly = typeof(CreateOrgUnitCommand).Assembly;
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddValidatorsFromAssembly(applicationAssembly);

        // DB-backed authorization enrichment (permissions + data-scope) after Keycloak authn.
        services.AddScoped<IClaimsTransformation, IdentityClaimsTransformation>();

        return services;
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapOrgUnitEndpoints();
        endpoints.MapRoleEndpoints();
        endpoints.MapUserEndpoints();
    }

    public async Task ApplyMigrationsAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityAccessDbContext>();
        await db.Database.MigrateAsync();

        var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        if (environment.IsDevelopment())
            await IdentityAccessSeeder.SeedAsync(db);
    }
}
