using Contracts.Contracts;
using Domain.Enums;
using Domain.Player;
using Domain.PlayerOption;
using Domain.Poll;
using Domain.Repositories;
using Domain.Trivia;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Application.Dashboard.Queries.GetDashboard;

public sealed class GetDashboardQueryHandler
    : IRequestHandler<GetDashboardQuery, ServiceResponse<DashboardResponse>>
{
    private readonly IRepository _repo;

    public GetDashboardQueryHandler(IRepository repo)
    {
        _repo = repo;
    }

    public async Task<ServiceResponse<DashboardResponse>> Handle(
        GetDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var orgId = OrganizationId.Of(request.OrganizationId);
        var now = DateTime.UtcNow;

        // ── Trending player options ────────────────────────────────────────────
        var trending = await _repo
            .Query<PlayerOption>()
            .Where(po => po.OrganizationId == orgId && po.ExpiresAt > now)
            .Join(
                _repo.Query<Player>(),
                po => po.PlayerId,
                p => p.Id,
                (po, p) => new TrendingPlayerOptionDto
                {
                    PlayerOptionId = po.Id.Value,
                    PlayerName = p.Name,
                    PositionLabel = p.Position,
                    VoteCount = po.Votes
                })
            .OrderByDescending(dto => dto.VoteCount)
            .ToListAsync(cancellationToken);

        // ── Active trivia questions ────────────────────────────────────────────
        var activeQuestions = await _repo
            .Query<TriviaQuestion>()
            .Where(q => q.Status == TriviaQuestionStatus.Active)
            .Join(
                _repo.Query<TriviaSeries>().Where(s => s.OrganizationId == orgId),
                q => q.SeriesId,
                s => s.Id,
                (q, s) => new { q, SeriesLabel = s.Name })
            .ToListAsync(cancellationToken);

        List<ActiveTriviaQuestionDto> triviaQuestions;
        if (activeQuestions.Count > 0)
        {
            var questionIds = activeQuestions.Select(x => x.q.Id).ToList();
            var myAnswers = await _repo
                .Query<TriviaAnswer>()
                .Where(a => a.UserId == request.UserId && questionIds.Contains(a.TriviaQuestionId))
                .ToDictionaryAsync(a => a.TriviaQuestionId, a => a, cancellationToken);

            triviaQuestions = activeQuestions.Select(x =>
            {
                myAnswers.TryGetValue(x.q.Id, out var answer);
                return new ActiveTriviaQuestionDto
                {
                    QuestionId = x.q.Id.Value,
                    QuestionText = x.q.QuestionText,
                    AnswerOptions = DeserializeOptions(x.q.OptionsJson),
                    SeriesLabel = x.SeriesLabel,
                    VoteRewardAmount = x.q.VoteReward,
                    AnsweredByMe = answer is not null,
                    SelectedAnswer = answer?.SelectedOption
                };
            }).ToList();
        }
        else
        {
            triviaQuestions = [];
        }

        // ── Active poll ───────────────────────────────────────────────────────
        var activePoll = await _repo
            .Query<Domain.Poll.Poll>()
            .Include(p => p.Options)
            .Where(p => p.OrganizationId == orgId && p.Status == PollStatus.Active)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        ActivePollDto? pollDto = null;
        if (activePoll is not null)
        {
            // Load vote counts from PollVote table
            var optionIds = activePoll.Options.Select(o => o.Id).ToList();
            var voteCounts = await _repo
                .Query<PollVote>()
                .Where(v => v.PollId == activePoll.Id)
                .GroupBy(v => v.PollOptionId)
                .Select(g => new { OptionId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.OptionId, x => x.Count, cancellationToken);

            // Check if user has voted
            var myVote = await _repo
                .Query<PollVote>()
                .Where(v => v.PollId == activePoll.Id && v.UserId == request.UserId)
                .FirstOrDefaultAsync(cancellationToken);

            pollDto = new ActivePollDto
            {
                PollId = activePoll.Id.Value,
                QuestionText = activePoll.QuestionText,
                VotedByMe = myVote is not null,
                SelectedOptionId = myVote?.PollOptionId.Value,
                Options = activePoll.Options
                    .OrderBy(o => o.SortOrder)
                    .Select(o =>
                    {
                        voteCounts.TryGetValue(o.Id, out var vc);
                        return new ActivePollOptionDto
                        {
                            PollOptionId = o.Id.Value,
                            OptionText = o.Text,
                            VoteCount = vc
                        };
                    })
                    .ToList()
            };
        }

        var response = new DashboardResponse
        {
            TrendingPlayerOptions = trending,
            ActiveTriviaQuestions = triviaQuestions,
            ActivePoll = pollDto
        };

        return ServiceResponse.Ok(response, "Dashboard loaded.");
    }

    private static List<string> DeserializeOptions(string json)
    {
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? []; }
        catch { return []; }
    }
}
