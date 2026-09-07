namespace Application.VideoGeneration.Dto;

public class VideoGenerationResponse
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Hashtags { get; set; } = new();
}
