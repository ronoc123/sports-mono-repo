using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Organizations.Entities;

namespace Domain.ValueObjects.ConcreteTypes
{
    public record PlayerOptionId
    {
        public Guid Value { get; set; }

    private PlayerOptionId(Guid value)
    {
        Value = value;
    }

    public static PlayerOptionId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value == Guid.Empty)
        {
            throw new DomainExceptions("PlayerOptionId cannot be empty");
        }

        return new PlayerOptionId(value);
    }
}
}
