namespace Application.Channel.Dto;

public class ChannelDetailResponse
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string StyleToneContext { get; init; } = string.Empty;
    public string? PromptTemplate { get; init; }
    public string? CharacterImageUrl { get; init; }
    public List<LinkedAccountResponse> LinkedAccounts { get; init; } = new();
    public DateTime? CreatedAt { get; init; }
}
