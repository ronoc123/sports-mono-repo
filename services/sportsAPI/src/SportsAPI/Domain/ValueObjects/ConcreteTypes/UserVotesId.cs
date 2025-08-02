using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ValueObjects.ConcreteTypes
{
    public record UserVotesId
    {
        public Guid Value { get; }

        private UserVotesId(Guid value)
        {
            Value = value;
        }

        public static UserVotesId Of(Guid value)
        {
            ArgumentNullException.ThrowIfNull(value);

            if (value == Guid.Empty)
            {
                throw new DomainExceptions("UserVotesId cannot be empty");
            }

            return new UserVotesId(value);
        }
    }
}
