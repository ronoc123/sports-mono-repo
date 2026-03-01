using Application.Admin.Queries.GetPackConfig;
using Contracts.Contracts;
using MediatR;

namespace Application.Admin.Commands.UpdatePackConfig;

public record UpdatePackConfigCommand(Guid OrgId, long PackPointCost)
    : IRequest<ServiceResponse<PackConfigDto>>;
