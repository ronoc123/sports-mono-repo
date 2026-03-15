using Application.Dto;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Domain.BacktestIntegrity;
using MediatR;

namespace Application.BacktestIntegrity.Commands.SubmitIntegrityAnswers;

public sealed class SubmitIntegrityAnswersCommandHandler
    : IRequestHandler<SubmitIntegrityAnswersCommand, ServiceResponse<BacktestStatusDto>>
{
    private readonly IBacktestSessionRepository _repo;

    public SubmitIntegrityAnswersCommandHandler(IBacktestSessionRepository repo) => _repo = repo;

    public async Task<ServiceResponse<BacktestStatusDto>> Handle(
        SubmitIntegrityAnswersCommand request,
        CancellationToken cancellationToken)
    {
        var session = await _repo.GetByUserIdAsync(request.UserId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(BacktestSession), request.UserId);

        var oldAnswers = session.Answers.ToList();

        session.SubmitAnswers(request.Answers
            .Select(a => (a.QuestionIndex, a.Answer, a.Reason))
            .ToList());

        _repo.ReplaceAnswers(oldAnswers, session.Answers);
        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResponse.Ok(new BacktestStatusDto
        {
            IsGateUnlocked = session.IsGateUnlocked,
            Expectancy = session.Metrics?.Expectancy,
            AnsweredCount = session.Answers.Count,
            MetricsSubmitted = session.Metrics is not null,
            Answers = session.Answers.Select(a => new IntegrityAnswerDto
            {
                Id = a.Id,
                QuestionIndex = a.QuestionIndex,
                Answer = a.Answer,
                JournaledReason = a.JournaledReason
            }).ToList()
        }, "Integrity answers submitted.");
    }
}
