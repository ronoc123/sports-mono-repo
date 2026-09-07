using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Channel;

public class LinkedAccount
{
    [BsonElement("platform")]
    public string Platform { get; set; } = string.Empty;

    [BsonElement("accountDisplayName")]
    public string AccountDisplayName { get; set; } = string.Empty;

    [BsonElement("encryptedRefreshToken")]
    public string EncryptedRefreshToken { get; set; } = string.Empty;

    [BsonElement("tokenIv")]
    public string TokenIv { get; set; } = string.Empty;

    [BsonElement("linkedAt")]
    public DateTime LinkedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("tokenStatus")]
    public string TokenStatus { get; set; } = "active";
}
