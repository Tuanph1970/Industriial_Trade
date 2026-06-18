using IndustryTrade.Modules.Notifications.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndustryTrade.Modules.Notifications.Infrastructure.Persistence;

/// <summary>Owns the <c>notifications</c> schema (schema-per-bounded-context, docs/design/04 §1).</summary>
public sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public const string Schema = "notifications";

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
    }
}

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notification");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Recipient).HasMaxLength(100);
        builder.Property(x => x.Title).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Message).HasColumnType("text").IsRequired();
        builder.Property(x => x.Category).HasMaxLength(50).IsRequired();
        builder.Property(x => x.RefId).HasMaxLength(100);
        builder.HasIndex(x => new { x.Recipient, x.IsRead });
        builder.HasIndex(x => x.CreatedAtUtc);
        builder.Ignore(x => x.DomainEvents);
    }
}
