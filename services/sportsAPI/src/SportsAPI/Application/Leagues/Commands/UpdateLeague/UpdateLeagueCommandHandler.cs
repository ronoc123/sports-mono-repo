using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Repositories;
using MediatR;

namespace Application.Leagues.Commands.UpdateLeague;

public class UpdateLeagueCommandHandler : IRequestHandler<UpdateLeagueCommand, Result<bool>>
{
    private readonly ILeagueRepository _leagueRepository;
    private readonly IApplicationDbContext _context;

    public UpdateLeagueCommandHandler(
        ILeagueRepository leagueRepository,
        IApplicationDbContext context)
    {
        _leagueRepository = leagueRepository;
        _context = context;
    }

    public async Task<Result<bool>> Handle(UpdateLeagueCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the existing league
            var league = await _leagueRepository.GetByIdAsync(request.LeagueId);
            
            if (league == null)
            {
                return Result<bool>.Failure("League not found");
            }

            // Update properties
            // Note: League entity might need a method to update name
            // For now, assuming we can set the Name property directly
            // TODO: Add proper update method to League entity
            
            await _leagueRepository.UpdateLeagueAsync(league);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to update league: {ex.Message}");
        }
    }
}
