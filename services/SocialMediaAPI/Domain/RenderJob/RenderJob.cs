using MongoDB.Bson.Serialization.Attributes;
using SportifyCore.Domain;

namespace Domain.RenderJob;

public record RenderJobClip
{
    [BsonElement("prompt")]
    public string Prompt { get; init; } = string.Empty;

    /// <summary>R2 key for the start frame (image_start). Null = use channel image or no start frame.</summary>
    [BsonElement("startImageKey")]
    public string? StartImageKey { get; init; }

    /// <summary>R2 key for the end frame (image_end). Null = no end-frame guidance.</summary>
    [BsonElement("endImageKey")]
    public string? EndImageKey { get; init; }
}

/// <summary>
/// A self-hosted video generation job. Assets live in Cloudflare R2;
/// this document contains only object keys and metadata — no binary data.
/// Processed by the VideoWorker service running on a GPU VM.
/// </summary>
[BsonIgnoreExtraElements]
public class RenderJob : Entity<string>
{
    [BsonElement("channelId")]
    public string ChannelId { get; set; } = string.Empty;

    /// <summary>Pending | Processing | Completed | Failed</summary>
    [BsonElement("status")]
    public string Status { get; set; } = "Pending";

    [BsonElement("prompt")]
    public string Prompt { get; set; } = string.Empty;

    /// <summary>Model identifier, e.g. "ltx-2.3", "wan"</summary>
    [BsonElement("model")]
    public string Model { get; set; } = string.Empty;

    [BsonElement("durationSeconds")]
    public int DurationSeconds { get; set; }

    /// <summary>e.g. "1280x720"</summary>
    [BsonElement("resolution")]
    public string Resolution { get; set; } = string.Empty;

    /// <summary>e.g. "16:9", "9:16"</summary>
    [BsonElement("aspectRatio")]
    public string AspectRatio { get; set; } = string.Empty;

    /// <summary>R2 object keys for reference images uploaded by the backend.</summary>
    [BsonElement("referenceImageKeys")]
    public List<string> ReferenceImageKeys { get; set; } = new();

    /// <summary>R2 object keys for keyframes (optional, may be empty).</summary>
    [BsonElement("keyframeKeys")]
    public List<string> KeyframeKeys { get; set; } = new();

    /// <summary>R2 object key of a previously generated video used as the video-to-video reference (optional).</summary>
    [BsonElement("referenceVideoKey")]
    public string? ReferenceVideoKey { get; set; }

    /// <summary>R2 object key for the channel's context audio (optional). Mixed into the output video by the worker.</summary>
    [BsonElement("contextAudioKey")]
    public string? ContextAudioKey { get; set; }

    /// <summary>
    /// R2 object key where the worker will write the generated MP4.
    /// Computed by the backend at job creation time: generation/{jobId}/output.mp4
    /// </summary>
    [BsonElement("outputVideoKey")]
    public string OutputVideoKey { get; set; } = string.Empty;

    /// <summary>Model-specific options (seed, steps, guidance scale, etc.)</summary>
    [BsonElement("modelOptions")]
    public Dictionary<string, string> ModelOptions { get; set; } = new();

    /// <summary>
    /// Multi-clip job: each element maps to one WanGP call.
    /// When non-empty, the worker generates one segment per clip and concatenates them.
    /// When empty, the worker uses the legacy flat Prompt/ReferenceImageKeys/KeyframeKeys fields.
    /// </summary>
    [BsonElement("clips")]
    public List<RenderJobClip> Clips { get; set; } = new();

    /// <summary>Set when status transitions to Processing.</summary>
    [BsonElement("workerId")]
    public string? WorkerId { get; set; }

    [BsonElement("startedAt")]
    public DateTime? StartedAt { get; set; }

    [BsonElement("completedAt")]
    public DateTime? CompletedAt { get; set; }

    [BsonElement("retryCount")]
    public int RetryCount { get; set; }

    [BsonElement("errorMessage")]
    public string? ErrorMessage { get; set; }
}
