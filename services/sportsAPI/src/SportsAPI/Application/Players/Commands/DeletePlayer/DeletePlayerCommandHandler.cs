using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Players.Commands.DeletePlayer;

public class DeletePlayerCommandHandler : IRequestHandler<DeletePlayerCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public DeletePlayerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(DeletePlayerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the existing player
            var playerId = PlayerId.Of(request.PlayerId);
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.Id == playerId, cancellationToken);
            
            if (player == null)
            {
                return Result<bool>.Failure("Player not found");
            }

            // Check business rules before deletion
            var hasActivePlayerOptions = await _context.PlayerOptions
                .AnyAsync(po => po.PlayerId == playerId && po.ExpiresAt > DateTime.UtcNow, cancellationToken);
            
            if (hasActivePlayerOptions)
            {
                return Result<bool>.Failure("Cannot delete player with active player options. Please expire or remove them first.");
            }

            // Remove player
            _context.Players.Remove(player);

            // Save changes
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to delete player: {ex.Message}");
        }
    }
}
