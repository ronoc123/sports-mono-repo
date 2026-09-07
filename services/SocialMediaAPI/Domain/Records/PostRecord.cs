using Domain.VideoGenerationJob;
using MongoDB.Bson.Serialization.Attributes;
using SportifyCore.Domain;

namespace Domain.Records;

[BsonIgnoreExtraElements]
public class PostRecord : Entity<string>
{
    [BsonElement("channelId")]
    public string ChannelId { get; set; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("hashtags")]
    public List<string> Hashtags { get; set; } = new();

    [BsonElement("videoReference")]
    public string VideoReference { get; set; } = string.Empty;

    [BsonElement("platformResults")]
    public List<PlatformResult> PlatformResults { get; set; } = new();

    [BsonElement("generationMetadata")]
    public GenerationMetadata? GenerationMetadata { get; set; }
}
