using IndustryTrade.Modules.SectorData.Application.Clusters;
using IndustryTrade.Modules.SectorData.Application.CommerceLocations;
using IndustryTrade.Modules.SectorData.Application.Ecommerce;
using IndustryTrade.Modules.SectorData.Application.Observations;
using IndustryTrade.Modules.SectorData.Application.PetroleumStations;
using IndustryTrade.Modules.SectorData.Application.Import;
using IndustryTrade.Modules.SectorData.Application.Violations;
using IndustryTrade.Modules.SectorData.Infrastructure.Import;
using IndustryTrade.Modules.SectorData.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IndustryTrade.Modules.SectorData.Infrastructure;

public static class SectorDataInfrastructure
{
    public static IServiceCollection AddSectorDataInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Postgres.");

        services.AddDbContext<SectorDataDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsHistoryTable("__ef_migrations_history", SectorDataDbContext.Schema);
                npgsql.UseNetTopologySuite();
            }));

        services.AddScoped<IObservationRepository, ObservationRepository>();
        services.AddScoped<IClusterRepository, ClusterRepository>();
        services.AddScoped<IViolationRepository, ViolationRepository>();
        services.AddScoped<IPetrolStationRepository, PetrolStationRepository>();
        services.AddScoped<ICommerceLocationRepository, CommerceLocationRepository>();
        services.AddScoped<IEcommerceParticipantRepository, EcommerceParticipantRepository>();

        // Batch-import file parsers (Strategy set, selected by the factory on file extension).
        services.AddSingleton<ITabularFileParser, ExcelFileParser>();
        services.AddSingleton<ITabularFileParser, CsvFileParser>();
        services.AddSingleton<ITabularFileParser, XmlFileParser>();
        services.AddSingleton<ITabularFileParserFactory, TabularFileParserFactory>();
        return services;
    }
}
