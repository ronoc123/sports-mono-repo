using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Domain.Repositories;
using Domain.Trivia;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using System.Text.Json;

namespace Application.Trivia.Commands.AddTriviaQuestion;

public sealed class AddTriviaQuestionCommandHandler
    : IRequestHandler<AddTriviaQuestionCommand, ServiceResponse<AddTriviaQuestionResult>>
{
    private readonly ITriviaSeriesRepository _repo;

    public AddTriviaQuestionCommandHandler(ITriviaSeriesRepository repo)
    {
        _repo = repo;
    }

    public async Task<ServiceResponse<AddTriviaQuestionResult>> Handle(
        AddTriviaQuestionCommand request, CancellationToken ct)
    {
        var series = await _repo.GetByIdAsync(TriviaSeriesId.Of(request.SeriesId), ct)
            ?? throw new DomainException($"Trivia series '{request.SeriesId}' not found.");

        if (series.OrganizationId != OrganizationId.Of(request.OrganizationId))
            throw new DomainException("Series does not belong to your organization.");

        var optionsJson = JsonSerializer.Serialize(request.AnswerOptions);

        var question = series.AddQuestion(
            request.QuestionText,
            optionsJson,
            request.CorrectAnswer,
            request.VoteRewardAmount);

        await _repo.SaveChangesAsync(ct);

        return ServiceResponse.Ok(
            new AddTriviaQuestionResult(question.Id.Value),
            "Trivia question added.");
    }
}
