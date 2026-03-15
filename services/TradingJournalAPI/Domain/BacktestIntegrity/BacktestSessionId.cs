namespace Domain.BacktestIntegrity;

public record BacktestSessionId(Guid Value)
{
    public static BacktestSessionId New() => new(Guid.NewGuid());
    public static BacktestSessionId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
