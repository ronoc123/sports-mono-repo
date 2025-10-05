using Application.Common.Models;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.PlayerOptions.Commands.CreatePlayerOption;

public record CreatePlayerOptionCommand(
    string Title,
    string Description,
    Guid PlayerId,
    OrganizationId OrganizationId,
    DateTime? ExpiresAt = null
) : IRequest<Result<Guid>>;
