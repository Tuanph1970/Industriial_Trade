using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.BuildingBlocks.Infrastructure.Persistence;
using IndustryTrade.Modules.Files.Application.Files;
using IndustryTrade.Modules.Files.Domain.Files;
using Microsoft.EntityFrameworkCore;

namespace IndustryTrade.Modules.Files.Infrastructure.Persistence;

internal sealed class FileResourceRepository(FilesDbContext db) : IFileResourceRepository
{
    public Task<FileResource?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Files.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<FileResource>> ListAsync(Specification<FileResource> spec, CancellationToken ct) =>
        await SpecificationEvaluator.Apply(db.Files.AsQueryable(), spec).ToListAsync(ct);

    public Task<int> CountAsync(Specification<FileResource> spec, CancellationToken ct) =>
        SpecificationEvaluator.Apply(db.Files.AsQueryable(), spec).CountAsync(ct);

    public async Task AddAsync(FileResource file, CancellationToken ct) => await db.Files.AddAsync(file, ct);

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct) =>
        await db.Files.Where(x => x.Id == id).ExecuteDeleteAsync(ct) > 0;

    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
