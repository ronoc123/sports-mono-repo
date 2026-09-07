using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Adapters;

public class HiggsFieldClaudeAdapter : IVideoGenerationAdapter
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    private readonly HttpClient _httpClient;
    private readonly VideoGenerationSettings _settings;
    private readonly ILogger<HiggsFieldClaudeAdapter> _logger;

    public HiggsFieldClaudeAdapter(
        HttpClient httpClient,
        IOptions<VideoGenerationSettings> settings,
        ILogger<HiggsFieldClaudeAdapter> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<VideoGenerationResult> GenerateAsync(
        VideoGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(request.ImageTempPath) || string.IsNullOrEmpty(request.RenderedPrompt))
        {
            _logger.LogWarning("HiggsFieldClaudeAdapter called without image or prompt — returning empty result.");
            return new VideoGenerationResult();
        }

        try
        {
            var imageBytes = await File.ReadAllBytesAsync(request.ImageTempPath, cancellationToken);
            var base64Image = Convert.ToBase64String(imageBytes);
            var mediaType = GetMediaType(request.ImageTempPath);

            var videoUrl = await CallAnthropicWithMcpAsync(
                base64Image, mediaType, request.RenderedPrompt, cancellationToken);

            if (string.IsNullOrEmpty(videoUrl))
            {
                _logger.LogWarning("No video URL found in Claude response.");
                return new VideoGenerationResult { VideoPath = null };
            }

            var videoTempPath = await DownloadVideoAsync(videoUrl, cancellationToken);

            return new VideoGenerationResult
            {
                VideoPath = videoTempPath,
                HiggsFieldModel = null, // model name extracted from response when available
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HiggsFieldClaudeAdapter failed during generation.");
            return new VideoGenerationResult { VideoPath = null };
        }
    }

    private async Task<string?> CallAnthropicWithMcpAsync(
        string base64Image,
        string mediaType,
        string renderedPrompt,
        CancellationToken cancellationToken)
    {
        var requestBody = new
        {
            model = _settings.Anthropic.Model,
            max_tokens = 2048,
            mcp_servers = new[]
            {
                new
                {
                    type = "url",
                    url = _settings.Higgsfield.McpEndpoint,
                    name = "higgsfield",
                    authorization_token = _settings.Higgsfield.AuthToken,
                }
            },
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "image",
                            source = new
                            {
                                type = "base64",
                                media_type = mediaType,
                                data = base64Image,
                            }
                        },
                        new
                        {
                            type = "text",
                            text = renderedPrompt,
                        }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.anthropic.com/v1/messages");

        httpRequest.Headers.Add("x-api-key", _settings.Anthropic.ApiKey);
        httpRequest.Headers.Add("anthropic-version", "2023-06-01");
        httpRequest.Headers.Add("anthropic-beta", "mcp-client-2025-04-04");
        httpRequest.Content = content;

        _logger.LogInformation("Sending generation request to Anthropic API (model: {Model})", _settings.Anthropic.Model);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Anthropic API returned {StatusCode}: {Body}", response.StatusCode, errorBody);
            return null;
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogDebug("Anthropic response received ({Length} chars)", responseBody.Length);

        return ExtractVideoUrl(responseBody);
    }

    private static string? ExtractVideoUrl(string responseBody)
    {
        // Parse the Anthropic message response and extract video URL from text content
        using var doc = JsonDocument.Parse(responseBody);

        if (!doc.RootElement.TryGetProperty("content", out var contentArray))
            return null;

        var sb = new StringBuilder();
        foreach (var block in contentArray.EnumerateArray())
        {
            if (block.TryGetProperty("type", out var typeEl) && typeEl.GetString() == "text"
                && block.TryGetProperty("text", out var textEl))
            {
                sb.AppendLine(textEl.GetString());
            }
        }

        var fullText = sb.ToString();

        // Match video URLs: direct video file URLs or Higgsfield/CDN URLs
        var urlPattern = new Regex(
            @"https?://[^\s""'<>]+(?:\.mp4|\.mov|\.webm|higgsfield[^\s""'<>]*|cdn[^\s""'<>]*\.mp4)",
            RegexOptions.IgnoreCase);

        var match = urlPattern.Match(fullText);
        return match.Success ? match.Value.TrimEnd('.', ',', ')') : null;
    }

    private async Task<string> DownloadVideoAsync(string videoUrl, CancellationToken cancellationToken)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "social-media-gen");
        Directory.CreateDirectory(tempDir);
        var tempPath = Path.Combine(tempDir, $"{Guid.NewGuid()}.mp4");

        _logger.LogInformation("Downloading generated video from {Url}", videoUrl);

        using var response = await _httpClient.GetAsync(
            videoUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var fileStream = File.Create(tempPath);
        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await responseStream.CopyToAsync(fileStream, cancellationToken);

        _logger.LogInformation("Video downloaded to {Path}", tempPath);
        return tempPath;
    }

    private static string GetMediaType(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "image/jpeg",
        };
    }
}
