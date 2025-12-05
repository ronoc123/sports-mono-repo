using Domain.Rewards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DomainServices.RewardService
{
  public interface IRewardRedemptionService
  {
    public void RedeemReward(RewardItem reward, VoteAccount.VoteAccount account, UserId userId, string redemptionId);

  }
}
