using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Organizations.Entities;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.PlayerOptions.Commands.CreatePlayerOption;

public class CreatePlayerOptionCommandHandler : IRequestHandler<CreatePlayerOptionCommand, Result<Guid>>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IApplicationDbContext _context;

    public CreatePlayerOptionCommandHandler(
        IOrganizationRepository organizationRepository,
        IApplicationDbContext context)
    {
        _organizationRepository = organizationRepository;
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreatePlayerOptionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate organization exists
            var organization = await _organizationRepository.GetOrganizationByIdAsync(request.OrganizationId);
            if (organization == null)
            {
                return Result<Guid>.Failure("Organization not found");
            }

            // Validate player exists and belongs to the organization or league
            var playerId = PlayerId.Of(request.PlayerId);
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.Id == playerId, cancellationToken);
            
            if (player == null)
            {
                return Result<Guid>.Failure("Player not found");
            }

            // Business rule: Player must be in the same league as the organization
            if (player.LeagueId != organization.LeagueId)
            {
                return Result<Guid>.Failure("Player must be in the same league as the organization");
            }

            // Create player option using organization domain method
            var playerOption = organization.CreatePlayerOption(
                request.Title,
                request.Description,
                playerId,
                request.ExpiresAt);

            // Save changes
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(playerOption.Id.Value);
        }
        catch (ArgumentException ex)
        {
            return Result<Guid>.Failure($"Invalid input: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            return Result<Guid>.Failure($"Business rule violation: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Failed to create player option: {ex.Message}");
        }
    }
}
