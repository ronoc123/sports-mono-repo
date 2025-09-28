using Application.Common.Interfaces;
using Contracts.Contracts;
using Domain.Leagues;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Leagues.Commands.CreateLeague;

public class CreateLeagueCommandHandler : IRequestHandler<CreateLeagueCommand, ServiceResponse<Guid>>
{
  private readonly ILeagueRepository _leagueRepository;

  public CreateLeagueCommandHandler(IApplicationDbContext context, ILeagueRepository leagueRepository)
    {
        _leagueRepository = leagueRepository;
  }

    public async Task<ServiceResponse<Guid>> Handle(CreateLeagueCommand request, CancellationToken cancellationToken)
    {
            var leagueId = LeagueId.Of(Guid.NewGuid());
            var league = League.Create(leagueId, request.Name);
            
            await _leagueRepository.AddAsync(league, cancellationToken);

            return ServiceResponse.Ok(leagueId.Value, "League successfully created.");
  }
}
