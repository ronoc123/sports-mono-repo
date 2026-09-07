namespace Application.PostRecord.Dto;

public class PostRecordDetailResponse
{
    public string Id { get; set; } = string.Empty;
    public string ChannelId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Hashtags { get; set; } = new();
    public string VideoReference { get; set; } = string.Empty;
    public DateTime? PostedAt { get; set; }
    public List<PlatformResultDetail> PlatformResults { get; set; } = new();
    public GenerationMetadataResponse? GenerationMetadata { get; set; }
}

public class GenerationMetadataResponse
{
    public string Method { get; set; } = string.Empty;
    public string? HiggsFieldModel { get; set; }
    public string RenderedPrompt { get; set; } = string.Empty;
    public string ImageReference { get; set; } = string.Empty;
}

public class PlatformResultDetail
{
    public string Platform { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? PublishedUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? PublishedAt { get; set; }
}
