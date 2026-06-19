using IndustryTrade.Modules.Files.Application.Files;
using Minio;
using Minio.DataModel.Args;

namespace IndustryTrade.Modules.Files.Infrastructure.Storage;

public sealed class MinioOptions
{
    public string Endpoint { get; set; } = "localhost:9000";
    public string AccessKey { get; set; } = "minioadmin";
    public string SecretKey { get; set; } = "minioadmin";
    public string Bucket { get; set; } = "industrytrade";
    public bool UseSsl { get; set; }
}

/// <summary>MinIO/S3-backed <see cref="IObjectStorage"/>. The bucket is created on first write.</summary>
internal sealed class MinioObjectStorage : IObjectStorage
{
    private readonly IMinioClient _client;
    private readonly string _bucket;
    private bool _bucketChecked;

    public MinioObjectStorage(MinioOptions options)
    {
        _client = new MinioClient()
            .WithEndpoint(options.Endpoint)
            .WithCredentials(options.AccessKey, options.SecretKey)
            .WithSSL(options.UseSsl)
            .Build();
        _bucket = options.Bucket;
    }

    public async Task PutAsync(string objectKey, Stream content, long size, string contentType, CancellationToken ct)
    {
        await EnsureBucketAsync(ct);
        await _client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_bucket).WithObject(objectKey)
            .WithStreamData(content).WithObjectSize(size).WithContentType(contentType), ct);
    }

    public async Task<Stream> GetAsync(string objectKey, CancellationToken ct)
    {
        var ms = new MemoryStream();
        await _client.GetObjectAsync(new GetObjectArgs()
            .WithBucket(_bucket).WithObject(objectKey)
            .WithCallbackStream(s => s.CopyTo(ms)), ct);
        ms.Position = 0;
        return ms;
    }

    public Task RemoveAsync(string objectKey, CancellationToken ct) =>
        _client.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(_bucket).WithObject(objectKey), ct);

    private async Task EnsureBucketAsync(CancellationToken ct)
    {
        if (_bucketChecked) return;
        var exists = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucket), ct);
        if (!exists)
            await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucket), ct);
        _bucketChecked = true;
    }
}
