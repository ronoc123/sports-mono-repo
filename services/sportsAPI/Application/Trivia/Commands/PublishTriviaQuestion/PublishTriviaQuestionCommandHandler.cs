using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Domain.Repositories;
using Domain.Trivia;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Trivia.Commands.PublishTriviaQuestion;

public sealed class PublishTriviaQuestionCommandHandler
    : IRequestHandler<PublishTriviaQuestionCommand, ServiceResponse<bool>>
{
    private readonly IRepository _repo;

    public PublishTriviaQuestionCommandHandler(IRepository repo)
    {
        _repo = repo;
    }

    public async Task<ServiceResponse<bool>> Handle(
        PublishTriviaQuestionCommand request, CancellationToken ct)
    {
        var question = await _repo
            .Query<TriviaQuestion>(asNoTracking: false)
            .FirstOrDefaultAsync(q => q.Id == TriviaQuestionId.Of(request.QuestionId), ct)
            ?? throw new DomainException($"Trivia question '{request.QuestionId}' not found.");

        question.Publish();
        await _repo.SaveChangesAsync(ct);

        return ServiceResponse.Ok(true, "Question published.");
    }
}
