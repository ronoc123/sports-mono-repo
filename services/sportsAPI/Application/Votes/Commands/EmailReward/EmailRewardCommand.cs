using Application.Dto.VoteAccount;
using Contracts.Contracts;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Votes.Commands.EmailReward
{
  public sealed record EmailRewardCommand(UserId UserId) : IRequest<ServiceResponse<bool>>;
}
