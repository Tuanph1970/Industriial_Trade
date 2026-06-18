using IndustryTrade.BuildingBlocks.Infrastructure.Outbox;
using IndustryTrade.Modules.IdentityAccess.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace IndustryTrade.IntegrationTests;

/// <summary>Starts a throwaway PostgreSQL (PostGIS) container and migrates the identity schema once.</summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgis/postgis:16-3.4")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public IdentityAccessDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<IdentityAccessDbContext>()
            .UseNpgsql(ConnectionString, npgsql =>
            {
                npgsql.MigrationsHistoryTable("__ef_migrations_history", IdentityAccessDbContext.Schema);
                npgsql.UseNetTopologySuite();
            })
            .AddInterceptors(new OutboxWriterInterceptor())
            .Options;
        return new IdentityAccessDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var db = CreateContext();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}

[CollectionDefinition("postgres")]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>;
