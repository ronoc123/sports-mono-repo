namespace Application.Common.Interfaces;

public interface IVideoGenerationOrchestrationService
{
    Task RunAsync(string jobId, CancellationToken cancellationToken);
}
