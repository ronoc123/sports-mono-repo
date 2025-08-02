using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Organizations.Entities;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Players.Commands.CreatePlayer;

public class CreatePlayerCommandHandler : IRequestHandler<CreatePlayerCommand, Result<Guid>>
{
    private readonly ILeagueRepository _leagueRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IApplicationDbContext _context;

    public CreatePlayerCommandHandler(
        ILeagueRepository leagueRepository,
        IOrganizationRepository organizationRepository,
        IApplicationDbContext context)
    {
        _leagueRepository = leagueRepository;
        _organizationRepository = organizationRepository;
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreatePlayerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate league exists
            var league = await _leagueRepository.GetByIdAsync(request.LeagueId);
            if (league == null)
            {
                return Result<Guid>.Failure("League not found");
            }

            // Validate organization exists if provided
            OrganizationId? organizationId = null;
            if (request.OrganizationId.HasValue)
            {
                var organization = await _organizationRepository.GetOrganizationByIdAsync(request.OrganizationId.Value);
                if (organization == null)
                {
                    return Result<Guid>.Failure("Organization not found");
                }

                if (organization.LeagueId.Value != request.LeagueId)
                {
                    return Result<Guid>.Failure("Organization must be in the same league as the player");
                }

                organizationId = OrganizationId.Of(request.OrganizationId.Value);
            }

            // Create player using domain factory method
            var player = Player.Create(
                request.Name,
                request.Position,
                request.ImageUrl,
                request.Age,
                LeagueId.Of(request.LeagueId),
                organizationId);

            // Add player to league
            league.AddPlayer(request.Name, request.Position, request.ImageUrl, request.Age, organizationId);

            // Save changes
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(player.Id.Value);
        }
        catch (ArgumentException ex)
        {
            return Result<Guid>.Failure($"Invalid input: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Failed to create player: {ex.Message}");
        }
    }
}
