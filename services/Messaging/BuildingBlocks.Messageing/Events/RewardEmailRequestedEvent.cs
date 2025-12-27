using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Messageing.Events
{
  public record RewardEmailRequestedEvent : IntergrationEvent
  {

    public Guid RewardId { get; set; }

    public string Email { get; set; }

    public string ClaimToken { get; set; }

    public DateTime ExpiresAt { get; set; }

  };

}
