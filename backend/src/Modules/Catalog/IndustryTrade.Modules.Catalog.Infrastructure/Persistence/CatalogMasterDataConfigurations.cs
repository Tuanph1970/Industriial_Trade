using IndustryTrade.Modules.Catalog.Domain.IndicatorSets;
using IndustryTrade.Modules.Catalog.Domain.ReportingPeriods;
using IndustryTrade.Modules.Catalog.Domain.ReportTemplates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndustryTrade.Modules.Catalog.Infrastructure.Persistence;

internal sealed class IndicatorSetConfiguration : IEntityTypeConfiguration<IndicatorSet>
{
    public void Configure(EntityTypeBuilder<IndicatorSet> builder)
    {
        builder.ToTable("indicator_set");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.IndicatorIds).HasColumnType("uuid[]"); // Guid[] → uuid[]
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Ignore(x => x.DomainEvents);
    }
}

internal sealed class ReportTemplateConfiguration : IEntityTypeConfiguration<ReportTemplate>
{
    public void Configure(EntityTypeBuilder<ReportTemplate> builder)
    {
        builder.ToTable("report_template");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Description).HasColumnType("text");
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Ignore(x => x.DomainEvents);

        builder.OwnsMany(x => x.Lines, line =>
        {
            line.ToTable("report_template_line");
            line.WithOwner().HasForeignKey("TemplateId");
            line.Property<int>("Id");
            line.HasKey("Id");
            line.Property(l => l.Label).HasMaxLength(250).IsRequired();
            line.HasIndex("TemplateId");
        });
        builder.Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ReportingPeriodConfiguration : IEntityTypeConfiguration<ReportingPeriodDefinition>
{
    public void Configure(EntityTypeBuilder<ReportingPeriodDefinition> builder)
    {
        builder.ToTable("reporting_period_definition");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Periodicity).HasConversion<int>();
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Ignore(x => x.DomainEvents);
    }
}
