using Application.Common.Models;
using MediatR;

namespace Application.Organizations.Commands.DeleteOrganization;

public record DeleteOrganizationCommand(Guid OrganizationId) : IRequest<Result<bool>>;
