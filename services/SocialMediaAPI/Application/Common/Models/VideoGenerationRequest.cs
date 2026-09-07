namespace Application.Common.Models;

public class VideoGenerationRequest
{
    public string ChannelId { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public string StyleToneContext { get; set; } = string.Empty;
    public string VideoReference { get; set; } = string.Empty;
    public string? UserPrompt { get; set; }
    public List<PostHistoryItem> RecentHistory { get; set; } = new();

    // Higgsfield generation fields — null when using stub adapter
    public string? ImageTempPath { get; set; }
    public string? RenderedPrompt { get; set; }
    public int TargetDurationSeconds { get; set; } = 15;
}

public class PostHistoryItem
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Hashtags { get; set; } = new();
    public DateTime? PostedAt { get; set; }
}
