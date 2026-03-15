namespace Domain.BiweeklyReview;

public record ReviewSessionId(Guid Value)
{
    public static ReviewSessionId New() => new(Guid.NewGuid());
    public static ReviewSessionId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
