using System.Text;
using System.Text.Json;
using Application.Common.Interfaces;
using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class ClaudeIdeationService : IClaudeIdeationService
{
    private readonly HttpClient _httpClient;
    private readonly VideoGenerationSettings _settings;
    private readonly ILogger<ClaudeIdeationService> _logger;

    private const string SystemPrompt =
        """
        You are a short-form social media video production specialist. When given a rough idea and channel context, you create a complete, production-ready video brief with exactly 6 keyframe scene descriptions.

        The character reference image (if attached) shows the channel's primary character — incorporate them naturally into each scene.

        Respond ONLY with valid JSON. No markdown code fences, no explanation, no extra text — just the raw JSON object using this exact structure:
        {
          "prompt": "A 2-4 sentence cinematic video generation prompt covering the overall concept, visual style, movement, and mood",
          "scenes": [
            "Scene 1: [character action, camera angle, setting, lighting, and emotional tone]",
            "Scene 2: ...",
            "Scene 3: ...",
            "Scene 4: ...",
            "Scene 5: ...",
            "Scene 6: ..."
          ]
        }
        """;

    public ClaudeIdeationService(
        HttpClient httpClient,
        IOptions<VideoGenerationSettings> settings,
        ILogger<ClaudeIdeationService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<IdeationResult?> IdeateAsync(
        IdeationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.Anthropic.ApiKey))
        {
            _logger.LogWarning("Anthropic API key is not configured — ideation unavailable.");
            return null;
        }

        try
        {
            var userMessage = BuildUserMessage(request);
            var messageContent = await BuildMessageContentAsync(request, userMessage, cancellationToken);
            var responseText = await CallAnthropicAsync(messageContent, cancellationToken);

            if (string.IsNullOrWhiteSpace(responseText))
                return null;

            return ParseIdeationResult(responseText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ClaudeIdeationService failed.");
            return null;
        }
    }

    private static string BuildUserMessage(IdeationRequest request)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Channel: {request.ChannelName}");
        sb.AppendLine($"Style/Tone: {request.StyleToneContext}");
        sb.AppendLine($"Prompt template: {(string.IsNullOrWhiteSpace(request.PromptTemplate) ? "None — use default channel style" : request.PromptTemplate)}");
        sb.AppendLine();
        sb.AppendLine($"My rough idea: {request.UserIdea}");
        sb.AppendLine();
        sb.Append("Generate the 6-scene video brief now.");
        return sb.ToString();
    }

    private static async Task<object[]> BuildMessageContentAsync(
        IdeationRequest request,
        string userMessage,
        CancellationToken cancellationToken)
    {
        var parts = new List<object>();

        if (!string.IsNullOrEmpty(request.CharacterImagePath) && File.Exists(request.CharacterImagePath))
        {
            var imageBytes = await File.ReadAllBytesAsync(request.CharacterImagePath, cancellationToken);
            var base64 = Convert.ToBase64String(imageBytes);
            var mediaType = GetMediaType(request.CharacterImagePath);

            parts.Add(new
            {
                type = "image",
                source = new
                {
                    type = "base64",
                    media_type = mediaType,
                    data = base64,
                }
            });
        }

        parts.Add(new { type = "text", text = userMessage });
        return parts.ToArray();
    }

    private async Task<string?> CallAnthropicAsync(object[] messageContent, CancellationToken cancellationToken)
    {
        var requestBody = new
        {
            model = _settings.Anthropic.Model,
            max_tokens = 2048,
            system = SystemPrompt,
            messages = new[]
            {
                new { role = "user", content = messageContent }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        httpRequest.Headers.Add("x-api-key", _settings.Anthropic.ApiKey);
        httpRequest.Headers.Add("anthropic-version", "2023-06-01");
        httpRequest.Content = content;

        _logger.LogInformation("Sending ideation request to Anthropic (model: {Model})", _settings.Anthropic.Model);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Anthropic API returned {StatusCode}: {Body}", response.StatusCode, errorBody);
            return null;
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return ExtractTextContent(responseBody);
    }

    private static string? ExtractTextContent(string responseBody)
    {
        using var doc = JsonDocument.Parse(responseBody);
        if (!doc.RootElement.TryGetProperty("content", out var contentArray))
            return null;

        var sb = new StringBuilder();
        foreach (var block in contentArray.EnumerateArray())
        {
            if (block.TryGetProperty("type", out var typeEl) && typeEl.GetString() == "text"
                && block.TryGetProperty("text", out var textEl))
            {
                sb.Append(textEl.GetString());
            }
        }

        return sb.ToString().Trim();
    }

    private IdeationResult? ParseIdeationResult(string text)
    {
        // Strip markdown code fences if Claude wrapped the JSON
        var cleaned = text.Trim();
        if (cleaned.StartsWith("```"))
        {
            var firstNewline = cleaned.IndexOf('\n');
            if (firstNewline >= 0)
                cleaned = cleaned[(firstNewline + 1)..];
            var lastFence = cleaned.LastIndexOf("```");
            if (lastFence >= 0)
                cleaned = cleaned[..lastFence].Trim();
        }

        try
        {
            using var doc = JsonDocument.Parse(cleaned);
            var root = doc.RootElement;

            var prompt = root.TryGetProperty("prompt", out var p) ? p.GetString() ?? string.Empty : string.Empty;
            var scenes = new List<string>();

            if (root.TryGetProperty("scenes", out var scenesEl))
            {
                foreach (var scene in scenesEl.EnumerateArray())
                    scenes.Add(scene.GetString() ?? string.Empty);
            }

            return new IdeationResult(prompt, scenes);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse Claude ideation JSON response.");
            return null;
        }
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
