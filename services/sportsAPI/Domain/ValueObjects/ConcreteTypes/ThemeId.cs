using BuildingBlocks.Exceptions;

namespace Domain.ValueObjects.ConcreteTypes;

public record ThemeId
{
    public Guid Value { get; }

    public ThemeId(Guid value)
    {
        Value = value;
    }

    public static ThemeId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value == Guid.Empty)
        {
            throw new DomainException("ThemeId cannot be empty");
        }

        return new ThemeId(value);
    }
}
