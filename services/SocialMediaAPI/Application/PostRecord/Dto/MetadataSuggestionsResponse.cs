namespace Application.PostRecord.Dto;

public class MetadataSuggestionsResponse
{
    public string SuggestedTitle { get; set; } = string.Empty;
    public string SuggestedDescription { get; set; } = string.Empty;
    public List<string> SuggestedHashtags { get; set; } = new();
}
