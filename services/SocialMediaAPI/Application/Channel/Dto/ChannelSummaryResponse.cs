namespace Application.Channel.Dto;

public class ChannelSummaryResponse
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int LinkedAccountCount { get; init; }
    public string? LastPostTitle { get; init; }
    public DateTime? LastPostDate { get; init; }
    public DateTime? CreatedAt { get; init; }
}
