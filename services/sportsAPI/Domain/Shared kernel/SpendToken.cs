using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Shared_kernel
{
  public sealed record SpendToken
  {
    public VoteAccountId AccountId { get; }
    public OrganizationId OrgId { get; }
    public PlayerOptionId PlayerOptionId { get; }
    public long Amount { get; }
    public string SpendId { get; }

    internal SpendToken(
        VoteAccountId accountId,
        OrganizationId orgId,
        PlayerOptionId playerOptionId,
        long amount,
        string spendId)
    {
      AccountId = accountId;
      OrgId = orgId;
      PlayerOptionId = playerOptionId;
      Amount = amount;
      SpendId = spendId;
    }
  }
}
