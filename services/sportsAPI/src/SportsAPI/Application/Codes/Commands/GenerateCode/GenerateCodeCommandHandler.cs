using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Repositories;
using Domain.SharedKernal;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Codes.Commands.GenerateCode;

public class GenerateCodeCommandHandler : IRequestHandler<GenerateCodeCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationRepository _organizationRepository;

    public GenerateCodeCommandHandler(IApplicationDbContext context, IOrganizationRepository organizationRepository)
    {
        _context = context;
        _organizationRepository = organizationRepository;
    }

    public async Task<Result<Guid>> Handle(GenerateCodeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify organization exists
            var organization = await _organizationRepository.GetOrganizationByIdAsync(request.OrganizationId);
            if (organization == null)
            {
                return Result<Guid>.Failure("Organization not found");
            }

            // Create new code
            var code = new Code
            {
                Id = CodeId.Of(Guid.NewGuid()),
                VotesAwarded = 50, // Default votes awarded
                IsRedeemed = false,
                RedeemedAt = null,
                RedeemerId = null,
                OrganizationId = OrganizationId.Of(request.OrganizationId),
                CreatedAt = DateTime.UtcNow
            };

            _context.Codes.Add(code);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(code.Id.Value);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Failed to generate code: {ex.Message}");
        }
    }
}
