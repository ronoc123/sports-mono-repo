namespace Application.VideoGeneration.Dto;

public class VideoGenerationJobResponse
{
    public string Id { get; set; } = string.Empty;
    public string ChannelId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? HiggsFieldModel { get; set; }
    public string RenderedPrompt { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class StartVideoGenerationResponse
{
    public string JobId { get; set; } = string.Empty;
}
