using Contracts.Contracts;
using Domain.Repositories;
using Domain.Shared_kernel;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Admin.Queries.GetTransactionAudit;

public sealed class GetTransactionAuditQueryHandler
    : IRequestHandler<GetTransactionAuditQuery, ServiceResponse<TransactionAuditResult>>
{
    private readonly IRepository _repo;

    public GetTransactionAuditQueryHandler(IRepository repo) => _repo = repo;

    public async Task<ServiceResponse<TransactionAuditResult>> Handle(
        GetTransactionAuditQuery request,
        CancellationToken cancellationToken)
    {
        var leagueId = LeagueId.Of(request.LeagueId);

        var query = _repo.Query<VoteTransaction>()
            .Where(t => t.LeagueId == leagueId);

        if (request.UserId.HasValue)
        {
            var userId = UserId.Of(request.UserId.Value);
            query = query.Where(t => t.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(request.Reason))
            query = query.Where(t => t.Reason == request.Reason);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(t => new TransactionAuditDto(
            t.Id,
            t.LeagueId.Value,
            t.UserId.Value,
            t.Amount,
            t.Reason,
            t.RefId,
            t.CreatedAt)).ToList();

        return ServiceResponse.Ok(
            new TransactionAuditResult(dtos, total, request.Page, request.PageSize),
            $"{total} transaction(s) found.");
    }
}
