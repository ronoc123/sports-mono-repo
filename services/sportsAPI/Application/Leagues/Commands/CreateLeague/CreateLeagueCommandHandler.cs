using Application.Common.Interfaces;
using Contracts.Contracts;
using Domain.Leagues;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Leagues.Commands.CreateLeague;

public class CreateLeagueCommandHandler : IRequestHandler<CreateLeagueCommand, ServiceResponse<LeagueId>>
{
  private readonly IRepository _repo;

  public CreateLeagueCommandHandler(IRepository repo)
  {
    _repo = repo;
  }

    public async Task<ServiceResponse<LeagueId>> Handle(CreateLeagueCommand request, CancellationToken cancellationToken)
    {
            var league = League.Create(request.Name);
            
            await _repo.AddAsync<League>(league, cancellationToken);
            await _repo.SaveChangesAsync();

            return ServiceResponse.Ok(league.Id, "League successfully created.");
  }
}
