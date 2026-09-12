using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VideoWorker.Abstractions;

namespace VideoWorker.Generators;

/// <summary>
/// Calls the thin Python generation adapter (services/generation-container)
/// which wraps WanGP's in-process Python API.
///
/// Contract:
///   POST  {ApiUrl}/generate
///   Body  application/json  — WanGpGenerateRequest (snake_case)
///   200   { "output_path": "/app/tmp/{jobId}/output.mp4" }
///   5xx   { "detail": "error message" }
///
/// Input/output files are exchanged via a shared Docker volume (/app/tmp).
/// The worker downloads reference images to that volume before calling this
/// service, and the adapter writes the generated MP4 to the same volume.
/// No large file transfers happen over HTTP.
/// </summary>
public sealed class WanGpVideoGenerator : IVideoGenerator
{
    private readonly HttpClient _http;
    private readonly ILogger<WanGpVideoGenerator> _logger;

    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    /// <summary>Video frames per second assumed for duration → frame-count conversion.</summary>
    private const int Fps = 24;

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

        _http = httpClientFactory.CreateClient("WanGp");
        _http.BaseAddress = new Uri(apiUrl.TrimEnd('/') + "/");
        _http.Timeout     = TimeSpan.FromSeconds(timeoutSeconds);
    }

    public async Task<VideoGenerationResult> GenerateAsync(
        VideoGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var (width, height) = ParseResolution(request.Resolution);
        var frameCount      = DurationToFrameCount(request.DurationSeconds);

        _logger.LogInformation(
            "WanGP: POST /generate (model={Model}, {Width}x{Height}, {FrameCount} frames, {RefCount} ref image(s), v2v={HasReferenceVideo})",
            request.Model, width, height, frameCount, request.ReferenceImagePaths.Count, request.VideoPath is not null);

        if (request.VideoPath is not null)
            _logger.LogInformation("WanGP: reference video for v2v conditioning: {VideoPath}", request.VideoPath);

        var body = new WanGpGenerateRequest
        {
            Prompt            = request.Prompt,
            Model             = request.Model,
            Width             = width,
            Height            = height,
            FrameCount        = frameCount,
            ReferenceImages   = request.ReferenceImagePaths,
            Keyframes         = request.KeyframePaths,
            ReferenceVideo    = request.VideoPath,
            OutputPath        = request.OutputPath,
            NumInferenceSteps = request.ModelOptions.TryGetValue("steps", out var s)
                                && int.TryParse(s, out var n) ? n : null,
        };

        try
        {
            using var response = await _http.PostAsJsonAsync(
                "generate", body, JsonOpts, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var detail = await TryReadDetailAsync(response, cancellationToken);
                _logger.LogError("WanGP adapter returned {StatusCode}: {Detail}",
                    (int)response.StatusCode, detail);
                return VideoGenerationResult.Failed(
                    $"Generation service error {(int)response.StatusCode}: {detail}");
            }

            var result = await response.Content
                .ReadFromJsonAsync<WanGpGenerateResponse>(JsonOpts, cancellationToken);

            if (result is null || string.IsNullOrEmpty(result.OutputPath))
            {
                _logger.LogError("WanGP adapter returned success but output_path is missing");
                return VideoGenerationResult.Failed("Generation service returned no output path.");
            }

            _logger.LogInformation("WanGP generation succeeded: {OutputPath}", result.OutputPath);
            return VideoGenerationResult.Succeeded(result.OutputPath);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError("WanGP adapter timed out after {Timeout}s", _http.Timeout.TotalSeconds);
            return VideoGenerationResult.Failed(
                $"Generation timed out after {_http.Timeout.TotalSeconds}s.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("WanGP generation cancelled");
            return VideoGenerationResult.Failed("Generation was cancelled.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error reaching generation service at {BaseAddress}",
                _http.BaseAddress);
            return VideoGenerationResult.Failed($"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling generation service");
            return VideoGenerationResult.Failed($"Unexpected error: {ex.Message}");
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Parses "1280x720" → (1280, 720). Falls back to 1280×720 on bad input.</summary>
    private static (int width, int height) ParseResolution(string resolution)
    {
        var parts = resolution.Split('x');
        if (parts.Length == 2
            && int.TryParse(parts[0], out var w)
            && int.TryParse(parts[1], out var h))
            return (w, h);

        return (1280, 720);
    }

    /// <summary>
    /// Converts duration in seconds to a WanGP frame count.
    /// WanGP defaults: 241 frames ≈ 10 s at 24 fps (frame count = seconds × fps + 1).
    /// </summary>
    private static int DurationToFrameCount(int durationSeconds) =>
        durationSeconds * Fps + 1;

    private static async Task<string> TryReadDetailAsync(
        HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var raw = await response.Content.ReadAsStringAsync(cancellationToken);
            // FastAPI error bodies: { "detail": "..." }
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("detail", out var detail))
                return detail.GetString() ?? raw;
            return raw;
        }
        catch
        {
            return "(unreadable response body)";
        }
    }

    // ── DTOs ──────────────────────────────────────────────────────────────────

    private sealed class WanGpGenerateRequest
    {
        public string       Prompt               { get; set; } = string.Empty;
        public string       Model                { get; set; } = string.Empty;
        public int          Width                { get; set; }
        public int          Height               { get; set; }
        public int          FrameCount           { get; set; }
        public List<string> ReferenceImages      { get; set; } = new();
        public List<string> Keyframes            { get; set; } = new();
        /// <summary>Local path of a reference video for video-to-video conditioning (optional).</summary>
        public string?      ReferenceVideo       { get; set; }
        /// <summary>Absolute path on the shared volume where the adapter must write the MP4.</summary>
        public string       OutputPath           { get; set; } = string.Empty;
        /// <summary>Overrides the model's default step count when set. Maps to num_inference_steps in WanGP.</summary>
        public int?         NumInferenceSteps    { get; set; }
    }

    private sealed class WanGpGenerateResponse
    {
        public string? OutputPath { get; set; }
    }
}
