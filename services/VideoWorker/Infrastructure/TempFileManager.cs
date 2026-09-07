using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace VideoWorker.Infrastructure;

public class TempFileManager
{
    private readonly string _baseDirectory;
    private readonly ILogger<TempFileManager> _logger;

    public TempFileManager(IConfiguration configuration, ILogger<TempFileManager> logger)
    {
        _baseDirectory = configuration["Worker:TempDirectory"] ?? Path.Combine(Path.GetTempPath(), "video-generation");
        _logger = logger;
    }

    /// <summary>Creates a job-scoped temporary directory and returns its path.</summary>
    public string CreateJobDirectory(string jobId)
    {
        var path = Path.Combine(_baseDirectory, jobId);
        Directory.CreateDirectory(path);
        _logger.LogDebug("Created temp directory: {Path}", path);
        return path;
    }

    /// <summary>Deletes the directory and all its contents. Never throws.</summary>
    public void Cleanup(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
                _logger.LogDebug("Cleaned up temp directory: {Path}", directory);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to clean up temp directory {Path} — will be retried later", directory);
        }
    }
}
