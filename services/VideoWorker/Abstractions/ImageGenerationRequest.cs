namespace VideoWorker.Abstractions;

public class ImageGenerationRequest
{
    public string Prompt { get; set; } = string.Empty;

    /// <summary>Model identifier (e.g. "h3-fl2va", "ltx-2.3").</summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>e.g. "480x832"</summary>
    public string Resolution { get; set; } = string.Empty;

    /// <summary>
    /// Absolute local path where the generator must write the output image (PNG).
    /// </summary>
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>Model-specific settings (seed, steps, etc.).</summary>
    public Dictionary<string, string> ModelOptions { get; set; } = new();
}
