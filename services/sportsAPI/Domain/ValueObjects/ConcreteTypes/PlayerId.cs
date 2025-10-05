using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ValueObjects.ConcreteTypes
{
    public record PlayerId
    {
        public Guid Value { get; }

        private PlayerId(Guid value)
        {
            Value = value;
        }

        public static PlayerId Of(Guid value)
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value == Guid.Empty)
            {
                throw new DomainExceptions("PlayerId cannot be empty");
            }

            return new PlayerId(value);
        }
    }
}
