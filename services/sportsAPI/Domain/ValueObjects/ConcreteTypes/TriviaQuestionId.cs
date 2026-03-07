using BuildingBlocks.Exceptions;

namespace Domain.ValueObjects.ConcreteTypes
{
    public record TriviaQuestionId
    {
        public Guid Value { get; set; }

        public TriviaQuestionId(Guid value)
        {
            Value = value;
        }

        public static TriviaQuestionId Of(Guid value)
        {
            if (value == Guid.Empty)
                throw new DomainException("TriviaQuestionId cannot be empty");
            return new TriviaQuestionId(value);
        }
    }
}
