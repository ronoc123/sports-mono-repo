using Domain.ValueObjects.ConcreteTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.VM
{
  public sealed class VoteTransaction
  {
    public long Id { get; private set; }

    public OrganizationId OrgId { get; private set; }
    public UserId UserId { get; private set; }

    // Use negative for spends, positive for credits (or invert if you prefer)
    public long Amount { get; private set; }

    // 'voucher_redeem', 'vote_spend', 'adjust'
    public string Reason { get; private set; } = default!;

    // Optional linkage to external thing (e.g., voucher id)
    public long? RefId { get; private set; }

    // NEW: which option this spend belongs to (null for non-vote transactions)
    public PlayerOptionId? PlayerOptionId { get; private set; }

    // NEW: idempotency key for vote spends
    public string? SpendId { get; private set; }

    // Optional: store choice if needed later
    public int? Choice { get; private set; } // map to enum in app layer

    public DateTimeOffset CreatedAt { get; private set; }
  }

}
