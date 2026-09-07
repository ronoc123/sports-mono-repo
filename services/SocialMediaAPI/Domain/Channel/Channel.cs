using MongoDB.Bson.Serialization.Attributes;
using SportifyCore.Domain;

namespace Domain.Channel;

[BsonIgnoreExtraElements]
public class Channel : Aggregate<string>
{
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("styleToneContext")]
    public string StyleToneContext { get; set; } = string.Empty;

    [BsonElement("linkedAccounts")]
    public List<LinkedAccount> LinkedAccounts { get; set; } = new();

    [BsonElement("promptTemplate")]
    public string? PromptTemplate { get; set; }

    [BsonElement("characterImagePath")]
    public string? CharacterImagePath { get; set; }

    public void AddLinkedAccount(LinkedAccount account)
    {
        LinkedAccounts.RemoveAll(a => a.Platform == account.Platform);
        LinkedAccounts.Add(account);
    }

    public void RemoveLinkedAccount(string platform)
    {
        LinkedAccounts.RemoveAll(a => a.Platform == platform);
    }

    public void MarkAccountTokenInvalid(string platform)
    {
        var account = LinkedAccounts.FirstOrDefault(a => a.Platform == platform);
        if (account is not null)
            account.TokenStatus = "invalid";
    }
}
