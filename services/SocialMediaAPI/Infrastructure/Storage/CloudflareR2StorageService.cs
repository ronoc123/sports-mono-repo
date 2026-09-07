using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Storage;

public class CloudflareR2StorageService : IR2StorageService, IDisposable
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucketName;

    public CloudflareR2StorageService(IConfiguration configuration)
    {
        var accountId  = configuration["CloudflareR2:AccountId"]  ?? throw new InvalidOperationException("CloudflareR2:AccountId is required.");
        var accessKey  = configuration["CloudflareR2:AccessKey"]  ?? throw new InvalidOperationException("CloudflareR2:AccessKey is required.");
        var secretKey  = configuration["CloudflareR2:SecretKey"]  ?? throw new InvalidOperationException("CloudflareR2:SecretKey is required.");
        _bucketName    = configuration["CloudflareR2:BucketName"] ?? throw new InvalidOperationException("CloudflareR2:BucketName is required.");

        var credentials = new BasicAWSCredentials(accessKey, secretKey);
        var config = new AmazonS3Config
        {
            ServiceURL       = $"https://{accountId}.r2.cloudflarestorage.com",
            ForcePathStyle   = true,
            AuthenticationRegion = "auto",
        };

        _s3 = new AmazonS3Client(credentials, config);
    }

    public async Task UploadAsync(
        string objectKey,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var request = new PutObjectRequest
        {
            BucketName       = _bucketName,
            Key              = objectKey,
            InputStream      = content,
            ContentType      = contentType,
            AutoCloseStream  = false,
            UseChunkEncoding = false, // R2 does not support STREAMING-AWS4-HMAC-SHA256-PAYLOAD-TRAILER
        };

        await _s3.PutObjectAsync(request, cancellationToken);
    }

    public async Task<Stream> DownloadAsync(
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        var request = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key        = objectKey,
        };

        var response = await _s3.GetObjectAsync(request, cancellationToken);
        return response.ResponseStream;
    }

    public async Task<string> GetPresignedUrlAsync(
        string objectKey,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key        = objectKey,
            Expires    = DateTime.UtcNow.Add(expiry),
            Protocol   = Protocol.HTTPS,
            Verb       = HttpVerb.GET,
        };

        return await Task.FromResult(_s3.GetPreSignedURL(request));
    }

    public void Dispose() => _s3.Dispose();
}
