namespace Application.PostCycle.Dto;

public class PostCycleJobResponse
{
    public string Id { get; set; } = string.Empty;
    public string ChannelId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<PlatformJobResponse> PlatformJobs { get; set; } = new();
    public DateTime? CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class PlatformJobResponse
{
    public string Platform { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }
    public string? ExternalPostId { get; set; }
    public string? ErrorMessage { get; set; }
    public bool RequiresReauth { get; set; }
}

public class StartPostCycleResponse
{
    public string JobId { get; set; } = string.Empty;
}
