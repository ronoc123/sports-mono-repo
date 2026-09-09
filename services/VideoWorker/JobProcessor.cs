using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VideoWorker.Abstractions;
using VideoWorker.Infrastructure;

namespace VideoWorker;

/// <summary>
/// Orchestrates the complete lifecycle of a single render job:
/// download assets → generate video → upload result → update job status → cleanup.
/// Writes a heartbeat to MongoDB during generation so stale-job recovery can
/// distinguish an active job from a crashed worker.
/// </summary>
public class JobProcessor
{
    private readonly MongoJobQueue _jobQueue;
    private readonly R2AssetService _r2;
    private readonly TempFileManager _tempFiles;
    private readonly IVideoGenerator _generator;
    private readonly TimeSpan _heartbeatInterval;
    private readonly ILogger<JobProcessor> _logger;

    public JobProcessor(
        MongoJobQueue jobQueue,
        R2AssetService r2,
        TempFileManager tempFiles,
        IVideoGenerator generator,
        IConfiguration configuration,
        ILogger<JobProcessor> logger)
    {
        _jobQueue  = jobQueue;
        _r2        = r2;
        _tempFiles = tempFiles;
        _generator = generator;
        _logger    = logger;
        _heartbeatInterval = TimeSpan.FromSeconds(
            double.TryParse(configuration["Worker:HeartbeatIntervalSeconds"], out var h) ? h : 30);
    }

    public async Task ProcessAsync(RenderJobDocument job, CancellationToken cancellationToken)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["JobId"] = job.Id,
            ["Model"] = job.Model,
        });

        var started = DateTime.UtcNow;
        _logger.LogInformation("Processing job (model={Model}, duration={Duration}s, prompt={Prompt})",
            job.Model, job.DurationSeconds, Truncate(job.Prompt, 80));

        var tempDir = _tempFiles.CreateJobDirectory(job.Id);

        // Heartbeat runs independently of the generation cancellation token so it
        // continues writing even if the generator is doing a long blocking call.
        using var heartbeatCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var heartbeatTask = RunHeartbeatAsync(job.Id, heartbeatCts.Token);

        try
        {
            // 1. Download reference images from R2
            var refImagePaths = new List<string>();
            foreach (var key in job.ReferenceImageKeys)
                refImagePaths.Add(await _r2.DownloadToLocalAsync(key, tempDir, cancellationToken));

            // 2. Download keyframes from R2 (may be empty)
            var keyframePaths = new List<string>();
            foreach (var key in job.KeyframeKeys)
                keyframePaths.Add(await _r2.DownloadToLocalAsync(key, tempDir, cancellationToken));

            // 3. Download reference video from R2 for video-to-video conditioning (optional)
            string? referenceVideoPath = null;
            if (!string.IsNullOrEmpty(job.ReferenceVideoKey))
            {
                referenceVideoPath = await _r2.DownloadToLocalAsync(job.ReferenceVideoKey, tempDir, cancellationToken);
                _logger.LogInformation("Reference video downloaded for v2v conditioning: {VideoPath}", referenceVideoPath);
            }

            _logger.LogInformation("Assets downloaded: {RefCount} reference image(s), {FrameCount} keyframe(s), referenceVideo={HasRefVideo}",
                refImagePaths.Count, keyframePaths.Count, referenceVideoPath is not null);

            // 5. Build generator request
            var outputPath = Path.Combine(tempDir, "output.mp4");
            var request = new VideoGenerationRequest
            {
                Prompt              = job.Prompt,
                Model               = job.Model,
                ReferenceImagePaths = refImagePaths,
                KeyframePaths       = keyframePaths,
                VideoPath           = referenceVideoPath,
                DurationSeconds     = job.DurationSeconds,
                Resolution          = job.Resolution,
                AspectRatio         = job.AspectRatio,
                OutputPath          = outputPath,
                ModelOptions        = job.ModelOptions,
            };

            // 6. Generate
            _logger.LogInformation("Generation starting...");
            var result = await _generator.GenerateAsync(request, cancellationToken);

            if (!result.Success)
            {
                await _jobQueue.MarkFailedAsync(
                    job.Id, result.ErrorMessage ?? "Generation failed with no details.", cancellationToken);
                return;
            }

            var elapsed = DateTime.UtcNow - started;
            _logger.LogInformation("Generation complete in {ElapsedSeconds:F1}s: {VideoPath}",
                elapsed.TotalSeconds, result.VideoPath);

            // 7. Upload MP4 to R2
            await _r2.UploadFromLocalAsync(result.VideoPath!, job.OutputVideoKey, "video/mp4", cancellationToken);
            _logger.LogInformation("Result uploaded to R2: {Key}", job.OutputVideoKey);

            // 8. Mark Completed
            await _jobQueue.MarkCompletedAsync(job.Id, job.OutputVideoKey, cancellationToken);

            _logger.LogInformation("Job completed. Total wall-clock: {ElapsedSeconds:F1}s",
                (DateTime.UtcNow - started).TotalSeconds);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Job processing was cancelled");
            await TryMarkFailedAsync(job.Id, "Worker was cancelled during processing.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing job");
            await TryMarkFailedAsync(job.Id, ex.Message);
        }
        finally
        {
            await heartbeatCts.CancelAsync();
            try { await heartbeatTask; } catch { /* already cancelled */ }

            _tempFiles.Cleanup(tempDir);
            _logger.LogInformation("Temp files cleaned up");
        }
    }

    private async Task RunHeartbeatAsync(string jobId, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_heartbeatInterval, cancellationToken);
                await _jobQueue.UpdateHeartbeatAsync(jobId, cancellationToken);
                _logger.LogDebug("Heartbeat written for job {JobId}", jobId);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to write heartbeat for job {JobId}", jobId);
            }
        }
    }

    /// <summary>
    /// Uses FFmpeg to mix context audio into the generated video.
    /// Audio is trimmed to the video's duration. Returns the path to the mixed output.
    /// </summary>
    private async Task<string> MixAudioAsync(
        string videoPath,
        string audioPath,
        string tempDir,
        CancellationToken cancellationToken)
    {
        var mixedPath = Path.Combine(tempDir, "output_with_audio.mp4");

        // -c:v copy  — no video re-encode
        // -c:a aac   — encode audio to AAC for MP4 container compatibility
        // -map 0:v:0 — take video stream from first input
        // -map 1:a:0 — take audio stream from second input
        // -shortest  — stop when the shortest stream ends (trims audio to video length)
        var args = $"-y -i \"{videoPath}\" -i \"{audioPath}\" -c:v copy -c:a aac -map 0:v:0 -map 1:a:0 -shortest \"{mixedPath}\"";

        using var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName               = "ffmpeg",
                Arguments              = args,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                CreateNoWindow         = true,
            }
        };

        process.Start();
        var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            _logger.LogWarning("FFmpeg audio mix failed (exit {Code}): {Stderr}", process.ExitCode, stderr);
            // Fall back to the original video without audio rather than failing the job
            return videoPath;
        }

        return mixedPath;
    }

    private async Task TryMarkFailedAsync(string jobId, string error)
    {
        try
        {
            await _jobQueue.MarkFailedAsync(jobId, error, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark job {JobId} as Failed in MongoDB", jobId);
        }
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength] + "...";
}
