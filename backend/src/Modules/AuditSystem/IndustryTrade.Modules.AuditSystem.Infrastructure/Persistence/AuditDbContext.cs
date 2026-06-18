using IndustryTrade.Modules.AuditSystem.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndustryTrade.Modules.AuditSystem.Infrastructure.Persistence;

/// <summary>Owns the <c>audit</c> schema (schema-per-bounded-context, docs/design/04 §1, §3.6).</summary>
public sealed class AuditDbContext(DbContextOptions<AuditDbContext> options) : DbContext(options)
{
    public const string Schema = "audit";

    public DbSet<AuditLogEntry> Entries => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditDbContext).Assembly);
    }
}

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> builder)
    {
        builder.ToTable("log_entry");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Actor).HasMaxLength(100);
        builder.Property(x => x.Action).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Payload).HasColumnType("jsonb");
        builder.Property(x => x.Error).HasColumnType("text");
        builder.HasIndex(x => x.AtUtc);
        builder.HasIndex(x => x.Actor);
        builder.HasIndex(x => x.Action);
        builder.Ignore(x => x.DomainEvents);
    }
}
