using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Web;
using IndustryTrade.Modules.AuditSystem.Application;
using IndustryTrade.Modules.AuditSystem.Infrastructure;
using IndustryTrade.Modules.AuditSystem.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IndustryTrade.Modules.AuditSystem.Api;

public sealed class AuditSystemModule : IModule
{
    public string Name => "AuditSystem";

    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuditSystemInfrastructure(configuration);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetAuditLogsQuery).Assembly));
        return services;
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/audit/logs")
            .WithTags("Audit Log")
            .RequireAuthorization();

        group.MapGet("/", async (ISender sender,
            int page = 1, int pageSize = 20, string? actor = null, string? action = null, DateTime? fromUtc = null) =>
            ApiResults.Match(await sender.Send(
                new GetAuditLogsQuery(new PageRequest(page, pageSize), actor, action, fromUtc))));
    }

    public async Task ApplyMigrationsAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        await db.Database.MigrateAsync();
    }
}
