using Contracts.Contracts;
using MediatR;

namespace Application.Poll.Queries.GetPolls;

public record GetPollsQuery(Guid OrganizationId) : IRequest<ServiceResponse<List<PollDto>>>;
