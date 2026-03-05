using Application.Common.Models;
using Contracts.Contracts;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.PlayerOptions.Commands.VotePlayerOption;


public record VotePlayerOptionCommand(
    Guid PlayerOptionId,
    Guid UserId,
    int VoteAmount,
    Guid LeagueId
) : IRequest<ServiceResponse<bool>>;
