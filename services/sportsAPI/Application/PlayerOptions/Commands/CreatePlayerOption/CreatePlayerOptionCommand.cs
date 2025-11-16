using Application.Common.Models;
using Application.PlayerOptions.Queries.GetAllPlayerOptions;
using Contracts.Contracts;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.PlayerOptions.Commands.CreatePlayerOption;

public record CreatePlayerOptionCommand(
    string Title,
    string Description,
    Guid PlayerId,
    OrganizationId OrganizationId,
    DateTime? ExpiresAt = null
) : IRequest<ServiceResponse<Guid>>;
