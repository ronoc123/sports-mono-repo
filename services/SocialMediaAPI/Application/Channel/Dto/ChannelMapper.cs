namespace Application.Channel.Dto;

public static class ChannelMapper
{
    public static ChannelDetailResponse ToDetailResponse(global::Domain.Channel.Channel channel) => new()
    {
        Id = channel.Id,
        Name = channel.Name,
        Description = channel.Description,
        StyleToneContext = channel.StyleToneContext,
        PromptTemplate = channel.PromptTemplate,
        CharacterImageUrl = channel.CharacterImagePath != null
            ? $"/api/channels/{channel.Id}/image"
            : null,
        CreatedAt = channel.CreatedAt,
        LinkedAccounts = channel.LinkedAccounts.Select(a => new LinkedAccountResponse
        {
            Platform = a.Platform,
            AccountDisplayName = a.AccountDisplayName,
            LinkedAt = a.LinkedAt,
            TokenStatus = a.TokenStatus,
        }).ToList(),
    };
}
