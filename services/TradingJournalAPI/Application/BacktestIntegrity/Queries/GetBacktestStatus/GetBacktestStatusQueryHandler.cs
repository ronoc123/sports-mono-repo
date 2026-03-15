using Application.Dto;
using Contracts.Contracts;
using Domain.BacktestIntegrity;
using MediatR;

namespace Application.BacktestIntegrity.Queries.GetBacktestStatus;

public sealed class GetBacktestStatusQueryHandler
    : IRequestHandler<GetBacktestStatusQuery, ServiceResponse<BacktestStatusDto>>
{
    private readonly IBacktestSessionRepository _repo;

    public GetBacktestStatusQueryHandler(IBacktestSessionRepository repo) => _repo = repo;

    public async Task<ServiceResponse<BacktestStatusDto>> Handle(
        GetBacktestStatusQuery request,
        CancellationToken cancellationToken)
    {
        var session = await _repo.GetByUserIdAsync(request.UserId, cancellationToken);

        if (session is null)
            return ServiceResponse.Ok<BacktestStatusDto>(null!, "No backtest session found.");

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
        }, "Success");
    }
}
