using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VideoWorker.Infrastructure;

namespace VideoWorker;

public class Worker : BackgroundService
{
    private readonly MongoJobQueue _jobQueue;
    private readonly JobProcessor _jobProcessor;
    private readonly string _workerId;
    private readonly TimeSpan _pollInterval;
    private readonly TimeSpan _staleJobTimeout;
    private readonly int _maxRetries;
    private readonly int _staleCheckEveryNPolls;
    private readonly ILogger<Worker> _logger;

    private int _pollCount;

    public Worker(
        MongoJobQueue jobQueue,
        JobProcessor jobProcessor,
        IConfiguration configuration,
        ILogger<Worker> logger)
    {
        _jobQueue  = jobQueue;
        _jobProcessor = jobProcessor;
        _logger    = logger;

        _workerId = configuration["Worker:Id"]
            ?? Environment.MachineName;

        _pollInterval = TimeSpan.FromSeconds(
            double.TryParse(configuration["Worker:PollIntervalSeconds"], out var p) ? p : 5);

        _staleJobTimeout = TimeSpan.FromMinutes(
            double.TryParse(configuration["Worker:StaleJobTimeoutMinutes"], out var s) ? s : 30);

        _maxRetries = int.TryParse(configuration["Worker:MaxRetries"], out var r) ? r : 3;

        // Run the stale check every N polls. At default 5s poll, every 12 polls = ~60s.
        _staleCheckEveryNPolls = int.TryParse(configuration["Worker:StaleCheckEveryNPolls"], out var n) ? n : 12;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Worker {WorkerId} started. Poll: {PollInterval}s, StaleTimeout: {StaleTimeout}min, MaxRetries: {MaxRetries}",
            _workerId, _pollInterval.TotalSeconds, _staleJobTimeout.TotalMinutes, _maxRetries);

        while (!stoppingToken.IsCancellationRequested)
        {
            // Periodic stale job recovery (Story 9.1)
            _pollCount++;
            if (_pollCount % _staleCheckEveryNPolls == 0)
                await RecoverStaleJobsAsync(stoppingToken);

            RenderJobDocument? job = null;

            try
            {
                job = await _jobQueue.TryClaimNextJobAsync(_workerId, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error polling MongoDB — will retry in {Interval}s",
                    _pollInterval.TotalSeconds);
                await DelayOrStop(_pollInterval, stoppingToken);
                continue;
            }

            if (job == null)
            {
                _logger.LogDebug("No pending jobs. Waiting {Interval}s...", _pollInterval.TotalSeconds);
                await DelayOrStop(_pollInterval, stoppingToken);
                continue;
            }

            await _jobProcessor.ProcessAsync(job, stoppingToken);
        }

        _logger.LogInformation("Worker {WorkerId} stopped", _workerId);
    }

    /// <summary>
    /// Finds Processing jobs whose heartbeat has gone silent and either requeues them
    /// (if within retry limit) or marks them permanently Failed.
    /// </summary>
    private async Task RecoverStaleJobsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var threshold = DateTime.UtcNow - _staleJobTimeout;
            var staleJobs = await _jobQueue.GetStaleProcessingJobsAsync(threshold, cancellationToken);

            if (staleJobs.Count == 0)
                return;

            _logger.LogWarning("Found {Count} stale Processing job(s) — recovering...", staleJobs.Count);

            foreach (var job in staleJobs)
            {
                if (job.RetryCount < _maxRetries)
                {
                    _logger.LogWarning(
                        "Requeueing stale job {JobId} (retry {Retry}/{Max}, worker was {WorkerId})",
                        job.Id, job.RetryCount + 1, _maxRetries, job.WorkerId);

                    await _jobQueue.RequeueAsync(job.Id, cancellationToken);
                }
                else
                {
                    _logger.LogError(
                        "Job {JobId} exceeded max retries ({Max}) — marking permanently Failed",
                        job.Id, _maxRetries);

                    await _jobQueue.MarkFailedAsync(
                        job.Id,
                        $"Max retries ({_maxRetries}) exceeded after worker crash/timeout.",
                        cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Shutting down — fine
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during stale job recovery — will try again next cycle");
        }
    }

    private static async Task DelayOrStop(TimeSpan delay, CancellationToken cancellationToken)
    {
        try { await Task.Delay(delay, cancellationToken); }
        catch (OperationCanceledException) { /* normal shutdown */ }
    }
}
