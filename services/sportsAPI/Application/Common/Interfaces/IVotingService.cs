using Domain.Rewards;
using Domain.ValueObjects.ConcreteTypes;
using Domain.VoteAccount;

namespace Application.Common.Interfaces
{
    public interface IVotingService
    {

        public Task<VoteAccount> FindOrCreateVoteAccount(Guid organizationId, Guid userId, CancellationToken ct);

        public Task<RewardItem> CreateRewardAsync(Guid orgId, long voteValue, CancellationToken ct);

        public Task<string> GenerateRewardQrAsync(RewardItemId rewardId, string qrSecret, CancellationToken ct);
    }
}
