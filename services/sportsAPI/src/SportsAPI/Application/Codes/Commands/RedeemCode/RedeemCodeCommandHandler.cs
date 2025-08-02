using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Codes.Commands.RedeemCode;

public class RedeemCodeCommandHandler : IRequestHandler<RedeemCodeCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserRepository _userRepository;

    public RedeemCodeCommandHandler(IApplicationDbContext context, IUserRepository userRepository)
    {
        _context = context;
        _userRepository = userRepository;
    }

    public async Task<Result<bool>> Handle(RedeemCodeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the code
            var code = await _context.Codes
                .FirstOrDefaultAsync(c => c.Id.Value == request.CodeId, cancellationToken);

            if (code == null)
            {
                return Result<bool>.Failure("Code not found");
            }

            if (code.IsRedeemed)
            {
                return Result<bool>.Failure("Code has already been redeemed");
            }

            // Get the user
            var user = await _userRepository.GetUserByIdAsync(request.UserId);
            if (user == null)
            {
                return Result<bool>.Failure("User not found");
            }

            // Redeem the code
            user.RedeemCode(code);
            user.AddVotesForOrganization(code.OrganizationId, code.VotesAwarded);

            // Update code status
            code.IsRedeemed = true;
            code.RedeemedAt = DateTime.UtcNow;
            code.RedeemerId = UserId.Of(request.UserId);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to redeem code: {ex.Message}");
        }
    }
}
