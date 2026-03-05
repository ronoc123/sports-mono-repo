using MediatR;

namespace Application.Admin.Queries.ExportTransactionsCsv;

public record ExportTransactionsCsvQuery(
    Guid LeagueId,
    Guid? UserId = null,
    string? Reason = null
) : IRequest<byte[]>;
