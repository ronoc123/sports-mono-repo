using Application.Common.Models;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Organizations.Queries.GetOrganizationDetails;

public record GetOrganizationDetailsQuery(OrganizationId OrganizationId) : IRequest<Result<OrganizationDetailsDto>>;
