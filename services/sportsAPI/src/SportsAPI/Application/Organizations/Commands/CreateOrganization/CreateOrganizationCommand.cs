using Application.Common.Models;
using MediatR;

namespace Application.Organizations.Commands.CreateOrganization;

public record CreateOrganizationCommand(
    string Name,
    Guid LeagueId,
    string? TeamId,
    string? TeamName,
    string? TeamShortName,
    int? FormedYear,
    string? Sport,
    string? Stadium,
    string? Location,
    int? StadiumCapacity,
    string? Website,
    string? Facebook,
    string? Twitter,
    string? Instagram,
    string? Description,
    string? Color1,
    string? Color2,
    string? Color3,
    string? BadgeUrl,
    string? LogoUrl,
    string? Fanart1Url,
    string? Fanart2Url,
    string? Fanart3Url
) : IRequest<Result<Guid>>;
