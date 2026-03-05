using Contracts.Contracts;
using MediatR;

namespace Application.Admin.Queries.GetTransactionAudit;

public record GetTransactionAuditQuery(
    Guid LeagueId,
    Guid? UserId = null,
    string? Reason = null,
    int Page = 1,
    int PageSize = 50
) : IRequest<ServiceResponse<TransactionAuditResult>>;

public record TransactionAuditResult(
    List<TransactionAuditDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public record TransactionAuditDto(
    long Id,
    Guid OrgId,
    Guid UserId,
    long Amount,
    string Reason,
    string? RefId,
    DateTimeOffset CreatedAt
);
