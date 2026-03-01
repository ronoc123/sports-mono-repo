using Contracts.Contracts;
using Domain.Marketplace;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using Domain.VoteAccount;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Marketplace.Queries.GetAvailableBalance;

public sealed class GetAvailableBalanceQueryHandler
    : IRequestHandler<GetAvailableBalanceQuery, ServiceResponse<long>>
{
    private readonly IRepository _repo;

    public GetAvailableBalanceQueryHandler(IRepository repo) => _repo = repo;

    public async Task<ServiceResponse<long>> Handle(
        GetAvailableBalanceQuery request,
        CancellationToken cancellationToken)
    {
        // VoteAccount.Balance already reflects DeductForBidEscrow deductions,
        // so available = Balance directly. This endpoint exposes it clearly.
        var account = await _repo.GetByIdAsync<VoteAccount>(
            cancellationToken,
            OrganizationId.Of(request.OrgId),
            UserId.Of(request.UserId));

        var balance = account?.Balance ?? 0;

        return ServiceResponse.Ok(balance);
    }
}
