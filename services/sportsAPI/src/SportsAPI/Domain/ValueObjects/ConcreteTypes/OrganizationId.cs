using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ValueObjects.ConcreteTypes
{
    public record OrganizationId
    {
        public Guid Value { get; set; }
        private OrganizationId(Guid value)
        {
            Value = value;
        }

        public static OrganizationId Of(Guid value)
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value == Guid.Empty)
            {
                throw new DomainExceptions("OrganizationId cannot be empty");
            }

            return new OrganizationId(value);
        }
    }
}
