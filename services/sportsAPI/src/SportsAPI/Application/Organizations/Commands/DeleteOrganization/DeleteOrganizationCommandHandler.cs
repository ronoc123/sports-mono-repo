using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Repositories;
using MediatR;

namespace Application.Organizations.Commands.DeleteOrganization;

public class DeleteOrganizationCommandHandler : IRequestHandler<DeleteOrganizationCommand, Result<bool>>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IApplicationDbContext _context;

    public DeleteOrganizationCommandHandler(
        IOrganizationRepository organizationRepository,
        IApplicationDbContext context)
    {
        _organizationRepository = organizationRepository;
        _context = context;
    }

    public async Task<Result<bool>> Handle(DeleteOrganizationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if organization exists
            var organization = await _organizationRepository.GetOrganizationByIdAsync(request.OrganizationId);
            
            if (organization == null)
            {
                return Result<bool>.Failure("Organization not found");
            }

            // Check if organization can be deleted (business rules)
            if (organization.IsLocked)
            {
                return Result<bool>.Failure("Cannot delete a locked organization");
            }

            // TODO: Add additional business rules
            // - Check if organization has active codes
            // - Check if organization has active votes
            // - Check if organization has players

            // Delete the organization
            await _organizationRepository.DeleteOrganizationAsync(request.OrganizationId);
            
            // Save changes
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to delete organization: {ex.Message}");
        }
    }
}
