using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Exceptions
{
  public abstract class DomainException : Exception
  {
    public virtual string Code => ErrorCodes.DomainRule;
    protected DomainException(string message) : base(message) { }
  }
}
