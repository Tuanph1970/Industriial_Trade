using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IndustryTrade.Modules.IdentityAccess.Infrastructure.Persistence;

/// <summary>Lets `dotnet ef` build the context at design time (migrations) without the web host.</summary>
public sealed class IdentityAccessDbContextFactory : IDesignTimeDbContextFactory<IdentityAccessDbContext>
{
    public IdentityAccessDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? "Host=localhost;Port=5432;Database=industrytrade;Username=itrade;Password=itrade_dev_pw";

        var options = new DbContextOptionsBuilder<IdentityAccessDbContext>()
            .UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsHistoryTable("__ef_migrations_history", IdentityAccessDbContext.Schema);
                npgsql.UseNetTopologySuite();
            })
            .Options;

        return new IdentityAccessDbContext(options);
    }
}
