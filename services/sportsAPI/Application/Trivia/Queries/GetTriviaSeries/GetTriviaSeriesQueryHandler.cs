using Contracts.Contracts;
using Domain.Repositories;
using Domain.Trivia;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Application.Trivia.Queries.GetTriviaSeries;

public sealed class GetTriviaSeriesQueryHandler
    : IRequestHandler<GetTriviaSeriesQuery, ServiceResponse<List<TriviaSeriesDto>>>
{
    private readonly IRepository _repo;

    public GetTriviaSeriesQueryHandler(IRepository repo)
    {
        _repo = repo;
    }

    public async Task<ServiceResponse<List<TriviaSeriesDto>>> Handle(
        GetTriviaSeriesQuery request, CancellationToken ct)
    {
        var orgId = OrganizationId.Of(request.OrganizationId);

        // Load all series for the org with their questions
        var seriesList = await _repo
            .Query<TriviaSeries>()
            .Include(s => s.Questions)
            .Where(s => s.OrganizationId == orgId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);

        if (seriesList.Count == 0)
            return ServiceResponse.Ok(new List<TriviaSeriesDto>(), "No trivia series found.");

        // Collect all question IDs across all series
        var questionIds = seriesList
            .SelectMany(s => s.Questions)
            .Select(q => q.Id)
            .ToList();

        // Single query: participation counts per question
        var participationCounts = await _repo
            .Query<TriviaAnswer>()
            .Where(a => questionIds.Contains(a.TriviaQuestionId))
            .GroupBy(a => a.TriviaQuestionId)
            .Select(g => new { QuestionId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.QuestionId, x => x.Count, ct);

        var result = seriesList.Select(s => new TriviaSeriesDto
        {
            SeriesId = s.Id.Value,
            SeriesName = s.Name,
            CreatedAt = s.CreatedAt,
            Questions = s.Questions.Select(q =>
            {
                var options = DeserializeOptions(q.OptionsJson);
                participationCounts.TryGetValue(q.Id, out var count);
                return new TriviaQuestionDto
                {
                    QuestionId = q.Id.Value,
                    QuestionText = q.QuestionText,
                    AnswerOptions = options,
                    CorrectAnswer = q.CorrectOption,
                    VoteRewardAmount = q.VoteReward,
                    Status = q.Status,
                    ParticipationCount = count,
                    CreatedAt = q.CreatedAt
                };
            }).OrderBy(q => q.CreatedAt).ToList()
        }).ToList();

        return ServiceResponse.Ok(result, "Trivia series retrieved.");
    }

    private static List<string> DeserializeOptions(string json)
    {
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? []; }
        catch { return []; }
    }
}
