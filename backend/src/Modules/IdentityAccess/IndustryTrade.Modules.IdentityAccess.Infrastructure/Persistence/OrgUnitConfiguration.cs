using IndustryTrade.Modules.IdentityAccess.Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndustryTrade.Modules.IdentityAccess.Infrastructure.Persistence;

internal sealed class OrgUnitConfiguration : IEntityTypeConfiguration<OrgUnit>
{
    public void Configure(EntityTypeBuilder<OrgUnit> builder)
    {
        builder.ToTable("org_unit");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Type).HasConversion<int>();

        // Materialized tree path as PostgreSQL `ltree`, with a GIST index for fast subtree /
        // data-scope queries via the `<@` descendant operator (docs/design/04 §3.1).
        builder.Property(x => x.Path).HasColumnType("ltree").IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.ParentId);
        builder.HasIndex(x => x.Path).HasMethod("gist");

        // Self-reference for the tree (no FK to other schemas; this is within `identity`).
        builder.HasOne<OrgUnit>()
            .WithMany()
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
