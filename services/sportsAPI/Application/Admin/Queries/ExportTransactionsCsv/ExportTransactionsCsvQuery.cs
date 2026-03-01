using MediatR;

namespace Application.Admin.Queries.ExportTransactionsCsv;

public record ExportTransactionsCsvQuery(
    Guid OrgId,
    Guid? UserId = null,
    string? Reason = null
) : IRequest<byte[]>;
