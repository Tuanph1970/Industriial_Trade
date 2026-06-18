using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IndustryTrade.Modules.Integration.Infrastructure.Persistence;

public sealed class IntegrationDbContextFactory : IDesignTimeDbContextFactory<IntegrationDbContext>
{
    public IntegrationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? "Host=localhost;Port=5432;Database=industrytrade;Username=itrade;Password=itrade_dev_pw";

        var options = new DbContextOptionsBuilder<IntegrationDbContext>()
            .UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__ef_migrations_history", IntegrationDbContext.Schema))
            .Options;

        return new IntegrationDbContext(options);
    }
}
