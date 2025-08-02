using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ValueObjects.ConcreteTypes
{
    public record CodeId
    {
        public Guid Value { get; }

        private CodeId(Guid value)
        {
            Value = value;
        }

        public static CodeId Of(Guid value)
        {
            ArgumentNullException.ThrowIfNull(value);

            if (value == Guid.Empty)
            {
                throw new DomainExceptions("CodeId cannot be empty");
            }

            return new CodeId(value);
        }
    }
}
