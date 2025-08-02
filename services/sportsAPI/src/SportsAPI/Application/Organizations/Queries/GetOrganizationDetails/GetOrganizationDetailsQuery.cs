using Application.Common.Models;
using MediatR;

namespace Application.Organizations.Queries.GetOrganizationDetails;

public record GetOrganizationDetailsQuery(Guid OrganizationId) : IRequest<Result<OrganizationDetailsDto>>;
