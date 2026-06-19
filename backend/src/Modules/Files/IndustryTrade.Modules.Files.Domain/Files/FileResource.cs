using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.Files.Domain.Files;

/// <summary>
/// Metadata for a stored file/document (UC-4). The bytes live in object storage (MinIO/S3) under
/// <see cref="ObjectKey"/>; this aggregate is the catalogued, searchable, audited record of it.
/// Optionally tagged with a free-text <see cref="Category"/> and linked to a source entity.
/// </summary>
public sealed class FileResource : AggregateRoot<Guid>, IAuditable
{
    private FileResource() { } // EF

    private FileResource(Guid id, string fileName, string contentType, long sizeBytes,
        string objectKey, string? category) : base(id)
    {
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        ObjectKey = objectKey;
        Category = category;
    }

    public string FileName { get; private set; } = default!;
    public string ContentType { get; private set; } = default!;
    public long SizeBytes { get; private set; }
    /// <summary>Storage key (object name) in the bucket — opaque to callers.</summary>
    public string ObjectKey { get; private set; } = default!;
    public string? Category { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public string? ModifiedBy { get; private set; }

    public static FileResource Create(string fileName, string contentType, long sizeBytes,
        string objectKey, string? category)
    {
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name is required.", nameof(fileName));
        if (string.IsNullOrWhiteSpace(objectKey)) throw new ArgumentException("Object key is required.", nameof(objectKey));
        if (sizeBytes < 0) throw new ArgumentOutOfRangeException(nameof(sizeBytes));

        return new FileResource(Guid.NewGuid(), fileName.Trim(),
            string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            sizeBytes, objectKey, string.IsNullOrWhiteSpace(category) ? null : category.Trim())
        {
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
