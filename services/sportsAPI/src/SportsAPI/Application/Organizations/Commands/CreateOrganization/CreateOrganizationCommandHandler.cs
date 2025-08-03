using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Organizations;
using Domain.Organizations.Entities;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using Domain.ValueObjects;
using MediatR;

namespace Application.Organizations.Commands.CreateOrganization;

public class CreateOrganizationCommandHandler : IRequestHandler<CreateOrganizationCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILeagueRepository _leagueRepository;

    public CreateOrganizationCommandHandler(IApplicationDbContext context, ILeagueRepository leagueRepository)
    {
        _context = context;
        _leagueRepository = leagueRepository;
    }

    public async Task<Result<Guid>> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify league exists
            var league = await _leagueRepository.GetByIdAsync(request.LeagueId);
            if (league == null)
            {
                return Result<Guid>.Failure("League not found");
            }

            // Create value objects
            var venue = new Venue(
                request.Stadium ?? string.Empty,
                request.Location ?? string.Empty,
                request.StadiumCapacity ?? 0
            );

            var mediaAssets = new MediaAssets(
                request.BadgeUrl ?? string.Empty,
                request.LogoUrl ?? string.Empty,
                request.Fanart1Url ?? string.Empty,
                request.Fanart2Url ?? string.Empty,
                request.Fanart3Url ?? string.Empty
            );

            var socialLinks = new SocialLinks(
                request.Website ?? string.Empty,
                request.Facebook ?? string.Empty,
                request.Twitter ?? string.Empty,
                request.Instagram ?? string.Empty
            );

            var teamColors = new TeamColors(
                request.Color1 ?? string.Empty,
                request.Color2 ?? string.Empty,
                request.Color3 ?? string.Empty
            );

            // Create organization using domain factory method
            var organization = Organization.Create(
                OrganizationId.Of(Guid.NewGuid()),
                LeagueId.Of(request.LeagueId),
                request.Name,
                request.TeamId,
                request.TeamName,
                request.TeamShortName,
                request.FormedYear,
                request.Sport,
                venue,
                mediaAssets,
                socialLinks,
                teamColors,
                request.Description
            );

            // Add organization to league - temporarily commented out to debug ID issue
            // league.AddOrganization(organization);

            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(organization.Id.Value);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Failed to create organization: {ex.Message}");
        }
    }
}
