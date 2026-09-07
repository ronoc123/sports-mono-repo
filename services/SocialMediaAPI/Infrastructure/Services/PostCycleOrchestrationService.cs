using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.PostCycle;
using Domain.Records;
using SportifyCore.Domain;

namespace Infrastructure.Services;

public class PostCycleOrchestrationService : IPostCycleOrchestrationService
{
    private readonly IPostCycleRepository _postCycleRepository;
    private readonly IRepository<global::Domain.Channel.Channel, string> _channelRepository;
    private readonly IPostRecordRepository _postRecordRepository;
    private readonly IEncryptionService _encryption;
    private readonly IEnumerable<ISocialMediaAdapter> _adapters;

    public PostCycleOrchestrationService(
        IPostCycleRepository postCycleRepository,
        IRepository<global::Domain.Channel.Channel, string> channelRepository,
        IPostRecordRepository postRecordRepository,
        IEncryptionService encryption,
        IEnumerable<ISocialMediaAdapter> adapters)
    {
        _postCycleRepository = postCycleRepository;
        _channelRepository = channelRepository;
        _postRecordRepository = postRecordRepository;
        _encryption = encryption;
        _adapters = adapters;
    }

    public async Task RunAsync(string jobId, CancellationToken cancellationToken)
    {
        var job = await _postCycleRepository.GetByIdAsync(jobId, cancellationToken);
        if (job is null) return;

        var channel = await _channelRepository.GetByIdAsync(job.ChannelId, cancellationToken);
        if (channel is null)
        {
            job.Status = "Failed";
            job.CompletedAt = DateTime.UtcNow;
            await _postCycleRepository.UpdateAsync(job, cancellationToken);
            CleanupTempFile(job.VideoPath);
            return;
        }

        // Initialise per-platform jobs
        job.PlatformJobs = channel.LinkedAccounts
            .Select(a => new PlatformJob { Platform = a.Platform, Status = "Pending" })
            .ToList();
        await _postCycleRepository.UpdateAsync(job, cancellationToken);

        try
        {
            foreach (var account in channel.LinkedAccounts)
            {
                var platformJob = job.PlatformJobs.First(pj => pj.Platform == account.Platform);
                platformJob.Status = "Uploading";
                await _postCycleRepository.UpdateAsync(job, cancellationToken);

                var adapter = _adapters.FirstOrDefault(a =>
                    a.Platform.Equals(account.Platform, StringComparison.OrdinalIgnoreCase));

                if (adapter is null)
                {
                    platformJob.Status = "Failed";
                    platformJob.ErrorMessage = $"No adapter found for platform '{account.Platform}'.";
                    await _postCycleRepository.UpdateAsync(job, cancellationToken);
                    continue;
                }

                var publishRequest = new PublishRequest
                {
                    ChannelId = job.ChannelId,
                    Platform = account.Platform,
                    EncryptedRefreshToken = account.EncryptedRefreshToken,
                    TokenIv = account.TokenIv,
                    Title = job.Title,
                    Description = job.Description,
                    Hashtags = job.Hashtags,
                    VideoPath = job.VideoPath,
                };

                var result = await adapter.PublishAsync(publishRequest, cancellationToken);

                platformJob.Status = result.Status;
                platformJob.VideoUrl = result.VideoUrl;
                platformJob.ExternalPostId = result.ExternalPostId;
                platformJob.ErrorMessage = result.ErrorMessage;
                platformJob.RequiresReauth = result.RequiresReauth;
                platformJob.CompletedAt = DateTime.UtcNow;

                if (result.RequiresReauth)
                {
                    channel.MarkAccountTokenInvalid(account.Platform);
                    await _channelRepository.UpdateAsync(channel, cancellationToken);
                }

                await _postCycleRepository.UpdateAsync(job, cancellationToken);
            }

            await WritePostRecordAsync(job, cancellationToken);
            FinaliseJobStatus(job);
        }
        finally
        {
            job.CompletedAt = DateTime.UtcNow;
            await _postCycleRepository.UpdateAsync(job, cancellationToken);
            CleanupTempFile(job.VideoPath);
        }
    }

