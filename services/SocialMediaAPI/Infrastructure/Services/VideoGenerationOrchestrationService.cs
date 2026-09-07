using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class VideoGenerationOrchestrationService : IVideoGenerationOrchestrationService
{
    private readonly IVideoGenerationJobRepository _jobRepository;
    private readonly IVideoGenerationAdapter _adapter;
    private readonly ILogger<VideoGenerationOrchestrationService> _logger;

    public VideoGenerationOrchestrationService(
        IVideoGenerationJobRepository jobRepository,
        IVideoGenerationAdapter adapter,
        ILogger<VideoGenerationOrchestrationService> logger)
    {
        _jobRepository = jobRepository;
        _adapter = adapter;
        _logger = logger;
    }

    public async Task RunAsync(string jobId, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken);
        if (job is null)
        {
            _logger.LogWarning("VideoGenerationJob {JobId} not found.", jobId);
            return;
        }

        job.Status = "Generating";
        await _jobRepository.UpdateAsync(job, cancellationToken);

        string? imageTempPath = job.ImageTempPath;

        try
        {
            _logger.LogInformation("Starting video generation for job {JobId}", jobId);

            var request = new VideoGenerationRequest
            {
                ChannelId = job.ChannelId,
                ImageTempPath = job.ImageTempPath,
                RenderedPrompt = job.RenderedPrompt,
            };

            var result = await _adapter.GenerateAsync(request, cancellationToken);

            if (!string.IsNullOrEmpty(result.VideoPath))
            {
                job.Status = "Ready";
                job.VideoTempPath = result.VideoPath;
                job.HiggsFieldModel = result.HiggsFieldModel;
                job.CompletedAt = DateTime.UtcNow;
                _logger.LogInformation("Video generation ready for job {JobId}: {Path}", jobId, result.VideoPath);
            }
            else
            {
                job.Status = "Failed";
                job.ErrorMessage = "Video generation completed but no video file was returned.";
                job.CompletedAt = DateTime.UtcNow;
                _logger.LogWarning("Video generation returned no video for job {JobId}", jobId);
            }
        }
        catch (Exception ex)
        {
            job.Status = "Failed";
            job.ErrorMessage = ex.Message;
            job.CompletedAt = DateTime.UtcNow;
            _logger.LogError(ex, "Video generation failed for job {JobId}", jobId);
        }
        finally
        {
            await _jobRepository.UpdateAsync(job, cancellationToken);

            // Always delete the image temp file — it served its purpose
            CleanupFile(imageTempPath);
        }
    }

    private static void CleanupFile(string? path)
    {
        if (string.IsNullOrEmpty(path)) return;
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // Best-effort; TempFileCleanupService handles orphaned files
        }
    }
}
