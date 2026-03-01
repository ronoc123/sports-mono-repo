using Contracts.Contracts;
using MediatR;

namespace Application.Admin.Queries.GetPackConfig;

public record GetPackConfigQuery(Guid OrgId)
    : IRequest<ServiceResponse<PackConfigDto>>;

public record PackConfigDto(Guid OrgId, long PackPointCost);
