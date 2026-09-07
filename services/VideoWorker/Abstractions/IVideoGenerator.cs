namespace VideoWorker.Abstractions;

/// <summary>
/// Abstraction over the local video generation runtime (WanGP/LTX, Wan, HunyuanVideo, etc.).
/// Implementations must not throw — return VideoGenerationResult with Success=false on all errors.
/// </summary>
public interface IVideoGenerator
{
    Task<VideoGenerationResult> GenerateAsync(
        VideoGenerationRequest request,
        CancellationToken cancellationToken = default);
}
