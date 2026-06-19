using IndustryTrade.Modules.Catalog.Application.AdministrativeUnits;
using IndustryTrade.Modules.Catalog.Application.Classifications;
using IndustryTrade.Modules.Catalog.Application.Indicators;
using IndustryTrade.Modules.Catalog.Application.IndicatorSets;
using IndustryTrade.Modules.Catalog.Application.ReportingPeriods;
using IndustryTrade.Modules.Catalog.Application.ReportTemplates;
using IndustryTrade.Modules.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IndustryTrade.Modules.Catalog.Infrastructure;

public static class CatalogInfrastructure
{
    public static IServiceCollection AddCatalogInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Postgres.");

        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsHistoryTable("__ef_migrations_history", CatalogDbContext.Schema);
                npgsql.UseNetTopologySuite();
            }));

        services.AddScoped<IIndicatorRepository, IndicatorRepository>();
        services.AddScoped<IIndicatorSetRepository, IndicatorSetRepository>();
        services.AddScoped<IReportTemplateRepository, ReportTemplateRepository>();
        services.AddScoped<IReportingPeriodRepository, ReportingPeriodRepository>();
        services.AddScoped<IAdministrativeUnitRepository, AdministrativeUnitRepository>();
        services.AddScoped<IClassificationRepository, ClassificationRepository>();
        return services;
    }
}
