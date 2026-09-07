namespace Application.Common.Interfaces;

public interface IPostCycleOrchestrationService
{
    Task RunAsync(string jobId, CancellationToken cancellationToken);
    Task RetryPlatformAsync(string jobId, string platform, CancellationToken cancellationToken);
}
