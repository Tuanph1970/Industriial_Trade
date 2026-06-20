using IndustryTrade.BuildingBlocks.Infrastructure.Outbox;
using IndustryTrade.Modules.IdentityAccess.Domain.Organizations;
using IndustryTrade.Modules.IdentityAccess.Domain.Roles;
using IndustryTrade.Modules.IdentityAccess.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace IndustryTrade.Modules.IdentityAccess.Infrastructure.Persistence;

/// <summary>
/// Owns the <c>identity</c> schema (schema-per-bounded-context, docs/design/04 §1).
/// No cross-schema foreign keys — links to other contexts are by id only.
/// </summary>
public sealed class IdentityAccessDbContext(DbContextOptions<IdentityAccessDbContext> options)
    : DbContext(options)
{
    public const string Schema = "identity";

    public DbSet<OrgUnit> OrgUnits => Set<OrgUnit>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserAccount> Users => Set<UserAccount>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.HasPostgresExtension("ltree"); // org-unit tree path type + GIST index
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityAccessDbContext).Assembly);

        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.ToTable("outbox_message");
            b.HasKey(x => x.Id);
            b.Property(x => x.Type).HasMaxLength(500);
            b.HasIndex(x => x.ProcessedOnUtc);
        });
    }
}
