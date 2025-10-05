using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ValueObjects.ConcreteTypes
{
    public record LeagueId
    {
        public Guid Value { get; set; }
        private LeagueId(Guid value)
        {
            Value = value;
        }

        public static LeagueId Of(Guid value)
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value == Guid.Empty)
            {
                throw new DomainExceptions("LeagueId cannot be empty");
            }

            return new LeagueId(value);
        }
    }
}
