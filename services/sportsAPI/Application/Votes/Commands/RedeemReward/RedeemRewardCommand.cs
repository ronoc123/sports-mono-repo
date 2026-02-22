using Application.Dto.VoteAccount;
using Contracts.Contracts;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Votes.Commands.RedeemReward
{
  public sealed record RedeemRewardCommand(UserId UserId, string PromoCode) : IRequest<ServiceResponse<VoteAccountDto>>;
}
