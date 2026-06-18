using IndustryTrade.BuildingBlocks.Infrastructure.Outbox;
using IndustryTrade.Modules.Reporting.Domain.Campaigns;
using IndustryTrade.Modules.Reporting.Domain.Submissions;
using Microsoft.EntityFrameworkCore;

namespace IndustryTrade.Modules.Reporting.Infrastructure.Persistence;

/// <summary>Owns the <c>reporting</c> schema (schema-per-bounded-context, docs/design/04 §1).</summary>
public sealed class ReportingDbContext(DbContextOptions<ReportingDbContext> options) : DbContext(options)
{
    public const string Schema = "reporting";

    public DbSet<ReportingCampaign> Campaigns => Set<ReportingCampaign>();
    public DbSet<ReportSubmission> Submissions => Set<ReportSubmission>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReportingDbContext).Assembly);

        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.ToTable("outbox_message");
            b.HasKey(x => x.Id);
            b.Property(x => x.Type).HasMaxLength(500);
            b.HasIndex(x => x.ProcessedOnUtc);
        });
    }
}
