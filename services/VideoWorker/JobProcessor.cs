using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VideoWorker.Abstractions;
using VideoWorker.Infrastructure;

namespace VideoWorker;

/// <summary>
/// Orchestrates the complete lifecycle of a single render job:
/// download assets → generate video (one segment per clip) → concatenate → upload result → update job status → cleanup.
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
            string outputPath;

            if (job.Clips.Count > 0)
            {
                // ── Multi-clip path ──────────────────────────────────────────────
                _logger.LogInformation("Multi-clip job: {ClipCount} clip(s)", job.Clips.Count);

                var segmentPaths = new List<string>();
                string? previousLastFramePath = null;

                foreach (var (clip, idx) in job.Clips.Select((c, i) => (c, i)))
                {
                    var segDir = Path.Combine(tempDir, $"clip_{idx:D2}");
                    Directory.CreateDirectory(segDir);

                    var refImages = new List<string>();
                    if (!string.IsNullOrEmpty(clip.StartImageKey))
                        // Explicit start frame always wins
                        refImages.Add(await _r2.DownloadToLocalAsync(clip.StartImageKey, segDir, cancellationToken));
                    else if (previousLastFramePath is not null)
                        // Chain: use the last frame of the previous segment for visual continuity
                        refImages.Add(previousLastFramePath);

                    var keyframes = new List<string>();
                    if (!string.IsNullOrEmpty(clip.EndImageKey))
                        keyframes.Add(await _r2.DownloadToLocalAsync(clip.EndImageKey, segDir, cancellationToken));

                    var chained = previousLastFramePath is not null && string.IsNullOrEmpty(clip.StartImageKey);
                    _logger.LogInformation(
                        "Clip {Idx}/{Total}: startImage={HasStart} (chained={Chained}), endImage={HasEnd}, prompt={Prompt}",
                        idx + 1, job.Clips.Count,
                        refImages.Count > 0, chained, keyframes.Count > 0,
                        Truncate(clip.Prompt, 60));

                    var segRequest = new VideoGenerationRequest
                    {
                        Prompt              = clip.Prompt,
                        Model               = job.Model,
                        ReferenceImagePaths = refImages,
                        KeyframePaths       = keyframes,
                        DurationSeconds     = job.DurationSeconds,
                        Resolution          = job.Resolution,
                        AspectRatio         = job.AspectRatio,
                        OutputPath          = Path.Combine(segDir, "segment.mp4"),
                        ModelOptions        = job.ModelOptions,
                    };

                    var segResult = await _generator.GenerateAsync(segRequest, cancellationToken);

                    if (!segResult.Success)
                    {
                        await _jobQueue.MarkFailedAsync(
                            job.Id,
                            $"Clip {idx + 1} generation failed: {segResult.ErrorMessage ?? "no details"}",
                            cancellationToken);
                        return;
                    }

                    _logger.LogInformation("Clip {Idx} generated: {Path}", idx + 1, segResult.VideoPath);
                    segmentPaths.Add(segResult.VideoPath!);

                    // Extract the last frame so the next clip can use it as its start image.
                    // Best-effort: if extraction fails previousLastFramePath stays null and the
                    // next clip simply proceeds without a chained start frame.
                    if (idx < job.Clips.Count - 1)
                        previousLastFramePath = await ExtractLastFrameAsync(segResult.VideoPath!, segDir, cancellationToken);
                }

                outputPath = segmentPaths.Count == 1
                    ? segmentPaths[0]
                    : await ConcatenateSegmentsAsync(segmentPaths, tempDir, cancellationToken);

                // Mix context audio into the combined video if available
                if (!string.IsNullOrEmpty(job.ContextAudioKey))
                {
                    var audioPath = await _r2.DownloadToLocalAsync(job.ContextAudioKey, tempDir, cancellationToken);
                    outputPath = await MixAudioAsync(outputPath, audioPath, tempDir, cancellationToken);
                }
            }
            else
            {
                // ── Legacy single-segment path (backward compat) ─────────────────
                var refImagePaths = new List<string>();
                foreach (var key in job.ReferenceImageKeys)
                    refImagePaths.Add(await _r2.DownloadToLocalAsync(key, tempDir, cancellationToken));

                var keyframePaths = new List<string>();
                foreach (var key in job.KeyframeKeys)
                    keyframePaths.Add(await _r2.DownloadToLocalAsync(key, tempDir, cancellationToken));

                string? referenceVideoPath = null;
                if (!string.IsNullOrEmpty(job.ReferenceVideoKey))
                {
                    referenceVideoPath = await _r2.DownloadToLocalAsync(job.ReferenceVideoKey, tempDir, cancellationToken);
                    _logger.LogInformation("Reference video downloaded for v2v conditioning: {VideoPath}", referenceVideoPath);
                }

                _logger.LogInformation("Assets downloaded: {RefCount} reference image(s), {FrameCount} keyframe(s), referenceVideo={HasRefVideo}",
                    refImagePaths.Count, keyframePaths.Count, referenceVideoPath is not null);

                var legacyOutputPath = Path.Combine(tempDir, "output.mp4");
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
                    OutputPath          = legacyOutputPath,
                    ModelOptions        = job.ModelOptions,
                };

                _logger.LogInformation("Generation starting...");
                var result = await _generator.GenerateAsync(request, cancellationToken);

                if (!result.Success)
                {
                    await _jobQueue.MarkFailedAsync(
                        job.Id, result.ErrorMessage ?? "Generation failed with no details.", cancellationToken);
                    return;
                }

                outputPath = result.VideoPath!;
            }

            var elapsed = DateTime.UtcNow - started;
            _logger.LogInformation("Generation complete in {ElapsedSeconds:F1}s: {VideoPath}",
                elapsed.TotalSeconds, outputPath);

            // Upload MP4 to R2
            await _r2.UploadFromLocalAsync(outputPath, job.OutputVideoKey, "video/mp4", cancellationToken);
            _logger.LogInformation("Result uploaded to R2: {Key}", job.OutputVideoKey);

            // Mark Completed
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

    /// <summary>
    /// Extracts the very last frame of a video as a PNG using FFmpeg.
    /// Used to chain clips: the last frame of segment N becomes the start frame of segment N+1.
    /// </summary>
    private async Task<string?> ExtractLastFrameAsync(
        string videoPath,
        string segDir,
        CancellationToken cancellationToken)
    {
        var framePath = Path.Combine(segDir, "last_frame.png");
        // -sseof -0.1  — seek 0.1 s before end of file
        // -vframes 1   — capture exactly one frame
        var args = $"-y -sseof -0.1 -i \"{videoPath}\" -vframes 1 \"{framePath}\"";

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

        if (process.ExitCode != 0 || !File.Exists(framePath))
        {
            _logger.LogWarning("Failed to extract last frame from clip (exit {Code}): {Stderr}", process.ExitCode, stderr);
            return null;
        }

        _logger.LogDebug("Last frame extracted: {FramePath}", framePath);
        return framePath;
    }

    /// <summary>
    /// Concatenates multiple MP4 segments into a single file using FFmpeg stream copy (no re-encode).
    /// </summary>
    private async Task<string> ConcatenateSegmentsAsync(
        IReadOnlyList<string> segmentPaths,
        string tempDir,
        CancellationToken cancellationToken)
    {
        var listPath = Path.Combine(tempDir, "concat.txt");
        await File.WriteAllLinesAsync(
            listPath,
            segmentPaths.Select(p => $"file '{p.Replace("'", "'\\''")}'" ),
            cancellationToken);

        var outputPath = Path.Combine(tempDir, "concatenated.mp4");
        var args = $"-y -f concat -safe 0 -i \"{listPath}\" -c copy \"{outputPath}\"";

        _logger.LogInformation("Concatenating {Count} segments with FFmpeg", segmentPaths.Count);

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
            _logger.LogWarning("FFmpeg concat failed (exit {Code}): {Stderr}", process.ExitCode, stderr);
            throw new InvalidOperationException($"FFmpeg concat failed (exit {process.ExitCode}): {stderr}");
        }

        _logger.LogInformation("Segments concatenated: {OutputPath}", outputPath);
        return outputPath;
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
