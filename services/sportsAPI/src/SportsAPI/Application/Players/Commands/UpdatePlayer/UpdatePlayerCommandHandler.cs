using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Players.Commands.UpdatePlayer;

public class UpdatePlayerCommandHandler : IRequestHandler<UpdatePlayerCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public UpdatePlayerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(UpdatePlayerCommand request, CancellationToken cancellationToken)
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

            // Update player using domain method
            player.UpdatePlayerInfo(request.Name, request.Position, request.ImageUrl, request.Age);

            // Save changes
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (ArgumentException ex)
        {
            return Result<bool>.Failure($"Invalid input: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to update player: {ex.Message}");
        }
    }
}
