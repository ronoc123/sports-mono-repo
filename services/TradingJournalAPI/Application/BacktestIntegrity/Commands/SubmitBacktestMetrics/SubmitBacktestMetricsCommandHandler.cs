using Application.Dto;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Domain.BacktestIntegrity;
using MediatR;

namespace Application.BacktestIntegrity.Commands.SubmitBacktestMetrics;

public sealed class SubmitBacktestMetricsCommandHandler
    : IRequestHandler<SubmitBacktestMetricsCommand, ServiceResponse<BacktestStatusDto>>
{
    private readonly IBacktestSessionRepository _repo;

    public SubmitBacktestMetricsCommandHandler(IBacktestSessionRepository repo) => _repo = repo;

    public async Task<ServiceResponse<BacktestStatusDto>> Handle(
        SubmitBacktestMetricsCommand request,
        CancellationToken cancellationToken)
    {
        var session = await _repo.GetByUserIdAsync(request.UserId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(BacktestSession), request.UserId);

        session.SubmitMetrics(request.TotalTrades, request.WinRate, request.AvgRR);

        _repo.Update(session);
        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResponse.Ok(new BacktestStatusDto
        {
            IsGateUnlocked = session.IsGateUnlocked,
            Expectancy = session.Metrics!.Expectancy,
            AnsweredCount = session.Answers.Count,
            MetricsSubmitted = true,
            Answers = session.Answers.Select(a => new IntegrityAnswerDto
            {
                Id = a.Id,
                QuestionIndex = a.QuestionIndex,
                Answer = a.Answer,
                JournaledReason = a.JournaledReason
            }).ToList()
        }, "Backtest metrics submitted.");
    }
}
