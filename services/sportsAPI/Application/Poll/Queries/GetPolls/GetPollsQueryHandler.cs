using Contracts.Contracts;
using Domain.Poll;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Poll.Queries.GetPolls;

public sealed class GetPollsQueryHandler
    : IRequestHandler<GetPollsQuery, ServiceResponse<List<PollDto>>>
{
    private readonly IRepository _repo;

    public GetPollsQueryHandler(IRepository repo)
    {
        _repo = repo;
    }

    public async Task<ServiceResponse<List<PollDto>>> Handle(
        GetPollsQuery request, CancellationToken ct)
    {
        var orgId = OrganizationId.Of(request.OrganizationId);

        var polls = await _repo
            .Query<Domain.Poll.Poll>()
            .Include(p => p.Options)
            .Where(p => p.OrganizationId == orgId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        if (polls.Count == 0)
            return ServiceResponse.Ok(new List<PollDto>(), "No polls found.");

        // Load vote counts per option from PollVote table
        var pollIds = polls.Select(p => p.Id).ToList();
        var voteCounts = await _repo
            .Query<PollVote>()
            .Where(v => pollIds.Contains(v.PollId))
            .GroupBy(v => v.PollOptionId)
            .Select(g => new { OptionId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.OptionId, x => x.Count, ct);

        var result = polls.Select(p =>
        {
            var options = p.Options
                .OrderBy(o => o.SortOrder)
                .Select(o =>
                {
                    voteCounts.TryGetValue(o.Id, out var vc);
                    return new PollOptionDto
                    {
                        PollOptionId = o.Id.Value,
                        OptionText = o.Text,
                        VoteCount = vc,
                        SortOrder = o.SortOrder
                    };
                })
                .ToList();

            return new PollDto
            {
                PollId = p.Id.Value,
                QuestionText = p.QuestionText,
                Status = p.Status,
                TotalVotes = options.Sum(o => o.VoteCount),
                CreatedAt = p.CreatedAt,
                Options = options
            };
        }).ToList();

        return ServiceResponse.Ok(result, "Polls retrieved.");
    }
}
