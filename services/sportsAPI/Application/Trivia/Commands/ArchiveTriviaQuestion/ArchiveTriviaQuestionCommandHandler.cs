using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Domain.Repositories;
using Domain.Trivia;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Trivia.Commands.ArchiveTriviaQuestion;

public sealed class ArchiveTriviaQuestionCommandHandler
    : IRequestHandler<ArchiveTriviaQuestionCommand, ServiceResponse<bool>>
{
    private readonly IRepository _repo;

    public ArchiveTriviaQuestionCommandHandler(IRepository repo)
    {
        _repo = repo;
    }

    public async Task<ServiceResponse<bool>> Handle(
        ArchiveTriviaQuestionCommand request, CancellationToken ct)
    {
        var question = await _repo
            .Query<TriviaQuestion>(asNoTracking: false)
            .FirstOrDefaultAsync(q => q.Id == TriviaQuestionId.Of(request.QuestionId), ct)
            ?? throw new DomainException($"Trivia question '{request.QuestionId}' not found.");

        question.Archive();
        await _repo.SaveChangesAsync(ct);

        return ServiceResponse.Ok(true, "Question archived.");
    }
}
