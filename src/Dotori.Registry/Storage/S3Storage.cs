using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace Dotori.Registry.Storage;

/// <summary>
/// S3-compatible object storage backend.
/// Configure via environment variables:
///   AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, AWS_REGION, S3_BUCKET
/// For MinIO: also set S3_ENDPOINT (e.g. http://minio:9000)
/// </summary>
public sealed class S3Storage : IPackageStorage, IAsyncDisposable
{
    private readonly IAmazonS3 _client;
    private readonly string _bucket;

    public S3Storage()
    {
        var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID")
            ?? throw new InvalidOperationException("AWS_ACCESS_KEY_ID not set");
        var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")
            ?? throw new InvalidOperationException("AWS_SECRET_ACCESS_KEY not set");
        var region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";
        _bucket = Environment.GetEnvironmentVariable("S3_BUCKET")
            ?? throw new InvalidOperationException("S3_BUCKET not set");

        var credentials = new BasicAWSCredentials(accessKey, secretKey);
        var endpoint = Environment.GetEnvironmentVariable("S3_ENDPOINT");

        if (endpoint is not null)
        {
            // MinIO or S3-compatible endpoint
            var config = new AmazonS3Config
            {
                ServiceURL = endpoint,
                ForcePathStyle = true,
            };
            _client = new AmazonS3Client(credentials, config);
        }
        else
        {
            _client = new AmazonS3Client(credentials, RegionEndpoint.GetBySystemName(region));
        }
    }

    private static string Key(string owner, string name, string version) =>
        $"{owner}/{name}/{version}/{name}-{version}.dotori-pkg";

    public async Task<Stream> GetArchiveAsync(string owner, string name, string version, CancellationToken ct = default)
    {
        var response = await _client.GetObjectAsync(new GetObjectRequest
        {
            BucketName = _bucket,
            Key = Key(owner, name, version),
        }, ct);
        return response.ResponseStream;
    }

    public async Task SaveArchiveAsync(string owner, string name, string version, Stream data, CancellationToken ct = default)
    {
        await _client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _bucket,
            Key = Key(owner, name, version),
            InputStream = data,
            ContentType = "application/octet-stream",
        }, ct);
    }

    public async Task<bool> ExistsAsync(string owner, string name, string version, CancellationToken ct = default)
    {
        try
        {
            await _client.GetObjectMetadataAsync(new GetObjectMetadataRequest
            {
                BucketName = _bucket,
                Key = Key(owner, name, version),
            }, ct);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task DeleteArchiveAsync(string owner, string name, string version, CancellationToken ct = default)
    {
        await _client.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = _bucket,
            Key = Key(owner, name, version),
        }, ct);
    }

    public ValueTask DisposeAsync()
    {
        _client.Dispose();
        return ValueTask.CompletedTask;
    }
}
