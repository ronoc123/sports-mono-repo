using BuildingBlocks.Exceptions;
using Domain.Organizations.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ValueObjects.ConcreteTypes
{
    public record PlayerOptionId
    {
        public Guid Value { get; set; }

        public PlayerOptionId(Guid value)
        {
            Value = value;
        }

        public static PlayerOptionId Of(Guid value)
        {
            ArgumentNullException.ThrowIfNull(value);

            if (value == Guid.Empty)
            {
                throw new DomainException("PlayerOptionId cannot be empty");
            }

            return new PlayerOptionId(value);
        }
}
}
