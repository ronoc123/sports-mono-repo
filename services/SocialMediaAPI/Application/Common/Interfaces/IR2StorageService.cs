namespace Application.Common.Interfaces;

public interface IR2StorageService
{
    /// <summary>Uploads a stream to R2 under the given object key.</summary>
    Task UploadAsync(string objectKey, Stream content, string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>Downloads an R2 object and returns the response stream.</summary>
    Task<Stream> DownloadAsync(string objectKey, CancellationToken cancellationToken = default);

    /// <summary>Returns a time-limited presigned URL for direct access to an object.</summary>
    Task<string> GetPresignedUrlAsync(string objectKey, TimeSpan expiry,
        CancellationToken cancellationToken = default);

    /// <summary>Permanently deletes an object from R2. No-ops if the object does not exist.</summary>
    Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default);
}
