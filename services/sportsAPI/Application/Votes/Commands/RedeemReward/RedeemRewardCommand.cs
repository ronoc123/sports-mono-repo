using Application.Dto.VoteAccount;
using Contracts.Contracts;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Votes.Commands.RedeemReward
{
  public sealed record RedeemRewardCommand(UserId UserId, RewardItemId RewardItemId) : IRequest<ServiceResponse<VoteAccountDto>>;
}
