using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Common.Interfaces;
using Application.Common.Models;

namespace Infrastructure.Adapters;

public class YouTubeSocialMediaAdapter : ISocialMediaAdapter
{
    public string Platform => "YouTube";

    private readonly IYouTubeOAuthService _youTubeOAuth;
    private readonly IEncryptionService _encryption;
    private readonly HttpClient _httpClient;

    public YouTubeSocialMediaAdapter(
        IYouTubeOAuthService youTubeOAuth,
        IEncryptionService encryption,
        HttpClient httpClient)
    {
        _youTubeOAuth = youTubeOAuth;
        _encryption = encryption;
        _httpClient = httpClient;
    }

    public async Task<PlatformPublishResult> PublishAsync(
        PublishRequest request,
        CancellationToken cancellationToken)
    {
        string accessToken;
        try
        {
            // Decryption stays in Infrastructure — never Application layer
            var refreshToken = _encryption.Decrypt(request.EncryptedRefreshToken, request.TokenIv);
            accessToken = await _youTubeOAuth.RefreshAccessTokenAsync(refreshToken);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("invalid_grant") || ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            return new PlatformPublishResult
            {
                Status = "Failed",
                ErrorMessage = "OAuth token is invalid or expired. Re-authentication required.",
                RequiresReauth = true,
            };
        }

        try
        {
            var (videoId, videoUrl) = await UploadVideoAsync(request, accessToken, cancellationToken);
            return new PlatformPublishResult
            {
                Status = "Published",
                ExternalPostId = videoId,
                VideoUrl = videoUrl,
            };
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return new PlatformPublishResult
            {
                Status = "Failed",
                ErrorMessage = "Authorization failed during upload. Re-authentication required.",
                RequiresReauth = true,
            };
        }
        catch (Exception ex)
        {
            return new PlatformPublishResult
            {
                Status = "Failed",
                ErrorMessage = ex.Message,
            };
        }
    }

    private async Task<(string VideoId, string VideoUrl)> UploadVideoAsync(
        PublishRequest request,
        string accessToken,
        CancellationToken cancellationToken)
    {
        // Step 1: Initiate a resumable upload session
        var metadata = new
        {
            snippet = new
            {
                title = request.Title,
                description = BuildDescription(request),
                tags = request.Hashtags,
                categoryId = "17", // Sports
            },
            status = new { privacyStatus = "public" },
        };

        var metadataJson = JsonSerializer.Serialize(metadata);
        var initiateRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "https://www.googleapis.com/upload/youtube/v3/videos?uploadType=resumable&part=snippet,status");

        initiateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        initiateRequest.Content = new StringContent(metadataJson, Encoding.UTF8, "application/json");

        var fileInfo = new FileInfo(request.VideoPath);
        initiateRequest.Headers.Add("X-Upload-Content-Type", "video/*");
        initiateRequest.Headers.Add("X-Upload-Content-Length", fileInfo.Length.ToString());

        var initiateResponse = await _httpClient.SendAsync(initiateRequest, cancellationToken);
        initiateResponse.EnsureSuccessStatusCode();

        var uploadUri = initiateResponse.Headers.Location
            ?? throw new InvalidOperationException("YouTube did not return an upload URI.");

        // Step 2: Upload the video file to the resumable upload URI
        await using var fileStream = File.OpenRead(request.VideoPath);
        var uploadContent = new StreamContent(fileStream);
        uploadContent.Headers.ContentType = new MediaTypeHeaderValue("video/*");
        uploadContent.Headers.ContentLength = fileInfo.Length;

        var uploadRequest = new HttpRequestMessage(HttpMethod.Put, uploadUri)
        {
            Content = uploadContent,
        };

        var uploadResponse = await _httpClient.SendAsync(uploadRequest, cancellationToken);
        uploadResponse.EnsureSuccessStatusCode();

        var uploadResult = await uploadResponse.Content.ReadFromJsonAsync<YouTubeVideoResponse>(
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("YouTube did not return a video response.");

        var videoId = uploadResult.Id
            ?? throw new InvalidOperationException("YouTube response did not contain a video ID.");

        return (videoId, $"https://www.youtube.com/watch?v={videoId}");
    }

    private static string BuildDescription(PublishRequest request)
    {
        if (request.Hashtags.Count == 0)
            return request.Description;

        var hashtags = string.Join(" ", request.Hashtags.Select(h => $"#{h}"));
        return $"{request.Description}\n\n{hashtags}";
    }

    private record YouTubeVideoResponse([property: JsonPropertyName("id")] string? Id);
}
