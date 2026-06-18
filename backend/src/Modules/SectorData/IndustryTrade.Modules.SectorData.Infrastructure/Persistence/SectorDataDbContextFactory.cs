using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IndustryTrade.Modules.SectorData.Infrastructure.Persistence;

public sealed class SectorDataDbContextFactory : IDesignTimeDbContextFactory<SectorDataDbContext>
{
    public SectorDataDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? "Host=localhost;Port=5432;Database=industrytrade;Username=itrade;Password=itrade_dev_pw";

        var options = new DbContextOptionsBuilder<SectorDataDbContext>()
            .UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsHistoryTable("__ef_migrations_history", SectorDataDbContext.Schema);
                npgsql.UseNetTopologySuite();
            })
            .Options;

        return new SectorDataDbContext(options);
    }
}
