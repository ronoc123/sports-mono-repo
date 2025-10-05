using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.PlayerOptions.Commands.UpdatePlayerOption;

public class UpdatePlayerOptionCommandHandler : IRequestHandler<UpdatePlayerOptionCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public UpdatePlayerOptionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(UpdatePlayerOptionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the existing player option
            var playerOptionId = PlayerOptionId.Of(request.PlayerOptionId);
            var playerOption = await _context.PlayerOptions
                .FirstOrDefaultAsync(po => po.Id == playerOptionId, cancellationToken);
            
            if (playerOption == null)
            {
                return Result<bool>.Failure("Player option not found");
            }

            // Update using domain method
            playerOption.UpdateDetails(request.Title, request.Description);

            // Save changes
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (ArgumentException ex)
        {
            return Result<bool>.Failure($"Invalid input: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            return Result<bool>.Failure($"Business rule violation: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to update player option: {ex.Message}");
        }
    }
}
