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
    public VoteAccount.VoteAccount RedeemReward(RewardItem reward, VoteAccount.VoteAccount account)
    {
      var token = reward.GenerateRedemption(account.UserId, Guid.NewGuid().ToString());

      account.ApplyReward(token);

      reward.MarkRedeemed(token.RedeemingUser);

      return account;
    }
  }

}
