using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Records;

public class PlatformResult
{
    [BsonElement("platform")]
    public string Platform { get; set; } = string.Empty;

    [BsonElement("status")]
    public string Status { get; set; } = string.Empty; // "success" | "failed"

    [BsonElement("publishedUrl")]
    public string? PublishedUrl { get; set; }

    [BsonElement("errorMessage")]
    public string? ErrorMessage { get; set; }

    [BsonElement("publishedAt")]
    public DateTime? PublishedAt { get; set; }
}
