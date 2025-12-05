using Domain.DomainServices.VotingService.VotingService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Domain.DomainServices.Voting
{
  public class VotingService : IVotingService
  {
    public void Vote(VoteAccount.VoteAccount account, PlayerOption.PlayerOption option, UserId userId, long amount, string spendId)
    {
      // 1. Authorize spend
      var token = account.AuthorizeSpend(option.Id, amount, spendId);

      // 2. Apply the vote
      option.CastVote(userId, token);

      // 3. Debit the account
      account.ApplySpend(token);
    }
  }

}
