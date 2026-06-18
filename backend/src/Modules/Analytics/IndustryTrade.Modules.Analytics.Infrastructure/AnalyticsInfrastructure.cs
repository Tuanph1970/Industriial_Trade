using IndustryTrade.Modules.Analytics.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IndustryTrade.Modules.Analytics.Infrastructure;

public static class AnalyticsInfrastructure
{
    public static IServiceCollection AddAnalyticsInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Read-only: no DbContext/migration — queries run via Dapper against the shared connection.
        services.AddScoped<IAnalyticsQueries, AnalyticsQueries>();
        return services;
    }
}
