using IndustryTrade.Modules.Reporting.Domain.Campaigns;
using IndustryTrade.Modules.Reporting.Domain.Submissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndustryTrade.Modules.Reporting.Infrastructure.Persistence;

internal sealed class CampaignConfiguration : IEntityTypeConfiguration<ReportingCampaign>
{
    public void Configure(EntityTypeBuilder<ReportingCampaign> builder)
    {
        builder.ToTable("campaign");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>();
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Ignore(x => x.DomainEvents);
    }
}

internal sealed class SubmissionConfiguration : IEntityTypeConfiguration<ReportSubmission>
{
    public void Configure(EntityTypeBuilder<ReportSubmission> builder)
    {
        builder.ToTable("report_submission");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.State).HasConversion<int>();
        builder.HasIndex(x => new { x.OrgUnitId, x.State });
        builder.HasIndex(x => x.CampaignId);
        builder.Ignore(x => x.DomainEvents);

        // The submission owns its transition history (separate table, no aggregate of its own).
        builder.OwnsMany(x => x.History, h =>
        {
            h.ToTable("report_transition");
            h.WithOwner().HasForeignKey("SubmissionId");
            h.Property<int>("Id");
            h.HasKey("Id");
            h.Property(t => t.FromState).HasConversion<int>();
            h.Property(t => t.ToState).HasConversion<int>();
            h.Property(t => t.Action).HasMaxLength(50).IsRequired();
            h.Property(t => t.ActorName).HasMaxLength(100);
            h.Property(t => t.Note).HasColumnType("text");
            h.HasIndex("SubmissionId");
        });
        builder.Navigation(x => x.History).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Auto-extracted report content (indicator lines + values), bound to a Catalog template.
        builder.OwnsMany(x => x.Lines, l =>
        {
            l.ToTable("report_line");
            l.WithOwner().HasForeignKey("SubmissionId");
            l.Property<int>("Id");
            l.HasKey("Id");
            l.Property(line => line.IndicatorCode).HasMaxLength(50);
            l.Property(line => line.Label).HasMaxLength(250).IsRequired();
            l.Property(line => line.ValueText).HasColumnType("text");
            l.HasIndex("SubmissionId");
        });
        builder.Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
