using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VideoWorker.Infrastructure;

/// <summary>
/// Local representation of the render_jobs MongoDB document.
/// Mirrors Domain.RenderJob.RenderJob in SocialMediaAPI — kept in sync manually.
/// Does not inherit from SportifyCore to keep VideoWorker independent.
/// </summary>
[BsonIgnoreExtraElements]
public class RenderJobDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("_id")]
    public string Id { get; set; } = string.Empty;

    [BsonElement("channelId")]
    public string ChannelId { get; set; } = string.Empty;

    [BsonElement("status")]
    public string Status { get; set; } = string.Empty;

    [BsonElement("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [BsonElement("model")]
    public string Model { get; set; } = string.Empty;

    [BsonElement("durationSeconds")]
    public int DurationSeconds { get; set; }

    [BsonElement("resolution")]
    public string Resolution { get; set; } = string.Empty;

    [BsonElement("aspectRatio")]
    public string AspectRatio { get; set; } = string.Empty;

    [BsonElement("referenceImageKeys")]
    public List<string> ReferenceImageKeys { get; set; } = new();

    [BsonElement("keyframeKeys")]
    public List<string> KeyframeKeys { get; set; } = new();

    [BsonElement("outputVideoKey")]
    public string OutputVideoKey { get; set; } = string.Empty;

    [BsonElement("modelOptions")]
    public Dictionary<string, string> ModelOptions { get; set; } = new();

    [BsonElement("workerId")]
    public string? WorkerId { get; set; }

    [BsonElement("createdAt")]
    public DateTime? CreatedAt { get; set; }

    [BsonElement("startedAt")]
    public DateTime? StartedAt { get; set; }

    [BsonElement("completedAt")]
    public DateTime? CompletedAt { get; set; }

    [BsonElement("retryCount")]
    public int RetryCount { get; set; }

    [BsonElement("errorMessage")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Updated periodically by the worker while Processing.
    /// Used by stale-job recovery to distinguish an active job from a crashed worker.
    /// </summary>
    [BsonElement("lastHeartbeatAt")]
    public DateTime? LastHeartbeatAt { get; set; }
}
