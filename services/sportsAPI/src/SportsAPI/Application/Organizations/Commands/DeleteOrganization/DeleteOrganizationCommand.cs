using Application.Common.Models;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Organizations.Commands.DeleteOrganization;

public record DeleteOrganizationCommand(OrganizationId OrganizationId) : IRequest<Result<bool>>;
