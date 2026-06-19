using IndustryTrade.Modules.Catalog.Domain.AdministrativeUnits;
using IndustryTrade.Modules.Catalog.Domain.Classifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndustryTrade.Modules.Catalog.Infrastructure.Persistence;

internal sealed class AdministrativeUnitConfiguration : IEntityTypeConfiguration<AdministrativeUnit>
{
    public void Configure(EntityTypeBuilder<AdministrativeUnit> builder)
    {
        builder.ToTable("administrative_unit");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Level).HasConversion<int>();
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.ParentId);
        builder.Ignore(x => x.DomainEvents);
    }
}

internal sealed class ClassificationConfiguration : IEntityTypeConfiguration<Classification>
{
    public void Configure(EntityTypeBuilder<Classification> builder)
    {
        builder.ToTable("classification");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Description).HasColumnType("text");
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Ignore(x => x.DomainEvents);

        builder.OwnsMany(x => x.Items, item =>
        {
            item.ToTable("classification_item");
            item.WithOwner().HasForeignKey("ClassificationId");
            item.Property<int>("Id");
            item.HasKey("Id");
            item.Property(i => i.Code).HasMaxLength(50).IsRequired();
            item.Property(i => i.Name).HasMaxLength(250).IsRequired();
            item.HasIndex("ClassificationId");
        });
        builder.Navigation(x => x.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
