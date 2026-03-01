using Application.Cards.Commands.PurchasePack;
using Contracts.Contracts;
using Domain.Cards;
using Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Admin.Queries.GetPackConfig;

public sealed class GetPackConfigQueryHandler
    : IRequestHandler<GetPackConfigQuery, ServiceResponse<PackConfigDto>>
{
    private readonly IRepository _repo;

    public GetPackConfigQueryHandler(IRepository repo) => _repo = repo;

    public async Task<ServiceResponse<PackConfigDto>> Handle(
        GetPackConfigQuery request,
        CancellationToken cancellationToken)
    {
        var config = await _repo.Query<PackConfig>()
            .FirstOrDefaultAsync(p => p.OrgId == request.OrgId, cancellationToken);

        // Fall back to hard-coded default if no DB record exists yet
        var cost = config?.PackPointCost ?? PurchasePackCommandHandler.DefaultPackCost;

        return ServiceResponse.Ok(new PackConfigDto(request.OrgId, cost), "Pack config loaded.");
    }
}
