using MongoDB.Bson.Serialization.Attributes;

namespace Domain.VideoGenerationJob;

public class GenerationMetadata
{
    [BsonElement("method")]
    public string Method { get; set; } = "higgsfield-claude-mcp";

    [BsonElement("higgsFieldModel")]
    public string? HiggsFieldModel { get; set; }

    [BsonElement("renderedPrompt")]
    public string RenderedPrompt { get; set; } = string.Empty;

    [BsonElement("imageReference")]
    public string ImageReference { get; set; } = string.Empty; // original filename, not path
}
