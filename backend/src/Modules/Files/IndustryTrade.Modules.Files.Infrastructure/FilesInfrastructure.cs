using IndustryTrade.Modules.Files.Application.Files;
using IndustryTrade.Modules.Files.Infrastructure.Persistence;
using IndustryTrade.Modules.Files.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IndustryTrade.Modules.Files.Infrastructure;

public static class FilesInfrastructure
{
    public static IServiceCollection AddFilesInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Postgres.");

        services.AddDbContext<FilesDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__ef_migrations_history", FilesDbContext.Schema)));

        services.AddScoped<IFileResourceRepository, FileResourceRepository>();

        var section = configuration.GetSection("Minio");
        var minio = new MinioOptions
        {
            Endpoint = section["Endpoint"] ?? "localhost:9000",
            AccessKey = section["AccessKey"] ?? "minioadmin",
            SecretKey = section["SecretKey"] ?? "minioadmin",
            Bucket = section["Bucket"] ?? "industrytrade",
            UseSsl = bool.TryParse(section["UseSsl"], out var ssl) && ssl,
        };
        services.AddSingleton(minio);
        services.AddSingleton<IObjectStorage, MinioObjectStorage>();
        return services;
    }
}
