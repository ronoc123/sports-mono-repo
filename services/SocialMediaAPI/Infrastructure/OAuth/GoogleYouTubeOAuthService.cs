using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.OAuth;

public class GoogleYouTubeOAuthService : IYouTubeOAuthService
{
    private static readonly string[] Scopes =
    [
        "https://www.googleapis.com/auth/youtube.upload",
        "https://www.googleapis.com/auth/youtube.readonly",
    ];

    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _redirectUri;

    public GoogleYouTubeOAuthService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _clientId = configuration["YouTube:ClientId"]
            ?? throw new InvalidOperationException("YouTube:ClientId is not configured.");
        _clientSecret = configuration["YouTube:ClientSecret"]
            ?? throw new InvalidOperationException("YouTube:ClientSecret is not configured.");
        _redirectUri = configuration["YouTube:RedirectUri"]
            ?? throw new InvalidOperationException("YouTube:RedirectUri is not configured.");
    }

    public string BuildAuthorizationUrl(string channelId)
    {
        var scope = Uri.EscapeDataString(string.Join(" ", Scopes));
        var redirectUri = Uri.EscapeDataString(_redirectUri);
        var state = Uri.EscapeDataString(channelId);

        return "https://accounts.google.com/o/oauth2/v2/auth"
            + $"?client_id={_clientId}"
            + $"&redirect_uri={redirectUri}"
            + "&response_type=code"
            + $"&scope={scope}"
            + "&access_type=offline"
            + "&prompt=consent"
            + $"&state={state}";
    }

    public async Task<(string AccessToken, string RefreshToken)> ExchangeCodeAsync(string authCode)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = authCode,
            ["client_id"] = _clientId,
            ["client_secret"] = _clientSecret,
            ["redirect_uri"] = _redirectUri,
            ["grant_type"] = "authorization_code",
        });

        var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", content);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>()
            ?? throw new InvalidOperationException("Failed to deserialize token response.");

        if (string.IsNullOrEmpty(tokenResponse.RefreshToken))
            throw new InvalidOperationException(
                "No refresh token returned. Ensure access_type=offline and prompt=consent are set.");

        return (tokenResponse.AccessToken, tokenResponse.RefreshToken);
    }

    public async Task<string> GetChannelTitleAsync(string accessToken)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            "https://www.googleapis.com/youtube/v3/channels?part=snippet&mine=true");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var channelsResponse = await response.Content.ReadFromJsonAsync<YouTubeChannelsResponse>()
            ?? throw new InvalidOperationException("Failed to deserialize YouTube channels response.");

        return channelsResponse.Items?.FirstOrDefault()?.Snippet?.Title ?? "Unknown Channel";
    }

    public async Task<string> RefreshAccessTokenAsync(string refreshToken)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["refresh_token"] = refreshToken,
            ["client_id"] = _clientId,
            ["client_secret"] = _clientSecret,
            ["grant_type"] = "refresh_token",
        });

        var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", content);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>()
            ?? throw new InvalidOperationException("Failed to deserialize token refresh response.");

        return tokenResponse.AccessToken;
    }

    private record GoogleTokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("refresh_token")] string? RefreshToken);

    private record YouTubeChannelsResponse(
        [property: JsonPropertyName("items")] List<YouTubeChannelItem>? Items);

    private record YouTubeChannelItem(
        [property: JsonPropertyName("snippet")] YouTubeChannelSnippet? Snippet);

    private record YouTubeChannelSnippet(
        [property: JsonPropertyName("title")] string? Title);
}
