namespace VideoWorker.Abstractions;

public class VideoGenerationResult
{
    public bool Success { get; set; }

    /// <summary>
    /// Local path to the generated MP4 on success (equals VideoGenerationRequest.OutputPath).
    /// Null on failure.
    /// </summary>
    public string? VideoPath { get; set; }

    /// <summary>Human-readable error message on failure. Null on success.</summary>
    public string? ErrorMessage { get; set; }

    public static VideoGenerationResult Succeeded(string videoPath) =>
        new() { Success = true, VideoPath = videoPath };

    public static VideoGenerationResult Failed(string errorMessage) =>
        new() { Success = false, ErrorMessage = errorMessage };
}
