using Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class TempFileCleanupService : BackgroundService
{
    private static readonly TimeSpan RunInterval = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan JobTimeout = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan GenerationJobTimeout = TimeSpan.FromMinutes(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TempFileCleanupService> _logger;

    public TempFileCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<TempFileCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(RunInterval, stoppingToken);
            await CleanupAsync(stoppingToken);
        }
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IPostCycleRepository>();

            var cutoff = DateTime.UtcNow - JobTimeout;
            var staleJobs = await repo.GetStaleRunningJobsAsync(cutoff, cancellationToken);

            foreach (var job in staleJobs)
            {
                job.Status = "TimedOut";
                job.CompletedAt = DateTime.UtcNow;
                await repo.UpdateAsync(job, cancellationToken);

                try
                {
                    if (File.Exists(job.VideoPath))
                        File.Delete(job.VideoPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete orphaned temp file {Path}", job.VideoPath);
                }

                _logger.LogInformation("Timed out stale PostCycleJob {JobId}", job.Id);
            }

            // Promote stale VideoGenerationJobs (Queued/Generating > 5 min) to TimedOut
            var genRepo = scope.ServiceProvider.GetRequiredService<IVideoGenerationJobRepository>();
            var genCutoff = DateTime.UtcNow - GenerationJobTimeout;
            var staleGenJobs = await genRepo.GetStaleJobsAsync(genCutoff, cancellationToken);

            foreach (var genJob in staleGenJobs)
            {
                genJob.Status = "TimedOut";
                genJob.CompletedAt = DateTime.UtcNow;
                await genRepo.UpdateAsync(genJob, cancellationToken);

                DeleteFile(genJob.ImageTempPath);
                if (genJob.VideoTempPath is not null)
                    DeleteFile(genJob.VideoTempPath);

                _logger.LogInformation("Timed out stale VideoGenerationJob {JobId}", genJob.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TempFileCleanupService encountered an error");
        }
    }

    private void DeleteFile(string? path)
    {
        if (string.IsNullOrEmpty(path)) return;
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete temp file {Path}", path);
        }
    }
}
