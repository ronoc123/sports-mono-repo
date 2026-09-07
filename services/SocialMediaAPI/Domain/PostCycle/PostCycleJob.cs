using Domain.VideoGenerationJob;
using MongoDB.Bson.Serialization.Attributes;
using SportifyCore.Domain;

namespace Domain.PostCycle;

[BsonIgnoreExtraElements]
public class PostCycleJob : Entity<string>
{
    [BsonElement("channelId")]
    public string ChannelId { get; set; } = string.Empty;

    [BsonElement("status")]
    public string Status { get; set; } = "Running"; // Running | Completed | PartialFailure | Failed | TimedOut

    [BsonElement("videoPath")]
    public string VideoPath { get; set; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("hashtags")]
    public List<string> Hashtags { get; set; } = new();

    [BsonElement("platformJobs")]
    public List<PlatformJob> PlatformJobs { get; set; } = new();

    [BsonElement("completedAt")]
    public DateTime? CompletedAt { get; set; }

    [BsonElement("generationMetadata")]
    public GenerationMetadata? GenerationMetadata { get; set; }
}
