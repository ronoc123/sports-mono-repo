namespace VideoWorker.Abstractions;

public class ImageGenerationResult
{
    public bool Success { get; set; }

    /// <summary>
    /// Local path to the generated image on success (equals ImageGenerationRequest.OutputPath).
    /// Null on failure.
    /// </summary>
    public string? ImagePath { get; set; }

    /// <summary>Human-readable error message on failure. Null on success.</summary>
    public string? ErrorMessage { get; set; }

    public static ImageGenerationResult Succeeded(string imagePath) =>
        new() { Success = true, ImagePath = imagePath };

    public static ImageGenerationResult Failed(string errorMessage) =>
        new() { Success = false, ErrorMessage = errorMessage };
}
