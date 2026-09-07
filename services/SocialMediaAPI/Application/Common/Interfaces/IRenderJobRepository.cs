using Domain.RenderJob;
using SportifyCore.Domain;

namespace Application.Common.Interfaces;

public interface IRenderJobRepository : IRepository<RenderJob, string>
{
    /// <summary>
    /// Atomically claims the oldest Pending job by transitioning it to Processing.
    /// Uses a single findOneAndUpdate — safe for concurrent workers.
    /// Returns null if no Pending jobs exist.
    /// </summary>
    Task<RenderJob?> TryClaimNextJobAsync(string workerId, CancellationToken cancellationToken = default);

    Task MarkCompletedAsync(string jobId, string outputVideoKey, CancellationToken cancellationToken = default);

    Task MarkFailedAsync(string jobId, string errorMessage, CancellationToken cancellationToken = default);

    /// <summary>Returns Processing jobs whose startedAt is older than the given threshold.</summary>
    Task<List<RenderJob>> GetStaleProcessingJobsAsync(DateTime olderThan, CancellationToken cancellationToken = default);

    /// <summary>Resets a job back to Pending and increments RetryCount.</summary>
    Task RequeueAsync(string jobId, CancellationToken cancellationToken = default);
}
