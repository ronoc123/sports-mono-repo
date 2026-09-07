using MongoDB.Bson.Serialization.Attributes;

namespace Domain.PostCycle;

public class PlatformJob
{
    [BsonElement("platform")]
    public string Platform { get; set; } = string.Empty;

    [BsonElement("status")]
    public string Status { get; set; } = "Pending"; // Pending | Uploading | Published | Failed

    [BsonElement("videoUrl")]
    public string? VideoUrl { get; set; }

    [BsonElement("externalPostId")]
    public string? ExternalPostId { get; set; }

    [BsonElement("errorMessage")]
    public string? ErrorMessage { get; set; }

    [BsonElement("requiresReauth")]
    public bool RequiresReauth { get; set; }

    [BsonElement("completedAt")]
    public DateTime? CompletedAt { get; set; }
}
