using Application.Common.Models;

namespace Application.Common.Interfaces;

public interface IVideoGenerationAdapter
{
    Task<VideoGenerationResult> GenerateAsync(
        VideoGenerationRequest request,
        CancellationToken cancellationToken = default);
}
