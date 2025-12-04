using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ValueObjects.ConcreteTypes
{
  public record PointsWalletId
  {
    public Guid Value { get; }

    public PointsWalletId(Guid value)
    {
      Value = value;
    }

    public static PointsWalletId Of(Guid value)
    {
      ArgumentNullException.ThrowIfNull(value);

      if (value == Guid.Empty)
      {
        throw new DomainExceptions("PointsWalletId cannot be empty");
      }

      return new PointsWalletId(value);
    }
  }
}
