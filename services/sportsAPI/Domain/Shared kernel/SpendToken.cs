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
    public LeagueId LeagueId { get; }
    public PlayerOptionId PlayerOptionId { get; }
    public long Amount { get; }
    public string SpendId { get; }

    internal SpendToken(
        VoteAccountId accountId,
        LeagueId leagueId,
        PlayerOptionId playerOptionId,
        long amount,
        string spendId)
    {
      AccountId = accountId;
      LeagueId = leagueId;
      PlayerOptionId = playerOptionId;
      Amount = amount;
      SpendId = spendId;
    }
  }
}
