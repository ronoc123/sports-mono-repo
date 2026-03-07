using Contracts.Contracts;
using MediatR;

namespace Application.Dashboard.Queries.GetDashboard;

public record GetDashboardQuery(Guid OrganizationId, Guid UserId) : IRequest<ServiceResponse<DashboardResponse>>;
