namespace Application.Common.Models;

public class VideoGenerationResult
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Hashtags { get; set; } = new();

    // Non-null when a real adapter generates a video file
    public string? VideoPath { get; set; }
    public string? HiggsFieldModel { get; set; }
}
