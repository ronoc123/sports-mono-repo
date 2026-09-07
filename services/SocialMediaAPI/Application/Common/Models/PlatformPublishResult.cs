namespace Application.Common.Models;

public class PlatformPublishResult
{
    public string Status { get; set; } = string.Empty; // "Published" | "Failed"
    public string? VideoUrl { get; set; }
    public string? ExternalPostId { get; set; }
    public string? ErrorMessage { get; set; }
    public bool RequiresReauth { get; set; }
}
