using BuildingBlocks.Exceptions;

namespace Domain.ValueObjects.ConcreteTypes
{
    public record PollId
    {
        public Guid Value { get; set; }

        public PollId(Guid value)
        {
            Value = value;
        }

        public static PollId Of(Guid value)
        {
            if (value == Guid.Empty)
                throw new DomainException("PollId cannot be empty");
            return new PollId(value);
        }
    }
}
