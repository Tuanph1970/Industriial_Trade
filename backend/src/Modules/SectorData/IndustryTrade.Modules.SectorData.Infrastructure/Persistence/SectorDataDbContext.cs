using IndustryTrade.BuildingBlocks.Infrastructure.Outbox;
using IndustryTrade.Modules.SectorData.Domain.Clusters;
using IndustryTrade.Modules.SectorData.Domain.Observations;
using Microsoft.EntityFrameworkCore;

namespace IndustryTrade.Modules.SectorData.Infrastructure.Persistence;

/// <summary>Owns the <c>sector</c> schema (schema-per-bounded-context, docs/design/04 §1).</summary>
public sealed class SectorDataDbContext(DbContextOptions<SectorDataDbContext> options) : DbContext(options)
{
    public const string Schema = "sector";

    public DbSet<IndicatorObservation> Observations => Set<IndicatorObservation>();
    public DbSet<IndustrialCluster> Clusters => Set<IndustrialCluster>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SectorDataDbContext).Assembly);

        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.ToTable("outbox_message");
            b.HasKey(x => x.Id);
            b.Property(x => x.Type).HasMaxLength(500);
            b.HasIndex(x => x.ProcessedOnUtc);
        });
    }
}
