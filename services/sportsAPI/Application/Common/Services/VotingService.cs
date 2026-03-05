using Application.Common.Interfaces;
using Domain.Organizations;
using Domain.Repositories;
using Domain.Rewards;
using Domain.ValueObjects.ConcreteTypes;
using Domain.VoteAccount;
using System.ComponentModel.DataAnnotations;

namespace Application.Common.Services
{
    public class VotingService : IVotingService
    {
        private readonly IRepository _repo;
        private readonly IRedemptionCodeGenerator _codeGenerator;
        private readonly IQrCodeGenerator _qrCodeGenerator;
        private readonly IUserDirectory _userDirectory;

        public VotingService(IRepository repo, IRedemptionCodeGenerator codeGenerator, IQrCodeGenerator qrCodeGenerator, IUserDirectory userDirectory)
        {
            _repo = repo;
            _codeGenerator = codeGenerator;
            _qrCodeGenerator = qrCodeGenerator;
            _userDirectory = userDirectory;
        }

        public async Task<RewardItem> CreateRewardAsync(Guid orgId, long voteValue, CancellationToken ct)
        {


            var promoCode = _codeGenerator.GeneratePromoCode();
            var qrSecret = _codeGenerator.GenerateQrSecret();
            // We may hash this later depending on use cases
            //var reward = RewardItem.Create(orgId, voteValue, _codeGenerator.Hash(promoCode), _codeGenerator.Hash(qrSecret));
            var reward = RewardItem.Create(OrganizationId.Of(orgId), voteValue, qrSecret, promoCode);

            await _repo.AddAsync(reward);

            return reward;
        }

        public async Task<VoteAccount> FindOrCreateVoteAccount(Guid organizationId, Guid userId, CancellationToken ct)
        {
            var user = await _userDirectory.GetUserByIdAsync(userId.ToString(), ct);
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            if (!Guid.TryParse(user.Id, out var userGuid))
            {
                throw new ArgumentException(nameof(userId), nameof(user));
            }

            var organization = await _repo.GetByIdAsync<Organization, OrganizationId>(OrganizationId.Of(organizationId));

            if (organization is null)
            {
                throw new ValidationException("Organization Not Found");
            }

            var account = await _repo.GetByIdAsync<VoteAccount>(ct, organization.LeagueId, UserId.Of(userId));

            if (account is null)
            {
                account = VoteAccount.Create(organization.LeagueId, UserId.Of(userId), 0);
                await _repo.AddAsync(account);
            }

            return account;
        }

        public async Task<string> GenerateRewardQrAsync(RewardItemId rewardId, string qrSecret, CancellationToken ct)
        {

            var reward = await _repo.GetByIdAsync<RewardItem, RewardItemId>(rewardId);
            if (reward is null)
                throw new ValidationException("Reward not found.");

            var qrUrl = $"https://yourapp.com/redeem?c={qrSecret}";
            return _qrCodeGenerator.GenerateBase64(qrUrl);
        }

    }
}


