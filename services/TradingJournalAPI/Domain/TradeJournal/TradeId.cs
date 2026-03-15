namespace Domain.TradeJournal;

public record TradeId(Guid Value)
{
    public static TradeId New() => new(Guid.NewGuid());
    public static TradeId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
