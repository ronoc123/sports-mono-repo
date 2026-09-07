using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace VideoWorker.Infrastructure;

public class R2AssetService : IDisposable
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucketName;
    private readonly ILogger<R2AssetService> _logger;

    public R2AssetService(IConfiguration configuration, ILogger<R2AssetService> logger)
    {
        var accountId = configuration["CloudflareR2:AccountId"]  ?? throw new InvalidOperationException("CloudflareR2:AccountId is required.");
        var accessKey = configuration["CloudflareR2:AccessKey"]  ?? throw new InvalidOperationException("CloudflareR2:AccessKey is required.");
        var secretKey = configuration["CloudflareR2:SecretKey"]  ?? throw new InvalidOperationException("CloudflareR2:SecretKey is required.");
        _bucketName   = configuration["CloudflareR2:BucketName"] ?? throw new InvalidOperationException("CloudflareR2:BucketName is required.");

        var credentials = new BasicAWSCredentials(accessKey, secretKey);
        var config = new AmazonS3Config
        {
            ServiceURL           = $"https://{accountId}.r2.cloudflarestorage.com",
            ForcePathStyle       = true,
            AuthenticationRegion = "auto",
        };

        _s3 = new AmazonS3Client(credentials, config);
        _logger = logger;
    }

    /// <summary>
    /// Downloads an R2 object to a local file inside localDirectory.
    /// The file name is derived from the object key.
    /// Returns the local absolute path.
    /// </summary>
    public async Task<string> DownloadToLocalAsync(
        string objectKey,
        string localDirectory,
        CancellationToken cancellationToken)
    {
        var fileName  = Path.GetFileName(objectKey);
        var localPath = Path.Combine(localDirectory, fileName);

        _logger.LogDebug("Downloading R2 object {Key} → {LocalPath}", objectKey, localPath);

        var request = new GetObjectRequest { BucketName = _bucketName, Key = objectKey };
        using var response = await _s3.GetObjectAsync(request, cancellationToken);
        await using var fileStream = File.Create(localPath);
        await response.ResponseStream.CopyToAsync(fileStream, cancellationToken);

        _logger.LogDebug("Downloaded {Key} ({Bytes} bytes)", objectKey, new FileInfo(localPath).Length);

        return localPath;
    }

    /// <summary>Uploads a local file to R2 under the given object key.</summary>
    public async Task UploadFromLocalAsync(
        string localPath,
        string objectKey,
        string contentType,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Uploading {LocalPath} → R2 {Key}", localPath, objectKey);

        await using var fileStream = File.OpenRead(localPath);
        var request = new PutObjectRequest
        {
            BucketName       = _bucketName,
            Key              = objectKey,
            InputStream      = fileStream,
            ContentType      = contentType,
            AutoCloseStream  = false,
            UseChunkEncoding = false, // R2 does not support STREAMING-AWS4-HMAC-SHA256-PAYLOAD-TRAILER
        };

        await _s3.PutObjectAsync(request, cancellationToken);
        _logger.LogDebug("Uploaded {Key}", objectKey);
    }

    public void Dispose() => _s3.Dispose();
}
