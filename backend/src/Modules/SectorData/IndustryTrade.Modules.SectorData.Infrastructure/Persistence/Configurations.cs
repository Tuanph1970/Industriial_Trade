using IndustryTrade.Modules.SectorData.Domain.Clusters;
using IndustryTrade.Modules.SectorData.Domain.CommerceLocations;
using IndustryTrade.Modules.SectorData.Domain.Ecommerce;
using IndustryTrade.Modules.SectorData.Domain.Observations;
using IndustryTrade.Modules.SectorData.Domain.PetroleumStations;
using IndustryTrade.Modules.SectorData.Domain.Violations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndustryTrade.Modules.SectorData.Infrastructure.Persistence;

internal sealed class ObservationConfiguration : IEntityTypeConfiguration<IndicatorObservation>
{
    public void Configure(EntityTypeBuilder<IndicatorObservation> builder)
    {
        builder.ToTable("indicator_observation");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Value).HasColumnType("numeric(18,4)");
        builder.Property(x => x.ValueText).HasMaxLength(1000);
        builder.Property(x => x.Source).HasMaxLength(250);
        builder.Property(x => x.Status).HasConversion<int>();
        // Lookup + data-scope (by org unit) + period filtering.
        builder.HasIndex(x => new { x.IndicatorId, x.OrgUnitId, x.PeriodYear });
        builder.HasIndex(x => x.OrgUnitId);
        builder.Ignore(x => x.DomainEvents);
    }
}

internal sealed class ClusterConfiguration : IEntityTypeConfiguration<IndustrialCluster>
{
    public void Configure(EntityTypeBuilder<IndustrialCluster> builder)
    {
        builder.ToTable("industrial_cluster");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        builder.Property(x => x.AreaHa).HasColumnType("numeric(12,2)");
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.Location).HasColumnType("geometry (Point, 4326)");
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.OrgUnitId);
        builder.HasIndex(x => x.Location).HasMethod("gist"); // PostGIS spatial index
        builder.Ignore(x => x.DomainEvents);
    }
}

internal sealed class ViolationConfiguration : IEntityTypeConfiguration<MarketViolationCase>
{
    public void Configure(EntityTypeBuilder<MarketViolationCase> builder)
    {
        builder.ToTable("market_violation_case");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CaseNo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Group).HasConversion<int>();
        builder.Property(x => x.BusinessName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.ViolationContent).HasColumnType("text").IsRequired();
        builder.Property(x => x.SanctionContent).HasColumnType("text");
        builder.Property(x => x.FineAmount).HasColumnType("numeric(18,2)");
        builder.Property(x => x.Status).HasConversion<int>();
        builder.HasIndex(x => x.CaseNo).IsUnique();
        builder.HasIndex(x => new { x.OrgUnitId, x.Group });
        builder.Ignore(x => x.DomainEvents);
    }
}

internal sealed class PetroleumStationConfiguration : IEntityTypeConfiguration<PetroleumStation>
{
    public void Configure(EntityTypeBuilder<PetroleumStation> builder)
    {
        builder.ToTable("petroleum_station");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        builder.Property(x => x.LicenseNo).HasMaxLength(100);
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.Location).HasColumnType("geometry (Point, 4326)");
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.OrgUnitId);
        builder.HasIndex(x => x.Location).HasMethod("gist");
        builder.Ignore(x => x.DomainEvents);
    }
}

internal sealed class CommerceLocationConfiguration : IEntityTypeConfiguration<CommerceLocation>
{
    public void Configure(EntityTypeBuilder<CommerceLocation> builder)
    {
        builder.ToTable("commerce_location");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Type).HasConversion<int>();
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.Location).HasColumnType("geometry (Point, 4326)");
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => new { x.OrgUnitId, x.Type });
        builder.HasIndex(x => x.Location).HasMethod("gist");
        builder.Ignore(x => x.DomainEvents);
    }
}

internal sealed class EcommerceParticipantConfiguration : IEntityTypeConfiguration<EcommerceParticipant>
{
    public void Configure(EntityTypeBuilder<EcommerceParticipant> builder)
    {
        builder.ToTable("ecommerce_participant");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TaxCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.BusinessName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Platforms).HasColumnType("text[]");
        builder.Property(x => x.MainGoods).HasMaxLength(1000);
        builder.HasIndex(x => x.TaxCode).IsUnique();
        builder.HasIndex(x => x.OrgUnitId);
        builder.Ignore(x => x.DomainEvents);
    }
}
