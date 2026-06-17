using IndustryTrade.BuildingBlocks.Infrastructure.Outbox;
using IndustryTrade.Modules.Catalog.Domain.Indicators;
using Microsoft.EntityFrameworkCore;

namespace IndustryTrade.Modules.Catalog.Infrastructure.Persistence;

/// <summary>Owns the <c>catalog</c> schema (schema-per-bounded-context, docs/design/04 §1).</summary>
public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public const string Schema = "catalog";

    public DbSet<Indicator> Indicators => Set<Indicator>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);

        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.ToTable("outbox_message");
            b.HasKey(x => x.Id);
            b.Property(x => x.Type).HasMaxLength(500);
            b.HasIndex(x => x.ProcessedOnUtc);
        });
    }
}
