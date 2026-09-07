using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VideoWorker.Abstractions;

namespace VideoWorker.Generators;

/// <summary>
/// Calls a self-hosted WanGP / LTX generation service over HTTP.
///
/// SPIKE TODO (Story 6.2):
///   - Nail down the exact WanGP REST contract (request/response shape).
///   - Decide whether we pass image bytes in the body (multipart) or pre-signed R2 URLs.
///   - Add retry/back-off for transient 503s during cold-start on the GPU VM.
///   - Stream progress events if WanGP exposes a /status polling endpoint.
///
/// Contract assumed today (placeholder — update once spike is done):
///   POST  {ApiUrl}/generate
///   Body  application/json  { "prompt": "...", "model": "...", ... }
///   200   { "video_path": "/workdir/output.mp4" }   — path inside the container
///   The worker then reads that path via a shared Docker volume.
/// </summary>
public sealed class WanGpVideoGenerator : IVideoGenerator
{
    private readonly HttpClient _http;
    private readonly ILogger<WanGpVideoGenerator> _logger;

    // JSON property names match the assumed WanGP REST schema.
    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    public WanGpVideoGenerator(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<WanGpVideoGenerator> logger)
    {
        _logger = logger;

        var apiUrl = configuration["VideoGenerator:WanGp:ApiUrl"]
            ?? throw new InvalidOperationException("VideoGenerator:WanGp:ApiUrl is required.");

        var timeoutSeconds = double.TryParse(
            configuration["VideoGenerator:WanGp:TimeoutSeconds"], out var t) ? t : 1800;

        // Named client registered in Program.cs so tests can substitute a mock handler.
        _http = httpClientFactory.CreateClient("WanGp");
        _http.BaseAddress = new Uri(apiUrl.TrimEnd('/') + "/");
        _http.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
    }

    public async Task<VideoGenerationResult> GenerateAsync(
        VideoGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "WanGP: sending generate request (model={Model}, duration={Duration}s)",
            request.Model, request.DurationSeconds);

        try
        {
            // ── Build request body ────────────────────────────────────────────
            // SPIKE TODO: replace with the real WanGP payload shape.
            var body = new WanGpGenerateRequest
            {
                Prompt          = request.Prompt,
                Model           = request.Model,
                DurationSeconds = request.DurationSeconds,
                Resolution      = request.Resolution,
                AspectRatio     = request.AspectRatio,
                // Keyframe paths assumed accessible via a shared volume.
                // SPIKE TODO: decide whether to pass bytes or volume paths.
                ReferenceImages = request.ReferenceImagePaths,
                Keyframes       = request.KeyframePaths,
                OutputPath      = request.OutputPath,
                Options         = request.ModelOptions,
            };

            using var response = await _http.PostAsJsonAsync(
                "generate", body, JsonOpts, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var detail = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "WanGP returned {StatusCode}: {Detail}",
                    (int)response.StatusCode, detail);
                return VideoGenerationResult.Failed(
                    $"WanGP HTTP {(int)response.StatusCode}: {detail}");
            }

            // ── Parse response ────────────────────────────────────────────────
            var result = await response.Content.ReadFromJsonAsync<WanGpGenerateResponse>(
                JsonOpts, cancellationToken);

            if (result is null || string.IsNullOrEmpty(result.VideoPath))
            {
                _logger.LogError("WanGP response was empty or missing video_path");
                return VideoGenerationResult.Failed("WanGP returned no video_path in response.");
            }

            _logger.LogInformation("WanGP generation succeeded: {VideoPath}", result.VideoPath);
            return VideoGenerationResult.Succeeded(result.VideoPath);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError("WanGP generation timed out after {Timeout}s", _http.Timeout.TotalSeconds);
            return VideoGenerationResult.Failed($"WanGP timed out after {_http.Timeout.TotalSeconds}s.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("WanGP generation was cancelled");
            return VideoGenerationResult.Failed("Generation was cancelled.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error calling WanGP at {BaseAddress}", _http.BaseAddress);
            return VideoGenerationResult.Failed($"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling WanGP");
            return VideoGenerationResult.Failed($"Unexpected error: {ex.Message}");
        }
    }

    // ── DTOs (placeholder shapes — update after spike) ────────────────────────

    private sealed class WanGpGenerateRequest
    {
        public string Prompt { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int DurationSeconds { get; set; }
        public string Resolution { get; set; } = string.Empty;
        public string AspectRatio { get; set; } = string.Empty;
        public List<string> ReferenceImages { get; set; } = new();
        public List<string> Keyframes { get; set; } = new();
        // Path on the shared volume where WanGP should write its output.
        public string OutputPath { get; set; } = string.Empty;
        public Dictionary<string, string> Options { get; set; } = new();
    }

    private sealed class WanGpGenerateResponse
    {
        public string? VideoPath { get; set; }
    }
}
