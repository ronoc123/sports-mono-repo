using Application.Common.Models;
using MediatR;

namespace Application.PlayerOptions.Commands.CreatePlayerOption;

public record CreatePlayerOptionCommand(
    string Title,
    string Description,
    Guid PlayerId,
    Guid OrganizationId,
    DateTime? ExpiresAt = null
) : IRequest<Result<Guid>>;
