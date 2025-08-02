using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Repositories;
using MediatR;

namespace Application.Leagues.Commands.DeleteLeague;

public class DeleteLeagueCommandHandler : IRequestHandler<DeleteLeagueCommand, Result<bool>>
{
    private readonly ILeagueRepository _leagueRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IApplicationDbContext _context;

    public DeleteLeagueCommandHandler(
        ILeagueRepository leagueRepository,
        IOrganizationRepository organizationRepository,
        IApplicationDbContext context)
    {
        _leagueRepository = leagueRepository;
        _organizationRepository = organizationRepository;
        _context = context;
    }

    public async Task<Result<bool>> Handle(DeleteLeagueCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if league exists
            var league = await _leagueRepository.GetByIdAsync(request.LeagueId);
            
            if (league == null)
            {
                return Result<bool>.Failure("League not found");
            }

            // Check if league has organizations (business rule)
            var organizations = await _organizationRepository.GetAllOrganizationsAsync();
            var hasOrganizations = organizations.Any(o => o.LeagueId.Value == request.LeagueId);
            
            if (hasOrganizations)
            {
                return Result<bool>.Failure("Cannot delete league that has organizations. Please remove all organizations first.");
            }

            // Delete the league
            await _leagueRepository.DeleteLeagueAsync(request.LeagueId);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to delete league: {ex.Message}");
        }
    }
}
