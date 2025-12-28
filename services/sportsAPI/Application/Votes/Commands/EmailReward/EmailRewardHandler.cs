using Application.Common.Interfaces;
using Application.Dto.VoteAccount;
using AutoMapper;
using BuildingBlocks.Messageing.Events;
using BuildingBlocks.Messageing.Publisher;
using Contracts.Contracts;
using Domain.DomainServices.RewardService;
using Domain.Repositories;
using Domain.Rewards;
using Domain.ValueObjects.ConcreteTypes;
using Domain.VoteAccount;
using MediatR;


namespace Application.Votes.Commands.EmailReward
{
    public class EmailRewardHandler : IRequestHandler<EmailRewardCommand, ServiceResponse<string>>
    {
        private readonly IRepository _repo;
        private readonly IMapper _mapper;
        private readonly IRewardRedemptionService _rewardService;
        private readonly IEventPublisher _publisher;
        private readonly IUserDirectory _userDirectory;

        public EmailRewardHandler(IRepository repo, IMapper mapper, IRewardRedemptionService rewardService, IEventPublisher publisher, IUserDirectory userDirectory)
        {
            _repo = repo;
            _mapper = mapper;
            _rewardService = rewardService;
            _publisher = publisher;
            _userDirectory = userDirectory;
        }

        public async Task<ServiceResponse<string>> Handle(EmailRewardCommand request, CancellationToken ct)
        {
            var userId = request.UserId;
            var user = await _userDirectory.GetUserByIdAsync(userId.ToString(), ct);
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            if (!Guid.TryParse(user.Id, out var userGuid))
            {
                throw new ArgumentException(nameof(userId), nameof(user));
            }

            var domainUserId = UserId.Of(userGuid);
            var account = await _repo.GetByIdAsync<VoteAccount>(ct, OrganizationId.Of(request.OrganizationId), domainUserId);


            if (account is null)
            {
                account = VoteAccount.Create(OrganizationId.Of(request.OrganizationId), domainUserId, 0);
                await _repo.AddAsync(account);
            }

            var reward = RewardItem.Create(OrganizationId.Of(request.OrganizationId), request.Amount);

            var res = _rewardService.RedeemReward(reward, account);

            var dto = _mapper.Map<VoteAccountDto>(res);

            var emailEvent = new RewardEmailRequestedEvent
            {
                RewardId = reward.Id.Value,
                Email = user.Email,
                RedeemedAt = DateTime.UtcNow,
            };

            await _repo.SaveChangesAsync(ct);

            await _publisher.PublishAsync(emailEvent, ct);


            return ServiceResponse.Ok("Reward Sent", "Success");
        }
    }
}
