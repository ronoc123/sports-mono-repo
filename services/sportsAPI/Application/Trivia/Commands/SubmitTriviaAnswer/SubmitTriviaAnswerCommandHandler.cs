using Application.Common.Interfaces;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Domain.Enums;
using Domain.Organizations;
using Domain.Repositories;
using Domain.Trivia;
using Domain.ValueObjects.ConcreteTypes;
using Domain.VoteAccount;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Trivia.Commands.SubmitTriviaAnswer;

public sealed class SubmitTriviaAnswerCommandHandler
    : IRequestHandler<SubmitTriviaAnswerCommand, ServiceResponse<SubmitTriviaAnswerResult>>
{
    private readonly IRepository _repo;
    private readonly IApplicationDbContext _db;
    private readonly IBalanceNotificationService _balanceNotifier;

    public SubmitTriviaAnswerCommandHandler(
        IRepository repo,
        IApplicationDbContext db,
        IBalanceNotificationService balanceNotifier)
    {
        _repo = repo;
        _db = db;
        _balanceNotifier = balanceNotifier;
    }

    public async Task<ServiceResponse<SubmitTriviaAnswerResult>> Handle(
        SubmitTriviaAnswerCommand request, CancellationToken ct)
    {
        var questionId = TriviaQuestionId.Of(request.TriviaQuestionId);
        var userId = request.UserId;

        // 1. Load the question (tracked for IncrementAnswerCount)
        var question = await _repo
            .Query<TriviaQuestion>(asNoTracking: false)
            .FirstOrDefaultAsync(q => q.Id == questionId, ct)
            ?? throw new DomainException($"Trivia question '{request.TriviaQuestionId}' not found.");

        if (question.Status != TriviaQuestionStatus.Active)
            throw new DomainException("This trivia question is not currently active.");

        // 2. Idempotency guard — reject duplicate submissions
        var alreadyAnswered = await _db.TriviaAnswers
            .AnyAsync(a => a.UserId == userId && a.TriviaQuestionId == questionId, ct);

        if (alreadyAnswered)
            throw new DomainException("You have already answered this trivia question.");

        // 3. Evaluate correctness
        var isCorrect = string.Equals(
            request.SelectedAnswer.Trim(),
            question.CorrectOption.Trim(),
            StringComparison.OrdinalIgnoreCase);

        // 4. Record the answer and update participation count
        var answer = TriviaAnswer.Record(questionId, userId, request.SelectedAnswer.Trim(), isCorrect);
        question.IncrementAnswerCount();
        await _db.TriviaAnswers.AddAsync(answer, ct);

        // 5. Credit vote reward if correct
        var votesEarned = 0;
        if (isCorrect && question.VoteReward > 0)
        {
            // Resolve the league via the organisation
            var org = await _repo
                .Query<Organization>()
                .FirstOrDefaultAsync(o => o.Id == OrganizationId.Of(request.OrganizationId), ct)
                ?? throw new DomainException("Organisation not found.");

            var account = await _repo.GetByIdAsync<VoteAccount>(
                ct,
                LeagueId.Of(org.LeagueId.Value),
                UserId.Of(userId));

            if (account is not null)
            {
                account.CreditTriviaReward(question.VoteReward, request.TriviaQuestionId.ToString());
                _repo.Update(account);
                votesEarned = question.VoteReward;
            }
        }

        await _db.SaveChangesAsync(ct);

        if (isCorrect && votesEarned > 0)
        {
            _ = _balanceNotifier.NotifyBalanceChangedAsync(userId.ToString(), 0, ct);
        }

        return ServiceResponse.Ok(
            new SubmitTriviaAnswerResult(isCorrect, votesEarned, question.CorrectOption),
            isCorrect ? "Correct! Votes credited." : "Incorrect answer.");
    }
}
