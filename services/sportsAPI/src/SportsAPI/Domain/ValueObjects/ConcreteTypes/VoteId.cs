using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ValueObjects.ConcreteTypes
{
    public record VoteId
    {
        public Guid Value { get; }

        private VoteId(Guid value)
        {
            Value = value;
        }

        public static VoteId Of(Guid value)
        {
            ArgumentNullException.ThrowIfNull(value);

            if (value == Guid.Empty)
            {
                throw new DomainExceptions("VoteId cannot be empty");
            }

            return new VoteId(value);
        }
    }
}
