using Application.Dto.VoteAccount;
using AutoMapper;
using Contracts.Contracts;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using Domain.VoteAccount;
using MediatR;

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
            var leagueId = request.LeagueId;

            var account = await _repo.GetByIdAsync<VoteAccount>(cancellationToken, leagueId, userId);

            if (account is null)
            {
              account = VoteAccount.Create(leagueId, userId, 0);
              await _repo.AddAsync(account);
              await _repo.SaveChangesAsync(cancellationToken);
              return ServiceResponse.Ok(_mapper.Map<VoteAccountDto>(account), "Success");
            }

            var dto = _mapper.Map<VoteAccountDto>(account);
            return ServiceResponse.Ok(dto, "Success");
      }
  }
}
