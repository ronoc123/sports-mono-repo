using Application.Dto;
using Contracts.Contracts;
using Domain.BacktestIntegrity;
using MediatR;

namespace Application.BacktestIntegrity.Commands.CreateBacktestSession;

public sealed class CreateBacktestSessionCommandHandler
    : IRequestHandler<CreateBacktestSessionCommand, ServiceResponse<BacktestSessionDto>>
{
    private readonly IBacktestSessionRepository _repo;

    public CreateBacktestSessionCommandHandler(IBacktestSessionRepository repo) => _repo = repo;

    public async Task<ServiceResponse<BacktestSessionDto>> Handle(
        CreateBacktestSessionCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await _repo.GetByUserIdAsync(request.UserId, cancellationToken);
        if (existing is not null)
            return ServiceResponse.Ok(ToDto(existing), "Backtest session already exists.");

        var session = BacktestSession.Create(request.UserId);
        await _repo.AddAsync(session, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResponse.Ok(ToDto(session), "Backtest session created.");
    }

    private static BacktestSessionDto ToDto(BacktestSession s) => new()
    {
        Id = s.Id.Value,
        UserId = s.UserId,
        IsGateUnlocked = s.IsGateUnlocked,
        Metrics = s.Metrics is null ? null : new BacktestMetricsDto
        {
            TotalTrades = s.Metrics.TotalTrades,
            WinRate = s.Metrics.WinRate,
            AvgRR = s.Metrics.AvgRR,
            Expectancy = s.Metrics.Expectancy
        },
        Answers = s.Answers.Select(a => new IntegrityAnswerDto
        {
            Id = a.Id,
            QuestionIndex = a.QuestionIndex,
            Answer = a.Answer,
            JournaledReason = a.JournaledReason
        }).ToList(),
        CreatedAt = s.CreatedAt
    };
}
