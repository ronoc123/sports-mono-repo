using Application.Dto.Player;
using Contracts.Contracts;
using MediatR;

namespace Application.Players.Queries.GetRosterByOrganization;

public record GetRosterByOrganizationQuery(Guid OrganizationId) : IRequest<ServiceResponse<List<PlayerDto>>>;
