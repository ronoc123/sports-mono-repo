namespace Domain.ValueObjects.ConcreteTypes;

public record ThemeId
{
    public Guid Value { get; }

    private ThemeId(Guid value)
    {
        Value = value;
    }

    public static ThemeId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value == Guid.Empty)
        {
            throw new DomainExceptions("ThemeId cannot be empty");
        }

        return new ThemeId(value);
    }
}
