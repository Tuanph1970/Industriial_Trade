using IndustryTrade.Modules.Files.Domain.Files;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndustryTrade.Modules.Files.Infrastructure.Persistence;

/// <summary>Owns the <c>files</c> schema (schema-per-bounded-context, docs/design/04 §1).</summary>
public sealed class FilesDbContext(DbContextOptions<FilesDbContext> options) : DbContext(options)
{
    public const string Schema = "files";

    public DbSet<FileResource> Files => Set<FileResource>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FilesDbContext).Assembly);
    }
}

internal sealed class FileResourceConfiguration : IEntityTypeConfiguration<FileResource>
{
    public void Configure(EntityTypeBuilder<FileResource> builder)
    {
        builder.ToTable("file_resource");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FileName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(150).IsRequired();
        builder.Property(x => x.ObjectKey).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(100);
        builder.HasIndex(x => x.ObjectKey).IsUnique();
        builder.HasIndex(x => x.Category);
        builder.Ignore(x => x.DomainEvents);
    }
}
