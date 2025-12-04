using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstractions
{
  public interface IDomainEventDispatcher
  {
    Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct);
  }

}
