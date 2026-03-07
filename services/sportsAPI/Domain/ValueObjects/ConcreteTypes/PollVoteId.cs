using BuildingBlocks.Exceptions;

namespace Domain.ValueObjects.ConcreteTypes
{
    public record PollVoteId
    {
        public Guid Value { get; set; }

        public PollVoteId(Guid value)
        {
            Value = value;
        }

        public static PollVoteId Of(Guid value)
        {
            if (value == Guid.Empty)
                throw new DomainException("PollVoteId cannot be empty");
            return new PollVoteId(value);
        }
    }
}
