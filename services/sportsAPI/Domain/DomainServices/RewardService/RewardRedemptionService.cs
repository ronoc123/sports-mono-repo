using Domain.Rewards;

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
