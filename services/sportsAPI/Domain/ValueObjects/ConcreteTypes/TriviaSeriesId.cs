using BuildingBlocks.Exceptions;

namespace Domain.ValueObjects.ConcreteTypes
{
    public record TriviaSeriesId
    {
        public Guid Value { get; set; }

        public TriviaSeriesId(Guid value)
        {
            Value = value;
        }

        public static TriviaSeriesId Of(Guid value)
        {
            if (value == Guid.Empty)
                throw new DomainException("TriviaSeriesId cannot be empty");
            return new TriviaSeriesId(value);
        }
    }
}
