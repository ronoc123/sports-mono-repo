using Application.Common.Interfaces;
using Application.Common.Models;

namespace Infrastructure.Adapters;

public class StubVideoGenerationAdapter : IVideoGenerationAdapter
{
    public Task<VideoGenerationResult> GenerateAsync(
        VideoGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var mostRecent = request.RecentHistory.FirstOrDefault();

        var title = mostRecent is not null
            ? $"[AI] {mostRecent.Title}"
            : $"[AI] New Video for {request.ChannelName}";

        var description = mostRecent is not null
            ? $"[AI] {mostRecent.Description}"
            : $"[AI] Exciting new content from {request.ChannelName}. {request.StyleToneContext}";

        var hashtags = mostRecent?.Hashtags.Take(5).ToList()
            ?? new List<string> { "sports", "highlights" };

        if (!string.IsNullOrWhiteSpace(request.UserPrompt))
        {
            title = $"[AI] {request.UserPrompt.Trim()} | {request.ChannelName}";
        }

        var result = new VideoGenerationResult
        {
            Title = title,
            Description = description,
            Hashtags = hashtags,
        };

        return Task.FromResult(result);
    }
}
