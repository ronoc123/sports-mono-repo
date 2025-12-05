using Domain.Rewards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.VoteAccount;

namespace Domain.DomainServices.RewardService
{
  public class RewardRedemptionService : IRewardRedemptionService
  {
    public void RedeemReward(RewardItem reward, VoteAccount.VoteAccount account, UserId userId, string redemptionId)
    {
      var token = reward.GenerateRedemption(userId, redemptionId);

      account.ApplyReward(token);

      reward.MarkRedeemed(token.RedeemingUser);
    }
  }

}
