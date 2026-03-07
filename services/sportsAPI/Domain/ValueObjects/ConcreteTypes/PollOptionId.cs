using BuildingBlocks.Exceptions;

namespace Domain.ValueObjects.ConcreteTypes
{
    public record PollOptionId
    {
        public Guid Value { get; set; }

        public PollOptionId(Guid value)
        {
            Value = value;
        }

        public static PollOptionId Of(Guid value)
        {
            if (value == Guid.Empty)
                throw new DomainException("PollOptionId cannot be empty");
            return new PollOptionId(value);
        }
    }
}
