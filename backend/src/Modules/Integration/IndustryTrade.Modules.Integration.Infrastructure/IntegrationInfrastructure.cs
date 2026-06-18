using IndustryTrade.Modules.Integration.Application.Services;
using IndustryTrade.Modules.Integration.Application.Status;
using IndustryTrade.Modules.Integration.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IndustryTrade.Modules.Integration.Infrastructure;

public static class IntegrationInfrastructure
{
    public static IServiceCollection AddIntegrationInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Postgres.");

        services.AddDbContext<IntegrationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__ef_migrations_history", IntegrationDbContext.Schema)));

        services.AddScoped<IDataSharingServiceRepository, DataSharingServiceRepository>();
        services.AddScoped<IConnectionStatusStore, ConnectionStatusStore>();
        services.AddScoped<IConnectionProbe, ConnectionProbe>();
        return services;
    }
}
