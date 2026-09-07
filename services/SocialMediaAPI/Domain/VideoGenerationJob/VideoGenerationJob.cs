using MongoDB.Bson.Serialization.Attributes;
using SportifyCore.Domain;

namespace Domain.VideoGenerationJob;

[BsonIgnoreExtraElements]
public class VideoGenerationJob : Entity<string>
{
    [BsonElement("channelId")]
    public string ChannelId { get; set; } = string.Empty;

    [BsonElement("status")]
    public string Status { get; set; } = "Queued";
    // Queued | Generating | Ready | Failed | TimedOut | Consumed

    [BsonElement("imageTempPath")]
    public string ImageTempPath { get; set; } = string.Empty;

    [BsonElement("imageFileName")]
    public string ImageFileName { get; set; } = string.Empty; // original upload filename

    [BsonElement("videoTempPath")]
    public string? VideoTempPath { get; set; }

    [BsonElement("renderedPrompt")]
    public string RenderedPrompt { get; set; } = string.Empty;

    [BsonElement("higgsFieldModel")]
    public string? HiggsFieldModel { get; set; }

    [BsonElement("errorMessage")]
    public string? ErrorMessage { get; set; }

    [BsonElement("completedAt")]
    public DateTime? CompletedAt { get; set; }
}
