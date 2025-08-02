using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Leagues;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Leagues.Commands.CreateLeague;

public class CreateLeagueCommandHandler : IRequestHandler<CreateLeagueCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateLeagueCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateLeagueCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var leagueId = LeagueId.Of(Guid.NewGuid());
            var league = League.Create(leagueId, request.Name);

            _context.Leagues.Add(league);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(league.Id.Value);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Failed to create league: {ex.Message}");
        }
    }
}
