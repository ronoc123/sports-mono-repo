using System.Text;
using Domain.Repositories;
using Domain.Shared_kernel;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Admin.Queries.ExportTransactionsCsv;

public sealed class ExportTransactionsCsvQueryHandler
    : IRequestHandler<ExportTransactionsCsvQuery, byte[]>
{
    private readonly IRepository _repo;

    public ExportTransactionsCsvQueryHandler(IRepository repo) => _repo = repo;

    public async Task<byte[]> Handle(
        ExportTransactionsCsvQuery request,
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

        var rows = await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("Id,LeagueId,UserId,Amount,Reason,RefId,CreatedAt");

        foreach (var r in rows)
        {
            sb.AppendLine(string.Join(',',
                r.Id,
                r.LeagueId.Value,
                r.UserId.Value,
                r.Amount,
                EscapeCsv(r.Reason),
                EscapeCsv(r.RefId ?? ""),
                r.CreatedAt.ToString("O")));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
