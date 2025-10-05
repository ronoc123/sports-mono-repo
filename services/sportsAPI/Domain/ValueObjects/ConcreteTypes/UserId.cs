using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ValueObjects.ConcreteTypes
{
    public record UserId
    {

        public Guid Value { get; }
        private UserId(Guid value)
        {
            Value = value;
        }

        public static UserId Of(Guid value)
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value == Guid.Empty)
            {
                throw new DomainExceptions("UserId cannot be empty");
            }

            return new UserId(value);
        }
    }
}
