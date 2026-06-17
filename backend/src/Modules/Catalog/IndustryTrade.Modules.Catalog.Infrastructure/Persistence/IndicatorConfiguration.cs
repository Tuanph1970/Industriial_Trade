using IndustryTrade.Modules.Catalog.Domain.Indicators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndustryTrade.Modules.Catalog.Infrastructure.Persistence;

internal sealed class IndicatorConfiguration : IEntityTypeConfiguration<Indicator>
{
    public void Configure(EntityTypeBuilder<Indicator> builder)
    {
        builder.ToTable("indicator");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Unit).HasMaxLength(50);
        builder.Property(x => x.DataType).HasConversion<int>();
        builder.Property(x => x.Sector).HasConversion<int>();
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Sector);
        builder.Ignore(x => x.IsActive);       // computed from RetiredAt
        builder.Ignore(x => x.DomainEvents);
    }
}
