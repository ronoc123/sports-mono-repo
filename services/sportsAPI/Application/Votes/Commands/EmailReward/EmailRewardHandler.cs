using Application.Common.Interfaces;
using Application.Dto.VoteAccount;
using AutoMapper;
using BuildingBlocks.Messageing.Events;
using BuildingBlocks.Messageing.Publisher;
using Contracts.Contracts;
using Domain.DomainServices.RewardService;
using Domain.Repositories;
using MediatR;


namespace Application.Votes.Commands.EmailReward
{
    public class EmailRewardRedemptionHandler : IRequestHandler<EmailRewardRedemptionCommand, ServiceResponse<string>>
    {
        private readonly IRepository _repo;
        private readonly IMapper _mapper;
        private readonly IRewardRedemptionService _rewardService;
        private readonly IEventPublisher _publisher;
        private readonly IVotingService _votingService;

        public EmailRewardRedemptionHandler(IRepository repo, IMapper mapper, IRewardRedemptionService rewardService, IEventPublisher publisher, IVotingService votingService)
        {
            _repo = repo;
            _mapper = mapper;
            _rewardService = rewardService;
            _publisher = publisher;
            _votingService = votingService;
        }

        public async Task<ServiceResponse<string>> Handle(EmailRewardRedemptionCommand request, CancellationToken ct)
        {

            var account = await _votingService.FindOrCreateVoteAccount(request.OrganizationId, request.UserId, ct);

            var reward = await _votingService.CreateRewardAsync(request.OrganizationId, request.Amount, ct);

            var res = _rewardService.RedeemReward(reward, account);

            var dto = _mapper.Map<VoteAccountDto>(res);

            var emailEvent = new RewardEmailRequestedEvent
            {
                RewardId = reward.Id.Value,
                Email = "Testing",
                RedeemedAt = DateTime.UtcNow,
                OrganizationId = account.OrgId.Value,
                UserId = account.UserId.Value,
                Message = "You received a reward!",
                Title = $"New Votes for the Rams!"
            };

            await _repo.SaveChangesAsync(ct);

            await _publisher.PublishAsync(emailEvent, ct);


            return ServiceResponse.Ok("Reward Sent", "Success");
        }
    }
}
