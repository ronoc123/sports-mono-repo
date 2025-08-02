using Application.Common.Models;
using MediatR;

namespace Application.Organizations.Commands.UpdateOrganization;

public record UpdateOrganizationCommand(
    Guid OrganizationId,
    string Name,
    string? TeamId,
    string? TeamName,
    string? TeamShortName,
    int? FormedYear,
    string? Sport,
    string? Description,
    // Venue properties
    string? Stadium,
    string? Location,
    int? StadiumCapacity,
    // Media properties
    string? BadgeUrl,
    string? LogoUrl,
    string? Fanart1Url,
    string? Fanart2Url,
    string? Fanart3Url,
    // Social properties
    string? Website,
    string? Facebook,
    string? Twitter,
    string? Instagram,
    // Team colors
    string? Color1,
    string? Color2,
    string? Color3
) : IRequest<Result<bool>>;
