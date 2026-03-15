namespace Domain.AIConversation;

public record AISessionId(Guid Value)
{
    public static AISessionId New() => new(Guid.NewGuid());
    public static AISessionId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
