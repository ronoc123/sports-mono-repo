using Application.Dto.VoteAccount;
using AutoMapper;
using Contracts.Contracts;
using Domain.DomainServices.RewardService;
using Domain.Repositories;
using Domain.Rewards;
using Domain.VoteAccount;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Application.Votes.Commands.RedeemReward
{
    public class EmailRewardHandler : IRequestHandler<RedeemRewardCommand, ServiceResponse<VoteAccountDto>>
    {
        private readonly IRepository _repo;
        private readonly IMapper _mapper;
        private readonly IRewardRedemptionService _rewardService;

        public EmailRewardHandler(IRepository repo, IMapper mapper, IRewardRedemptionService rewardService)
        {
            _repo = repo;
            _mapper = mapper;
            _rewardService = rewardService;
        }

        public async Task<ServiceResponse<VoteAccountDto>> Handle(RedeemRewardCommand request, CancellationToken ct)
        {
            var userId = request.UserId;

            var reward = await _repo.Find<RewardItem>()
                .Where(r => r.PromoCode == request.PromoCode)
                .FirstOrDefaultAsync(ct);

            if (reward is null)
                throw new ValidationException($"Reward with code '{request.PromoCode}' not found.");

            var account = await _repo.GetByIdAsync<VoteAccount>(ct, reward.OrganizationId, userId);

            if (account is null)
            {
                account = VoteAccount.Create(reward.OrganizationId, userId, 0);
                await _repo.AddAsync(account);
            }

            var res = _rewardService.RedeemReward(reward, account);

            await _repo.SaveChangesAsync(ct);

            var dto = _mapper.Map<VoteAccountDto>(res);

            return ServiceResponse.Ok(dto, "Success");
        }
    }
}
