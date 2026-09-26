namespace VideoWorker.Abstractions;

/// <summary>
/// Abstraction over the local video generation runtime (WanGP/LTX, Wan, HunyuanVideo, etc.).
/// Implementations must not throw — return result with Success=false on all errors.
/// </summary>
public interface IVideoGenerator
{
    Task<VideoGenerationResult> GenerateAsync(
        VideoGenerationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a single still image (text-to-image) used as a start-frame keyframe.
    /// Must not throw — return ImageGenerationResult with Success=false on all errors.
    /// </summary>
    Task<ImageGenerationResult> GenerateImageAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default);
}