    public async Task RetryPlatformAsync(string jobId, string platform, CancellationToken cancellationToken)
    {
        var job = await _postCycleRepository.GetByIdAsync(jobId, cancellationToken);
        if (job is null) return;

        var channel = await _channelRepository.GetByIdAsync(job.ChannelId, cancellationToken);
        if (channel is null) return;

        var account = channel.LinkedAccounts.FirstOrDefault(a =>
            a.Platform.Equals(platform, StringComparison.OrdinalIgnoreCase));
        if (account is null) return;

        var platformJob = job.PlatformJobs.FirstOrDefault(pj =>
            pj.Platform.Equals(platform, StringComparison.OrdinalIgnoreCase));
        if (platformJob is null) return;

        var adapter = _adapters.FirstOrDefault(a =>
            a.Platform.Equals(platform, StringComparison.OrdinalIgnoreCase));

        if (adapter is null)
        {
            platformJob.Status = "Failed";
            platformJob.ErrorMessage = $"No adapter found for platform '{platform}'.";
            await _postCycleRepository.UpdateAsync(job, cancellationToken);
            return;
        }

        try
        {
            platformJob.Status = "Uploading";
            await _postCycleRepository.UpdateAsync(job, cancellationToken);

            var publishRequest = new PublishRequest
            {
                ChannelId = job.ChannelId,
                Platform = account.Platform,
                EncryptedRefreshToken = account.EncryptedRefreshToken,
                TokenIv = account.TokenIv,
                Title = job.Title,
                Description = job.Description,
                Hashtags = job.Hashtags,
                VideoPath = job.VideoPath,
            };

            var result = await adapter.PublishAsync(publishRequest, cancellationToken);

            platformJob.Status = result.Status;
            platformJob.VideoUrl = result.VideoUrl;
            platformJob.ExternalPostId = result.ExternalPostId;
            platformJob.ErrorMessage = result.ErrorMessage;
            platformJob.RequiresReauth = result.RequiresReauth;
            platformJob.CompletedAt = DateTime.UtcNow;

            if (result.RequiresReauth)
            {
                channel.MarkAccountTokenInvalid(platform);
                await _channelRepository.UpdateAsync(channel, cancellationToken);
            }

            FinaliseJobStatus(job);
            job.CompletedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            platformJob.Status = "Failed";
            platformJob.ErrorMessage = ex.Message;
            FinaliseJobStatus(job);
            job.CompletedAt = DateTime.UtcNow;
        }

        await _postCycleRepository.UpdateAsync(job, cancellationToken);
    }

    private async Task WritePostRecordAsync(PostCycleJob job, CancellationToken cancellationToken)
    {
        var postRecord = new PostRecord
        {
            ChannelId = job.ChannelId,
            Title = job.Title,
            Description = job.Description,
            Hashtags = job.Hashtags,
            VideoReference = Path.GetFileName(job.VideoPath),
            PlatformResults = job.PlatformJobs.Select(pj => new global::Domain.Records.PlatformResult
            {
                Platform = pj.Platform,
                Status = pj.Status == "Published" ? "success" : "failed",
                PublishedUrl = pj.VideoUrl,
                ErrorMessage = pj.ErrorMessage,
                PublishedAt = pj.CompletedAt,
            }).ToList(),
            GenerationMetadata = job.GenerationMetadata,
        };

        await _postRecordRepository.AddAsync(postRecord, cancellationToken);
    }

    private static void FinaliseJobStatus(PostCycleJob job)
    {
        var allPublished = job.PlatformJobs.All(pj => pj.Status == "Published");
        var anyPublished = job.PlatformJobs.Any(pj => pj.Status == "Published");
        var anyPending = job.PlatformJobs.Any(pj => pj.Status is "Pending" or "Uploading");

        if (anyPending) return; // still in flight

        job.Status = allPublished ? "Completed" : anyPublished ? "PartialFailure" : "Failed";
    }

    private static void CleanupTempFile(string videoPath)
    {
        try
        {
            if (File.Exists(videoPath))
                File.Delete(videoPath);
        }
        catch
        {
            // Best-effort cleanup; TempFileCleanupService handles orphaned files
        }
    }
}
