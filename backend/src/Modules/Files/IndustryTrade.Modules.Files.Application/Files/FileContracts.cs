using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.Modules.Files.Domain.Files;

namespace IndustryTrade.Modules.Files.Application.Files;

public static class FilePermissions
{
    public const string Read = "files.read";
    public const string Manage = "files.manage";
}

public sealed record FileResourceDto(
    Guid Id, string FileName, string ContentType, long SizeBytes, string? Category,
    string? UploadedBy, DateTime UploadedAtUtc)
{
    public static FileResourceDto FromEntity(FileResource f) =>
        new(f.Id, f.FileName, f.ContentType, f.SizeBytes, f.Category, f.CreatedBy, f.CreatedAtUtc);
}

/// <summary>Bytes + the headers needed to stream a download back to the client.</summary>
public sealed record FileDownload(string FileName, string ContentType, Stream Content);

public interface IFileResourceRepository
{
    Task<FileResource?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<FileResource>> ListAsync(Specification<FileResource> spec, CancellationToken ct);
    Task<int> CountAsync(Specification<FileResource> spec, CancellationToken ct);
    Task AddAsync(FileResource file, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}

/// <summary>Port over the object store (MinIO/S3). Keeps the domain/app free of the storage SDK.</summary>
public interface IObjectStorage
{
    Task PutAsync(string objectKey, Stream content, long size, string contentType, CancellationToken ct);
    Task<Stream> GetAsync(string objectKey, CancellationToken ct);
    Task RemoveAsync(string objectKey, CancellationToken ct);
}

public sealed class FileSearchSpec : Specification<FileResource>
{
    public FileSearchSpec(PageRequest request, string? category = null, bool forCount = false)
    {
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim().ToLower();
            Where(f => f.FileName.ToLower().Contains(kw));
        }
        if (!string.IsNullOrWhiteSpace(category))
            Where(f => f.Category == category);

        if (!forCount)
        {
            ApplyOrderByDescending(f => f.CreatedAtUtc);
            ApplyPaging(request.Skip, request.NormalizedPageSize);
        }
    }
}
