using Contracts.Contracts;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.PlayerOptions.Commands.CreatePlayerOption;

public record CreatePlayerOptionCommand(string Title, string Description, PlayerId PlayerId, OrganizationId OrganizationId, DateTime? ExpiresAt = null) : IRequest<ServiceResponse<Guid>>;
