using IndustryTrade.Modules.IdentityAccess.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndustryTrade.Modules.IdentityAccess.Infrastructure.Persistence;

internal sealed class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
    public void Configure(EntityTypeBuilder<UserAccount> builder)
    {
        builder.ToTable("user_account");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.FullName).HasMaxLength(250);
        builder.Property(x => x.Email).HasMaxLength(250);
        builder.Property(x => x.SubjectId).HasMaxLength(100);
        builder.Property(x => x.RoleIds).HasColumnType("uuid[]"); // Guid[] → uuid[]
        builder.HasIndex(x => x.UserName).IsUnique();
        builder.HasIndex(x => x.SubjectId);
        builder.HasIndex(x => x.OrgUnitId);
        builder.Ignore(x => x.DomainEvents);
    }
}
