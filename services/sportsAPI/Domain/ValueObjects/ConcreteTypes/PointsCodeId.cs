using BuildingBlocks.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ValueObjects.ConcreteTypes
{
    public record PointCodeId
    {
        public Guid Value { get; }

        public PointCodeId(Guid value)
        {
            Value = value;
        }

        public static PointCodeId Of(Guid value)
        {
            ArgumentNullException.ThrowIfNull(value);

            if (value == Guid.Empty)
            {
                throw new DomainException("PointCodeId cannot be empty");
            }

            return new PointCodeId(value);
        }
    }
}
