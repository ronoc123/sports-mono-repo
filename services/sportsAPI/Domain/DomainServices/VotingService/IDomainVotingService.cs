using Domain.Rewards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DomainServices.VotingService.VotingService
{
  public interface IDomainVotingService
  {
    public void Vote(VoteAccount.VoteAccount account, PlayerOption.PlayerOption option, UserId userId, long amount);
  }
}
