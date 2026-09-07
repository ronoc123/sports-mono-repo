namespace Application.Channel.Dto;

public class LinkedAccountResponse
{
    public string Platform { get; init; } = string.Empty;
    public string AccountDisplayName { get; init; } = string.Empty;
    public DateTime LinkedAt { get; init; }
    public string TokenStatus { get; init; } = "active";
}
