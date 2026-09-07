namespace VideoWorker.Abstractions;

public class VideoGenerationRequest
{
    public string Prompt { get; set; } = string.Empty;

    /// <summary>Model identifier passed through from the job document (e.g. "ltx-2.3").</summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>Local file paths of reference images downloaded from R2.</summary>
    public List<string> ReferenceImagePaths { get; set; } = new();

    /// <summary>Local file paths of keyframe images downloaded from R2 (may be empty).</summary>
    public List<string> KeyframePaths { get; set; } = new();

    public int DurationSeconds { get; set; }

    /// <summary>e.g. "1280x720"</summary>
    public string Resolution { get; set; } = string.Empty;

    /// <summary>e.g. "16:9"</summary>
    public string AspectRatio { get; set; } = string.Empty;

    /// <summary>
    /// Absolute local path where the generator must write the output MP4.
    /// JobProcessor passes this in; generator writes to it on success.
    /// </summary>
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>Model-specific settings (seed, steps, guidance scale, etc.).</summary>
    public Dictionary<string, string> ModelOptions { get; set; } = new();
}
