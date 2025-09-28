using Application.Common.Models;
using Contracts.Contracts;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Organizations.Queries.GetOrganizationDetails;

public record GetOrganizationDetailsQuery(OrganizationId OrganizationId) : IRequest<ServiceResponse<OrganizationDetailsDto>>;
