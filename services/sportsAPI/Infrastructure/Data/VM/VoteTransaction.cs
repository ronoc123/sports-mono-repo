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
    public Domain.ValueObjects.ConcreteTypes.OrganizationId OrgId { get; private set; }
    public UserId UserId { get; private set; }
    public long Amount { get; private set; }           // +credit, -spend
    public string Reason { get; private set; } = default!; // 'voucher_redeem','vote_spend','adjust'
    public long? RefId { get; private set; }           // optional linkage
    public DateTimeOffset CreatedAt { get; private set; }
  }
}
