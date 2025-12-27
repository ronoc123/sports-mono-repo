using AutoMapper;
using BuildingBlocks.Messageing.Events;
using BuildingBlocks.Messageing.Publisher;
using Contracts.Contracts;
using Domain.DomainServices.RewardService;
using Domain.Repositories;
using MediatR;


namespace Application.Votes.Commands.EmailReward
{
  public class EmailRewardHandler : IRequestHandler<EmailRewardCommand, ServiceResponse<bool>>
  {
    private readonly IRepository _repo;
    private readonly IMapper _mapper;
    private readonly IRewardRedemptionService _rewardService;
    private readonly IEventPublisher _publisher;

    public EmailRewardHandler(IRepository repo, IMapper mapper, IRewardRedemptionService rewardService, IEventPublisher publisher)
    {
      _repo = repo;
      _mapper = mapper;
      _rewardService = rewardService;
      _publisher = publisher;
    }

    public async Task<ServiceResponse<bool>> Handle(EmailRewardCommand request, CancellationToken ct)
    {


      var emailEvent = new RewardEmailRequestedEvent
      {
        RewardId = Guid.NewGuid(),
        Email = "test@example.com",
        ClaimToken = Guid.NewGuid().ToString(),
        ExpiresAt = DateTime.UtcNow,
      };

      await _publisher.PublishAsync(emailEvent, ct);

      return null;
    }
  }
}
