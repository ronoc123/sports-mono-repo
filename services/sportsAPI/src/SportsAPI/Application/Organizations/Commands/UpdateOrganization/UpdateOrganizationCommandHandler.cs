using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Organizations.Commands.UpdateOrganization;

public class UpdateOrganizationCommandHandler : IRequestHandler<UpdateOrganizationCommand, Result<bool>>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IApplicationDbContext _context;

    public UpdateOrganizationCommandHandler(
        IOrganizationRepository organizationRepository,
        IApplicationDbContext context)
    {
        _organizationRepository = organizationRepository;
        _context = context;
    }

    public async Task<Result<bool>> Handle(UpdateOrganizationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the existing organization
            var organization = await _organizationRepository.GetOrganizationByIdAsync(request.OrganizationId);
            
            if (organization == null)
            {
                return Result<bool>.Failure("Organization not found");
            }

            // Update properties
            organization.Name = request.Name;
            organization.FormedYear = request.FormedYear;
            organization.Description = request.Description;

            // Update team info using the domain method
            organization.UpdateTeamInfo(request.TeamId, request.TeamName, request.TeamShortName, request.Sport);

            // Note: For now, we're updating simple properties
            // TODO: Update value objects (Venue, MediaAssets, SocialLinks, TeamColors) when they're properly configured

            // Update the organization
            await _organizationRepository.UpdateOrganizationAsync(organization);
            
            // Save changes
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to update organization: {ex.Message}");
        }
    }
}
