using IndustryTrade.Modules.Integration.Domain.Services;
using IndustryTrade.Modules.Integration.Domain.Status;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndustryTrade.Modules.Integration.Infrastructure.Persistence;

/// <summary>Owns the <c>integration</c> schema (schema-per-bounded-context, docs/design/04 §1).</summary>
public sealed class IntegrationDbContext(DbContextOptions<IntegrationDbContext> options) : DbContext(options)
{
    public const string Schema = "integration";

    public DbSet<DataSharingService> Services => Set<DataSharingService>();
    public DbSet<ConnectionStatusCheck> StatusChecks => Set<ConnectionStatusCheck>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IntegrationDbContext).Assembly);
    }
}

internal sealed class ServiceConfiguration : IEntityTypeConfiguration<DataSharingService>
{
    public void Configure(EntityTypeBuilder<DataSharingService> builder)
    {
        builder.ToTable("data_sharing_service");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Direction).HasConversion<int>();
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.EndpointUrl).HasMaxLength(500);
        builder.Property(x => x.Description).HasColumnType("text");
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Ignore(x => x.DomainEvents);
    }
}

internal sealed class StatusCheckConfiguration : IEntityTypeConfiguration<ConnectionStatusCheck>
{
    public void Configure(EntityTypeBuilder<ConnectionStatusCheck> builder)
    {
        builder.ToTable("connection_status_check");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Component).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Detail).HasMaxLength(500);
        builder.HasIndex(x => x.CheckedAtUtc);
        builder.Ignore(x => x.DomainEvents);
    }
}
