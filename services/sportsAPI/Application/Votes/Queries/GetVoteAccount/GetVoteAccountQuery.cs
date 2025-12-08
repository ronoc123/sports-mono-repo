using Application.Dto.VoteAccount;
using Contracts.Contracts;
using Contracts.Responses;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Votes.Queries.GetVoteAccount
{

      public record GetVoteAccountQuery(
      UserId UserId,
      OrganizationId OrganizationId
    ) : IRequest<ServiceResponse<VoteAccountDto>>;
}
