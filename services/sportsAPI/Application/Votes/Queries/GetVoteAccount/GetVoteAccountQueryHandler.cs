using Application.Dto.VoteAccount;
using Application.Players.Queries.GetAllPlayers;
using AutoMapper;
using Contracts.Contracts;
using Contracts.Responses;
using Domain.Player;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using Domain.VoteAccount;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Votes.Queries.GetVoteAccount
{
  public class GetVoteAccountQueryHandler : IRequestHandler<GetVoteAccountQuery, ServiceResponse<VoteAccountDto>>
  {
      private readonly IRepository _repo;

      private readonly IMapper _mapper;
      public GetVoteAccountQueryHandler(IRepository repo, IMapper mapper)
      {
        _repo = repo;
        _mapper = mapper;
      }
      public async Task<ServiceResponse<VoteAccountDto>> Handle(GetVoteAccountQuery request, CancellationToken cancellationToken)
      {
            var userId = request.UserId;
            var orgId = request.OrganizationId;

            var account = await _repo.GetByIdAsync<VoteAccount>(cancellationToken, orgId, userId);

            if (account is null)
            {
              account = VoteAccount.Create(orgId, userId, 0);
              await _repo.AddAsync(account);
              await _repo.SaveChangesAsync(cancellationToken);
              return ServiceResponse.Ok(_mapper.Map<VoteAccountDto>(account), "Success");
            }

            // Existing account → return DTO
            var dto = _mapper.Map<VoteAccountDto>(account);
            return ServiceResponse.Ok(dto, "Success");

    }
  }
}
