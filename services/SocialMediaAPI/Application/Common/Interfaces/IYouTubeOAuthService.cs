namespace Application.Common.Interfaces;

public interface IYouTubeOAuthService
{
    string BuildAuthorizationUrl(string channelId);
    Task<(string AccessToken, string RefreshToken)> ExchangeCodeAsync(string authCode);
    Task<string> RefreshAccessTokenAsync(string refreshToken);
    Task<string> GetChannelTitleAsync(string accessToken);
}
