using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IndustryTrade.Modules.Catalog.Infrastructure.Persistence;

/// <summary>Design-time factory for `dotnet ef` migrations.</summary>
public sealed class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? "Host=localhost;Port=5432;Database=industrytrade;Username=itrade;Password=itrade_dev_pw";

        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsHistoryTable("__ef_migrations_history", CatalogDbContext.Schema);
                npgsql.UseNetTopologySuite();
            })
            .Options;

        return new CatalogDbContext(options);
    }
}
