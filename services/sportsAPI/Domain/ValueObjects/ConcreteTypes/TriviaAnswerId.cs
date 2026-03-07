using BuildingBlocks.Exceptions;

namespace Domain.ValueObjects.ConcreteTypes
{
    public record TriviaAnswerId
    {
        public Guid Value { get; set; }

        public TriviaAnswerId(Guid value)
        {
            Value = value;
        }

        public static TriviaAnswerId Of(Guid value)
        {
            if (value == Guid.Empty)
                throw new DomainException("TriviaAnswerId cannot be empty");
            return new TriviaAnswerId(value);
        }
    }
}
