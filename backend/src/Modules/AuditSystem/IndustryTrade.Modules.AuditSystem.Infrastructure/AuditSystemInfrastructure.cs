using IndustryTrade.BuildingBlocks.Application.Auditing;
using IndustryTrade.Modules.AuditSystem.Application;
using IndustryTrade.Modules.AuditSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IndustryTrade.Modules.AuditSystem.Infrastructure;

public static class AuditSystemInfrastructure
{
    public static IServiceCollection AddAuditSystemInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Postgres.");

        services.AddDbContext<AuditDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__ef_migrations_history", AuditDbContext.Schema)));

        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAuditSink, AuditSink>();
        return services;
    }
}
